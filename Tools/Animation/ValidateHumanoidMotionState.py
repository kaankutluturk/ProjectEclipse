"""Blender integration: isolate partial actions and restore authoring state."""
import argparse
from pathlib import Path
import sys
import bpy
sys.path.insert(0,str(Path(__file__).resolve().parent))
import ImportCharacter
import HumanoidMotion
import CharacterPipeline as pipeline
from RigMapping import match_bones

parser = argparse.ArgumentParser()
parser.add_argument('--source',type=Path,required=True)
parser.add_argument('--rig',type=Path,required=True)
args = parser.parse_args(sys.argv[sys.argv.index('--')+1:])
bpy.ops.wm.open_mainfile(filepath=str(args.source.resolve()))
armature = bpy.data.objects['ForeignRig']; scene = bpy.context.scene
mapping = match_bones([b.name for b in armature.data.bones])
scene.frame_set(9,subframe=.25)
bone = armature.pose.bones['mixamorig:RightArm']; bone.rotation_euler.z = .9
before = bone.matrix_basis.copy(); action = armature.animation_data.action
clip,_ = HumanoidMotion.sample(pipeline.model(args.rig),armature,mapping,bpy.data.actions['OriginalStrike'])
assert armature.animation_data.action == action and scene.frame_current == 9 and abs(scene.frame_subframe-.25)<1e-6
assert max(abs(before[i][j]-bone.matrix_basis[i][j]) for i in range(4) for j in range(4))<1e-6
pose = dict(zip(clip['names'],clip['frames'][0]))
direction = [b-a for a,b in zip(pose['NShoulder_2'],pose['NElbow_2'])]
assert abs(direction[0])<.0001 and abs(direction[1])<.0001 and direction[2]<-1, 'Unkeyed old pose contaminated action'
try: HumanoidMotion.sample(pipeline.model(args.rig),armature,mapping,bpy.data.actions.new('Empty'))
except ValueError: pass
else: raise AssertionError('Empty action accepted')
print('PASS: partial-action isolation, previous action/frame/subframe/pose restoration and empty-action rejection')
