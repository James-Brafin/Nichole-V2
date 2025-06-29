using HarmonyLib;
using JamesBrafin.Nichole;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace JamesBrafin.Nichole.Features;
#nullable enable

public class CardBrowseFilterManager
{
    static ModEntry Instance => ModEntry.Instance;
    static Harmony Harmony => Instance.Harmony;
    static IModData ModData => Instance.Helper.ModData;

    internal const string FilterReagentKey = "FilterReagent";

    public CardBrowseFilterManager()
    {
        Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(ACardSelect), nameof(ACardSelect.BeginWithRoute)),
            transpiler: new HarmonyMethod(GetType(), nameof(ACardSelect_BeginWithRoute_Transpiler))
        );
        Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(CardBrowse), nameof(CardBrowse.GetCardList)),
            postfix: new HarmonyMethod(GetType(), nameof(CardBrowse_GetCardList_Postfix))
        );
    }

    private static IEnumerable<CodeInstruction> ACardSelect_BeginWithRoute_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase originalMethod)
    {
        return new SequenceBlockMatcher<CodeInstruction>(instructions)
            .Find(
                ILMatches.Newobj(typeof(CardBrowse).GetConstructor([])!),
                ILMatches.Instruction(OpCodes.Dup)
            )
            .Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, new List<CodeInstruction> {
                new(OpCodes.Dup),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(CardBrowseFilterManager), nameof(CopyDataToCardBrowse))),
            })
            .AllElements();
    }

    private static void CopyDataToCardBrowse(CardBrowse cardBrowse, ACardSelect cardSelect)
    {
        if (ModData.TryGetModData<bool>(cardSelect, FilterReagentKey, out var filterWild))
            ModData.SetModData(cardBrowse, FilterReagentKey, filterWild);
    }


    private static void CardBrowse_GetCardList_Postfix(CardBrowse __instance, ref List<Card> __result, G g)
    {
        bool doesFilterWild = ModData.TryGetModData<bool>(__instance, FilterReagentKey, out var filterWild);
        Combat combat = g.state.route as Combat ?? DB.fakeCombat;
        if ((doesFilterWild) && __instance.browseSource != CardBrowse.Source.Codex)
        {
            __result.RemoveAll(delegate (Card c)
            {
                CardData data = c.GetDataWithOverrides(g.state);

                if (doesFilterWild)
                {
                    if (ReagentManager.IsReagent(c, g.state) != filterWild)
                        return true;
                }
                return false;
            });
        }
    }
}