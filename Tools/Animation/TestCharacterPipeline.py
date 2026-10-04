"""Exercise authored point animation and model validation without Blender."""
import copy
import math
import tempfile
import unittest
from pathlib import Path
import xml.etree.ElementTree as ET
import CharacterPipeline as pipeline


class PipelineTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.path = Path(self.temp.name)
        self.rig_file = self.path / 'body.xml'
        self.rig_file.write_text('<Scene><Nodes><A Type="Node" Mass="1"/><B Type="Node" Mass="1"/><M Type="MacroNode" NodesCount="2" ChildNode1="A" ChildNode2="B" LCC1="0.5" LCC2="0.5"/></Nodes><Edges><AB Type="Edge" End1="A" End2="B"/></Edges><Figures><Body Type="Capsule" Edge="AB" Radius1="2" Radius2="3"/></Figures></Scene>')
        self.rig = pipeline.model(self.rig_file)
        self.source = {'names': ['B', 'M', 'A'], 'fps': 30, 'frames': [[[2, 0, 0], [99, 99, 99], [0, 0, 0]], [[4, 0, 0], [99, 99, 99], [2, 0, 0]]]}

    def tearDown(self):
        self.temp.cleanup()

    def test_reorder_resample_helpers_and_binary(self):
        clip = pipeline.bake(self.rig, self.source)
        self.assertEqual(clip['names'], ['A', 'B', 'M'])
        self.assertEqual(len(clip['frames']), 3)
        self.assertEqual(clip['frames'][1], [[1, 0, 0], [3, 0, 0], [2, 0, 0]])
        output = self.path / 'test.bytes'
        pipeline.write_animation(output, clip)
        self.assertEqual(pipeline.read_animation(output, self.rig), clip)
        output.write_bytes(output.read_bytes()[:-1])
        with self.assertRaises(ValueError): pipeline.read_animation(output, self.rig)

    def test_bad_animation_inputs(self):
        for change in [lambda s: s.update(fps=0), lambda s: s.update(names=['A', 'A', 'M']),
                       lambda s: s['frames'][0][0].__setitem__(0, float('nan')),
                       lambda s: s['frames'][0].pop(), lambda s: s.update(frames=[]),
                       lambda s: s.update(fps=float('inf')), lambda s: s.update(fps=240)]:
            source = copy.deepcopy(self.source); change(source)
            with self.assertRaises(ValueError): pipeline.bake(self.rig, source)

    def test_bad_model_references(self):
        for source in ['<Scene><Nodes><A Type="Node"/><A Type="Node"/></Nodes><Figures/></Scene>',
                       '<Scene><Nodes><A Type="Node"/></Nodes><Edges><E Type="Edge" End1="A" End2="missing"/></Edges><Figures/></Scene>',
                       '<Scene><Nodes><A Type="MacroNode" NodesCount="1" ChildNode1="A"/></Nodes><Figures/></Scene>',
                       '<Scene><Nodes><A Type="Node" Mass="nan"/></Nodes><Figures/></Scene>',
                       '<Scene><Nodes><A Type="Node" Mass="-1"/></Nodes><Figures/></Scene>',
                       '<!DOCTYPE Scene><Scene><Figures/></Scene>']:
            self.rig_file.write_text(source)
            with self.assertRaises(ValueError): pipeline.model(self.rig_file)

    def test_skin_binds_to_body(self):
        skin = self.path / 'skin.xml'
        skin.write_text('<Scene><Nodes><S Type="MacroNode" NodesCount="1" ChildNode1="A" LCC1="1"/></Nodes><Edges/><Figures><F Type="Triangle" Node1="A" Node2="B" Node3="S"/></Figures></Scene>')
        self.assertEqual(pipeline.model(skin, self.rig).tag, 'Scene')
        with self.assertRaises(ValueError): pipeline.model(skin)

    def test_relative_and_absolute_skinned_bindings(self):
        skin = self.path / 'skin.xml'
        base = '<Scene><Nodes><S Type="SkinnedNode" BonesCount="1" BoneStart1="A" BoneEnd1="B" Weight1="1" Along1="0.5" {}/></Nodes><Edges/><Figures/></Scene>'
        combined = ET.fromstring(ET.tostring(self.rig))
        for fields, expected in (('Across1="0.25"', [1.0, 0.5]), ('Offset1="3" Extend1="1"', [2.0, 3.0])):
            skin.write_text(base.format(fields))
            nodes = pipeline.model(skin, self.rig).find('Nodes')
            model = copy.deepcopy(combined); model.find('Nodes').extend(list(nodes))
            # A=(0,0) and B=(2,0): relative Across scales with the 2-unit segment;
            # absolute Offset/Extend stay in native units (counter-clockwise normal is +Y).
            positions = {'A': [0, 0, 0], 'B': [2, 0, 0], 'M': [1, 0, 0]}
            pipeline.helper_positions(model, positions)
            self.assertEqual(positions['S'][:2], expected)
        for fields in ('', 'Across1="0" Offset1="0"', 'Across1="0" Extend1="1"', 'Offset1="1001"', 'Offset1="0" Extend1="-1001"'):
            skin.write_text(base.format(fields))
            with self.assertRaises(ValueError): pipeline.model(skin, self.rig)

    def test_visual_proportion_rig(self):
        # A standing canonical-style pose; proportions shorten legs and torso.
        p = {'NPivot': [0, 100, 0], 'NStomach': [0, 115, 0], 'NChest': [0, 135, 0], 'NNeck': [0, 160, 0],
             'NHead': [0, 170, 0], 'NTop': [0, 190, 0], 'NChestF': [10, 135, 0], 'NPelvisF': [10, 100, 0]}
        for s, z in (('1', 10), ('2', -10)):
            p.update({'NShoulder_' + s: [0, 155, z], 'NElbow_' + s: [10, 120, z], 'NWrist_' + s: [30, 100, z],
                      'NFingertips_' + s: [40, 98, z], 'NHip_' + s: [0, 95, z / 2], 'NKnee_' + s: [10, 50, z / 2],
                      'NAnkle_' + s: [0, 8, z / 2], 'NHeel_' + s: [-3, 0, z / 2], 'NToeTip_' + s: [15, 0, z / 2]})
        native = lambda a, b: math.dist(p[a], p[b])
        lengths = {key: native(*key) * (.8 if key[0] in ('NHip_1', 'NHip_2', 'NKnee_1', 'NKnee_2') else 1.1)
                   for key in pipeline.PROPORTION_SEGMENTS}
        v = pipeline.visual_joints(p, lengths)
        for name in ('NWrist_1', 'NAnkle_2', 'NHeel_1'):
            self.assertEqual(v[name], p[name])  # gameplay contact points stay put
        for a, b in pipeline.TORSO_SEGMENTS + (('NHip_1', 'NKnee_1'), ('NKnee_1', 'NAnkle_1'), ('NShoulder_2', 'NElbow_2')):
            self.assertAlmostEqual(math.dist(v[a], v[b]), lengths[(a, b)], places=4)
        self.assertLess(v['NPivot'][1], p['NPivot'][1])  # shorter legs lower the drawn pelvis
        scene = ET.fromstring('<Scene><Figures/><Proportions Version="1">' + ''.join(
            f'<Segment Start="{a}" End="{b}" Length="{lengths[(a, b)]}"/>' for a, b in pipeline.PROPORTION_SEGMENTS) + '</Proportions></Scene>')
        self.assertEqual(pipeline.read_proportions(scene), {k: float(f'{x}') for k, x in lengths.items()})
        scene.find('Proportions').remove(scene.find('Proportions')[0])
        with self.assertRaisesRegex(ValueError, 'missing'): pipeline.read_proportions(scene)

    def test_3d_skin_frames(self):
        p = {'NPivot': [0, 100, 0], 'NStomach': [0, 115, 0], 'NChest': [0, 135, 0], 'NNeck': [0, 160, 0],
             'NHead': [0, 170, 0], 'NTop': [0, 190, 0], 'NPelvisF': [0, 100, 10], 'NStomachF': [0, 115, 10],
             'NChestF': [0, 135, 10], 'NHeadF': [5, 175, 10]}
        for s, x in (('1', 10), ('2', -10)):
            p.update({'NShoulder_' + s: [x, 155, 0], 'NElbow_' + s: [x * 3, 155, 0], 'NWrist_' + s: [x * 5, 155, 0],
                      'NFingertips_' + s: [x * 6, 155, 0], 'NKnuckles_' + s: [x * 5.5, 155, 0], 'NHip_' + s: [x, 95, 0], 'NKnee_' + s: [x, 50, 0],
                      'NAnkle_' + s: [x, 8, 0], 'NHeel_' + s: [x, 0, -3], 'NToeTip_' + s: [x, 0, 15]})
        # Rest = current pose, so every limb inherits its parent's frame unchanged.
        rest = {}
        base = {key: pipeline.frame(pipeline._sub(p[key[1]], p[key[0]]), pipeline._sub(p[front], p[origin]))
                for key, (origin, front) in pipeline.TORSO_FRONT.items()}
        for child, parent in pipeline.LIMB_PARENTS:
            columns = base[parent]
            rest[child] = pipeline._unit(pipeline._transpose_apply(columns, pipeline._sub(p[child[1]], p[child[0]])))
            base[child] = columns
        frames = pipeline.visual_frames(p, p, rest)
        for child, parent in pipeline.LIMB_PARENTS:
            for axis, expected in zip(frames[child][1], base[parent]):
                for value, wanted in zip(axis, expected):
                    self.assertAlmostEqual(value, wanted, places=6)
        # Bending the elbow 90 degrees rotates a forearm-local point with it.
        bent = dict(p, NWrist_1=[30, 135, 0])
        frames = pipeline.visual_frames(bent, bent, rest)
        origin, columns = frames[('NElbow_1', 'NWrist_1')]
        point = pipeline._add(origin, pipeline._apply(columns, pipeline._transpose_apply(base[('NChest', 'NNeck')], [20, 0, 0])))
        self.assertAlmostEqual(point[0], 30, places=4); self.assertAlmostEqual(point[1], 135, places=4)

    def test_reversed_limb_labels_are_unswapped(self):
        p = {'NPivot': [0, 100, 0], 'NStomach': [0, 115, 0], 'NChest': [0, 135, 0], 'NNeck': [0, 160, 0],
             'NPelvisF': [10, 100, 0], 'NChestF': [10, 135, 0]}
        for s, z in (('1', -10), ('2', 10)):  # up x front = -Z, so _1 belongs at -Z
            for joint in ('NShoulder', 'NElbow', 'NWrist', 'NFingertips', 'NHip', 'NKnee', 'NAnkle', 'NHeel', 'NToeTip'):
                p[joint + '_' + s] = [0, 50, z]
        self.assertGreater(pipeline.limb_side(p, *pipeline.PAIR_GROUPS[0][1]), 0)
        swapped = dict(p)
        for joint in ('NShoulder', 'NElbow', 'NHip', 'NToeTip'):
            swapped[joint + '_1'], swapped[joint + '_2'] = p[joint + '_2'], p[joint + '_1']
        swapped.update({j + '_1': p[j + '_2'] for j in ('NWrist', 'NFingertips', 'NKnee', 'NAnkle', 'NHeel')})
        swapped.update({j + '_2': p[j + '_1'] for j in ('NWrist', 'NFingertips', 'NKnee', 'NAnkle', 'NHeel')})
        self.assertEqual(pipeline.unswap_pairs(swapped), p)
        self.assertEqual(pipeline.unswap_pairs(p), p)

    def test_finger_curl_folds_a_fist(self):
        pivots = [[10, 0, 0], [14, 0, 0], [17, 0, 0]]
        tip = [20, 0, 0]
        self.assertEqual(pipeline.curl_point(tip, pivots, 0.0, [0, 0, 1]), tip)  # open hand
        folded = pipeline.curl_point(tip, pivots, 1.0, [0, 0, -1])  # full curl: 260 degrees in total
        self.assertLess(folded[0], 14)  # the fingertip comes back over the knuckles
        self.assertAlmostEqual(math.dist(folded, pivots[0]), math.dist(pipeline.curl_point(tip, pivots, 1, [0, 0, -1]), pivots[0]))
        self.assertAlmostEqual(abs(folded[2]), 0, places=6)  # stays in the curl plane
        palm = [[1, 0, 0], [0, 1, 0], [0, 0, 1]]
        share, axis = pipeline.finger_curl({'NWrist_1': [0, 0, 0], 'NKnuckles_1': [10, 0, 0], 'NFingertips_1': [10, -10, 0]},
                                           palm, '1', 0.0)
        self.assertAlmostEqual(share, 1.0)  # 90-degree native bend is past the full-curl angle
        self.assertAlmostEqual(axis[2], -1.0)

    def test_dual_quaternion_blend(self):
        key = ('NPivot', 'NStomach')
        turn = pipeline._axis_rotation([0.0, 0.0, 1.0], 90.0)  # posed frame: source turned 90 degrees
        source = [[1.0, 0.0, 0.0], [0.0, 1.0, 0.0], [0.0, 0.0, 1.0]]
        frames = {key: ([10.0, 0.0, 0.0], turn)}
        rest = {'frames': {key: ([2.0, 0.0, 0.0], source)}}
        node = ET.fromstring('<S BonesCount="1" BoneStart1="NPivot" BoneEnd1="NStomach" Weight1="1" LocalX1="3" LocalY1="0" LocalZ1="0"/>')
        x, y = pipeline.dual_quaternion_point(node, frames, rest, {})
        self.assertAlmostEqual(x, 10.0); self.assertAlmostEqual(y, 3.0)  # O + R L, as linear skinning
        # Two halves of the same rigid transform blend to that transform.
        node = ET.fromstring('<S BonesCount="2" BoneStart1="NPivot" BoneEnd1="NStomach" Weight1="0.5" LocalX1="3" LocalY1="0" LocalZ1="0" '
                             'BoneStart2="NPivot" BoneEnd2="NStomach" Weight2="0.5" LocalX2="3" LocalY2="0" LocalZ2="0"/>')
        self.assertEqual([round(v, 6) for v in pipeline.dual_quaternion_point(node, frames, rest, {})], [10.0, 3.0])
        # Blending identity with a half turn keeps the length that linear blending would shrink.
        frames2 = {key: ([0.0, 0.0, 0.0], source), ('NStomach', 'NChest'): ([0.0, 0.0, 0.0], pipeline._axis_rotation([0.0, 0.0, 1.0], 120.0))}
        rest2 = {'frames': {key: ([0.0, 0.0, 0.0], source), ('NStomach', 'NChest'): ([0.0, 0.0, 0.0], source)}}
        node = ET.fromstring('<S BonesCount="2" BoneStart1="NPivot" BoneEnd1="NStomach" Weight1="0.5" LocalX1="10" LocalY1="0" LocalZ1="0" '
                             'BoneStart2="NStomach" BoneEnd2="NChest" Weight2="0.5" LocalX2="10" LocalY2="0" LocalZ2="0"/>')
        self.assertAlmostEqual(math.hypot(*pipeline.dual_quaternion_point(node, frames2, rest2, {})), 10.0, places=5)

    def test_hand_roll_and_rest_elements(self):
        p = {'NKnuckles_1': [0, 0, 0], 'NKnucklesS_1': [0, 5, 0]}
        identity = [[1.0, 0.0, 0.0], [0.0, 1.0, 0.0], [0.0, 0.0, 1.0]]
        roll = pipeline.hand_roll(p, identity, [1.0, 0.0, 0.0], '1')
        p['NKnucklesS_1'] = [0, 0, 5]  # side turned 90 degrees about the palm
        turned = (pipeline.hand_roll(p, identity, [1.0, 0.0, 0.0], '1') - roll + 180) % 360 - 180
        self.assertAlmostEqual(turned, 90.0, places=4)
        self.assertEqual(pipeline.hand_twist(p, identity, [1.0, 0.0, 0.0], '1', None), 0.0)
        scene = ET.fromstring('<Scene><Rest Version="1"><Dynamic Id="0" Parent="NChest-NNeck" Head="1,2,3" Dir="0,-1,0" Length="20"/>'
                              '<Frame Dynamic="0" O="1,2,3" A="1,0,0" B="0,1,0"/></Rest></Scene>')
        rest = {}
        for element in scene.find('Rest'):
            (pipeline._read_dynamic if element.tag == 'Dynamic' else pipeline._read_frame)(element, rest)
        self.assertEqual(rest['dynamic'][0]['parent'], ('NChest', 'NNeck'))
        self.assertIn(('dyn', 0), rest['frames'])
        bad = ET.fromstring('<Dynamic Id="0" Parent="#3" Head="0,0,0" Dir="0,1,0" Length="1"/>')
        with self.assertRaises(ValueError): pipeline._read_dynamic(bad, {})

    def test_expanded_edges_and_inherited_capsules(self):
        self.rig_file.write_text(ET.tostring(self.rig, encoding='unicode').replace('End2="B"', 'End2="B" Iterations="2"').replace('Edge="AB"', 'Edge="ABCI1"'))
        base = pipeline.model(self.rig_file)
        skin = self.path / 'skin.xml'
        skin.write_text('<Scene><Nodes/><Edges/><Figures><C Type="Capsule" Edge="ABCI1"/></Figures></Scene>')
        self.assertEqual(pipeline.model(skin, base).tag, 'Scene')
        skin.write_text('<Scene><Nodes/><Edges><ABCI1 Type="Edge" End1="A" End2="B"/></Edges><Figures/></Scene>')
        with self.assertRaisesRegex(ValueError, 'Duplicate expanded edge'): pipeline.model(skin, base)

    def test_runtime_geometry_contract(self):
        source = ET.tostring(self.rig, encoding='unicode')
        for old, new in [('NodesCount="2"', 'NodesCount="0"'),
                         ('LCC1="0.5"', ''),
                         ('Radius1="2"', 'Radius1="-1"'),
                         ('End2="B"', 'End2="B" Length="-1"'),
                         ('End2="B"', 'End2="B" Iterations="33"'),
                         ('End2="B"', 'End2="B" Iterations="0"'),
                         ('Type="MacroNode"', 'Type="CenterOfMass"'),
                         ('Mass="1"', 'Mass="0"')]:
            invalid = source.replace(old, new)
            # Positive-mass COM is valid; exercise its zero-total rejection.
            if old == 'Type="MacroNode"':
                invalid = invalid.replace('Mass="1"', 'Mass="0"')
            elif old == 'Mass="1"':
                invalid = invalid.replace('Type="MacroNode"', 'Type="CenterOfMass"')
            self.rig_file.write_text(invalid)
            with self.assertRaises(ValueError): pipeline.model(self.rig_file)


if __name__ == '__main__':
    unittest.main()
