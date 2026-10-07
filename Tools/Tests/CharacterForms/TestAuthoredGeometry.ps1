$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$fixture = Join-Path $root ('Temp/AuthoredGeometry-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'Assets/Scripts/Eclipse/Modding/ModCharacterGeometry.cs') -Destination $fixture
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'AuthoredGeometryTests.cs') -Destination $fixture
$loader = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/ModelLoader.cs')
$load = [regex]::Match($loader, '(?ms)^\tpublic static void Load\(.*?^\t\}').Value
$helpers = [regex]::Match($loader, '(?m)^    private static string AuthoredModelPath.*\r?\n    private static bool IsAuthoredModel[^\r\n]+').Value
if (!$load -or !$helpers) { throw 'Production model loading extraction failed.' }
$stub = @'
using System;
using System.Collections.Generic;
using System.Xml;
class ModelParameters { public string EclipseBodyModel; public string[] EclipseSkinModels = Array.Empty<string>(); }
class Model { public ModelParameters Parameters = new ModelParameters(); }
class ModelObject {
 public Model Model = new Model(); public Model GetModel() => Model;
 public void FindPivotNode() {} public void CalculateTotalWeight() {} public List<object> GetAllNodes() => new List<object>();
 public void SetFileNames(List<string> paths) {} public void BuildPairNodes() {} public void ResolveMacroNodeWeights() {}
}
static class SF2Paths { public static string GetModelsPath() => "fixture"; }
static class GameLog { public static void Error(string format, params object[] args) {} }
static class ModelLoader {
 public static int Parsed; public static readonly Cache DocumentCache = new Cache();
 public sealed class Cache { public readonly Dictionary<string,XmlDocument> Documents = new Dictionary<string,XmlDocument>(); public XmlDocument GetDocument(string root,string path) => Documents[path]; }
 static void Parse(ModelObject model, XmlDocument document) { Parsed++; }
 static void PostProcessNodes(List<object> nodes) {}
 LOAD
 HELPERS
}
'@
[IO.File]::WriteAllText((Join-Path $fixture 'ModelLoading.cs'), $stub.Replace('LOAD', $load).Replace('HELPERS', $helpers))
'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>' | Set-Content -LiteralPath (Join-Path $fixture 'AuthoredGeometry.csproj')
dotnet run --project (Join-Path $fixture 'AuthoredGeometry.csproj') -- $root
if ($LASTEXITCODE -ne 0) { throw 'Authored character geometry acceptance failed.' }
