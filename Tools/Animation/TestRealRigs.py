"""Re-import a local folder of real rigged models and check each result.

python Tools/Animation/TestRealRigs.py --blender "C:/Program Files/Blender Foundation/Blender 3.6/blender.exe" \
    --rig Temp/CharacterCore/models/mdl_skeleton.xml --models D:/MyRigs --output Temp/RealRigReport

Third-party models stay where they are and are never copied into the repository;
the output folder (keep it under the ignored Temp/) receives one package and
preview.png per model plus report.json. A model passes when it imports, keeps at
least one mesh, stays inside its budget, and every skin vertex in the idle
stance, a kick and a handspring lies within 1.5 statures of the pelvis. The
previews still need a human look; numbers cannot judge a silhouette.
"""
import argparse
import json
import math
from pathlib import Path
import re
import subprocess
import sys
import xml.etree.ElementTree as ET

sys.path.insert(0, str(Path(__file__).resolve().parent))
import CharacterPipeline as pipeline

ANIMATIONS = Path(__file__).resolve().parents[2] / 'Assets/Resources/gamedata/animations/binary'
FRAMES = (('stance_idle.bytes', 3), ('front_kick.bytes', 14), ('back_handflip.bytes', 8))
EXTENSIONS = ('.glb', '.gltf', '.fbx', '.blend')


def check(package):
    body = pipeline.model(package / 'assets/models/body.xml')
    skin = pipeline.model(package / 'assets/models/skin1.xml', body)
    combined = ET.fromstring(ET.tostring(body)); combined.find('Nodes').extend(list(skin.find('Nodes')))
    worst = 0.0
    # Rest stature: a tucked mid-air pose would shrink a per-frame head-to-ankle measure.
    rest = {n.tag: [float(n.get(axis, 0)) for axis in 'XYZ'] for n in body.find('Nodes')}
    stature = math.dist(rest['NTop'], rest['NAnkle_1'])
    for name, index in FRAMES:
        clip = pipeline.read_animation(ANIMATIONS / name, body)
        frame = dict(zip(clip['names'], clip['frames'][index]))
        positions = {k: list(v) for k, v in frame.items()}
        pipeline.helper_positions(combined, positions, pipeline.read_proportions(skin), pipeline.read_rest(skin))
        pivot = frame['NPivot']
        for node in skin.find('Nodes'):
            worst = max(worst, math.dist(positions[node.tag][:2], pivot[:2]) / stature)
    return worst


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('--blender', required=True)
    parser.add_argument('--rig', type=Path, required=True)
    parser.add_argument('--models', type=Path, required=True, help='Folder searched recursively for .glb/.gltf/.fbx/.blend')
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--max-vertices', type=int, default=3000)
    args = parser.parse_args()
    importer = Path(__file__).resolve().parent / 'ImportCharacter.py'
    args.output.mkdir(parents=True, exist_ok=True)
    results = []
    for source in sorted(p for p in args.models.rglob('*') if p.suffix.lower() in EXTENSIONS):
        mod_id = 'local.rig-' + re.sub(r'[^a-z0-9]+', '-', source.stem.lower()).strip('-')[:60]
        package = args.output / mod_id
        if package.exists():
            import shutil
            shutil.rmtree(package)
        command = [args.blender, '--background', '--factory-startup', '--python-exit-code', '1', '--python', str(importer), '--',
                   '--source', str(source), '--rig', str(args.rig), '--mod-id', mod_id, '--output', str(package),
                   '--title', source.stem[:80], '--max-vertices', str(args.max_vertices)]
        for name, index in FRAMES:
            command += ['--preview-animation', str(ANIMATIONS / name) + ':' + str(index)]
        run = subprocess.run(command, capture_output=True, text=True, errors='replace')
        row = {'source': str(source), 'package': str(package)}
        if run.returncode != 0:
            errors = [l for l in (run.stdout + run.stderr).splitlines() if re.match(r'^(?!Error: )[\w.]*?\w*(Error|Exceeded|Exception): ', l)]
            row.update(ok=False, error=errors[-1] if errors else 'Blender exited with ' + str(run.returncode))
        else:
            report = json.loads((package / 'import.json').read_text(encoding='utf-8'))
            vertices = sum(m.get('vertices', 0) for m in report['meshes'])
            reach = check(package)
            row.update(ok=vertices > 0 and vertices <= args.max_vertices and reach < 1.5, vertices=vertices, reach=round(reach, 3),
                       method=report['mapping_method'], notes=report.get('mapping_notes', []),
                       skipped=[m['name'] for m in report['meshes'] if m.get('skipped')], preview=str(package / 'preview.png'))
        results.append(row)
        print(('PASS ' if row['ok'] else 'FAIL ') + source.name + ': ' +
              (row.get('error') or f"{row['vertices']} vertices, reach {row['reach']}, {row['method']}, skipped {len(row['skipped'])}"))
    (args.output / 'report.json').write_text(json.dumps(results, indent=2), encoding='utf-8')
    failed = [r for r in results if not r['ok']]
    print(f'{len(results) - len(failed)}/{len(results)} models passed; previews in {args.output}')
    sys.exit(1 if failed or not results else 0)


if __name__ == '__main__':
    main()
