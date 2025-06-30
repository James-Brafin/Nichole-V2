using JamesBrafin.Nichole;
using Nanoray.PluginManager;
using Nickel;
using System.Reflection;
using JamesBrafin.Nichole.Features;
using JamesBrafin.Nichole.Features.Actions;
using JamesBrafin.Nichole.Cards;

namespace JamesBrafin.Nichole.Artifacts;

/*
 * Artifacts are a nice way to accentuate a character's potential.
 * They can be simple effects that occur at simple times, they can modify an existing mechanic or one introduced by the character.
 * Similarly to cards, ensure you add this type to your ModEntry for registration.
 */
public class InspiredMix : Artifact, IRegisterable
{
    private static Spr SpriteActive;
    private static Spr SpriteInactive;
    public bool active = true;
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        SpriteActive = helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/artifacts/InspiredMix.png")).Sprite;
        SpriteInactive = helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/artifacts/InspiredMix.png")).Sprite;
        helper.Content.Artifacts.RegisterArtifact(new ArtifactConfiguration
        {
            ArtifactType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new ArtifactMeta
            {
                pools = [ArtifactPool.Common],
                owner = ModEntry.Instance.NicholeMain_Deck.Deck
            },
            Sprite = SpriteActive,
            Name = ModEntry.Instance.AnyLoc.Bind(["artifact", "InspiredMix", "name"]).Localize,
            Description = ModEntry.Instance.AnyLoc.Bind(["artifact", "InspiredMix", "desc"]).Localize,
        });
    }


    /*
     * Unlike Cards, Artifacts have no required methods. Implement the ones you need, and leave the rest unimplemented.
     * By default, Artifacts have everything implemented with methods that do nothing, so there is no need to call the super.
     */

    public override Spr GetSprite()
    {
        return active ? SpriteActive : SpriteInactive;
    }
    public override void OnTurnEnd(State state, Combat combat)
    {
        active = true;
    }
    public override void OnCombatEnd(State state)
    {
        active = true;
    }

    public override void OnPlayerPlayCard(int energyCost, Deck deck, Card card, State state, Combat combat, int handPosition, int handCount)
    {
        if (ReagentManager.IsReagent(card, state) && active == true)
        {
            combat.Queue(new ADrawCard { count = 1 });
            active = false;
        }
    }
}