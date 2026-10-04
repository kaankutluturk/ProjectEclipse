import unittest
from RigMapping import match_bones, infer_humanoid, ROLES


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


if __name__ == '__main__':
    unittest.main()
