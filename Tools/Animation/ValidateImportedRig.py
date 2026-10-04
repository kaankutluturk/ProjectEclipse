"""Check original imported-rig integration outputs, not a universal rig claim."""
import argparse
import json
from pathlib import Path
import xml.etree.ElementTree as ET
import CharacterPipeline as pipeline


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('fixture', type=Path)
    parser.add_argument('--rig', type=Path, required=True)
    args = parser.parse_args()
    original = pipeline.model(args.rig)
    for kind in ('blend', 'glb', 'fbx', 'reduced'):
        package = args.fixture / ('local.import-' + kind)
        body = pipeline.model(package / 'assets/models/body.xml')
        skin = pipeline.model(package / 'assets/models/skin1.xml', body)
        assert ET.tostring(body.find('Nodes')) == ET.tostring(original.find('Nodes')), 'Core point identity/order changed'
        assert ET.tostring(body.find('Edges')) == ET.tostring(original.find('Edges')), 'Core collision/physics edges changed'
        assert not list(body.find('Figures')), 'Original visible body should be replaced'
        report = json.loads((package / 'import.json').read_text(encoding='utf-8'))
        assert len(report['mapping']) == 17 and len(report['inherited_bones']) == 2
        count = sum(row['vertices'] for row in report['meshes'])
        assert count == len(skin.find('Nodes')) and count <= report['max_vertices']
        assert kind == 'reduced' or any(n.get('BonesCount') == '2' for n in skin.find('Nodes')), 'Mixed source weights lost'
        positions = {n.tag: [float(n.get(axis, 0)) for axis in 'XYZ'] for n in body.find('Nodes')}
        combined = ET.fromstring(ET.tostring(body)); combined.find('Nodes').extend(list(skin.find('Nodes')))
        pipeline.helper_positions(combined, positions)
        assert all(len(p) == 3 for p in positions.values())
        assert all(p[2] == 0 for name, p in positions.items() if name.startswith('Imported'))
        assert not (package / 'assets/animations').exists(), 'Default combat needs no generated source clips'
        print(f'PASS: {kind}: {count} vertices; automatic mapping, mixed/inherited weights, unchanged core gameplay rig, planar bake and native default package')


if __name__ == '__main__':
    main()
