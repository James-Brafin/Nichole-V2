using FSPRO;
using Nickel;
using Shockah.Kokoro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamesBrafin.Nichole.Features.Actions;

internal sealed class ReactionManager
{
    internal static ISpriteEntry ReactionIcon = null!;

    internal readonly IKokoroApi.IV2.IActionCostsApi.IHook Hook = new ReactionCostHook();
    public ReactionManager()
    {
        ModEntry.Instance.KokoroApi.ActionCosts.RegisterHook(Hook);
        ReactionIcon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Actions/Reaction.png"));
    }
}
internal sealed class ReactionCost : IKokoroApi.IV2.IActionCostsApi.IResource
{
    public string ResourceKey => "Nichole::Reaction";
    public int GetCurrentResourceAmount(State state, Combat combat)
    {
        int index = combat.hand.Count - 1;
        int reactionCounter = 0;
        int? currentCard = ModEntry.Instance.helper.ModData.ObtainModData<int?>(combat, "Card");
        while (index >= 0)
        {
            if (combat.hand[index].uuid != currentCard)
            {
                if (ReagentManager.IsReagent(combat.hand[index], state))
                {
                    reactionCounter++;
                }
            }
            index--;
        }
        return reactionCounter;
    }

    public void Pay(State s, Combat c, int amount)
    {
        int index = c.hand.Count - 1;
        while (index >= 0 && amount > 0)
        {
            if (ReagentManager.IsReagent(c.hand[index], s))
            {
                s.RemoveCardFromWhereverItIs(c.hand[index].uuid);
                c.hand[index].OnDiscard(s, c);
                c.SendCardToDiscard(s, c.hand[index]);
                return;
            }
            index--;
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
