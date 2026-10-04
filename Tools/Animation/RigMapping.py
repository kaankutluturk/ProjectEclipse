"""Semantic bone matching for the imported fighter adapter (no Blender dependency)."""
import re
import math

CENTRAL = {
    'pelvis': ('pelvis', 'hips', 'hip'),
    'spine': ('spine', 'spine1', 'spine01', 'spine001'),
    'chest': ('chest', 'upperchest', 'spine2', 'spine02', 'spine002', 'spine03', 'spine003'),
    'neck': ('neck', 'neck1', 'neck01'),
    'head': ('head', 'head1'),
}
LIMBS = {
    'upper_arm': ('upperarm', 'arm', 'armupper'),
    'forearm': ('forearm', 'lowerarm', 'armlower'),
    'hand': ('hand', 'wrist'),
    'thigh': ('thigh', 'upleg', 'upperleg', 'legupper'),
    'shin': ('shin', 'calf', 'leg', 'lowerleg', 'leglower'),
    'foot': ('foot', 'ankle'),
}
ROLES = tuple(CENTRAL) + tuple(side + '_' + role for side in ('left', 'right') for role in LIMBS)
REQUIRED = set(ROLES) - {'spine', 'neck'}


def normalized(name):
    name = name.rsplit(':', 1)[-1].lower()
    name = re.sub(r'^(def|org|mch)[-_]', '', name)
    return re.sub(r'[^a-z0-9]', '', name)


def aliases(role):
    if role in CENTRAL:
        return set(CENTRAL[role])
    side, part = role.split('_', 1)
    return {candidate for base in LIMBS[part] for marker in (side, side[0])
            for candidate in (marker + base, base + marker)}


def match_bones(names, overrides=None):
    names = list(names)
    overrides = {} if overrides is None else overrides
    if not isinstance(overrides, dict) or set(overrides) - set(ROLES):
        raise ValueError('Mapping keys must be semantic roles: ' + ', '.join(ROLES))
    result = {}
    for role in ROLES:
        if role in overrides:
            if overrides[role] not in names:
                raise ValueError(role + ': source bone does not exist: ' + str(overrides[role]))
            result[role] = overrides[role]
            continue
        found = [name for name in names if normalized(name) in aliases(role)]
        if len(found) > 1:
            raise ValueError(role + ': ambiguous bones ' + ', '.join(found) + '; supply --mapping')
        if found:
            result[role] = found[0]
    missing = sorted(REQUIRED - set(result))
    if missing:
        raise ValueError('Cannot identify ' + ', '.join(missing) + '; supply --mapping with these roles')
    if len(set(result.values())) != len(result):
        raise ValueError('Each mapped semantic role needs a distinct source bone')
    return result


def infer_humanoid(parents, positions):
    """Infer only clear pelvis/trunk/chest with two three-segment arm chains.

    No exporter-specific bone numbers. Ambiguous or unusual topologies reject.
    Positions are rest heads in a common coordinate space.
    """
    children = {name: [] for name in parents}
    for name, parent in parents.items():
        if parent in children:
            children[parent].append(name)
    def chain(name):
        result = [name]
        while len(children[name]) == 1:
            name = children[name][0]; result.append(name)
        return result
    def difference(a, b):
        return tuple(x - y for x, y in zip(positions[a], positions[b]))
    def unit(vector):
        length = math.sqrt(sum(x * x for x in vector))
        return tuple(x / length for x in vector) if length > 1e-8 else (0, 0, 0)
    def dot(a, b):
        return sum(x * y for x, y in zip(a, b))
    candidates = []
    for pelvis, branches in children.items():
        if len(branches) != 3:
            continue
        for trunk_start in branches:
            trunk = chain(trunk_start); chest = trunk[-1]
            if len(children[chest]) != 3:
                continue
            up = unit(difference(chest, pelvis))
            head_scores = sorted(((dot(unit(difference(chain(branch)[-1], chest)), up), branch)
                                  for branch in children[chest]), reverse=True)
            if head_scores[0][0] < .65 or head_scores[0][0] - head_scores[1][0] < .2:
                continue
            head_chain = chain(head_scores[0][1])
            if len(head_chain) not in (1, 2):
                continue
            arms = [chain(b) for b in children[chest] if b != head_scores[0][1]]
            legs = [chain(b) for b in branches if b != trunk_start]
            if any(len(c) != 3 for c in arms) or any(len(c) not in (3, 4) for c in legs):
                continue
            # Legs must extend down the trunk axis. Arms need separated roots.
            if any(dot(unit(difference(c[2], pelvis)), up) > -.2 for c in legs):
                continue
            separation = difference(arms[0][0], arms[1][0])
            if sum(x*x for x in separation) < 1e-8:
                continue
            def side_key(name):
                if re.search(r'(^|[_. :\-])(left|l)([_. :\-]|$)', name, re.IGNORECASE):
                    return 'left'
                if re.search(r'(^|[_. :\-])(right|r)([_. :\-]|$)', name, re.IGNORECASE):
                    return 'right'
                return None
            labelled = {side_key(c[0]): c for c in arms}
            if set(labelled) == {'left', 'right'}:
                left, right = labelled['left'], labelled['right']
            else:
                axis = max(range(3), key=lambda i: abs(separation[i]))
                left, right = sorted(arms, key=lambda c: positions[c[0]][axis], reverse=True)
            lateral = unit(difference(left[0], right[0]))
            left_leg, right_leg = sorted(legs, key=lambda c: dot(difference(c[0], pelvis), lateral), reverse=True)
            mapping = {'pelvis': pelvis, 'chest': chest, 'head': head_chain[-1]}
            if len(trunk) > 1:
                mapping['spine'] = trunk[0]
            if len(head_chain) > 1:
                mapping['neck'] = head_chain[0]
            for side, arm, leg in (('left', left, left_leg), ('right', right, right_leg)):
                mapping.update({side+'_'+role: name for role, name in zip(('upper_arm','forearm','hand'), arm)})
                mapping.update({side+'_'+role: name for role, name in zip(('thigh','shin','foot'), leg)})
            candidates.append(mapping)
    if len(candidates) != 1:
        raise ValueError('Hierarchy matching needs one unambiguous humanoid pelvis/trunk/head and arm/leg layout; supply --mapping')
    return candidates[0]
