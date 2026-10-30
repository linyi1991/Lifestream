using ECommons.ExcelServices;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Lifestream.Tasks.Login;
public static unsafe partial class TaskChangeCharacter
{
    public static void ConnectToDc(string currentWorld, int account)
    {
        var world = RelogWorlds.Resolve(currentWorld)
            ?? throw new InvalidOperationException("找不到可登入的世界，無法連線至資料中心。");
        var dc = (int)world.DataCenter.RowId;
        if((int)Svc.Data.Language < 4)
        {
            P.TaskManager.Enqueue(ClickSelectDataCenter, new(timeLimitMS: 1000000));
            P.TaskManager.Enqueue(() => SelectDataCenter(dc), $"Connect to DC {dc}");
            P.TaskManager.Enqueue(() => SelectServiceAccount(account), $"SelectServiceAccount {account}");
        }
        else
        {
            P.TaskManager.Enqueue(ClickStart);
        }
    }

}
