"""Semantic bone matching for the imported fighter adapter (no Blender dependency).

map_rig() is the importer entry point. Names identify limbs and the head when
they are recognizable; the skeleton hierarchy then derives the pelvis, spine,
chest and neck so numbered spine chains, clavicles, twist and end bones do not
need a mapping file. Unfamiliar names fall back to topology/rest positions.
"""
import re
import math

CENTRAL = {
    'pelvis': ('pelvis', 'hips', 'hip', 'lowerbody'),
    'spine': ('spine', 'spine1', 'spine01', 'spine001', 'waist', 'abdomen', 'torso'),
    'chest': ('chest', 'upperchest', 'spine2', 'spine02', 'spine002', 'spine03', 'spine003', 'upperbody', 'ribcage'),
    'neck': ('neck', 'neck1', 'neck01'),
    'head': ('head', 'head1'),
}
LIMBS = {
    'upper_arm': ('upperarm', 'arm', 'armupper', 'uparm', 'shldr', 'shldrbend'),
    'forearm': ('forearm', 'lowerarm', 'armlower', 'loarm', 'elbow', 'forearmbend'),
    'hand': ('hand', 'wrist'),
    'thigh': ('thigh', 'upleg', 'upperleg', 'legupper', 'uleg', 'thighbend'),
    'shin': ('shin', 'calf', 'leg', 'lowerleg', 'leglower', 'loleg', 'knee'),
    'foot': ('foot', 'ankle'),
}
ROLES = tuple(CENTRAL) + tuple(side + '_' + role for side in ('left', 'right') for role in LIMBS)
REQUIRED = set(ROLES) - {'spine', 'neck'}
SIDES = {'l': 'left', 'left': 'left', 'lft': 'left', 'r': 'right', 'right': 'right', 'rgt': 'right'}
# Exporter/rig-tool prefixes that carry no anatomy: Mixamo, Rigify, VRoid,
# 3ds Max Biped, Character Creator, Valve and common generic joint labels.
NOISE = {'mixamorig', 'def', 'org', 'mch', 'j', 'bip', 'bip01', 'bip001', 'bip002', 'cc', 'base',
         'valvebiped', 'c', 'jnt', 'joint', 'bone', 'bn', 'sk', 'rig', 'armature', 'deform', 'b'}


def tokens(name):
    name = re.split(r'[:|]', name)[-1]
    name = re.sub(r'([a-z0-9])([A-Z])', r'\1 \2', name)
    name = re.sub(r'([A-Z]+)([A-Z][a-z])', r'\1 \2', name)
    result = []
    for token in re.split(r'[^A-Za-z0-9]+', name.lower()):
        if not token:
            continue
        # Lowercase glued sides such as "leftarm" or "rightupleg".
        for side in ('left', 'right'):
            if token.startswith(side) and len(token) > len(side):
                result.append(side); token = token[len(side):]
                break
        result.append(token)
    return result


def shared_prefix(names):
    """A leading word nearly every bone shares (a model ID such as "4jtr01t0 l thigh").

    Exporters often prefix every bone with the model or rig name, separated by a
    space or underscore instead of a ':' namespace. Such a word is not anatomy.
    """
    names = list(names)
    firsts = [tokens(name)[:1] for name in names]
    counts = {}
    for first in firsts:
        if first:
            counts[first[0]] = counts.get(first[0], 0) + 1
    if len(names) < 8 or not counts:
        return None
    word, count = max(counts.items(), key=lambda item: item[1])
    anatomy = {alias for values in CENTRAL.values() for alias in values} | {
        alias for values in LIMBS.values() for alias in values}
    if count < .8 * len(names) or word in SIDES or word in anatomy:
        return None
    return word


def describe(name, prefix=None):
    """Return (side or None, anatomical base name) for a bone name."""
    side = None; base = []
    words = tokens(name)
    if prefix and words[:1] == [prefix]:
        words = words[1:]
    for token in words:
        if token in SIDES and side is None:
            side = SIDES[token]
        elif token not in NOISE:
            base.append(token)
    return side, ''.join(base)


def normalized(name):
    side, base = describe(name)
    return (side or '') + base


def role_matches(role, name, prefix=None):
    side, base = describe(name, prefix)
    if role in CENTRAL:
        return side is None and base in CENTRAL[role]
    want, part = role.split('_', 1)
    return side == want and base in LIMBS[part]


def match_bones(names, overrides=None):
    """Names-only matching. Ambiguous or missing required roles raise ValueError."""
    names = list(names)
    overrides = _check_overrides(names, overrides)
    result = dict(overrides)
    prefix = shared_prefix(names)
    for role in ROLES:
        if role in result:
            continue
        found = [name for name in names if role_matches(role, name, prefix)]
        if len(found) > 1:
            raise ValueError(role + ': ambiguous bones ' + ', '.join(found) + '; supply --mapping')
        if found:
            result[role] = found[0]
    _finish(result)
    return result


def suggest_roles(names, overrides=None):
    """Best-effort role -> bone guesses (unique name matches) for a manual mapping UI."""
    names = list(names)
    prefix = shared_prefix(names)
    result = {role: bone for role, bone in (overrides or {}).items() if role in ROLES and bone in names}
    for role in ROLES:
        if role not in result:
            found = [name for name in names if role_matches(role, name, prefix)]
            if len(found) == 1:
                result[role] = found[0]
    return result


def _check_overrides(names, overrides):
    overrides = {} if overrides is None else overrides
    if not isinstance(overrides, dict) or set(overrides) - set(ROLES):
        raise ValueError('Mapping keys must be semantic roles: ' + ', '.join(ROLES))
    for role, name in overrides.items():
        if name not in names:
            raise ValueError(role + ': source bone does not exist: ' + str(name))
    return dict(overrides)


def _finish(result):
    missing = sorted(REQUIRED - set(result))
    if missing:
        raise ValueError('Cannot identify ' + ', '.join(missing) + '; supply --mapping with these roles')
    if len(set(result.values())) != len(result):
        raise ValueError('Each mapped semantic role needs a distinct source bone')


class _Tree:
    """Bone hierarchy without unweighted leaf markers (end/nub/IK target bones)."""

    def __init__(self, parents, positions, weighted):
        keep = set(parents)
        if weighted is not None:
            changed = True
            while changed:
                changed = False
                for name in list(keep):
                    if name not in weighted and not any(parents[c] == name for c in keep):
                        keep.discard(name); changed = True
        self.parents = {n: (parents[n] if parents[n] in keep else None) for n in keep}
        self.positions = positions
        self.children = {n: [] for n in keep}
        for name, parent in self.parents.items():
            if parent is not None:
                self.children[parent].append(name)
        for values in self.children.values():
            values.sort()
        self.size = {}
        for name in keep:
            self._size(name)

    def _size(self, name):
        if name not in self.size:
            self.size[name] = 1 + sum(self._size(c) for c in self.children[name])
        return self.size[name]

    def ancestors(self, name):
        result = []
        while name is not None:
            result.append(name); name = self.parents[name]
        return result

    def is_ancestor(self, older, name):
        return older in self.ancestors(name)

    def common(self, a, b):
        seen = set(self.ancestors(a))
        for name in self.ancestors(b):
            if name in seen:
                return name
        return None

    def path(self, ancestor, name):
        """Bones strictly below ancestor down to name, top first."""
        chain = []
        while name is not None and name != ancestor:
            chain.append(name); name = self.parents[name]
        return list(reversed(chain)) if name == ancestor else None

    def chain(self, name):
        """Follow the main branch; leaf twist/helper side branches do not end it."""
        result = [name]
        while True:
            kids = self.children[name]
            if not kids:
                return result
            big = [k for k in kids if self.size[k] > 1]
            if len(kids) == 1:
                name = kids[0]
            elif len(big) == 1:
                name = big[0]
            else:
                return result
            result.append(name)


def _vector(a, b):
    return tuple(x - y for x, y in zip(a, b))


def _length(v):
    return math.sqrt(sum(x * x for x in v))


def _unit(v):
    length = _length(v)
    return tuple(x / length for x in v) if length > 1e-8 else (0, 0, 0)


def _dot(a, b):
    return sum(x * y for x, y in zip(a, b))


def _triple(tree, chain, offsets=(0, 1)):
    """Pick (upper, lower, end) from a limb chain with an optional clavicle/hip start.

    Upper and lower limb segments have comparable lengths; a clavicle or a
    finger/toe continuation does not.
    """
    best = None
    for o in offsets:
        if len(chain) < o + 3:
            continue
        a, b, c = chain[o:o + 3]
        upper = _length(_vector(tree.positions[b], tree.positions[a]))
        lower = _length(_vector(tree.positions[c], tree.positions[b]))
        if min(upper, lower) < 1e-8:
            continue
        score = abs(upper - lower) / max(upper, lower)
        if best is None or score < best[0]:
            best = (score, (a, b, c))
    return None if best is None else best[1]


def infer_humanoid(parents, positions, weighted=None):
    """Infer a clear pelvis/trunk/head and two arm and leg branches from topology.

    Positions are rest heads in a common coordinate space. Clavicles, toes,
    end bones, twist leaves and extra props are tolerated. Ambiguous or unusual
    topologies reject instead of guessing.
    """
    tree = _Tree(parents, positions, weighted)
    candidates = []
    for pelvis, branches in tree.children.items():
        if len(branches) < 3:
            continue
        for trunk_start in branches:
            trunk = tree.chain(trunk_start); chest = trunk[-1]
            kids = tree.children[chest]
            if len(kids) < 3:
                continue
            up = _unit(_vector(positions[chest], positions[pelvis]))
            if up == (0, 0, 0):
                continue

            def reach(branch, sign):
                return max(sign * _dot(_vector(positions[n], positions[pelvis]), up)
                           for n in tree.chain(branch))
            head_scores = sorted(((_dot(_unit(_vector(positions[tree.chain(k)[-1]], positions[chest])), up), k)
                                  for k in kids), reverse=True)
            if head_scores[0][0] < .65 or head_scores[0][0] - head_scores[1][0] < .2:
                continue
            head_chain = tree.chain(head_scores[0][1])
            if len(head_chain) > 4:
                continue
            arm_roots = [k for k in kids if k != head_scores[0][1] and len(tree.chain(k)) >= 3]
            if len(arm_roots) != 2:
                continue
            leg_roots = sorted((b for b in branches if b != trunk_start and len(tree.chain(b)) >= 3),
                               key=lambda b: reach(b, -1), reverse=True)
            if len(leg_roots) < 2 or (len(leg_roots) > 2 and reach(leg_roots[1], -1) - reach(leg_roots[2], -1) < 1e-6):
                continue
            arms = [_triple(tree, tree.chain(k)) for k in arm_roots]
            legs = [_triple(tree, tree.chain(k)) for k in leg_roots[:2]]
            if None in arms or None in legs:
                continue
            # Legs must extend down the trunk axis. Arms need separated roots.
            if any(_dot(_unit(_vector(positions[c[2]], positions[pelvis])), up) > -.2 for c in legs):
                continue
            separation = _vector(positions[arms[0][0]], positions[arms[1][0]])
            if _length(separation) < 1e-8:
                continue
            labelled = {describe(c[0], shared_prefix(parents))[0]: c for c in arms}
            if set(labelled) == {'left', 'right'}:
                left, right = labelled['left'], labelled['right']
            else:
                axis = max(range(3), key=lambda i: abs(separation[i]))
                left, right = sorted(arms, key=lambda c: positions[c[0]][axis], reverse=True)
            lateral = _unit(_vector(positions[left[0]], positions[right[0]]))
            left_leg, right_leg = sorted(legs, key=lambda c: _dot(_vector(positions[c[0]], positions[pelvis]), lateral), reverse=True)
            mapping = {'pelvis': pelvis, 'chest': chest, 'head': head_chain[-1]}
            if len(trunk) > 1:
                mapping['spine'] = trunk[0]
            if len(head_chain) > 1:
                mapping['neck'] = head_chain[0]
            for side, arm, leg in (('left', left, left_leg), ('right', right, right_leg)):
                mapping.update({side+'_'+role: name for role, name in zip(('upper_arm', 'forearm', 'hand'), arm)})
                mapping.update({side+'_'+role: name for role, name in zip(('thigh', 'shin', 'foot'), leg)})
            candidates.append(mapping)
    if len(candidates) != 1:
        raise ValueError('Hierarchy matching needs one unambiguous humanoid pelvis/trunk/head and arm/leg layout; supply --mapping')
    return candidates[0]


def _named(tree, names, role, taken, outermost=True, prefix=None):
    found = [n for n in names if n in tree.parents and n not in taken and role_matches(role, n, prefix)]
    if len(found) > 1 and outermost:
        # Prefer the bone nearest the root when one candidate contains the other
        # (for example a hand and its "hand_end" helper).
        roots = [n for n in found if not any(o != n and tree.is_ancestor(o, n) for o in found)]
        found = roots if len(roots) == 1 else found
    return found


def map_rig(names, parents, positions, weighted=None, overrides=None):
    """Return (mapping, method, notes) for a humanoid skeleton.

    names: candidate (deform) bones; parents: name -> parent name or None among
    them; positions: rest heads; weighted: bones that own positive vertex
    weights (optional); overrides: role -> exact bone name.
    """
    names = list(names)
    overrides = _check_overrides(names, overrides)
    tree = _Tree({n: parents.get(n) for n in names}, positions, weighted)
    notes = []
    mapping = dict(overrides)
    taken = set(mapping.values())
    prefix = shared_prefix(names)
    if prefix:
        notes.append('Ignored the shared bone-name prefix "' + prefix + '".')
    limb_roles = [s + '_' + r for s in ('left', 'right') for r in LIMBS]
    for role in limb_roles + ['head']:
        if role in mapping:
            continue
        found = _named(tree, names, role, taken, prefix=prefix)
        if len(found) == 1:
            mapping[role] = found[0]; taken.add(found[0])
    complete = all(r in mapping for r in limb_roles + ['head'])
    if complete and _limbs_consistent(tree, mapping):
        method = 'names'
        arms = tree.common(mapping['left_upper_arm'], mapping['right_upper_arm'])
        legs = tree.common(mapping['left_thigh'], mapping['right_thigh'])
        if 'pelvis' not in mapping:
            named = _named(tree, names, 'pelvis', taken, outermost=False, prefix=prefix)
            named = [n for n in named if legs is not None and tree.is_ancestor(n, legs)]
            # The nearest named ancestor of both thighs; otherwise their junction.
            named.sort(key=lambda n: -len(tree.ancestors(n)))
            mapping['pelvis'] = named[0] if named else legs
            if mapping['pelvis'] is None:
                raise ValueError('The thighs do not share a parent; supply --mapping with pelvis')
        if 'chest' not in mapping:
            mapping['chest'] = arms
            if arms is None or arms == mapping['pelvis']:
                named = _named(tree, names, 'chest', taken, prefix=prefix)
                if len(named) != 1:
                    raise ValueError('The arms do not branch from a distinct chest; supply --mapping with chest')
                mapping['chest'] = named[0]
        junction = tree.common(mapping['pelvis'], mapping['chest'])
        between = tree.path(junction, mapping['chest']) or []
        # Legs hung beside a named pelvis/hips bone (both under a bare root): the
        # named bone at the bottom of the trunk is the better pelvis.
        if ('pelvis' not in overrides and between and between[0] != mapping['chest'] and
                role_matches('pelvis', between[0], prefix) and between[0] not in mapping.values()):
            mapping['pelvis'] = between[0]
            notes.append('Pelvis is the trunk bone ' + between[0] + '; the legs branch beside it.')
            junction = tree.common(mapping['pelvis'], mapping['chest'])
            between = tree.path(junction, mapping['chest']) or []
        if 'spine' not in mapping and len(between) > 1 and between[0] not in mapping.values():
            mapping['spine'] = between[0]
        neck_path = tree.path(mapping['chest'], mapping['head']) or []
        if 'neck' not in mapping and len(neck_path) > 1 and neck_path[0] not in mapping.values():
            mapping['neck'] = neck_path[0]
        if not tree.is_ancestor(mapping['chest'], mapping['head']):
            notes.append('Head is not parented below the chest; using its rest position only.')
    else:
        method = 'hierarchy'
        inferred = infer_humanoid({n: parents.get(n) for n in names}, positions, weighted)
        mapping = dict(inferred, **overrides)
    _finish(mapping)
    return mapping, method, notes


def _limbs_consistent(tree, mapping):
    for side in ('left', 'right'):
        for chain in (('upper_arm', 'forearm', 'hand'), ('thigh', 'shin', 'foot')):
            bones = [mapping[side + '_' + role] for role in chain]
            if not (tree.is_ancestor(bones[0], bones[1]) and tree.is_ancestor(bones[1], bones[2])):
                return False
    return True
