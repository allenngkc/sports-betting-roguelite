using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// Item grants for the audit paths (economy rework: the engine now exposes public
/// Run.GrantRelic/GrantConsumable seams, so the old reflection backdoor is gone).
/// Passives are granted once at run start; a granted consumable is REFILLED at every round's
/// open by RunPlayer (modeling a steady supply), because a single-use item granted once would
/// audit as noise.
/// </summary>
public static class ItemGrant
{
    private static readonly Dictionary<string, RelicDefinition> RelicById = BuildRelics();
    private static readonly Dictionary<string, ConsumableDefinition> ConsumableById = BuildConsumables();

    public static bool IsConsumable(string id) => ConsumableById.ContainsKey(id);

    public static string NameOf(string id)
        => RelicById.TryGetValue(id, out var r) ? r.Name
            : ConsumableById.TryGetValue(id, out var c) ? c.Name : id;

    public static double PriceOf(string id)
        => RelicById.TryGetValue(id, out var r) ? r.Price
            : ConsumableById.TryGetValue(id, out var c) ? c.Price : 0.0;

    /// <summary>Grants the passives in order (acquisition order) to a fresh round-1 run.</summary>
    public static void GrantRelics(Run run, IReadOnlyList<string> relicIds)
    {
        foreach (string id in relicIds)
        {
            if (!RelicById.TryGetValue(id, out RelicDefinition? def))
                throw new ArgumentException($"Unknown relic id '{id}'");
            run.GrantRelic(def);
        }
    }

    /// <summary>Tops the run up to one held copy of the consumable (RunPlayer calls per round).</summary>
    public static void RefillConsumable(Run run, string id)
    {
        if (!ConsumableById.TryGetValue(id, out ConsumableDefinition? def))
            throw new ArgumentException($"Unknown consumable id '{id}'");
        if (!run.OwnsConsumable(id))
            run.GrantConsumable(def);
    }

    private static Dictionary<string, RelicDefinition> BuildRelics()
    {
        var map = new Dictionary<string, RelicDefinition>();
        foreach (RelicDefinition d in RelicCatalog.All) map[d.Id] = d;
        return map;
    }

    private static Dictionary<string, ConsumableDefinition> BuildConsumables()
    {
        var map = new Dictionary<string, ConsumableDefinition>();
        foreach (ConsumableDefinition d in RelicCatalog.Consumables) map[d.Id] = d;
        return map;
    }
}
