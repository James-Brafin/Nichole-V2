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
    internal static ICardTraitEntry Reagent { get; private set; } = null!;
    private static readonly IModCards CardsHelper = ModEntry.Instance.Helper.Content.Cards;
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        var icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/actions/Reagent.png"));
        Reagent = ModEntry.Instance.Helper.Content.Cards.RegisterTrait("Reagent", new()
        {
            Icon = (_, _) => icon.Sprite,
            Name = ModEntry.Instance.AnyLoc.Bind(["trait", "Reagent", "name"]).Localize,
            Tooltips = (_, _) =>
                [
                    new GlossaryTooltip($"trait.{ModEntry.Instance.Package.Manifest.UniqueName}::Reagent")
                    {
                        Icon = icon.Sprite,
                        TitleColor = Colors.cardtrait,
                        Title = ModEntry.Instance.Loc.Localize(["trait", "Reagent", "name"]),
                        Description = ModEntry.Instance.Loc.Localize(["trait", "Reagent", "description"])
                    }
                ]
        });

    }

    public static bool IsReagent(Card card, State s)
    {
        return CardsHelper.IsCardTraitActive(s, card, Reagent);
    }
}


