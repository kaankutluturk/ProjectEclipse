$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$fixture = Join-Path $root ('Temp/PlaybackTiming-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture -Force | Out-Null
$sources = @('Assets/Scripts/Eclipse/Runtime/PlaybackTiming.cs', 'Tools/Tests/Combat/PlaybackTimingTests.cs') | ForEach-Object {
    '<Compile Include="' + [Security.SecurityElement]::Escape((Join-Path $root $_)) + '" />'
}
@"
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
 <ItemGroup>$sources</ItemGroup>
</Project>
"@ | Set-Content -Encoding UTF8 -LiteralPath (Join-Path $fixture 'Fixture.csproj')
dotnet run --project (Join-Path $fixture 'Fixture.csproj')
if ($LASTEXITCODE -ne 0) { throw 'Playback timing regression failed.' }
