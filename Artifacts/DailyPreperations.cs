using JamesBrafin.Nichole;
using JamesBrafin.Nichole.Cards;
using JamesBrafin.Nichole.Features;
using JamesBrafin.Nichole.Features.Actions;
using Nanoray.PluginManager;
using Nickel;
using System.Collections.Generic;
using System.Reflection;

namespace JamesBrafin.Nichole.Artifacts;

/*
 * Artifacts are a nice way to accentuate a character's potential.
 * They can be simple effects that occur at simple times, they can modify an existing mechanic or one introduced by the character.
 * Similarly to cards, ensure you add this type to your ModEntry for registration.
 */
public class DailyPreperations : Artifact, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Artifacts.RegisterArtifact(new ArtifactConfiguration
        {
            ArtifactType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new ArtifactMeta
            {
                pools = [ArtifactPool.Common],
                owner = ModEntry.Instance.NicholeMain_Deck.Deck
            },
            Name = ModEntry.Instance.AnyLoc.Bind(["artifact", "DailyPreperations", "name"]).Localize,
            Description = ModEntry.Instance.AnyLoc.Bind(["artifact", "DailyPreperations", "desc"]).Localize,
            /*
             * For Artifacts with just one sprite, registering them at the place of usage helps simplify things.
             */
            Sprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/artifacts/DailyPreperations.png")).Sprite
        });
    }

    /*
     * Unlike Cards, Artifacts have no required methods. Implement the ones you need, and leave the rest unimplemented.
     * By default, Artifacts have everything implemented with methods that do nothing, so there is no need to call the super.
     */
    public override void OnCombatStart(State state, Combat c)
    {
        c.QueueImmediate(new AAddCard()
        {
            amount = 2,
            destination = CardDestination.Deck,
            card = new SimpleSolution(),
        });
    }

    public override List<Tooltip>? GetExtraTooltips() => [
    new TTCard {
            card = new SimpleSolution(){upgrade = Upgrade.A},
        }
];
}