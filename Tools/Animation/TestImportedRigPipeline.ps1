param([Parameter(Mandatory=$true)][string]$Blender,[Parameter(Mandatory=$true)][string]$Rig)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$rigPath=(Resolve-Path -LiteralPath $Rig).Path
$fixture=Join-Path $root ('Temp/ImportedRig-'+[Guid]::NewGuid().ToString('N'))
Write-Host "Imported-rig fixture: $fixture"
& $Blender --background --factory-startup --python-exit-code 1 --python (Join-Path $PSScriptRoot 'CreateImportedRigFixture.py') -- --output $fixture
if($LASTEXITCODE -ne 0){throw 'Original source rig creation failed.'}
foreach($kind in @('blend','glb','fbx','reduced')){
    $extension=if($kind -eq 'reduced'){'glb'}else{$kind}
    $arguments=@('--background','--factory-startup','--python-exit-code','1','--python',(Join-Path $PSScriptRoot 'ImportCharacter.py'),'--','--source',(Join-Path $fixture "foreign.$extension"),'--rig',$rigPath,'--mod-id',"local.import-$kind",'--output',(Join-Path $fixture "local.import-$kind"))
    if($kind -eq 'reduced'){$arguments+=@('--max-vertices','100')}
    & $Blender @arguments
    if($LASTEXITCODE -ne 0){throw "Actual $kind source import failed."}
}
python (Join-Path $PSScriptRoot 'ValidateImportedRig.py') $fixture --rig $rigPath
if($LASTEXITCODE -ne 0){throw 'Imported native geometry contracts failed.'}
& (Join-Path $PSScriptRoot 'TestCharacterLua.ps1') -Package (Join-Path $fixture 'local.import-blend') -Packaged -Playable -ImportedRig
Write-Host 'PASS: original weighted humanoid Blender/FBX/glTF imports, reduction, offline geometry and public Lua. Run the separate full-game acceptance for combat/rendering.'
