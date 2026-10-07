$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$camera=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/Camera.cs')
$viewer=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/ViewerModel.cs')
$a=[regex]::Match($camera,'(?ms)^    internal bool ReplaceModel\(.*?^    \}').Value
$b=[regex]::Match($viewer,'(?ms)^    internal bool ReplaceModel\(.*?^    \}').Value
if(!$a -or !$b){throw 'Camera/viewer replacement extraction failed.'}
$fixture=Join-Path $root ('Temp/CameraReplacement-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$code=@'
using System;
using System.Collections.Generic;
struct Color{}
class Transform{public Transform Parent;public bool Fail;public void SetParent(Transform p,bool world){if(Fail)throw new InvalidOperationException("parent");Parent=p;}}
class GameObject{public Transform transform=new Transform();}
class ModelNode{}
class ModelObject{public Model Owner;public bool Missing;public ModelNode Focus=new ModelNode(),Other=new ModelNode();public Model get_Model()=>Owner;public ModelNode GetNodeByName(string s)=>Missing?null:s=="focus"?Focus:Other;}
class Model{public int Index;public GameObject Object=new GameObject();public ModelObject Body;public Model(){Body=new ModelObject{Owner=this};}public ModelObject GetBodyObject()=>Body;public GameObject GetGameObject()=>Object;public void set_color(Color c){}}
class Settings{public string BindingNode="focus",CameraNode="other";}
static class GameUtils{public static Settings GetCameraSettings()=>new Settings();}
class Location{public Color modelsColor;}
class ViewerModel{
 public List<ModelObject> models=new List<ModelObject>();public ModelObject firstFighter,secondFighter;public GameObject _UnityObject=new GameObject();
 VIEWER
}
class Render{public ViewerModel Viewer=new ViewerModel();public HashSet<Model> Attached=new HashSet<Model>();public void AttachModelEffects(Model m){Attached.Add(m);}public void DetachModelEffects(Model m){Attached.Remove(m);}public ViewerModel GetViewerModel()=>Viewer;}
class Camera{
 List<Model> _models=new List<Model>();Render _render=new Render();Location _location=new Location();ModelNode _focusNode,_secondaryFocusNode;
 CAMERA
 static void Check(bool x,string why){if(!x)throw new Exception(why);}
 public static void Main(){
  var c=new Camera();var left=new Model{Index=7};var right=new Model{Index=9};var next=new Model();
  var v=c._render.Viewer;c._models.AddRange(new[]{left,right});v.models.AddRange(new[]{left.Body,right.Body});v.firstFighter=left.Body;v.secondFighter=right.Body;c._render.Attached.UnionWith(new[]{left,right});
  Check(c.ReplaceModel(left,next,true),"player replacement");
  Check(c._models[0]==next&&c._models[1]==right&&v.models[0]==next.Body&&v.models[1]==right.Body,"stable slots");
  Check(v.firstFighter==next.Body&&v.secondFighter==right.Body,"primary pointers");
  Check(next.Index==7&&right.Index==9&&next.Object.transform.Parent==v._UnityObject.transform,"index/parenting");
  Check(c._focusNode==next.Body.Focus&&c._secondaryFocusNode==next.Body.Other,"focus transfer");
  Check(c._render.Attached.SetEquals(new[]{next,right}),"listener transfer");
  Check(!c.ReplaceModel(left,new Model(),true)&&!c.ReplaceModel(next,right,true),"stale/duplicate rejection");
  var bad=new Model();bad.Body.Missing=true;Check(!c.ReplaceModel(next,bad,true)&&!c._render.Attached.Contains(bad),"missing focus leaves current model");
  bad=new Model();v.models[0]=left.Body;Check(!c.ReplaceModel(next,bad,true),"viewer mismatch");Check(!c._render.Attached.Contains(bad)&&c._models[0]==next,"mismatch rollback");v.models[0]=next.Body;
  bad=new Model();bad.Object.transform.Fail=true;bool threw=false;try{c.ReplaceModel(next,bad,true);}catch(InvalidOperationException){threw=true;}
  Check(threw&&!c._render.Attached.Contains(bad)&&v.models[0]==next.Body,"parenting failure preserves slot/listeners");
  var final=new Model();Check(c.ReplaceModel(right,final,false),"opponent replacement");Check(v.secondFighter==final.Body&&v.firstFighter==next.Body&&c._focusNode==next.Body.Focus,"opponent retains player focus");
  Console.WriteLine("PASS: production Camera/Viewer replacement; stable slots, primary references, focus, listener transfer, stale/duplicate/missing-focus rejection and parenting/mismatch failure. Unity and renderer services controlled.");
 }
}
'@
$code=$code.Replace('VIEWER',$b).Replace('CAMERA',$a)
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code)
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><NoWarn>CS0649</NoWarn></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if($LASTEXITCODE -ne 0){throw 'Camera replacement checks failed.'}
