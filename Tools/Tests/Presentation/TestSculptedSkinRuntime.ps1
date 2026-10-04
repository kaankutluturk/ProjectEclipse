$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$fixture=Join-Path $root ('Temp/SculptedSkin-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'SculptedSkinTests.cs') -Destination $fixture
Copy-Item -LiteralPath (Join-Path $root 'Assets/Scripts/Eclipse/Rendering/SculptedFighterSkin.cs') -Destination $fixture
$version=(Select-String -LiteralPath (Join-Path $root 'ProjectSettings/ProjectVersion.txt') -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
$unity=[Security.SecurityElement]::Escape("F:/UnityInstalls/$version/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll")
@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup><ItemGroup>
<Reference Include="UnityEngine.CoreModule"><HintPath>$unity</HintPath></Reference>
</ItemGroup></Project>
"@ | Set-Content -LiteralPath (Join-Path $fixture 'SculptedSkin.csproj')
dotnet run --project (Join-Path $fixture 'SculptedSkin.csproj')
if($LASTEXITCODE -ne 0){throw 'Sculpted skin runtime checks failed.'}
