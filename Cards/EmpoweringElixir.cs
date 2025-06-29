using JamesBrafin.Nichole.Features;
using JamesBrafin.Nichole.Features.Actions;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JamesBrafin.Nichole.Cards;

internal sealed class EmpoweringElixir : Card, ICard
{
    /* For a bit more info on the Register Method, look at InternalInterfaces.cs and 1. CARDS section in ModEntry */
    public static void Register(IModHelper helper)
    {
        helper.Content.Cards.RegisterCard("EmpoweringElixir", new()
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new()
            {
                /* We don't assign cards to characters, but rather to decks! It's important to keep that in mind */
                deck = ModEntry.Instance.NicholeMain_Deck.Deck,

                /* The vanilla rarities are Rarity.common, Rarity.uncommon, Rarity.rare */
                rarity = Rarity.common,

                /* Some vanilla cards don't upgrade, some only upgrade to A, but most upgrade to either A or B */
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            /* AnyLocalizations.Bind().Localize will find the 'name' of 'Foxtale' in 'card', in the locale file, and feed it here. The output for english in-game from this is 'Fox Tale' */
            Name = ModEntry.Instance.AnyLoc.Bind(["card", "EmpoweringElixir", "name"]).Localize
        });
    }
    public override CardData GetData(State state)
    {
        CardData data = new CardData()
        {
            /* Give your card some meta data, such as giving it an energy cost, making it exhaustable, and more */
            cost = upgrade == Upgrade.A ? 2 : 3,
            exhaust = true

            /* if we don't set a card specific 'art' (a 'Spr' type) here, the game will give it the deck's 'DefaultCardArt'
            /* if we don't set a card specific 'description' (a 'string' type) here, the game will attempt to use iconography using the provided CardAction types from GetActions() */
        };
        return data;
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => upgrade switch
    {
        _ => new HashSet<ICardTraitEntry>() { ReagentManager.Reagent }
    };
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
                    new AStatus()
                    {
                        status = Status.powerdrive,
                        statusAmount = 1,
                        targetPlayer = true
                    },
                    ModEntry.Instance.KokoroApi.OnDiscard.MakeAction(
                        new AStatus()
                    {
                        status = Status.overdrive,
                        statusAmount = 1,
                        targetPlayer = true
                    }).AsCardAction
                };
                /* Remember to always break it up! */
                break;
            case Upgrade.A:
                actions = new()
                {
                    new AStatus()
                    {
                        status = Status.powerdrive,
                        statusAmount = 1,
                        targetPlayer = true
                    },
                    ModEntry.Instance.KokoroApi.OnDiscard.MakeAction(
                        new AStatus()
                    {
                        status = Status.overdrive,
                        statusAmount = 1,
                        targetPlayer = true
                    }).AsCardAction
                };
                break;
            case Upgrade.B:
                actions = new()
                {
                    new AStatus()
                    {
                        status = Status.powerdrive,
                        statusAmount = 1,
                        targetPlayer = true
                    },
                    ModEntry.Instance.KokoroApi.OnDiscard.MakeAction(
                        new AStatus()
                    {
                        status = Status.overdrive,
                        statusAmount = 2,
                        targetPlayer = true
                    }).AsCardAction
                };
                break;
        }
        return actions;
    }
}