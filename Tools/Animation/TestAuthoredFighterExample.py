"""Reproduce the authored fighter assets and check point/skin contracts."""
import hashlib
import json
from pathlib import Path
import tempfile
import unittest

import BuildAuthoredFighterExample as example
import CharacterPipeline as pipeline


class AuthoredExampleTests(unittest.TestCase):
    def setUp(self):
        self.mod = Path(__file__).resolve().parents[2] / 'Mods/example.authored-fighter'
        self.body = self.mod / 'assets/models/body.xml'
        self.rig = pipeline.model(self.body)

    def test_reproduces_shipped_assets_without_overwriting(self):
        with tempfile.TemporaryDirectory() as temporary:
            output = example.build(self.body, Path(temporary)/'generated')
            for name, folder in [('body.xml','models'),('sash.xml','models'),('strike.bytes','animations'),('strike.rig.json','animations')]:
                self.assertEqual((output/name).read_bytes(), (self.mod/'assets'/folder/name).read_bytes())
            with self.assertRaisesRegex(ValueError, 'fresh output'): example.build(self.body, output)
            metadata = json.loads((output/'strike.rig.json').read_text())
            self.assertEqual(metadata['frames'], 61)
            self.assertEqual(metadata['animation_sha256'], hashlib.sha256((output/'strike.bytes').read_bytes()).hexdigest())

    def test_native_point_order_motion_and_lengths(self):
        source = example.motion(self.rig)
        self.assertEqual(len(source['names']), 67)
        self.assertEqual(source['names'], [n.tag for n in self.rig.find('Nodes')])
        self.assertEqual(source['frames'][0], source['frames'][-1])
        self.assertNotEqual(source['frames'][0], source['frames'][22])
        index = {n:i for i,n in enumerate(source['names'])}
        for frame in source['frames']:
            for a,b in [('NShoulder_1','NElbow_1'),('NElbow_1','NWrist_1')]:
                expected = example.length(example.sub(source['frames'][0][index[a]], source['frames'][0][index[b]]))
                self.assertAlmostEqual(example.length(example.sub(frame[index[a]],frame[index[b]])), expected, places=7)
        raw = pipeline.read_animation(self.mod/'assets/animations/strike.bytes', self.rig)
        self.assertEqual(raw['names'], source['names'])
        self.assertEqual(len(raw['frames']), 61)

    def test_sash_is_connected_and_deforms_with_authored_pose(self):
        skin = pipeline.model(self.mod/'assets/models/sash.xml', self.rig)
        self.assertEqual(len(skin.find('Nodes')), 6)
        faces = list(skin.find('Figures')); self.assertEqual(len(faces), 4)
        edges = {}
        for face in faces:
            nodes = [face.get('Node'+str(i)) for i in (1,2,3)]
            for a,b in zip(nodes,nodes[1:]+nodes[:1]):
                key = tuple(sorted((a,b))); edges[key] = edges.get(key,0)+1
        self.assertEqual(sum(n==2 for n in edges.values()), 3)
        source = example.motion(self.rig)
        combined = example.ET.fromstring(example.ET.tostring(self.rig))
        combined.find('Nodes').extend(list(skin.find('Nodes')))
        poses = []
        for frame in (source['frames'][0],source['frames'][22]):
            points = dict(zip(source['names'],frame))
            pipeline.helper_positions(combined,points)
            poses.append(points)
        self.assertNotEqual(poses[0]['AuthoredSashV0'],poses[1]['AuthoredSashV0'])
        # A body-local triangle moves differently at the shoulder and sash hem.
        delta_top = example.sub(poses[1]['AuthoredSashV0'],poses[0]['AuthoredSashV0'])
        delta_bottom = example.sub(poses[1]['AuthoredSashV4'],poses[0]['AuthoredSashV4'])
        self.assertGreater(example.length(example.sub(delta_top,delta_bottom)),1)


if __name__ == '__main__': unittest.main()
