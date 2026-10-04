$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$fixture=Join-Path $root ('Temp/CameraNativePolicies-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CameraNativePolicyTests.cs') -Destination $fixture
$sources=@('Assets/Scripts/Eclipse/Runtime/Modding/ModId.cs','Assets/Scripts/Eclipse/Runtime/Modding/ModCameraRuntime.cs','Assets/Scripts/Eclipse/Modding/FightCameraControl.cs')
$includes=($sources | ForEach-Object {'<Compile Include="'+[Security.SecurityElement]::Escape((Join-Path $root $_))+'" />'}) -join "`n"
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup><ItemGroup>'+$includes+'</ItemGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Camera native policy checks failed.' }
