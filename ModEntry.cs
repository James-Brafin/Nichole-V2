using HarmonyLib;
using JamesBrafin.Nichole.Artifacts;
using JamesBrafin.Nichole.Cards;
using JamesBrafin.Nichole.Features.Actions;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using Shockah.Kokoro;
using System;
using System.Collections.Generic;
using System.Linq;


namespace JamesBrafin.Nichole;

public sealed class ModEntry : SimpleMod
{
    internal readonly IKokoroApi.IV2 KokoroApi;
    public IModHelper helper { get; }

    internal Harmony Harmony { get; }

    internal static ModEntry Instance { get; private set; } = null!;
    internal ILocalizationProvider<IReadOnlyList<string>> AnyLoc { get; }
    internal ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Loc { get; }

    internal ISpriteEntry Nichole_CardBackground { get; }
    internal ISpriteEntry Nichole_CardFrame { get; }
    internal ISpriteEntry Nichole_Panel{ get; }

    internal ISpriteEntry ReagentIcon { get; }
    internal ISpriteEntry ReactionIcon { get; }

    internal ISpriteEntry ReactionOff { get; }
    internal ISpriteEntry CountInDiscard { get; }

    internal IDeckEntry NicholeMain_Deck { get; }
    internal IDeckEntry NicholeToken_Deck{ get; }

    internal Status RedrawStatus { get; }
    internal ICardTraitEntry ReagentTrait { get; }

    internal static IReadOnlyList<Type> Nichole_Tokens_Types { get; } = [
       typeof(SimpleSolution),

    ];

    internal static IReadOnlyList<Type> Nichole_StarterCard_Types { get; } = [
        typeof(PotionPrep),
        typeof(VialToss)
    ];

    internal static IReadOnlyList<Type> Nichole_AlternateCard_Types { get; } = [
        typeof(QuickGuard),
        typeof(FlexStrike)
];

    internal static IReadOnlyList<Type> Nichole_CommonCard_Types { get; } = [
        typeof(LiquidIdea),
        typeof(DoubleThrow),
        typeof(AnalysisAttack),
        typeof(SpeedStrike),
        typeof(ImpulseBrew)
    ];

    internal static IReadOnlyList<Type> Nichole_UncommonCard_Types { get; } = [
        typeof(ChemicalReaction),
        typeof(ConfusingFumes),
        typeof(Adrenaline),
        typeof(CaffeineShot),
        typeof(EmpoweringElixir),
        typeof(FortifyMind),
        typeof(ReagentDupe)
    ];

    internal static IReadOnlyList<Type> Nichole_RareCard_Types { get; } = [
        typeof(RageSerum),
        typeof(BagDump),
        typeof(Revisit),
        typeof(CausticFoam),
        typeof(AdamantiumSkin)
    ];

    /* We can use an IEnumerable to combine the lists we made above, and modify it if needed
     * Maybe you created a new list for Uncommon cards, and want to add it.
     * If so, you can .Concat(TheUncommonListYouMade) */
    internal static IEnumerable<Type> Nichole_AllCard_Types
        => Nichole_StarterCard_Types
        .Concat(Nichole_AlternateCard_Types)
        .Concat(Nichole_CommonCard_Types)
        .Concat(Nichole_UncommonCard_Types)
        .Concat(Nichole_RareCard_Types)
        .Concat(Nichole_Tokens_Types);

    /* We'll organize our artifacts the same way: making lists and then feed those to an IEnumerable */
    internal static IReadOnlyList<Type> Nichole_CommonArtifact_Types { get; } = [
        typeof(Adaptability),
        typeof(DailyPreperations),
        typeof(DelayedReaction),
        typeof(InspiredMix),
        typeof(MakeshiftIngredients)
    ];

    internal static IReadOnlyList<Type> Nichole_BossArtifact_Types { get; } = [
        typeof(ChemicalBooster),
        typeof(Gratification),
        typeof(PotionSaver)
    ];

    internal static IEnumerable<Type> Nichole_AllArtifact_Types
        => Nichole_CommonArtifact_Types
        .Concat(Nichole_BossArtifact_Types);

    private static IEnumerable<Type> AllRegisterableTypes =
    Nichole_AllCard_Types
        .Concat(Nichole_AllArtifact_Types);

    public ModEntry(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
    {
        Instance = this;

        this.helper = helper;

        Harmony = new(package.Manifest.UniqueName);

        KokoroApi = helper.ModRegistry.GetApi<IKokoroApi>("Shockah.Kokoro")!.V2;

        RedrawStatus = KokoroApi.RedrawStatus.Status;

        /* These localizations lists help us organize our mod's text and messages by language.
         * For general use, prefer AnyLocalizations, as that will provide an easier time to potential localization submods that are made for your mod 
         * IMPORTANT: These localizations are found in the i18n folder (short for internationalization). The Demo Mod comes with a barebones en.json localization file that you might want to check out before continuing 
         * Whenever you add a card, artifact, character, ship, pretty much whatever, you will want to update your locale file in i18n with the necessary information */
        this.AnyLoc = new JsonLocalizationProvider(
            tokenExtractor: new SimpleLocalizationTokenExtractor(),
            localeStreamFunction: locale => package.PackageRoot.GetRelativeFile($"i18n/{locale}.json").OpenRead()
        );
        this.Loc = new MissingPlaceholderLocalizationProvider<IReadOnlyList<string>>(
            new CurrentLocaleOrEnglishLocalizationProvider<IReadOnlyList<string>>(this.AnyLoc)
        );

        /* Assigning our ISpriteEntry objects manually. This is the easiest way to do it when starting out!
         * Of note: GetRelativeFile is case sensitive. Double check you've written the file names correctly */
        Nichole_CardBackground = RegisterSprite(package, "assets/characters/Nichole_Card_Background.png");
        Nichole_CardFrame = RegisterSprite(package, "assets/characters/Nichole_Card_CardFrame.png");
        Nichole_Panel = RegisterSprite(package, "assets/characters/Nichole_Character_Panel.png");
        ReagentIcon = RegisterSprite(package, "assets/actions/Reagent.png");
        ReactionIcon = RegisterSprite(package, "assets/actions/Reaction.png");
        ReactionOff = RegisterSprite(package, "assets/actions/ReactionOff.png");
        CountInDiscard = RegisterSprite(package, "assets/actions/dest_discard.png");

        _ = new ReactionCostManager();

        ReagentTrait = helper.Content.Cards.RegisterTrait("ReagentTrait", new()
        {
            Name = this.AnyLoc.Bind(["cardtrait", "Reagent", "name"]).Localize,
            Icon = (state, card) => ReagentIcon.Sprite,
            Tooltips = (state, card) => [
                new GlossaryTooltip($"action.{Instance.Package.Manifest.UniqueName}::Impaired")
                {
                    Icon = ReagentIcon.Sprite,
                    TitleColor = Colors.cardtrait,
                    Title = Loc.Localize(["cardTrait", "Reagent", "name"]),
                    Description = Loc.Localize(["cardTrait", "Reagent", "desc"])
                }
            ]
        });

        /* Decks are assigned separate of the character. This is because the game has decks like Trash which is not related to a playable character
         * Do note that Color accepts a HEX string format (like Color("a1b2c3")) or a Float RGB format (like Color(0.63, 0.7, 0.76). It does NOT allow a traditional RGB format (Meaning Color(161, 178, 195) will NOT work) */
        NicholeMain_Deck = Helper.Content.Decks.RegisterDeck("NicholeDeck", new DeckConfiguration()
        {
            Definition = new DeckDef()
            {
                /* This color is used in various situations. 
                 * It is used as the deck's rarity 'shine'
                 * If a playable character uses this deck, the character Name will use this color
                 * If a playable character uses this deck, the character mini panel will use this color */
                color = new Color("dbbc7a"),

                /* This color is for the card name in-game
                 * Make sure it has a good contrast against the CardFrame, and take rarity 'shine' into account as well */
                titleColor = new Color("000000")
            },
            DefaultCardArt = Nichole_CardBackground.Sprite,
            BorderSprite = Nichole_CardFrame.Sprite,

            /* Since this deck will be used by our Demo Character, we'll use their name. */
            Name = this.AnyLoc.Bind(["character", "Nichole"]).Localize,
        });

        NicholeToken_Deck = Helper.Content.Decks.RegisterDeck("Nichole_Tokens", new DeckConfiguration()
        {
            Definition = new DeckDef()
            {
                /* This color is used in various situations. 
                 * It is used as the deck's rarity 'shine'
                 * If a playable character uses this deck, the character Name will use this color
                 * If a playable character uses this deck, the character mini panel will use this color */
                color = new Color("dbbc7a"),

                /* This color is for the card name in-game
                 * Make sure it has a good contrast against the CardFrame, and take rarity 'shine' into account as well */
                titleColor = new Color("000000")
            },
            DefaultCardArt = Nichole_CardBackground.Sprite,
            BorderSprite = Nichole_CardFrame.Sprite,

            /* Since this deck will be used by our Demo Character, we'll use their name. */
            Name = this.AnyLoc.Bind(["character", "Nichole_Tokens"]).Localize,
        });

        foreach (var type in AllRegisterableTypes)
            AccessTools.DeclaredMethod(type, nameof(IRegisterable.Register))?.Invoke(null, [package, helper]);

        Instance.Helper.Content.Characters.V2.RegisterCharacterAnimation(new CharacterAnimationConfigurationV2
        {
            CharacterType = NicholeMain_Deck.Deck.Key(),
            LoopTag = "neutral",
            Frames = [
                    RegisterSprite(package, "assets/characters/Nichole_Neutral_0.png").Sprite
                ]
        });

        Instance.Helper.Content.Characters.V2.RegisterCharacterAnimation(new CharacterAnimationConfigurationV2
        {
            CharacterType = NicholeMain_Deck.Deck.Key(),
            LoopTag = "mini",
            Frames = [
            RegisterSprite(package, "assets/characters/Nichole_Mini.png").Sprite
        ]
        });

        Instance.Helper.Content.Characters.V2.RegisterCharacterAnimation(new CharacterAnimationConfigurationV2
        {
            CharacterType = NicholeMain_Deck.Deck.Key(),
            LoopTag = "gameover",
            Frames = [
           RegisterSprite(package, "assets/characters/Nichole_Scawed.png").Sprite
        ]
        });

        Instance.Helper.Content.Characters.V2.RegisterCharacterAnimation(new CharacterAnimationConfigurationV2
        {
            CharacterType = NicholeMain_Deck.Deck.Key(),
            LoopTag = "squint",
            Frames = [
            RegisterSprite(package, "assets/characters/Nichole_Angy.png").Sprite
        ]
        });

        /*Of Note: You may notice we aren't assigning these ICharacterEntry and ICharacterAnimationEntry to any object, unlike stuff above,
        * It's totally fine to assign them, if you'd like, but we don't have a reason to in this mod */
        helper.Content.Characters.V2.RegisterPlayableCharacter("Nichole", new()
        {
            Deck = NicholeMain_Deck.Deck,
            Description = this.AnyLoc.Bind(["character", "desc"]).Localize,
            BorderSprite = Nichole_Panel.Sprite,
            Starters = new()
            {
                cards = [
                    new PotionPrep(),
                    new VialToss(),
                ]
            }
        });
    }

    public static ISpriteEntry RegisterSprite(IPluginPackage<IModManifest> package, string dir)
    {
        return Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile(dir));
    }
}
