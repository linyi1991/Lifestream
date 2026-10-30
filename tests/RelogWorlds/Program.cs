using Lifestream;
using ECommons;
using ECommons.DalamudServices;
using Lifestream.Tasks.Login;
using Lumina.Excel.Sheets;
using static Lifestream.Lifestream;
int passed=0;void Check(bool value,string label){if(!value)throw new Exception(label);Console.WriteLine($"PASS {++passed}: {label}");}
void Data(string canonical="Titan"){
 Svc.Data.Language=7;Svc.Data.Worlds.Clear();Svc.Data.Centers.Clear();Svc.Data.Centers.Add(new(151,8));Svc.Data.Centers.Add(new(1,1));
 for(uint i=4028;i<=4035;i++)Svc.Data.Worlds.Add(new(i,i==4035?canonical:"fixture-"+i,false,new(151)));
 Svc.Data.Worlds.Add(new(61,"Titan",true,new(1)));Svc.Data.Worlds.Add(new(4036,"Development",false,new(151)));
}
Data();
foreach(var alias in new[]{"泰坦","Titan","tItAn"," Titan "})Check(RelogWorlds.Resolve(alias)?.RowId==4035,"TC alias resolves official non-public row 4035: "+alias);
foreach(var alias in new[]{"伊弗利特","火神","Ifrit","迦樓羅","風神","Garuda","利維坦","Leviathan","鳳凰","Phoenix","奧汀","Odin","巴哈姆特","巴哈","Bahamut","拉姆","Ramuh"})Check(RelogWorlds.Resolve(alias)?.RowId is >=4028 and <=4034,"other TC production alias accepted: "+alias);
foreach(var invalid in new string?[]{null,""," ","泰坦假的","Titan-invalid","Development","4035","世界不存在"})Check(RelogWorlds.Resolve(invalid!)==null,"unknown/development/unsupported numeric name rejected");
Svc.Data.Language=1;Check(RelogWorlds.Resolve("Titan")?.RowId==61,"global client preserves JP Titan despite TC rows");Check(RelogWorlds.Resolve("泰坦")==null,"global client cannot use TC exception");Check(RelogWorlds.Resolve("tItAn")==null,"global public-world exact spelling behavior is preserved");Svc.Data.Language=7;
Svc.Data.Worlds.RemoveAll(w=>w.RowId==4035);Check(RelogWorlds.Resolve("Titan")==null,"missing Taiwan row never falls through to JP Titan");
Data();Svc.Data.Worlds.RemoveAll(w=>w.RowId==4035);Svc.Data.Worlds.Add(new(4035,"Titan",false,new(1)));Check(RelogWorlds.Resolve("泰坦")==null,"wrong data center rejects TC exception");
Data();Svc.Data.Centers.RemoveAll(w=>w.RowId==151);Check(RelogWorlds.Resolve("泰坦")==null,"missing TC data-center metadata fails closed");
Data();Svc.Data.Centers.RemoveAll(w=>w.RowId==151);Svc.Data.Centers.Add(new(151,1));Check(RelogWorlds.Resolve("泰坦")==null,"wrong region rejects TC exception");
Data();Check(RelogWorlds.CharacterKeysMatch("合成角色@Titan","合成角色@泰坦"),"account lookup compares TC world identity without migrating keys");Check(!RelogWorlds.CharacterKeysMatch("合成角色@Titan","另一角色@泰坦"),"character names are never normalized or mixed");
C.ServiceAccounts.Clear();C.ServiceAccounts.Add("合成角色@泰坦",4);C.UseAutoRetainerAccounts=false;
Check(Utils.GetServiceAccount("合成角色@Titan")==4,"local account alias lookup retains configured account");Check(C.ServiceAccounts.Count==1&&C.ServiceAccounts["合成角色@泰坦"]==4,"local account settings remain unchanged");
C.ServiceAccounts.Add("合成角色@Titan",6);Check(Utils.GetServiceAccount("合成角色@Titan")==6,"exact configured key wins over aliases");
try{Utils.GetServiceAccount("合成角色@ tItAn ");throw new Exception("ambiguous account accepted");}catch(InvalidOperationException){Check(true,"ambiguous alias accounts fail rather than silently using another account");}
Fixture.Reset();try{Utils.ChangeCharacter("合成角色"," tItAn ");throw new Exception("ambiguous account request accepted");}catch(InvalidOperationException){Check(P.TaskManager.Count==0&&Fixture.LoggedIn,"ambiguous service-account aliases fail before any logout");}
C.UseAutoRetainerAccounts=true;Check(Utils.GetServiceAccount("合成角色@Titan")==3,"AR configured account keeps precedence via TC alias identity");
// Production enqueue, connect, world/character selection and IPC dispatch methods;
// only native game UI, logout/login confirmation, data sheets and queue clock are simulated.
foreach(var canonical in new[]{"Titan","泰坦"})foreach(var input in new[]{"Titan","泰坦"}){
 Data(canonical);Fixture.Reset();Check(Utils.ChangeCharacter("合成角色",input)==ErrorCode.Success,"live-to-target request accepted for input/sheet spellings: "+input+"/"+canonical);
 Check(!Fixture.Trace.Any()&&P.TaskManager.Count>0,"accepted request has not logged out or claimed login yet");P.TaskManager.Run();
 Check(Fixture.Trace.SequenceEqual(new[]{"logout","connect-title","lobby-world-selected","ipc-character-dispatch","character-selected","login-confirmed"}),"production queue traverses logout, connect, native world selection, IPC character dispatch, select and confirm");
 Check(Fixture.LoggedIn&&Fixture.Selected==4035,"same-named JP character never selected; correct 4035 target reaches confirmed fixture login");
 Check(!Fixture.Trace.Contains("global-dc-menu"),"client language 7 uses existing title-start path without culture conversion");
}
Data();Fixture.Reset(false);Check(Utils.ChangeCharacter("合成角色","Titan")==ErrorCode.Success,"title-screen relog remains accepted");P.TaskManager.Run();Check(!Fixture.Trace.Contains("logout")&&Fixture.LoggedIn&&Fixture.Selected==4035,"title-screen production chain logs in without extra logout");
foreach(var invalid in new[]{"Development","泰坦假的",""}){Data();Fixture.Reset();Check(Utils.ChangeCharacter("合成角色",invalid)==ErrorCode.Invalid_world_specified&&P.TaskManager.Count==0&&Fixture.LoggedIn,"invalid world rejected before any logout or queue side effects");}
Data();Fixture.Reset();Svc.Data.Worlds.RemoveAll(w=>w.RowId==4035);Check(Utils.ChangeCharacter("合成角色","Titan")==ErrorCode.Invalid_world_specified&&P.TaskManager.Count==0,"missing official target rejects request before logout even with JP Titan present");
Console.WriteLine($"TOTAL {passed} PASS; production managed flow with synthetic sheets/UI/queue; no game validation");
