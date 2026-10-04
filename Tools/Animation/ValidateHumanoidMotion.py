"""Independent source-direction, segment-length and root-jump acceptance."""
import json
from pathlib import Path
import sys
import math
import CharacterPipeline as pipeline

package = Path(sys.argv[1]); rig = pipeline.model(package/'assets/models/body.xml')
ref = {n.tag:[float(n.get(a,0)) for a in 'XYZ'] for n in rig.find('Nodes')}
def diff(a,b): return [x-y for x,y in zip(a,b)]
def length(v): return math.sqrt(sum(x*x for x in v))
for name in ('strike','jump'):
    clip = pipeline.read_animation(package/f'assets/animations/{name}.bytes',rig)
    report = json.loads((package/f'assets/animations/{name}.retarget.json').read_text(encoding='utf-8'))
    assert len(clip['frames']) == 61 and report['source_fps'] == 30
    for frame,observation in zip(clip['frames'],report['observations']):
        p = dict(zip(clip['names'],frame)); s = observation['points']
        for side,suffix in (('left','1'),('right','2')):
            for child,parent,source_child,source_parent in (
                ('NElbow_','NShoulder_','forearm','upper_arm'),
                ('NWrist_','NElbow_','hand','forearm'),
                ('NKnee_','NHip_','shin','thigh'),
                ('NAnkle_','NKnee_','foot','shin')):
                target = diff(p[child+suffix],p[parent+suffix])
                source = diff(s[side+'_'+source_child],s[side+'_'+source_parent])
                assert abs(length(target)-length(diff(ref[child+suffix],ref[parent+suffix]))) < .0002
                assert sum(a*b for a,b in zip(target,source))/(length(target)*length(source)) > .99999
    points = [dict(zip(clip['names'],f)) for f in clip['frames']]
    wrist = [p['NWrist_1'][0] for p in points]
    assert max(wrist)-min(wrist)>5, 'Evaluated arm motion was lost'
    if name == 'jump':
        assert points[30]['NPivot'][1]-points[0]['NPivot'][1]>20, 'Jump root motion was flattened'
        assert abs(points[-1]['NPivot'][1]-points[0]['NPivot'][1])<.0002
print('PASS: two 61-sample evaluated actions; source joint directions, core lengths, arm motion and retained jump root height')
