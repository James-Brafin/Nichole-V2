using JamesBrafin.Nichole;
using Nickel;
using System.Collections.Generic;

#nullable enable
namespace JamesBrafin.Nichole.Features.Actions;

public class AMakeReagent : CardAction
{
    public bool permanent;
    public bool showCard;


    public override Route? BeginWithRoute(G g, State s, Combat c)
    {
        if (selectedCard != null)
        {
            ModEntry.Instance.Helper.Content.Cards.SetCardTraitOverride(s, selectedCard, ReagentManager.Trait, true, permanent);
            // ModEntry.Instance.WildManager.SetWild(selectedCard, true, permanent);

            return showCard ? new CustomShowCards
            {
                message = ModEntry.Instance.Loc.Localize(["action", "makeReagent", "showCardText"]),
                cardIds = [selectedCard.uuid]
            } : null;
        }
        return null;
    }

    public override Icon? GetIcon(State s)
    {
        return new Icon(ModEntry.Instance.ReagentIcon.Sprite, null, Colors.textMain);
    }

    public string GetDuration()
    {
        return permanent ? Loc.T("actionShared.durationForever") : Loc.T("actionShared.durationCombat");
    }

    public override List<Tooltip> GetTooltips(State s)
    {
        return [
            new GlossaryTooltip($"action.{GetType().Namespace!}::MakeReagent") {
                Icon = ModEntry.Instance.ReagentIcon.Sprite,
                TitleColor = Colors.action,
                Title = ModEntry.Instance.Loc.Localize(["action", "makeReagent", "name"]),
                Description = ModEntry.Instance.Loc.Localize(["action", "makeReagent", "description"], new { Duration = GetDuration() })
            },
            .. ReagentManager.Trait.Configuration.Tooltips!(s, selectedCard),
            // new CustomTTGlossary(
            //     CustomTTGlossary.GlossaryType.cardtrait,
            //     () => ModEntry.Instance.WildIcon.Sprite,
            //     () => ModEntry.Instance.Localizations.Localize(["trait", "wild", "name"]),
            //     () => ModEntry.Instance.Localizations.Localize(["trait", "wild", "description"]),
			// 	key: "trait.wild"
            // )
        ];
    }

    public override string? GetCardSelectText(State s)
    {
        return ModEntry.Instance.Loc.Localize(["action", "makeReagent", "cardSelectText", permanent ? "forever" : "temp"]);
    }
}