using ECommons.Automation;
using ECommons.Automation.UIInput;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace Lifestream.Tasks.Login;
public static unsafe partial class TaskChangeCharacter
{
    public static void Enqueue(string currentLoginWorld, string charaName, string charaWorld, int account)
    {
        if(Svc.ClientState.IsLoggedIn)
        {
            EnqueueLogout();
        }
        EnqueueLogin(currentLoginWorld, charaName, charaWorld, account);
    }

    public static void EnqueueLogout()
    {
        P.TaskManager.Enqueue(Logout);
        P.TaskManager.Enqueue(SelectYesLogout, new(timeLimitMS: 100000));
    }

    public static void EnqueueLogin(string currentLoginWorld, string charaName, string homeWorld, int account)
    {
        ConnectToDc(currentLoginWorld, account);
        P.TaskManager.Enqueue(TaskChangeCharacter.ResetWorldIndex);
        P.TaskManager.Enqueue(() => SelectCharacter(charaName, homeWorld, currentLoginWorld), $"Select chara {charaName}@{homeWorld}", new(timeLimitMS: 1000000));
        P.TaskManager.Enqueue(ConfirmLogin);
    }

    public static bool? SelectYesLogout()
    {
        if(!Svc.ClientState.IsLoggedIn) return true;
        var addon = Utils.GetSpecificYesno(Svc.Data.GetExcelSheet<Addon>()?.GetRow(115).Text.GetText());
        if(addon == null || !IsAddonReady(addon)) return false;
        if(Utils.GenericThrottle && EzThrottler.Throttle("ConfirmLogout"))
        {
            new AddonMaster.SelectYesno((nint)addon).Yes();
            return false;
        }
        return false;
    }

    public static bool? Logout()
    {
        var addon = Utils.GetSpecificYesno(Svc.Data.GetExcelSheet<Addon>()?.GetRow(115).Text.GetText());
        if(addon != null) return true;
        var isLoggedIn = Svc.Condition.Any();
        if(!isLoggedIn) return true;

        if(Player.Interactable && !Player.IsAnimationLocked && Utils.GenericThrottle && EzThrottler.Throttle("InitiateLogout"))
        {
            Chat.ExecuteCommand("/logout");
            return false;
        }
        return false;
    }

    public static bool? SelectServiceAccount(int account)
    {
        if(TryGetAddonByName<AtkUnitBase>("_CharaSelectWorldServer", out _))
        {
            return true;
        }
        if(TryGetAddonMaster<AddonMaster.SelectString>(out var m) && m.IsAddonReady)
        {
            var compareTo = Svc.Data.GetExcelSheet<Lobby>()?.GetRow(11).Text.GetText();
            if(m.Text == compareTo)
            {
                m.Entries[account].Select();
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    public static bool? ClickSelectDataCenter()
    {
        if(TryGetAddonByName<AtkUnitBase>("TitleDCWorldMap", out var addon) && addon->IsVisible)
        {
            PluginLog.Information($"Visible");
            Utils.RethrottleGeneric();
            return true;
        }
        if(TryGetAddonMaster<AddonMaster._TitleMenu>(out var m) && m.IsReady)
        {
            if(Utils.GenericThrottle && EzThrottler.Throttle("ClickTitleMenuStart"))
            {
                m.DataCenter();
                return false;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    public static bool? ClickStart()
    {
        if(TryGetAddonByName<AtkUnitBase>("_CharaSelectListMenu", out var addon) && addon->IsVisible)
        {
            PluginLog.Information($"Visible");
            Utils.RethrottleGeneric();
            return true;
        }
        if(TryGetAddonMaster<AddonMaster._TitleMenu>(out var m) && m.IsReady)
        {
            if(Utils.GenericThrottle && EzThrottler.Throttle("ClickTitleMenuStart"))
            {
                m.Start();
                return false;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    public static bool? SelectDataCenter(int dc)
    {
        if(TryGetAddonMaster<AddonMaster.TitleDCWorldMap>(out var m) && m.IsAddonReady)
        {
            if(Utils.GenericThrottle && EzThrottler.Throttle("ClickDCSelect"))
            {
                m.Select(dc);
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    public static void ResetWorldIndex() => WorldIndex = -1;

    public static int WorldIndex = -1;
    public static bool? ConfirmLogin()
    {
        if(TryGetAddonByName<AtkUnitBase>("SelectOk", out _))
        {
            return true;
        }
        if(Svc.ClientState.IsLoggedIn)
        {
            return true;
        }
        if(TryGetAddonMaster<AddonMaster.SelectYesno>(out var m) && m.IsAddonReady)
        {
            if(m.Text.ContainsAny(StringComparison.OrdinalIgnoreCase, Lang.LogInPartialText))
            {
                if(Utils.GenericThrottle && EzThrottler.Throttle("ConfirmLogin"))
                {
                    m.Yes();
                    return false;
                }
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    public static bool? CloseCharaSelect()
    {
        var lobby = AgentLobby.Instance();
        if(Utils.CanAutoLogin()) return true;
        if(!TryGetAddonByName<AtkUnitBase>("SelectOk", out _) && TryGetAddonByName<AtkUnitBase>("_CharaSelectReturn", out var addon) && IsAddonReady(addon) && (!lobby->AgentInterface.IsAgentActive() || !lobby->TemporaryLocked))
        {
            if(Utils.GenericThrottle)
            {
                addon->GetComponentButtonById(4)->ClickAddonButton(addon);
            }
        }
        return false;
    }
}
