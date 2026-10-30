using ECommons.ExcelServices;
using ECommons.GameHelpers;
using Lifestream.Tasks.CrossDC;
using Lifestream.Tasks.Login;
using Lifestream.IPC;

namespace Lifestream;

internal static unsafe partial class Utils
{
    public static ErrorCode ChangeCharacter(string charaName, string charaHomeWorld)
    {
        if(RelogWorlds.Resolve(charaHomeWorld) == null)
        {
            return ErrorCode.Invalid_world_specified;
        }
        // Validate alias-based account preferences before leaving the current character.
        if(RelogWorlds.TaiwanClient) _ = GetServiceAccount($"{charaName}@{charaHomeWorld}");
        if(Player.Available)
        {
            TaskLogout.Enqueue();
            P.TaskManager.Enqueue(() =>
            {
                P.TaskManager.InsertStack(() =>
                {
                    TaskConnectAndOpenCharaSelect.Enqueue(charaName, charaHomeWorld);
                });
            }, "Use TaskConnectAndOpenCharaSelect");
            P.TaskManager.Enqueue(() => IpcUtils.InitiateTravelFromCharaSelectScreenInternal(charaName, charaHomeWorld, null, false), "Use InitiateTravelFromCharaSelectScreenInternal");
            return ErrorCode.Success;
        }
        if(CanAutoLogin())
        {
            TaskConnectAndOpenCharaSelect.Enqueue(charaName, charaHomeWorld);
            P.TaskManager.Enqueue(() => IpcUtils.InitiateTravelFromCharaSelectScreenInternal(charaName, charaHomeWorld, null, false));
            return ErrorCode.Success;
        }
        return ErrorCode.Player_is_not_logged_in;
    }

}
