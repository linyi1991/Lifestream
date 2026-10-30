using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifestream.Tasks.Login;
public static unsafe class TaskConnectAndOpenCharaSelect
{
    public static bool Enqueue(string charaName, string homeWorld)
    {
        var account = Utils.GetServiceAccount($"{charaName}@{homeWorld}");
        if(RelogWorlds.Resolve(homeWorld) == null) throw new InvalidOperationException("找不到可登入的目標世界，請確認台服世界資料與名稱。");
        if(Utils.CanAutoLogin())
        {
            TaskChangeCharacter.ConnectToDc(homeWorld, account);
            P.TaskManager.Enqueue(TaskChangeCharacter.ResetWorldIndex);
            P.TaskManager.Enqueue(() => TaskChangeCharacter.SelectCharacter(charaName, homeWorld, homeWorld, onlyChangeWorld: true), $"Select chara {charaName}@{homeWorld}", new(timeLimitMS: 5.Minutes()));
            return true;
        }
        PluginLog.Error("Can not log in now");
        return false;
    }
}