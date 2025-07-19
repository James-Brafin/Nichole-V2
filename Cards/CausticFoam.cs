using JamesBrafin.Nichole.Features.Actions;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Shockah.Kokoro.IKokoroApi.IV2.IActionCostsApi;
using Nanoray.PluginManager;

namespace JamesBrafin.Nichole.Cards;

internal sealed class CausticFoam : Card, ICard
{
    /* For a bit more info on the Register Method, look at InternalInterfaces.cs and 1. CARDS section in ModEntry */
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard("CausticFoam", new()
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new()
            {
                /* We don't assign cards to characters, but rather to decks! It's important to keep that in mind */
                deck = ModEntry.Instance.NicholeMain_Deck.Deck,

                /* The vanilla rarities are Rarity.common, Rarity.uncommon, Rarity.rare */
                rarity = Rarity.rare,

                /* Some vanilla cards don't upgrade, some only upgrade to A, but most upgrade to either A or B */
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            /* AnyLocalizations.Bind().Localize will find the 'name' of 'Foxtale' in 'card', in the locale file, and feed it here. The output for english in-game from this is 'Fox Tale' */
            Name = ModEntry.Instance.AnyLoc.Bind(["card", "CausticFoam", "name"]).Localize
        });
    }
    public override CardData GetData(State state)
    {
        CardData data = new CardData()
        {
            /* Give your card some meta data, such as giving it an energy cost, making it exhaustable, and more */
            cost = upgrade == Upgrade.B ? 2 : 1,

            /* if we don't set a card specific 'art' (a 'Spr' type) here, the game will give it the deck's 'DefaultCardArt'
            /* if we don't set a card specific 'description' (a 'string' type) here, the game will attempt to use iconography using the provided CardAction types from GetActions() */
        };
        return data;
    }
    public override List<CardAction> GetActions(State s, Combat c)
    {
        /* The meat of the card, this is where we define what a card does, and some would say the most fun part of modding Cobalt Core happens here! */
        List<CardAction> actions = new();

        /* Since we want to have different actions for each Upgrade, we use a switch that covers the Upgrade paths we've defined */
        switch (upgrade)
        {
            case Upgrade.None:
                actions = new()
                {
                    new AAttack()
                    {
                        damage = GetDmg(s, 2)
                    },
                    ModEntry.Instance.KokoroApi.ActionCosts.MakeCostAction(
                        ModEntry.Instance.KokoroApi.ActionCosts.MakeResourceCost(new ReactionCost(), 1),
                        new AAttack()
                        {
                            damage = GetDmg(s, 1),
                            status = Status.corrode,
                            statusAmount = 1
                        }).AsCardAction
                };
                /* Remember to always break it up! */
                break;
            case Upgrade.A:
                actions = new()
                {
                    new AAttack()
                    {
                        damage = GetDmg(s, 3)
                    },
                    ModEntry.Instance.KokoroApi.ActionCosts.MakeCostAction(
                        ModEntry.Instance.KokoroApi.ActionCosts.MakeResourceCost(new ReactionCost(), 1),
                        new AAttack()
                        {
                            damage = GetDmg(s, 1),
                            status = Status.corrode,
                            statusAmount = 1
                        }).AsCardAction
                };
                break;
            case Upgrade.B:
                actions = new()
                {
                    new AAttack()
                    {
                        damage = GetDmg(s, 1),
                        statusAmount = 1
                    },
                    ModEntry.Instance.KokoroApi.ActionCosts.MakeCostAction(
                        ModEntry.Instance.KokoroApi.ActionCosts.MakeResourceCost(new ReactionCost(), 1),
                        new AAttack()
                        {
                            damage = GetDmg(s, 1),
                            status = Status.corrode,
                            statusAmount = 2
                        }).AsCardAction
                };
                break;
        }
        return actions;
    }
}