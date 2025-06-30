using FSPRO;
using Microsoft.Extensions.Logging;
using Nickel;
using Shockah.Kokoro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JamesBrafin.Nichole.Features.Actions;

internal sealed class ReactionCostManager
{
    internal static readonly ICardTraitEntry Trait = ModEntry.Instance.ReagentTrait;

    internal readonly IKokoroApi.IV2.IActionCostsApi.IHook Hook = new ReactionCostHook();
    public ReactionCostManager()
    {
        ModEntry.Instance.KokoroApi.ActionCosts.RegisterHook(Hook);
        ModEntry.Instance.KokoroApi.ActionCosts.RegisterResourceCostIcon(new ReactionCost(), ModEntry.Instance.ReactionIcon.Sprite, ModEntry.Instance.ReactionOff.Sprite);
    }
}
internal sealed class ReactionCost : IKokoroApi.IV2.IActionCostsApi.IResource
{
    public string ResourceKey => "Nichole::Reaction";
    public int GetCurrentResourceAmount(State state, Combat combat)
    {

        int reactionCounter = 0;
        int? currentCard = ModEntry.Instance.helper.ModData.ObtainModData<int?>(combat, "Card");
        foreach (Card card in combat.hand)
            {
            if (card.uuid != currentCard)
            {
                if (ReagentManager.IsReagent(card, state))
                {
                    reactionCounter++;
                }
            };
        }
        return reactionCounter;
    }

    public void Pay(State s, Combat c, int amount)
    {
       /*  int index = c.hand.Count - 1;
        while (index >= 0 && amount > 0)
        {
            if (ReagentManager.IsReagent(c.hand[index], s))
            {
                ModEntry.Instance.Logger.LogInformation("Check");
                s.RemoveCardFromWhereverItIs(c.hand[index].uuid);
                ModEntry.Instance.Logger.LogInformation("Check 2");
                c.hand[index].OnDiscard(s, c);
                ModEntry.Instance.Logger.LogInformation("Check 3");
                c.SendCardToDiscard(s, c.hand[index]);
                return;
            }
            index--;
        } */

        foreach(Card card in c.hand)
        {
            if (ReagentManager.IsReagent(card, s))
            {
                s.RemoveCardFromWhereverItIs(card.uuid);
                card.OnDiscard(s, c);
                c.SendCardToDiscard(s, card);
                return;
            }
        }
    }

    public IReadOnlyList<Tooltip> GetTooltips(State state, Combat combat, int amount)
        => [
            new GlossaryTooltip($"action.{ModEntry.Instance.Package.Manifest.UniqueName}::Impair")
        {
            Icon = ModEntry.Instance.ReactionIcon.Sprite,
            TitleColor = Colors.action,
            Title = ModEntry.Instance.Loc.Localize(["action", "ReactionCost", "name"]),
            Description = ModEntry.Instance.Loc.Localize(["action", "ReactionCost", "desc"])
        }
        ];
}

internal sealed class ReactionCostHook : IKokoroApi.IV2.IActionCostsApi.IHook
{
    public bool ModifyActionCost(IKokoroApi.IV2.IActionCostsApi.IHook.IModifyActionCostArgs args)
    {
        ModEntry.Instance.helper.ModData.SetModData(args.Combat, "Card", args.Card?.uuid);
        return false;
    }
}
