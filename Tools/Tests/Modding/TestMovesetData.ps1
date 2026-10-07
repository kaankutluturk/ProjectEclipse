$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$fixture = Join-Path $root ('Temp/MovesetData-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture -Force | Out-Null
$sources = @(Get-ChildItem -LiteralPath (Join-Path $root 'Assets/Scripts/Eclipse/Runtime/Modding') -Filter '*.cs' -File | Select-Object -ExpandProperty FullName)
$sources += @((Join-Path $root 'Assets/Scripts/Eclipse/Runtime/PlaybackTiming.cs'), (Join-Path $PSScriptRoot 'MovesetDataTests.cs'))
$includes = $sources | ForEach-Object { '<Compile Include="' + [Security.SecurityElement]::Escape($_) + '" />' }
@"
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
 <ItemGroup>$($includes -join "`n")</ItemGroup>
</Project>
"@ | Set-Content -Encoding UTF8 -LiteralPath (Join-Path $fixture 'Fixture.csproj')
dotnet run --project (Join-Path $fixture 'Fixture.csproj')
if ($LASTEXITCODE -ne 0) { throw 'Moveset data regression failed.' }
