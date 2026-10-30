using ECommons.ExcelServices;

namespace Lifestream;
internal static unsafe partial class Utils
{
    internal static int GetServiceAccount(string name, uint world) => GetServiceAccount($"{name}@{ExcelWorldHelper.GetName(world)}");

    internal static int GetServiceAccount(string nameWithWorld)
    {
        if(P.AutoRetainerApi?.Ready == true && C.UseAutoRetainerAccounts)
        {
            var chars = P.AutoRetainerApi.GetRegisteredCharacters();
            foreach(var c in chars)
            {
                var data = P.AutoRetainerApi.GetOfflineCharacterData(c);
                if(data != null)
                {
                    var name = $"{data.Name}@{data.World}";
                    if(RelogWorlds.CharacterKeysMatch(nameWithWorld, name) && data.ServiceAccount > -1)
                    {
                        return data.ServiceAccount;
                    }
                }
            }
        }
        if(C.ServiceAccounts.TryGetValue(nameWithWorld, out var ret))
        {
            if(ret > -1) return ret;
        }
        if(RelogWorlds.TaiwanClient)
        {
            var aliases = C.ServiceAccounts.Where(entry => entry.Value >= 0 && RelogWorlds.CharacterKeysMatch(nameWithWorld, entry.Key))
                .Select(entry => entry.Value).Distinct().ToArray();
            if(aliases.Length == 1) return aliases[0];
            if(aliases.Length > 1) throw new InvalidOperationException("台服世界別名對應到不同服務帳號，請先核對既有帳號設定。");
        }
        return 0;
    }

}
