using System;
using System.Collections.Generic;
using System.Reflection;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// HARNESS-ONLY BACKDOOR — must never migrate into engine or game code.
///
/// The relic power audit and the combo scan need a run to start already owning specific relics. The
/// frozen engine has no "grant relic" API: RelicCatalog prices are fixed and Run.BuyRelic requires
/// Phase.Shop and a bank deduction, so buying is both stochastic (the shop may not offer it) and
/// bank-distorting. The only clean, non-distorting way is to reach into Run's private EffectEngine and
/// append the relic directly, exactly as a purchase would — reflection, which is legitimate in a test
/// harness (this project) but would be a layering violation anywhere else.
/// </summary>
public static class RelicGrant
{
    private static readonly FieldInfo EffectsField =
        typeof(Run).GetField("_effects", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("Run._effects not found — engine layout changed.");

    private static readonly Dictionary<string, RelicDefinition> ById = BuildIndex();

    private static Dictionary<string, RelicDefinition> BuildIndex()
    {
        var map = new Dictionary<string, RelicDefinition>();
        foreach (RelicDefinition d in RelicCatalog.All) map[d.Id] = d;
        return map;
    }

    /// <summary>Grants the given relics (in order → acquisition order) to a freshly constructed run
    /// sitting in round-1 Betting. Must be called before the first bet.</summary>
    public static void Grant(Run run, IReadOnlyList<string> relicIds)
    {
        var effects = (EffectEngine)EffectsField.GetValue(run)!;
        foreach (string id in relicIds)
        {
            if (!ById.TryGetValue(id, out RelicDefinition? def))
                throw new ArgumentException($"Unknown relic id '{id}'");
            effects.Add(def); // appends in acquisition order, resets its per-round charge — as BuyRelic does
        }
    }
}
