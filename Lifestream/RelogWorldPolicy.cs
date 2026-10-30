namespace Lifestream;

// Narrow API13 adaptation of ffxiv-tc-port/Lifestream PublicWorlds (f4deed52).
// Keep policy independent of game services so metadata and alias failures can be tested offline.
internal readonly record struct RelogWorld(uint Id, string Name, bool Public, uint DataCenterId, byte Region);

internal static class RelogWorldPolicy
{
    private static readonly Dictionary<string, uint> TaiwanAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["伊弗利特"] = 4028, ["火神"] = 4028, ["Ifrit"] = 4028,
        ["迦樓羅"] = 4029, ["風神"] = 4029, ["Garuda"] = 4029,
        ["利維坦"] = 4030, ["Leviathan"] = 4030,
        ["鳳凰"] = 4031, ["Phoenix"] = 4031,
        ["奧汀"] = 4032, ["Odin"] = 4032,
        ["巴哈姆特"] = 4033, ["巴哈"] = 4033, ["Bahamut"] = 4033,
        ["拉姆"] = 4034, ["Ramuh"] = 4034,
        ["泰坦"] = 4035, ["Titan"] = 4035,
    };

    internal static bool IsTaiwanProduction(RelogWorld world)
        => world.Id is >= 4028 and <= 4035 && world.DataCenterId == 151 && world.Region == 8
            && !string.IsNullOrWhiteSpace(world.Name);

    internal static bool TryResolve(string input, IEnumerable<RelogWorld> worlds, bool taiwanClient, out RelogWorld result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var name = taiwanClient ? input.Trim() : input;
        // English names collide with JP worlds. Apply aliases only to client language 7.
        // A missing Taiwan row must fail closed, never fall through to JP Titan.
        if (taiwanClient && TaiwanAliases.TryGetValue(name, out var id))
        {
            foreach (var world in worlds)
                if (world.Id == id && IsTaiwanProduction(world)) { result = world; return true; }
            return false;
        }
        foreach (var world in worlds)
            if ((world.Public || taiwanClient && IsTaiwanProduction(world))
                && !string.IsNullOrWhiteSpace(world.Name)
                && string.Equals(world.Name, name, StringComparison.Ordinal))
            { result = world; return true; }
        return false;
    }

    internal static bool CharacterKeysMatch(string left, string right, IEnumerable<RelogWorld> worlds, bool taiwanClient)
    {
        if (string.Equals(left, right, StringComparison.Ordinal)) return true;
        if (!taiwanClient || left == null || right == null) return false;
        int a = left.LastIndexOf('@'), b = right.LastIndexOf('@');
        if (a <= 0 || b <= 0 || !string.Equals(left[..a], right[..b], StringComparison.Ordinal)) return false;
        return TryResolve(left[(a + 1)..], worlds, true, out var first)
            && TryResolve(right[(b + 1)..], worlds, true, out var second)
            && IsTaiwanProduction(first) && IsTaiwanProduction(second) && first.Id == second.Id;
    }
}
