param([string]$Unity = 'F:\UnityInstalls\6000.6.0f1\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$fixture = Join-Path $root 'Temp/OverworldMapsNativeSmoke'
if (Test-Path -LiteralPath $fixture) {
    $resolvedFixture = (Resolve-Path -LiteralPath $fixture).Path
    $resolvedTemp = (Resolve-Path -LiteralPath (Join-Path $root 'Temp')).Path
    if (!$resolvedFixture.StartsWith($resolvedTemp + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove fixture outside Temp: $resolvedFixture"
    }
    Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
}
foreach ($directory in @('Assets/Resources/SF2Content/Art', 'Assets/StreamingAssets/SF2Content/ArtBundles', 'Assets/TarAssets', 'Packages', 'ProjectSettings')) {
    New-Item -ItemType Directory -Path (Join-Path $fixture $directory) -Force | Out-Null
}
[IO.File]::WriteAllText((Join-Path $fixture 'Packages/manifest.json'), '{"dependencies":{"com.unity.modules.audio":"1.0.0","com.unity.modules.imageconversion":"1.0.0","com.unity.modules.jsonserialize":"1.0.0","com.unity.modules.unitywebrequest":"1.0.0"}}')
[IO.File]::WriteAllText((Join-Path $fixture 'ProjectSettings/ProjectVersion.txt'), (Get-Content -LiteralPath (Join-Path $root 'ProjectSettings/ProjectVersion.txt') -Raw))
Copy-Item -LiteralPath (Join-Path $root 'Assets/Scripts/Eclipse/Content/PackagedArtCatalog.cs') -Destination (Join-Path $fixture 'Assets')
Copy-Item -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/ResourcesAndBundles.cs') -Destination (Join-Path $fixture 'Assets')
Copy-Item -Path (Join-Path $root 'Assets/Scripts/Eclipse/Content/TarAssets/*.cs') -Destination (Join-Path $fixture 'Assets/TarAssets')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ValidateOverworldMaps.cs') -Destination (Join-Path $fixture 'Assets')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot '../Shared/NormalEditorPlayModeContext.cs') -Destination (Join-Path $fixture 'Assets')
$catalog = Get-Content -LiteralPath (Join-Path $root 'Assets/Resources/SF2Content/Art/catalog.json') -Raw | ConvertFrom-Json
$catalog.bundles = @($catalog.bundles | Where-Object name -In @('ZONE_1', 'ZONE_6', 'ZONES'))
[IO.File]::WriteAllText((Join-Path $fixture 'Assets/Resources/SF2Content/Art/catalog.json'), ($catalog | ConvertTo-Json -Depth 20))
foreach ($bundle in $catalog.bundles) {
    Copy-Item -LiteralPath (Join-Path $root ('Assets/StreamingAssets/SF2Content/ArtBundles/' + $bundle.file)) -Destination (Join-Path $fixture 'Assets/StreamingAssets/SF2Content/ArtBundles')
}
# The focused fixture has no active mods. Stub only the mod routing boundary;
# use the real ResourcesAndBundles, catalog, TAR decoder and native Sprite.Create.
[IO.File]::WriteAllText((Join-Path $fixture 'Assets/NoActiveMods.cs'), @'
using UnityEngine;
namespace Eclipse.Modding {
    public static class ModRuntime {
        public static HostStub Host = new HostStub();
        public static bool TryResolveCoreReplacement(string path, out string replacement) { replacement = null; return false; }
        public static bool TryLoadCoreSpriteReplacement(string path, string name, out Sprite sprite) { sprite = null; return false; }
        public static bool TryLoadQualified<T>(string path, out T asset) where T : Object { asset = null; return false; }
        public static bool TryLoadCore<T>(string path, out T asset) where T : Object { asset = Eclipse.Content.PackagedArtCatalog.Load<T>(path); return asset != null; }
        public static bool TryLoadQualifiedWithSubAssets<T>(string path, out T[] assets) where T : Object { assets = null; return false; }
        public static bool TryLoadCoreWithSubAssets<T>(string path, out T[] assets) where T : Object { assets = Eclipse.Content.PackagedArtCatalog.LoadWithSubAssets<T>(path); return assets != null; }
    }
    public class HostStub { public TypedAssetsStub TypedAssets = new TypedAssetsStub(); }
    public class TypedAssetsStub {
        public T LoadUnityAsset<T>(string path) where T : Object { return null; }
        public T[] LoadUnityAssets<T>(string path) where T : Object { return null; }
        public string LoadModelText(string path) { return null; }
    }
}
'@)
$log = Join-Path $root ('Temp/overworld-maps-' + [Guid]::NewGuid().ToString('N') + '.log')
$process = Start-Process -FilePath $Unity -WindowStyle Hidden -Wait -PassThru -ArgumentList @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $fixture + '"'), '-executeMethod', 'ValidateOverworldMaps.Run', '-logFile', ('"' + $log + '"'))
$passed = Select-String -LiteralPath $log -Pattern '\[OverworldMaps\] PASS'
if ($process.ExitCode -ne 0 -or !$passed) {
    Get-Content -LiteralPath $log -Tail 45
    throw "Native overworld validation failed: $log"
}
$passed.Line
