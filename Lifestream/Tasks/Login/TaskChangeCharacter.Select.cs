using ECommons.ExcelServices;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Lifestream.Tasks.Login;
public static unsafe partial class TaskChangeCharacter
{
    public static bool? SelectCharacter(string name, string homeWorld, string currentLoginWorld, bool callContextMenu = false, bool onlyChangeWorld = false)
    {
        currentLoginWorld ??= homeWorld;
        if(TryGetAddonByName<AtkUnitBase>("SelectYesno", out _))
        {
            Utils.RethrottleGeneric();
            return true;
        }
        if(TryGetAddonByName<AtkUnitBase>("SelectOk", out _))
        {
            Utils.RethrottleGeneric();
            return true;
        }
        if(callContextMenu && TryGetAddonByName<AtkUnitBase>("ContextMenu", out var cmenu) && IsAddonReady(cmenu))
        {
            Utils.RethrottleGeneric();
            return true;
        }
        if(TryGetAddonMaster<AddonMaster._CharaSelectListMenu>(out var m) && m.IsAddonReady && TryGetAddonMaster<AddonMaster._CharaSelectWorldServer>(out var mw))
        {
            if(m.TemporarilyLocked) return false;
            if(mw.Worlds.Length == 0) return false;
            foreach(var c in m.Characters)
            {
                if(c.Name == name && RelogWorlds.Matches(c.HomeWorld, homeWorld))
                {
                    if(Utils.GenericThrottle && EzThrottler.Throttle("SelectChara"))
                    {
                        if(onlyChangeWorld)
                        {
                            return true;
                        }
                        else
                        {
                            if(!callContextMenu)
                            {
                                PluginLog.Information($"Calling login on {c.Name}@{ExcelWorldHelper.GetName(c.HomeWorld)}, index={c.Index}");
                                c.Login();
                            }
                            else
                            {
                                c.OpenContextMenu();
                            }
                        }
                    }
                    return false;
                }
            }
            for(var i = 0; i < mw.Worlds.Length; i++)
            {
                var w = mw.Worlds[i];
                if(WorldIndex == -1)
                {
                    if(RelogWorlds.Resolve(currentLoginWorld) is { } loginWorld && RelogWorlds.Matches(loginWorld.RowId, w.Name))
                    {
                        if(Utils.GenericThrottle && EzThrottler.Throttle("SelectWorld", 150))
                        {
                            w.Select();
                            WorldIndex++;
                        }
                        return false;
                    }
                }
                else
                {
                    if(i == WorldIndex % mw.Worlds.Length)
                    {
                        if(Utils.GenericThrottle && EzThrottler.Throttle("SelectWorld", 150))
                        {
                            w.Select();
                            WorldIndex++;
                        }
                        return false;
                    }
                }
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

}
