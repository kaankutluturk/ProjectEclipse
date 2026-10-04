import unittest
from RigMapping import match_bones, infer_humanoid, map_rig, describe, ROLES


def humanoid(names, extra=()):
    """Build a Z-up rest skeleton from role->name with optional clavicles/toes/ends."""
    parents, points = {}, {}
    def add(name, parent, point):
        if name:
            parents[name] = parent; points[name] = point
        return name or parent
    add(names.get('pelvis_root'), None, (0, 0, .95))
    pelvis = add(names['pelvis'], names.get('pelvis_root'), (0, 0, 1))
    trunk = names.get('waist_parent', pelvis)
    z = 1.05
    for spine in names.get('spine', []):
        trunk = add(spine, trunk, (0, 0, z)); z += .1
    chest = add(names['chest'], trunk, (0, 0, z))
    neck = add(names.get('neck'), chest, (0, 0, z + .15))
    head = add(names['head'], neck, (0, 0, z + .25))
    add(names.get('head_end'), head, (0, 0, z + .45))
    for side, sign in (('left', 1), ('right', -1)):
        n = names[side]
        parent = add(n.get('clavicle'), chest, (sign * .05, 0, z + .1))
        upper = add(n['upper_arm'], parent, (sign * .2, 0, z + .1))
        add(n.get('upper_twist'), upper, (sign * .3, 0, z + .1))
        fore = add(n['forearm'], upper, (sign * .48, 0, z + .1))
        hand = add(n['hand'], fore, (sign * .74, 0, z + .1))
        for i, finger in enumerate(n.get('fingers', ())):
            add(finger, hand, (sign * .82, (i - 2) * .02, z + .1))
        thigh = add(n['thigh'], pelvis, (sign * .1, 0, .95))
        shin = add(n['shin'], thigh, (sign * .1, 0, .5))
        foot = add(n['foot'], shin, (sign * .1, 0, .08))
        toe = add(n.get('toe'), foot, (sign * .1, -.12, .02))
        add(n.get('toe_end'), toe, (sign * .1, -.18, .02))
    for name, parent, point in extra:
        add(name, parent, point)
    return parents, points


class MappingTests(unittest.TestCase):
    def test_mixamo(self):
        names = ['mixamorig:' + n for n in ('Hips', 'Spine', 'Spine2', 'Neck', 'Head')]
        names += ['mixamorig:' + s + n for s in ('Left', 'Right') for n in ('Arm', 'ForeArm', 'Hand', 'UpLeg', 'Leg', 'Foot')]
        result = match_bones(names + ['mixamorig:LeftHandIndex1'])
        self.assertEqual(len(result), 17)
        self.assertEqual(result['left_forearm'], 'mixamorig:LeftForeArm')

    def test_blender(self):
        names = ['hips', 'spine', 'chest', 'neck', 'head']
        names += ['DEF-' + n + '.' + s for s in ('L', 'R') for n in ('upper_arm', 'forearm', 'hand', 'thigh', 'shin', 'foot')]
        self.assertEqual(match_bones(names)['right_thigh'], 'DEF-thigh.R')

    def test_unreal(self):
        names = ['pelvis', 'spine_01', 'spine_03', 'neck_01', 'head']
        names += [n + '_' + s for s in ('l', 'r') for n in ('upperarm', 'lowerarm', 'hand', 'thigh', 'calf', 'foot')]
        self.assertEqual(match_bones(names)['chest'], 'spine_03')

    def test_unknown_names_require_small_role_mapping(self):
        overrides = {role: 'Joint' + str(i) for i, role in enumerate(ROLES)}
        self.assertEqual(match_bones(overrides.values(), overrides), overrides)
        with self.assertRaisesRegex(ValueError, 'Cannot identify'):
            match_bones(overrides.values())

    def test_ambiguity_and_bad_overrides(self):
        with self.assertRaisesRegex(ValueError, 'ambiguous'):
            match_bones(['Hips', 'pelvis'])
        with self.assertRaisesRegex(ValueError, 'source bone does not exist'):
            match_bones([], {'head': 'missing'})
        with self.assertRaisesRegex(ValueError, 'Mapping keys'):
            match_bones([], {'tentacle': 'tip'})

    def test_unknown_hierarchy_and_rotated_rest_space(self):
        parents = {'a': None, 'b': 'a', 'c': 'b', 'd': 'c', 'e': 'd'}
        points = {'a': (0, 0, 1), 'b': (0, 0, 1.2), 'c': (0, 0, 1.4),
                  'd': (0, 0, 1.6), 'e': (0, 0, 1.8)}
        for side, sign in (('Left', 1), ('Right', -1)):
            for i in range(3):
                arm, leg = side+'Joint'+str(i), side+'Lower'+str(i)
                parents[arm] = 'c' if i == 0 else side+'Joint'+str(i-1)
                parents[leg] = 'a' if i == 0 else side+'Lower'+str(i-1)
                points[arm] = (sign*(.15+i*.25), 0, 1.4)
                points[leg] = (sign*.1, 0, 1-i*.4)
            parents[side+'Toe'] = side+'Lower2'; points[side+'Toe'] = (sign*.1, .1, .1)
        mapping = infer_humanoid(parents, points)
        self.assertEqual(mapping['left_hand'], 'LeftJoint2')
        self.assertEqual(mapping['right_foot'], 'RightLower2')
        self.assertEqual(mapping['head'], 'e')
        # Rotate 90 degrees, translate and scale: inference is not tied to Z-up.
        rotated = {name: (z*3+7, x*3-2, y*3+8) for name, (x,y,z) in points.items()}
        self.assertEqual(infer_humanoid(parents, rotated), mapping)
        with self.assertRaisesRegex(ValueError, 'unambiguous'):
            infer_humanoid({'root': None, 'tail': 'root'}, {'root': (0,0,0), 'tail': (0,0,1)})


class RigTests(unittest.TestCase):
    def assertMapped(self, parents, points, expected, weighted=None, method=None):
        mapping, used, notes = map_rig(list(parents), parents, points, weighted)
        for role, name in expected.items():
            self.assertEqual(mapping.get(role), name, role)
        if method:
            self.assertEqual(used, method)
        return mapping

    def test_full_mixamo_with_spine1_clavicles_toes_and_ends(self):
        m = lambda n: 'mixamorig:' + n
        side = lambda s: {'clavicle': m(s + 'Shoulder'), 'upper_arm': m(s + 'Arm'), 'forearm': m(s + 'ForeArm'),
                          'hand': m(s + 'Hand'), 'fingers': [m(s + 'HandIndex1'), m(s + 'HandThumb1')],
                          'thigh': m(s + 'UpLeg'), 'shin': m(s + 'Leg'), 'foot': m(s + 'Foot'),
                          'toe': m(s + 'ToeBase'), 'toe_end': m(s + 'Toe_End')}
        parents, points = humanoid({'pelvis': m('Hips'), 'spine': [m('Spine'), m('Spine1')], 'chest': m('Spine2'),
                                    'neck': m('Neck'), 'head': m('Head'), 'head_end': m('HeadTop_End'),
                                    'left': side('Left'), 'right': side('Right')})
        self.assertMapped(parents, points, {'pelvis': m('Hips'), 'spine': m('Spine'), 'chest': m('Spine2'),
                                            'neck': m('Neck'), 'head': m('Head'), 'left_upper_arm': m('LeftArm'),
                                            'right_shin': m('RightLeg')}, method='names')

    def test_unreal_twist_and_clavicles(self):
        side = lambda s: {'clavicle': 'clavicle_' + s, 'upper_arm': 'upperarm_' + s, 'upper_twist': 'upperarm_twist_01_' + s,
                          'forearm': 'lowerarm_' + s, 'hand': 'hand_' + s, 'thigh': 'thigh_' + s, 'shin': 'calf_' + s,
                          'foot': 'foot_' + s, 'toe': 'ball_' + s}
        parents, points = humanoid({'pelvis_root': 'root', 'pelvis': 'pelvis', 'spine': ['spine_01', 'spine_02', 'spine_03', 'spine_04'],
                                    'chest': 'spine_05', 'neck': 'neck_01', 'head': 'head', 'left': side('l'), 'right': side('r')})
        self.assertMapped(parents, points, {'pelvis': 'pelvis', 'spine': 'spine_01', 'chest': 'spine_05',
                                            'left_forearm': 'lowerarm_l', 'right_foot': 'foot_r'})

    def test_vroid_biped_and_character_creator_names(self):
        self.assertEqual(describe('J_Bip_L_UpperArm'), ('left', 'upperarm'))
        self.assertEqual(describe('Bip01 R Calf'), ('right', 'calf'))
        self.assertEqual(describe('CC_Base_L_Forearm'), ('left', 'forearm'))
        self.assertEqual(describe('lShldrBend'), ('left', 'shldrbend'))
        self.assertEqual(describe('DEF-upper_arm.L.001'), ('left', 'upperarm001'))
        side = lambda s: {'clavicle': 'CC_Base_' + s + '_Clavicle', 'upper_arm': 'CC_Base_' + s + '_Upperarm',
                          'forearm': 'CC_Base_' + s + '_Forearm', 'hand': 'CC_Base_' + s + '_Hand',
                          'thigh': 'CC_Base_' + s + '_Thigh', 'shin': 'CC_Base_' + s + '_Calf', 'foot': 'CC_Base_' + s + '_Foot'}
        # Character Creator: the trunk branches from Hip beside, not below, Pelvis.
        parents, points = humanoid({'pelvis_root': 'CC_Base_Hip', 'pelvis': 'CC_Base_Pelvis', 'waist_parent': 'CC_Base_Hip',
                                    'spine': ['CC_Base_Waist', 'CC_Base_Spine01'], 'chest': 'CC_Base_Spine02',
                                    'neck': 'CC_Base_NeckTwist01', 'head': 'CC_Base_Head', 'left': side('L'), 'right': side('R')})
        self.assertMapped(parents, points, {'pelvis': 'CC_Base_Pelvis', 'spine': 'CC_Base_Waist', 'chest': 'CC_Base_Spine02',
                                            'neck': 'CC_Base_NeckTwist01', 'left_thigh': 'CC_Base_L_Thigh'})

    def test_model_id_prefix_and_legs_beside_pelvis(self):
        # Every bone carries a model ID word; the legs hang off a bare root beside "pelvis".
        p = lambda n: '4jtr01t0 ' + n
        side = lambda s: {'clavicle': p(s + ' clavicle'), 'upper_arm': p(s + ' upperarm'), 'forearm': p(s + ' forearm'),
                          'hand': p(s + ' hand'), 'fingers': [p(s + ' finger0'), p(s + ' finger1')],
                          'thigh': p(s + ' thigh'), 'shin': p(s + ' calf'), 'foot': p(s + ' foot'), 'toe': p(s + ' toe0')}
        parents, points = humanoid({'pelvis_root': p('trall'), 'pelvis': '4jtr01t0', 'waist_parent': '4jtr01t0',
                                    'spine': [p('pelvis'), p('spine'), p('spine1')], 'chest': p('spine2'),
                                    'neck': p('neck'), 'head': p('head'), 'left': side('l'), 'right': side('r')})
        mapping = self.assertMapped(parents, points, {'pelvis': p('pelvis'), 'spine': p('spine'), 'chest': p('spine2'),
                                                      'neck': p('neck'), 'head': p('head'), 'left_forearm': p('l forearm'),
                                                      'right_shin': p('r calf'), 'left_foot': p('l foot')}, method='names')
        self.assertEqual(mapping['right_upper_arm'], p('r upperarm'))

    def test_unweighted_end_bones_and_props_do_not_break_topology(self):
        side = lambda s: {'clavicle': s + 'b1', 'upper_arm': s + 'b2', 'forearm': s + 'b3', 'hand': s + 'b4',
                          'thigh': s + 'c1', 'shin': s + 'c2', 'foot': s + 'c3', 'toe': s + 'c4', 'toe_end': s + 'c5'}
        parents, points = humanoid({'pelvis': 'j0', 'spine': ['j1'], 'chest': 'j2', 'neck': 'j3', 'head': 'j4',
                                    'head_end': 'j5', 'left': side('p'), 'right': side('q')},
                                   extra=[('prop', 'j0', (0, .3, 1))])
        weighted = set(parents) - {'j5', 'pc5', 'qc5', 'prop'}
        mapping = self.assertMapped(parents, points, {'pelvis': 'j0', 'chest': 'j2', 'head': 'j4', 'neck': 'j3',
                                                      'left_upper_arm': 'pb2', 'left_hand': 'pb4', 'right_thigh': 'qc1',
                                                      'right_foot': 'qc3'}, weighted, method='hierarchy')
        self.assertEqual(mapping['spine'], 'j1')


if __name__ == '__main__':
    unittest.main()
