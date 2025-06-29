using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JamesBrafin.Nichole.Features.Actions;
public class AVariableHintDiscard : AVariableHint
{
    public int displayAdjustment = 0;

    public bool useRegularHandSprite = false;
    public bool alsoHand;
    public int handCount;

    public AVariableHintDiscard() : base()
    {
        hand = true;
    }

    public override Icon? GetIcon(State s)
    {
        return new Icon(useRegularHandSprite ? StableSpr.icons_dest_hand : ModEntry.Instance.CountInDiscard.Sprite, null, Colors.textMain);
    }

    public override List<Tooltip> GetTooltips(State s)
    {
        List<Tooltip> list = [];
        string parentheses = "";
        if (s.route is Combat c)
        {
            var amt = c.discard.Count;
            DefaultInterpolatedStringHandler stringHandler = new(22, 1);
            stringHandler.AppendLiteral(" </c>(<c=keyword>");
            stringHandler.AppendFormatted(amt + displayAdjustment);
            stringHandler.AppendLiteral("</c>)");

            parentheses = stringHandler.ToStringAndClear();
        }
        list.Add(new TTText(ModEntry.Instance.Loc.Localize(["action", "variableHintDiscard", "desc"], new { Amount = parentheses })));
        return list;
    }
}