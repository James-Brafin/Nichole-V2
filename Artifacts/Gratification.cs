using JamesBrafin.Nichole;
using Nanoray.PluginManager;
using Nickel;
using System.Reflection;
using JamesBrafin.Nichole.Features;
using JamesBrafin.Nichole.Features.Actions;

namespace JamesBrafin.Nichole.Artifacts;

/*
 * Artifacts are a nice way to accentuate a character's potential.
 * They can be simple effects that occur at simple times, they can modify an existing mechanic or one introduced by the character.
 * Similarly to cards, ensure you add this type to your ModEntry for registration.
 */
public class Gratification : Artifact, IRegisterable
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
            Name = ModEntry.Instance.AnyLoc.Bind(["artifact", "Gratification", "name"]).Localize,
            Description = ModEntry.Instance.AnyLoc.Bind(["artifact", "Gratification", "desc"]).Localize,
            /*
             * For Artifacts with just one sprite, registering them at the place of usage helps simplify things.
             */
            Sprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/artifacts/Gratification.png")).Sprite
        });
    }

    /*
     * Unlike Cards, Artifacts have no required methods. Implement the ones you need, and leave the rest unimplemented.
     * By default, Artifacts have everything implemented with methods that do nothing, so there is no need to call the super.
     */
    public override void OnTurnEnd(State s, Combat c)
    {
        int index = c.hand.Count - 1;
        while (index >= 0 )
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
}