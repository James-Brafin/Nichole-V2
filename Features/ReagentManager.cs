using HarmonyLib;
using Nanoray.PluginManager;
using Nickel;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace JamesBrafin.Nichole.Features;

internal sealed class ReagentManager
{
    internal static readonly ICardTraitEntry Trait = ModEntry.Instance.ReagentTrait;
    private static readonly IModCards CardsHelper = ModEntry.Instance.Helper.Content.Cards;
    public static bool IsReagent(Card card, State s)
    {
        return CardsHelper.IsCardTraitActive(s, card, Trait);
    }
}


