"""Author original strike/jump actions on the weighted import integration rig."""
import argparse
from pathlib import Path
import sys
import bpy

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--source',type=Path,required=True)
parser.add_argument('--output',type=Path,required=True)
args = parser.parse_args(sys.argv[sys.argv.index('--')+1:])
if args.output.exists(): raise ValueError('Output must be fresh')
bpy.ops.wm.open_mainfile(filepath=str(args.source.resolve()))
armature = bpy.data.objects['ForeignRig']
scene = bpy.context.scene; scene.render.fps = 30
animation = armature.animation_data_create()
for name,jump in (('OriginalStrike',False),('OriginalJump',True)):
    action = bpy.data.actions.new(name); action.use_fake_user = True
    animation.action = action
    for frame,value in ((1,0),(16,1),(31,0)):
        for bone in armature.pose.bones:
            bone.rotation_mode = 'XYZ'; bone.rotation_euler = (0,0,0); bone.location = (0,0,0)
        # Blender bone-local Y follows this fixture's vertical pelvis bone.
        armature.pose.bones['mixamorig:Hips'].location = (0,value*.22 if jump else 0,0)
        # Both hand motion and independent source finger animation are authored;
        # the retargeter intentionally follows only the mapped hand region.
        armature.pose.bones['mixamorig:LeftArm'].rotation_euler.z = value*.7
        armature.pose.bones['mixamorig:LeftForeArm'].rotation_euler.x = value*.4
        armature.pose.bones['mixamorig:LeftFingerExtra'].rotation_euler.x = value*.6
        for name in ('Hips','LeftArm','LeftForeArm','LeftFingerExtra'):
            bone = armature.pose.bones['mixamorig:'+name]
            bone.keyframe_insert(data_path='rotation_euler',frame=frame)
            bone.keyframe_insert(data_path='location',frame=frame)
    for curve in action.fcurves:
        for key in curve.keyframe_points: key.interpolation = 'LINEAR'
animation.action = bpy.data.actions['OriginalStrike']; scene.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=str(args.output.resolve()))
print('HUMANOID ACTION FIXTURE: '+str(args.output))
