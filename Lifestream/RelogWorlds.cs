using ECommons.ExcelServices;
using Lumina.Excel.Sheets;

namespace Lifestream;

internal static class RelogWorlds
{
    internal static bool TaiwanClient => (int)Svc.Data.Language == 7;

    private static IEnumerable<RelogWorld> Worlds()
    {
        var centers = Svc.Data.GetExcelSheet<WorldDCGroupType>();
        foreach (var world in Svc.Data.GetExcelSheet<World>())
        {
            var center = centers.GetRowOrDefault(world.DataCenter.RowId);
            yield return new(world.RowId, world.Name.ToString(), world.IsPublic, world.DataCenter.RowId, center?.Region ?? 0);
        }
    }

    internal static World? Resolve(string input)
        => RelogWorldPolicy.TryResolve(input, Worlds(), TaiwanClient, out var world)
            ? Svc.Data.GetExcelSheet<World>().GetRowOrDefault(world.Id) : null;

    internal static bool Matches(uint worldId, string name) => Resolve(name)?.RowId == worldId;

    internal static bool CharacterKeysMatch(string left, string right)
        => RelogWorldPolicy.CharacterKeysMatch(left, right, Worlds(), TaiwanClient);
}
