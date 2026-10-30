global using ECommons;
global using ECommons.DalamudServices;
global using ECommons.UIHelpers.AddonMasterImplementations;
global using ECommons.ExcelServices;
global using static Lifestream.Lifestream;
global using static ECommons.GenericHelpers;
using System.Collections;
using Lumina.Excel.Sheets;
namespace Lumina.Excel.Sheets {
 public interface IRow{uint RowId{get;}}
 public readonly record struct RowRef(uint RowId);
 public readonly record struct World(uint RowId,string Name,bool IsPublic,RowRef DataCenter):IRow;
 public readonly record struct WorldDCGroupType(uint RowId,byte Region):IRow;
}
namespace ECommons.DalamudServices {
 public sealed class Sheet<T>:List<T> where T:struct,IRow {public T? GetRowOrDefault(uint id)=>this.Where(x=>x.RowId==id).Select(x=>(T?)x).FirstOrDefault();}
 public static class Svc {public static readonly DataService Data=new();}
 public sealed class DataService {public int Language=7;public Sheet<World> Worlds=new();public Sheet<WorldDCGroupType> Centers=new();public Sheet<T> GetExcelSheet<T>()where T:struct,IRow=>(Sheet<T>)(object)(typeof(T)==typeof(World)?(object)Worlds:Centers);}
}
namespace ECommons.ExcelServices {public static class ExcelWorldHelper{public static World? Get(string name)=>Svc.Data.Worlds.Where(w=>w.Name==name).Select(w=>(World?)w).FirstOrDefault();public static string GetName(uint id)=>Svc.Data.Worlds.GetRowOrDefault(id)?.Name!;}}
namespace ECommons.GameHelpers{public static class Player {public static bool Available=true;}}
namespace ECommons.Throttlers{public static class EzThrottler{public static bool Throttle(string key,int ms=0)=>true;}}
namespace FFXIVClientStructs.FFXIV.Component.GUI{public struct AtkUnitBase{public int Dummy;}}
namespace ECommons {
 public static unsafe class GenericHelpers{
  public static int Minutes(this int value)=>value*60*1000;
  public static T? FirstOrNull<T>(this IEnumerable<T> values,Func<T,bool> predicate)where T:struct=>values.Where(predicate).Select(x=>(T?)x).FirstOrDefault();
  public static bool TryGetAddonByName<T>(string name,out T* ptr)where T:unmanaged {ptr=null;return name=="SelectYesno"&&Lifestream.Fixture.Prompt;}
  public static bool IsAddonReady(FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase* ptr)=>true;
  public static bool TryGetAddonMaster<T>(out T value)where T:class,new(){value=new T();return true;}
 }
}
namespace ECommons.UIHelpers.AddonMasterImplementations {
 public static class AddonMaster{
  public sealed class _CharaSelectListMenu{public bool IsAddonReady=>true;public bool TemporarilyLocked=>false;public Chara[] Characters=>Lifestream.Fixture.ActiveWorld==4035?[new("合成角色",61),new("合成角色",4035)]:[new("合成角色",61)];}
  public sealed class _CharaSelectWorldServer{public WorldChoice[] Worlds=>[new("其他世界",61),new(Lifestream.Fixture.LobbyName,4035)];}
 }
 public sealed record Chara(string Name,uint HomeWorld){public bool IsVisitingAnotherDC=>false;public uint CurrentWorld=>HomeWorld;public uint Index=>HomeWorld;public void Login(){Lifestream.Fixture.Selected=HomeWorld;Lifestream.Fixture.Prompt=true;Lifestream.Fixture.Trace.Add("character-selected");}public void OpenContextMenu()=>throw new Exception("not used");}
 public sealed record WorldChoice(string Name,uint Id){public void Select(){Lifestream.Fixture.ActiveWorld=Id;Lifestream.Fixture.Trace.Add("lobby-world-selected");}}
}
namespace Lifestream {
 public static class Fixture {public static readonly List<string> Trace=[];public static uint ActiveWorld=61,Selected;public static bool Prompt,LoggedIn;public static string LobbyName="泰坦";public static void Reset(bool logged=true){Trace.Clear();ActiveWorld=61;Selected=0;Prompt=false;LoggedIn=logged;ECommons.GameHelpers.Player.Available=logged;P.TaskManager.Clear();}}
 public static class Lifestream{public static readonly Plugin P=new();public static Config C=>P.Config;}
 public sealed class Plugin{public QueueManager TaskManager=new();public Config Config=new();public ArApi AutoRetainerApi=new();}
 public sealed class Config{public bool UseAutoRetainerAccounts=true;public Dictionary<string,int> ServiceAccounts=[];}
 public sealed class ArData{public string Name="合成角色",World="泰坦";public int ServiceAccount=3;}
 public sealed class ArApi{public bool Ready=true;public ArData Data=new();public string[] GetRegisteredCharacters()=>["fixture"];public ArData GetOfflineCharacterData(string id)=>Data;}
 public sealed class QueueManager {
  private readonly List<Func<bool>> tasks=[];private List<Func<bool>>? stack;
  public int Count=>tasks.Count;public void Clear()=>tasks.Clear();
  public void Enqueue(Action action)=>Add(()=>{action();return true;});public void Enqueue(Func<bool> fn)=>Add(fn);public void Enqueue(Func<bool?> fn)=>Add(()=>fn()!=false);
  public void Enqueue(Action action,string name,ECommons.Automation.NeoTaskManager.TaskManagerConfiguration? cfg=null)=>Enqueue(action);public void Enqueue(Func<bool?> fn,ECommons.Automation.NeoTaskManager.TaskManagerConfiguration cfg)=>Enqueue(fn);
  public void Enqueue(Func<bool?> fn,string name,ECommons.Automation.NeoTaskManager.TaskManagerConfiguration? cfg=null)=>Enqueue(fn);
  private void Add(Func<bool> fn){if(stack!=null)stack.Add(fn);else tasks.Add(fn);}
  public void InsertStack(Action fn){stack=[];fn();tasks.InsertRange(0,stack);stack=null;}
  public void Run(){for(int i=0;tasks.Count>0&&i<100;i++){var fn=tasks[0];if(fn())tasks.Remove(fn);}if(tasks.Count>0)throw new Exception("fixture queue stalled");}
 }
 internal static partial class Utils {public static bool CanAutoLogin()=>!ECommons.GameHelpers.Player.Available;public static bool GenericThrottle=>true;public static void RethrottleGeneric(){}public static List<World> GetVisitableWorldsFrom(World w)=>[];}
 public static class PluginLog{public static void Information(string msg){}public static void Error(string msg)=>throw new Exception(msg);}
}
namespace Lifestream.Tasks.Login{
 public static unsafe partial class TaskChangeCharacter{
  public static int WorldIndex=-1;public static void ResetWorldIndex()=>WorldIndex=-1;
  public static bool? ClickStart(){Fixture.Trace.Add("connect-title");return true;}
  public static bool? ClickSelectDataCenter(){Fixture.Trace.Add("global-dc-menu");return true;}
  public static bool? SelectDataCenter(int dc)=>true;public static bool? SelectServiceAccount(int account)=>true;
  public static bool? ConfirmLogin(){if(!Fixture.Prompt)return false;Fixture.LoggedIn=true;ECommons.GameHelpers.Player.Available=true;Fixture.Trace.Add("login-confirmed");return true;}
 }
}
namespace Lifestream.Tasks.CrossDC{public static class TaskLogout{public static void Enqueue(){P.TaskManager.Enqueue((Action)(()=>{Fixture.Trace.Add("logout");ECommons.GameHelpers.Player.Available=false;Fixture.LoggedIn=false;}));}}}
namespace Lifestream.GUI.Windows {
 public static class CharaSelectOverlay{
  public static bool TryGetValidCharaSelectListMenu(out AddonMaster._CharaSelectListMenu m){m=new();return true;}
  public static void ReconnectToValidDC(string n,uint current,uint home,World? dest,bool noLogin)=>Command(n,current,home,dest,noLogin);
  public static void Command(string n,uint current,uint home,World? dest,bool noLogin){if(dest!=null)throw new Exception("unexpected travel");Fixture.Trace.Add("ipc-character-dispatch");global::Lifestream.Tasks.CrossDC.TaskSelectChara.Enqueue(n,home,current);}
 }
}

namespace ECommons.Automation.NeoTaskManager{public sealed class TaskManagerConfiguration{public TaskManagerConfiguration(int timeLimitMS){}}}
