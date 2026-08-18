using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using HammerMod.Cards;
using HammerMod.Gameplay;
using HammerMod.Potions;
using HammerMod.Relics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HammerMod.Tests.Cards;

public sealed partial class CardContractTests
{
    private static readonly Type[] AllRegisteredCardTypes = typeof(HammerCard).Assembly
        .GetTypes()
        .Where(static type => !type.IsAbstract && type.GetCustomAttributesData().Any(
            static attribute => attribute.AttributeType.Name == "RegisterCardAttribute"))
        .OrderBy(static type => type.Name, StringComparer.Ordinal)
        .ToArray();

    private static readonly Type[] ConcreteCardTypes = typeof(HammerCard).Assembly
        .GetTypes()
        .Where(static type => !type.IsAbstract && typeof(HammerCard).IsAssignableFrom(type))
        .OrderBy(static type => type.Name, StringComparer.Ordinal)
        .ToArray();

    private static readonly Type[] RegisteredCardTypes = ConcreteCardTypes
        .Where(static type => type.GetCustomAttributesData().Any(
            static attribute => attribute.AttributeType.Name == "RegisterCardAttribute"))
        .ToArray();

    private static readonly Lazy<IReadOnlyDictionary<string, string>> EnglishLocalization =
        new(() => ReadLocalization("eng"));

    private static readonly Lazy<IReadOnlyDictionary<string, string>> ChineseLocalization =
        new(() => ReadLocalization("zhs"));

    public static IEnumerable<object[]> Cards =>
        RegisteredCardTypes.Select(static type => new object[] { type });

    [Fact]
    public void CardRegistrationContainsNoDuplicateRuntimeTypes()
    {
        Assert.NotEmpty(RegisteredCardTypes);
        Assert.Equal(87, AllRegisteredCardTypes.Length);
        Assert.Equal(
            AllRegisteredCardTypes.Length,
            AllRegisteredCardTypes.Distinct().Count());
        Assert.Equal(
            RegisteredCardTypes.Length,
            RegisteredCardTypes.Distinct().Count());
    }

    [Fact]
    public void EveryConcreteHammerCardIsRegistered()
    {
        Assert.Equal(ConcreteCardTypes, RegisteredCardTypes);
    }

    [Fact]
    public void CurrentCollectiblePoolCountsMatchTheDesignReport()
    {
        var cards = AllRegisteredCardTypes
            .Select(static type => Assert.IsAssignableFrom<CardModel>(Activator.CreateInstance(type)))
            .ToArray();

        Assert.Equal(4, cards.Count(static card => card.Rarity == CardRarity.Basic));
        Assert.Equal(19, cards.Count(static card => card.Rarity == CardRarity.Common));
        Assert.Equal(36, cards.Count(static card => card.Rarity == CardRarity.Uncommon));
        Assert.Equal(26, cards.Count(static card => card.Rarity == CardRarity.Rare));
        Assert.Equal(1, cards.Count(static card => card.Rarity == CardRarity.Ancient));
        Assert.Equal(1, cards.Count(static card => card.Rarity == CardRarity.Status));

        Assert.Equal(34, cards.Count(static card => card.Type == CardType.Attack));
        Assert.Equal(34, cards.Count(static card => card.Type == CardType.Skill));
        Assert.Equal(18, cards.Count(static card => card.Type == CardType.Power));
        Assert.Equal(1, cards.Count(static card => card.Type == CardType.Status));
    }

    [Theory]
    [MemberData(nameof(Cards))]
    public void RegisteredCardCanBeConstructedAndUpgraded(Type cardType)
    {
        var card = Assert.IsAssignableFrom<HammerCard>(Activator.CreateInstance(cardType));
        AssertCardContract(card);

        SetProperty(card, "IsMutable", true);
        var maxUpgradeLevel = Assert.IsType<int>(GetProperty(card, "MaxUpgradeLevel"));
        if (maxUpgradeLevel <= 0)
            return;

        Invoke(card, "UpgradeInternal");
        Invoke(card, "FinalizeUpgradeInternal");
        AssertCardContract(card);
    }

    [Theory]
    [MemberData(nameof(Cards))]
    public void RegisteredCardHasRequiredLocalization(Type cardType)
    {
        var prefix = $"HAMMER_MOD_CARD_{ToUpperSnakeCase(cardType.Name)}";
        var requiredKeys = new[]
        {
            $"{prefix}.title",
            $"{prefix}.description",
            $"{prefix}.smartDescription"
        };

        foreach (var localization in new[]
                 {
                     EnglishLocalization.Value,
                     ChineseLocalization.Value
                 })
        {
            foreach (var key in requiredKeys)
            {
                Assert.True(localization.TryGetValue(key, out var value), $"Missing {key}.");
                Assert.False(string.IsNullOrWhiteSpace(value), $"{key} must not be empty.");
            }
        }
    }

    [Fact]
    public void DynamicDescriptionsUseOfficialConditionalSyntax()
    {
        var combatPreviewTypes = RegisteredCardTypes
            .Where(static type => typeof(ICombatPreviewDescriptionCard).IsAssignableFrom(type))
            .ToArray();
        var targetPreviewTypes = RegisteredCardTypes
            .Where(static type => typeof(ITargetPreviewDescriptionCard).IsAssignableFrom(type))
            .ToArray();

        Assert.Equal(13, combatPreviewTypes.Length);
        Assert.Equal(3, targetPreviewTypes.Length);

        foreach (var localization in new[]
                 {
                     EnglishLocalization.Value,
                     ChineseLocalization.Value
                 })
        {
            Assert.DoesNotContain(
                localization.Keys,
                static key => key.EndsWith(".handDescription", StringComparison.Ordinal)
                    || key.EndsWith(".tierDescription", StringComparison.Ordinal));

            AssertConditionalDescriptions(localization, combatPreviewTypes, "{InCombat:\n");
            AssertConditionalDescriptions(localization, targetPreviewTypes, "{IsTargeting:\n");
        }
    }

    [Fact]
    public void ExactlySixCardsReleaseCharge()
    {
        var releaseCards = RegisteredCardTypes
            .Where(static type => typeof(IChargeReleaseCard).IsAssignableFrom(type))
            .ToArray();

        Assert.Equal(6, releaseCards.Length);
        Assert.Equal(
            new[]
            {
                typeof(MightyChargeRoll),
                typeof(ChargedUpswing),
                typeof(EarthStrike),
                typeof(ImpactCrater),
                typeof(MightyChargeBonk),
                typeof(SilkbindSpinningBludgeon)
            }.OrderBy(static type => type.Name),
            releaseCards.OrderBy(static type => type.Name));
    }

    [Fact]
    public void CardDescriptionsContainNoSemicolons()
    {
        foreach (var localization in new[]
                 {
                     EnglishLocalization.Value,
                     ChineseLocalization.Value
                 })
        {
            Assert.DoesNotContain(
                localization,
                static entry => (entry.Key.EndsWith(".description", StringComparison.Ordinal)
                        || entry.Key.EndsWith(".smartDescription", StringComparison.Ordinal))
                    && (entry.Value.Contains('；') || entry.Value.Contains(';')));
        }
    }

    [Fact]
    public void ChineseDescriptionsUseOfficialCompactSpacing()
    {
        var localizationDirectory = Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "localization",
            "zhs");

        foreach (var path in Directory.EnumerateFiles(localizationDirectory, "*.json"))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!property.Name.Contains("description", StringComparison.OrdinalIgnoreCase))
                    continue;

                var description = property.Value.GetString() ?? string.Empty;
                Assert.DoesNotContain(
                    ' ',
                    description);
            }
        }
    }

    [Fact]
    public void CardDescriptionsUseOfficialWordingConventions()
    {
        var chineseDescriptions = ChineseLocalization.Value
            .Where(static entry => entry.Key.EndsWith(".description", StringComparison.Ordinal)
                || entry.Key.EndsWith(".smartDescription", StringComparison.Ordinal))
            .Select(static entry => entry.Value)
            .ToArray();

        foreach (var description in chineseDescriptions)
        {
            Assert.DoesNotContain("若", description, StringComparison.Ordinal);
            Assert.DoesNotContain("临时力量", description, StringComparison.Ordinal);
            Assert.DoesNotContain("（当前", description, StringComparison.Ordinal);
            Assert.DoesNotContain("目标", description, StringComparison.Ordinal);
            Assert.DoesNotContain("队友", description, StringComparison.Ordinal);
        }

        var englishDescriptions = EnglishLocalization.Value
            .Where(static entry => entry.Key.EndsWith(".description", StringComparison.Ordinal)
                || entry.Key.EndsWith(".smartDescription", StringComparison.Ordinal))
            .Select(static entry => entry.Value);

        Assert.DoesNotContain(
            englishDescriptions,
            static description => description.Contains("(Currently", StringComparison.Ordinal));
    }

    [Fact]
    public void UserFacingDescriptionsAvoidTemporaryStrengthTerm()
    {
        foreach (var (language, forbiddenTerm) in new[]
                 {
                     ("eng", "temporary Strength"),
                     ("zhs", "临时力量")
                 })
        {
            foreach (var table in new[] { "cards", "relics", "potions", "powers" })
            {
                foreach (var (key, description) in ReadLocalization(language, table).Where(
                             static entry => entry.Key.EndsWith(
                                 ".description",
                                 StringComparison.Ordinal)
                                 || entry.Key.EndsWith(
                                     ".smartDescription",
                                     StringComparison.Ordinal)))
                {
                    Assert.DoesNotContain(
                        forbiddenTerm,
                        description,
                        StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }

    [Fact]
    public void CardsRelicsAndPotionsUseOfficialKeywordMarkupAndCardHoverTipMetadata()
    {
        var plainEnglishKeyword = new Regex(
            @"(?<!\[gold\])(?<!\{)\b(?:Release Charge|Draw Pile|Discard Pile|Back on Your Feet|Stunned|Stun|Charge|Regeneration|Vulnerable|Frail|Strength|Dexterity|Thorns|Weak|Block|Replay|Hand)\b(?!\[/gold\])",
            RegexOptions.CultureInvariant);
        var plainChineseKeyword = new Regex(
            @"(?<!\[gold\])(?:释放蓄力|抽牌堆|弃牌堆|倒地起身|临时力量|击晕|晕眩|蓄力|力量|敏捷|虚弱|易伤|脆弱|再生|荆棘|(?<!未)(?<!未被)格挡|手牌|重放)(?!\[/gold\])",
            RegexOptions.CultureInvariant);
        var nestedGoldTag = new Regex(
            @"\[gold\](?:(?!\[/gold\]).)*\[gold\]",
            RegexOptions.CultureInvariant | RegexOptions.Singleline);
        var goldTagInDynamicVarName = new Regex(
            @"\{[A-Za-z0-9_]*\[gold\]",
            RegexOptions.CultureInvariant);

        foreach (var (language, plainKeyword) in new[]
                 {
                     ("eng", plainEnglishKeyword),
                     ("zhs", plainChineseKeyword)
                 })
        {
            foreach (var table in new[] { "cards", "relics", "potions" })
            {
                var localization = ReadLocalization(language, table);
                foreach (var (key, description) in localization.Where(
                             static entry => entry.Key.EndsWith(
                                     ".description",
                                     StringComparison.Ordinal)
                                 || entry.Key.EndsWith(
                                     ".smartDescription",
                                     StringComparison.Ordinal)))
                {
                    Assert.Equal(
                        Regex.Matches(description, @"\[gold\]").Count,
                        Regex.Matches(description, @"\[/gold\]").Count);
                    Assert.False(
                        nestedGoldTag.IsMatch(description),
                        $"Nested gold tag in {key}.");
                    Assert.False(
                        goldTagInDynamicVarName.IsMatch(description),
                        $"Gold tag corrupts a dynamic variable in {key}.");
                    Assert.False(
                        plainKeyword.IsMatch(description),
                        $"Unmarked keyword in {key}: {plainKeyword.Match(description).Value}");
                }
            }
        }

        foreach (var cardType in AllRegisteredCardTypes)
        {
            var prefix = $"HAMMER_MOD_CARD_{ToUpperSnakeCase(cardType.Name)}";
            var description = EnglishLocalization.Value[$"{prefix}.description"];
            var card = Assert.IsAssignableFrom<CardModel>(Activator.CreateInstance(cardType));

            if (card is not HammerCard hammerCard)
            {
                if (card is Charge)
                    Assert.Contains("[gold]Charge[/gold]", description, StringComparison.Ordinal);
                if (description.Contains("[gold]Block[/gold]", StringComparison.Ordinal))
                    Assert.True(card.GainsBlock, $"{cardType.Name} needs a Block hover tip.");
                continue;
            }

            var mechanics = HammerCardHoverTips.GetMechanics(hammerCard);
            AssertMechanicForMarkup(description, "[gold]Charge[/gold]", mechanics, HammerCardMechanic.Charge, cardType);
            AssertMechanicForMarkup(description, "[gold]Stun[/gold]", mechanics, HammerCardMechanic.Stun, cardType);
            AssertMechanicForMarkup(description, "[gold]Stunned[/gold]", mechanics, HammerCardMechanic.Stunned, cardType);
            AssertMechanicForMarkup(description, "[gold]Release Charge[/gold]", mechanics, HammerCardMechanic.ChargeRelease, cardType);
            AssertMechanicForMarkup(description, "[gold]Strength[/gold]", mechanics, HammerCardMechanic.Strength, cardType);
            AssertMechanicForMarkup(description, "[gold]Dexterity[/gold]", mechanics, HammerCardMechanic.Dexterity, cardType);
            AssertMechanicForMarkup(description, "[gold]Weak[/gold]", mechanics, HammerCardMechanic.Weak, cardType);
            AssertMechanicForMarkup(description, "[gold]Vulnerable[/gold]", mechanics, HammerCardMechanic.Vulnerable, cardType);
            AssertMechanicForMarkup(description, "[gold]Frail[/gold]", mechanics, HammerCardMechanic.Frail, cardType);
            AssertMechanicForMarkup(description, "[gold]Regeneration[/gold]", mechanics, HammerCardMechanic.Regeneration, cardType);
            AssertMechanicForMarkup(description, "[gold]Thorns[/gold]", mechanics, HammerCardMechanic.Thorns, cardType);
            AssertMechanicForMarkup(description, "[gold]Replay[/gold]", mechanics, HammerCardMechanic.Replay, cardType);
            AssertMechanicForMarkup(description, "[gold]Back on Your Feet[/gold]", mechanics, HammerCardMechanic.BackOnYourFeet, cardType);

            if (description.Contains("[gold]Block[/gold]", StringComparison.Ordinal))
            {
                Assert.True(
                    card.GainsBlock || mechanics.HasFlag(HammerCardMechanic.Block),
                    $"{cardType.Name} needs a Block hover tip.");
            }
        }

        foreach (var language in new[] { "eng", "zhs" })
        {
            var keywords = ReadLocalization(language, "card_keywords");
            foreach (var keyword in new[] { "CHARGE", "CHARGE_RELEASE" })
            {
                Assert.False(string.IsNullOrWhiteSpace(
                    keywords[$"HAMMER_MOD_KEYWORD_{keyword}.title"]));
                Assert.False(string.IsNullOrWhiteSpace(
                    keywords[$"HAMMER_MOD_KEYWORD_{keyword}.description"]));
            }

            Assert.DoesNotContain("HAMMER_MOD_KEYWORD_STUN.title", keywords.Keys);
            Assert.DoesNotContain("HAMMER_MOD_KEYWORD_STUN.description", keywords.Keys);
        }

        Assert.Equal(StaticHoverTip.Stun, HammerCardHoverTips.StunHoverTip);

        Assert.Equal(
            HammerCardMechanic.Stun,
            new DownedPursuitCharm().HoverTipMechanics);
        Assert.Equal(
            HammerCardMechanic.Stun,
            new FlashBomb().HoverTipMechanics);
        Assert.Equal(
            HammerCardMechanic.Strength | HammerCardMechanic.Dexterity,
            new SlidingBoostJewel().HoverTipMechanics);
        Assert.Equal(
            HammerCardMechanic.Strength
            | HammerCardMechanic.Block,
            new CounterstrikeCharm().HoverTipMechanics);
        Assert.Equal(
            HammerCardMechanic.Strength,
            new MightSeed().HoverTipMechanics);
        Assert.Equal(
            HammerCardMechanic.Vulnerable,
            new Pitfall().HoverTipMechanics);
    }

    [Fact]
    public void EnergyGainDescriptionsUseOfficialEnergyIcons()
    {
        var cardPrefixes = new[]
        {
            "HAMMER_MOD_CARD_DEEP_BREATH",
            "HAMMER_MOD_CARD_ENDLESS_MOMENTUM",
            "HAMMER_MOD_CARD_FELYNE_KO_TECHNIQUE",
            "HAMMER_MOD_CARD_LAUNCH_TEAMMATE",
            "HAMMER_MOD_CARD_LEVERAGED_SWING",
            "HAMMER_MOD_CARD_HAMMER_IAI",
            "HAMMER_MOD_CARD_VICTORY_CHARGE",
            "HAMMER_MOD_CARD_WIREBUG_CONTINUATION"
        };

        foreach (var language in new[] { "eng", "zhs" })
        {
            var cards = ReadLocalization(language);
            foreach (var prefix in cardPrefixes)
            {
                Assert.Contains(":energyIcons(", cards[$"{prefix}.description"], StringComparison.Ordinal);
                Assert.Contains(":energyIcons(", cards[$"{prefix}.smartDescription"], StringComparison.Ordinal);
            }

            var powers = ReadLocalization(language, "powers");
            Assert.Contains(
                ":energyIcons(",
                powers["HAMMER_MOD_POWER_ENDLESS_MOMENTUM_POWER.description"],
                StringComparison.Ordinal);

            var relics = ReadLocalization(language, "relics");
            Assert.Contains(
                ":energyIcons(",
                relics["HAMMER_MOD_RELIC_WIREBUG_CAGE.description"],
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ChargeTierUpgradeValuesAreUpdatedAndMarkedForHighlighting()
    {
        AssertChargeTierUpgrade<EarthStrike>("DamageAt", [15, 20, 27, 36]);
        AssertChargeTierUpgrade<EarthStrike>("StunAt", [2, 3, 6, 8]);
        AssertChargeTierUpgrade<MightyChargeBonk>("DamageAt", [20, 28, 38, 50]);
        AssertChargeTierUpgrade<MightyChargeRoll>("BlockAt", [12, 18, 26, 32]);
        AssertChargeTierUpgrade<ChargedUpswing>("StunAt", [5, 9, 14, 20]);
        AssertChargeTierUpgrade<ImpactCrater>("DamageAt", [18, 22, 30, 40]);
        AssertChargeTierUpgrade<ImpactCrater>("StunAt", [5, 9, 14, 20]);
    }

    [Fact]
    public void RedesignedCardsKeepTheirRequestedCostsValuesAndKeywords()
    {
        Assert.Equal(
            "Borrowed Momentum",
            EnglishLocalization.Value["HAMMER_MOD_CARD_CHARGE_STEP.title"]);
        Assert.Equal(
            "Charge Step",
            EnglishLocalization.Value["HAMMER_MOD_CARD_KEEPING_SWAY.title"]);
        Assert.Equal(
            "借力蓄势",
            ChineseLocalization.Value["HAMMER_MOD_CARD_CHARGE_STEP.title"]);
        Assert.Equal(
            "蓄力垫步",
            ChineseLocalization.Value["HAMMER_MOD_CARD_KEEPING_SWAY.title"]);

        AssertCardValues<ChargedOverheadSmash>(1, ("NormalDamage", 9), ("ChargedDamage", 18));
        AssertUpgradedCardValues<ChargedOverheadSmash>(1, ("NormalDamage", 12), ("ChargedDamage", 24));

        AssertCardValues<SwitchGripSwing>(1, ("Damage", 9), ("Cards", 1));
        AssertUpgradedCardValues<SwitchGripSwing>(1, ("Damage", 10), ("Cards", 2));

        AssertCardValues<SweepThePath>(1, ("Damage", 6), ("Cards", 1));
        AssertUpgradedCardValues<SweepThePath>(1, ("Damage", 9), ("Cards", 1));

        AssertCardValues<ChargedSideSmash>(1, ("NormalDamage", 6), ("ChargedDamage", 12));
        AssertUpgradedCardValues<ChargedSideSmash>(1, ("NormalDamage", 8), ("ChargedDamage", 16));

        AssertCardValues<KeepingSway>(1, ("Block", 5), ("FullBlock", 15));
        AssertUpgradedCardValues<KeepingSway>(1, ("Block", 8), ("FullBlock", 18));

        AssertCardValues<BraceWithTheHammer>(1, ("NormalBlock", 8), ("ChargedBlock", 16));
        AssertUpgradedCardValues<BraceWithTheHammer>(1, ("NormalBlock", 11), ("ChargedBlock", 22));

        AssertCardValues<EmergencyEvade>(1, ("Block", 13), ("ChargeLoss", 1));
        AssertUpgradedCardValues<EmergencyEvade>(1, ("Block", 17), ("ChargeLoss", 1));

        AssertCardValues<WirebugSpin>(0, ("Damage", 3), ("BaseHits", 2), ("Cards", 1));
        AssertUpgradedCardValues<WirebugSpin>(0, ("Damage", 3), ("BaseHits", 2), ("Cards", 2));

        AssertCardValues<AffinitySliding>(0, ("NormalStrength", 2), ("ChargedStrength", 4));
        AssertUpgradedCardValues<AffinitySliding>(0, ("NormalStrength", 4), ("ChargedStrength", 6));

        AssertCardValues<LegSweepHammer>(1, ("Damage", 6), ("WeakPower", 1));
        AssertUpgradedCardValues<LegSweepHammer>(1, ("Damage", 8), ("WeakPower", 2));

        AssertCardValues<ChargeStep>(1, ("Block", 8), ("Charge", 2));
        AssertUpgradedCardValues<ChargeStep>(1, ("Block", 11), ("Charge", 2));

        AssertCardValues<BluntWeaponExpert>(
            0,
            ("Damage", 6),
            ("Stun", 3),
            ("Block", 5),
            ("Charge", 1));
        AssertUpgradedCardValues<BluntWeaponExpert>(
            0,
            ("Damage", 9),
            ("Stun", 4),
            ("Block", 8),
            ("Charge", 2));

        AssertCardValues<ChargeAsYouStrike>(1, ("Damage", 12), ("Charge", 2));
        AssertUpgradedCardValues<ChargeAsYouStrike>(1, ("Damage", 16), ("Charge", 2));

        AssertCardValues<StaminaDrainingHammer>(
            1,
            ("Damage", 4),
            ("Stun", 10),
            ("WeakPower", 1),
            ("VulnerablePower", 1));
        AssertUpgradedCardValues<StaminaDrainingHammer>(
            1,
            ("Damage", 6),
            ("Stun", 15),
            ("WeakPower", 2),
            ("VulnerablePower", 2));

        AssertCardValues<FindASlope>(0, ("Cards", 2));
        AssertUpgradedCardValues<FindASlope>(0, ("Cards", 3));
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(new FindASlope()));
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(CreateUpgradedCard<FindASlope>()));

        AssertCardValues<Wirefall>(0);
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(new Wirefall()));
        Assert.DoesNotContain(CardKeyword.Exhaust, GetKeywords(CreateUpgradedCard<Wirefall>()));

        AssertCardValues<Farcaster>(3);
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(new Farcaster()));
        Assert.DoesNotContain(CardKeyword.Exhaust, GetKeywords(CreateUpgradedCard<Farcaster>()));

        AssertCardValues<Coalescence>(3, ("MaxReduction", 5), ("StrengthPower", 1));
        AssertUpgradedCardValues<Coalescence>(3, ("MaxReduction", 5), ("StrengthPower", 2));

        AssertCardValues<FreeMeal>(2);
        AssertUpgradedCardValues<FreeMeal>(1);
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(new FreeMeal()));
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(CreateUpgradedCard<FreeMeal>()));

        AssertCardValues<LuckyVoucher>(2);
        AssertUpgradedCardValues<LuckyVoucher>(1);

        var bluntWeaponExpertDescription =
            ChineseLocalization.Value["HAMMER_MOD_CARD_BLUNT_WEAPON_EXPERT.description"];
        AssertOrderedText(
            bluntWeaponExpertDescription,
            "{Charge:diff()}",
            "{Block:diff()}",
            "{Damage:diff()}",
            "{Stun:diff()}");

        AssertOrderedText(
            ChineseLocalization.Value["HAMMER_MOD_CARD_CHARGE_AS_YOU_STRIKE.description"],
            "{Damage:diff()}",
            "{IfUpgraded:show:");
        AssertOrderedText(
            ChineseLocalization.Value["HAMMER_MOD_CARD_STAMINA_DRAINING_HAMMER.description"],
            "{Damage:diff()}",
            "{Stun:diff()}",
            "{WeakPower:diff()}",
            "{VulnerablePower:diff()}");

        AssertCardValues<DashJuice>(1, ("Block", 3));
        AssertUpgradedCardValues<DashJuice>(1, ("Block", 4));
        Assert.DoesNotContain(CardKeyword.Innate, GetKeywords(CreateUpgradedCard<DashJuice>()));

        AssertCardValues<InvincibleWindFireWheel>(2, ("Damage", 3), ("BaseHits", 4), ("StunPerHit", 2));
        AssertUpgradedCardValues<InvincibleWindFireWheel>(2, ("Damage", 3), ("BaseHits", 6), ("StunPerHit", 2));

        AssertCardValues<MightyUpswing>(2, ("Damage", 14), ("Stun", 8));
        AssertUpgradedCardValues<MightyUpswing>(2, ("Damage", 18), ("Stun", 10));

        AssertCardValues<HammerForHammer>(2, ("Damage", 8));
        AssertUpgradedCardValues<HammerForHammer>(2, ("Damage", 10));

        AssertCardValues<SteadierWithEverySpin>(0, ("BlockPerEnergy", 7), ("ExcessBlock", 3));
        AssertUpgradedCardValues<SteadierWithEverySpin>(0, ("BlockPerEnergy", 9), ("ExcessBlock", 4));

        AssertCardValues<TrueSpinningImpact>(0, ("Damage", 8), ("BonusHits", 0), ("StunPerEnergy", 2));
        AssertUpgradedCardValues<TrueSpinningImpact>(0, ("Damage", 8), ("BonusHits", 1), ("StunPerEnergy", 2));

        AssertCardValues<FocusBlowEarthquake>(0, ("Damage", 3), ("Stun", 1), ("VulnerablePower", 1));
        AssertUpgradedCardValues<FocusBlowEarthquake>(0, ("Damage", 5), ("Stun", 2), ("VulnerablePower", 2));

        AssertCardValues<DeepBreath>(0, ("Energy", 1), ("AttackEnergy", 2));
        AssertUpgradedCardValues<DeepBreath>(0, ("Energy", 1), ("AttackEnergy", 3));

        AssertCardValues<ImpactBurst>(1, ("StunPerHit", 1));
        AssertUpgradedCardValues<ImpactBurst>(1, ("StunPerHit", 2));

        AssertCardValues<EarthsplitterShock>(3, ("Damage", 24), ("Stun", 10));
        AssertUpgradedCardValues<EarthsplitterShock>(3, ("Damage", 32), ("Stun", 13));

        AssertCardValues<LaunchTeammate>(1, ("Block", 8), ("Energy", 1), ("StrengthPower", 2));
        AssertUpgradedCardValues<LaunchTeammate>(1, ("Block", 10), ("Energy", 1), ("StrengthPower", 2));

        AssertCardValues<UnloadingStance>(1, ("Block", 5), ("StrengthLoss", 3));
        AssertUpgradedCardValues<UnloadingStance>(1, ("Block", 7), ("StrengthLoss", 5));

        AssertCardValues<ConcussionGuard>(0);

        AssertCardValues<VictoryCharge>(0, ("Energy", 2), ("Cards", 2));
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(new VictoryCharge()));
        Assert.DoesNotContain(CardKeyword.Retain, GetKeywords(new VictoryCharge()));
        var upgradedVictoryCharge = CreateUpgradedCard<VictoryCharge>();
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(upgradedVictoryCharge));
        Assert.Contains(CardKeyword.Retain, GetKeywords(upgradedVictoryCharge));

        AssertCardValues<Overcharge>(2);
        AssertUpgradedCardValues<Overcharge>(2);
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(new Overcharge()));
        Assert.DoesNotContain(
            CardKeyword.Exhaust,
            GetKeywords(CreateUpgradedCard<Overcharge>()));

        AssertCardValues<Focus>(1, ("FullCards", 1));
        Assert.DoesNotContain(CardKeyword.Innate, GetKeywords(new Focus()));
        var upgradedFocus = CreateUpgradedCard<Focus>();
        AssertCardValues(upgradedFocus, 1, [("FullCards", 2)]);
        Assert.DoesNotContain(CardKeyword.Innate, GetKeywords(upgradedFocus));

        AssertCardValues<ChargeSwitchStrength>(1, ("StrengthPower", 0));
        Assert.DoesNotContain(CardKeyword.Innate, GetKeywords(new ChargeSwitchStrength()));
        var upgradedChargeSwitchStrength = CreateUpgradedCard<ChargeSwitchStrength>();
        AssertCardValues(upgradedChargeSwitchStrength, 1, [("StrengthPower", 0)]);
        Assert.Contains(CardKeyword.Innate, GetKeywords(upgradedChargeSwitchStrength));

        AssertCardValues<EndlessMomentum>(1, ("Energy", 1));
        AssertUpgradedCardValues<EndlessMomentum>(1, ("Energy", 2));

        AssertCardValues<WirebugContinuation>(2, ("RequiredEnergy", 2), ("Charge", 1));
        AssertUpgradedCardValues<WirebugContinuation>(2, ("RequiredEnergy", 2), ("Charge", 2));

        AssertCardValues<Partbreaker>(2, ("VulnerablePower", 1));
        AssertUpgradedCardValues<Partbreaker>(1, ("VulnerablePower", 1));

        AssertCardValues<HandCrankedTractor>(2, ("Replay", 1));
        Assert.Equal(StaticHoverTip.ReplayStatic, HandCrankedTractor.ReplayHoverTip);
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(new HandCrankedTractor()));
        Assert.DoesNotContain(
            CardKeyword.Exhaust,
            GetKeywords(CreateUpgradedCard<HandCrankedTractor>()));

        AssertCardValues<RecoveryMedicine>(1, ("RegenPower", 5));
        AssertUpgradedCardValues<RecoveryMedicine>(1, ("RegenPower", 5));
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(new RecoveryMedicine()));
        Assert.DoesNotContain(CardKeyword.Retain, GetKeywords(new RecoveryMedicine()));
        var upgradedRecoveryMedicine = CreateUpgradedCard<RecoveryMedicine>();
        Assert.Contains(CardKeyword.Exhaust, GetKeywords(upgradedRecoveryMedicine));
        Assert.Contains(CardKeyword.Retain, GetKeywords(upgradedRecoveryMedicine));

        AssertCardValues<HarderWithEverySmash>(3, ("StrengthPower", 3));
        AssertUpgradedCardValues<HarderWithEverySmash>(2, ("StrengthPower", 3));

        AssertCardValues<BloodRite>(2, ("DamageStep", 10), ("Healing", 1));
        AssertUpgradedCardValues<BloodRite>(1, ("DamageStep", 10), ("Healing", 1));
    }

    [Fact]
    public void RecoveryMedicineAllowsOnlyNonAttacksAndZeroDamageAttacks()
    {
        Assert.True(RecoveryMedicine.CanPlayWhenNoEnemyDealsAttackDamage([]));
        Assert.True(RecoveryMedicine.CanPlayWhenNoEnemyDealsAttackDamage([0, 0]));
        Assert.True(RecoveryMedicine.CanPlayWhenNoEnemyDealsAttackDamage([0, -1]));
        Assert.False(RecoveryMedicine.CanPlayWhenNoEnemyDealsAttackDamage([0, 1]));
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void IaiRequiresAtLeastOneCharge(int charge, bool expected)
    {
        Assert.Equal(expected, HammerIai.HasRequiredCharge(charge));
    }

    [Theory]
    [InlineData(0, 9, 12, 6, 8, 8, 11)]
    [InlineData(1, 9, 12, 6, 8, 8, 11)]
    [InlineData(2, 18, 24, 12, 16, 16, 22)]
    [InlineData(3, 18, 24, 12, 16, 16, 22)]
    public void ChargedThresholdCardsDoubleTheirBaseValueAtTwoCharge(
        int charge,
        int overhead,
        int upgradedOverhead,
        int side,
        int upgradedSide,
        int block,
        int upgradedBlock)
    {
        Assert.Equal(overhead, ChargedOverheadSmash.ResolveDamage(charge, upgraded: false));
        Assert.Equal(upgradedOverhead, ChargedOverheadSmash.ResolveDamage(charge, upgraded: true));
        Assert.Equal(side, ChargedSideSmash.ResolveDamage(charge, upgraded: false));
        Assert.Equal(upgradedSide, ChargedSideSmash.ResolveDamage(charge, upgraded: true));
        Assert.Equal(block, BraceWithTheHammer.ResolveBlock(charge, upgraded: false));
        Assert.Equal(upgradedBlock, BraceWithTheHammer.ResolveBlock(charge, upgraded: true));
    }

    [Fact]
    public void RedesignedContentKeepsTargetingAndRarityContracts()
    {
        var pitfall = new Pitfall();
        Assert.Equal(PotionRarity.Rare, pitfall.Rarity);
        Assert.Equal(TargetType.AnyEnemy, pitfall.TargetType);

        var recoveryMedicine = new RecoveryMedicine();
        Assert.Equal(CardType.Skill, recoveryMedicine.Type);
        Assert.Equal(CardRarity.Uncommon, recoveryMedicine.Rarity);
        Assert.Equal(TargetType.Self, recoveryMedicine.TargetType);
        Assert.False(recoveryMedicine.CanBeGeneratedInCombat);

        Assert.Equal(CardRarity.Rare, new WeaknessExploit().Rarity);

        var focusBlowEarthquake = new FocusBlowEarthquake();
        Assert.Equal(CardType.Attack, focusBlowEarthquake.Type);
        Assert.Equal(CardRarity.Uncommon, focusBlowEarthquake.Rarity);

        var affinitySliding = new AffinitySliding();
        Assert.Equal(CardType.Skill, affinitySliding.Type);
        Assert.Equal(CardRarity.Uncommon, affinitySliding.Rarity);
        Assert.Equal(TargetType.Self, affinitySliding.TargetType);

        Assert.Equal(TargetType.Self, new UnloadingStance().TargetType);

        Assert.Equal(RelicRarity.Rare, new CounterstrikeCharm().Rarity);
        Assert.Equal(RelicRarity.Common, new DownedPursuitCharm().Rarity);
    }

    [Fact]
    public void RedesignedDescriptionsUseCurrentDynamicVariables()
    {
        var multilineCards = new[]
        {
            "BIG_BANG_COMBO",
            "EMERGENCY_EVADE",
            "FACE_OFF",
            "FOCUS_BLOW_EARTHQUAKE",
            "HOME_RUN_SWING",
            "WATER_STRIKE",
            "LEVERAGED_SWING",
            "LEG_SWEEP_HAMMER",
            "DEEP_BREATH",
            "UNLOADING_STANCE",
            "WEAVE_AND_BONK",
            "CHARGE_STEP",
            "HAMMER_FOR_HAMMER",
            "BREAK_MOMENTUM",
            "WAKE_UP_HIT",
            "SWITCH_GRIP_SWING",
            "TOOL_SPECIALIST",
            "IMPACT_CRATER",
            "SHELL_BREAKER",
            "REPOSITION",
            "LAUNCH_TEAMMATE",
            "WIREBUG_SPIN",
            "AFFINITY_SLIDING",
            "SWEEP_THE_PATH",
            "INVINCIBLE_WIND_FIRE_WHEEL",
            "TRUE_SPINNING_IMPACT",
            "WARM_UP_EXERCISE"
        };

        foreach (var localization in new[]
                 {
                     EnglishLocalization.Value,
                     ChineseLocalization.Value
                 })
        {
            var offset = localization["HAMMER_MOD_CARD_HAMMER_FOR_HAMMER.description"];
            Assert.Contains("{Damage:diff()}", offset, StringComparison.Ordinal);
            Assert.DoesNotContain("{BaseDamage", offset, StringComparison.Ordinal);
            Assert.DoesNotContain("{DamagePerAttack", offset, StringComparison.Ordinal);

            var headHunter = localization["HAMMER_MOD_CARD_SMASH_THAT_HEAD.description"];
            Assert.Contains("{Damage:diff()}", headHunter, StringComparison.Ordinal);
            Assert.Contains("{StunMultiplier:diff()}", headHunter, StringComparison.Ordinal);
            Assert.DoesNotContain("{PrimedDamage", headHunter, StringComparison.Ordinal);

            var tractor = localization["HAMMER_MOD_CARD_HAND_CRANKED_TRACTOR.description"];
            Assert.Contains("{Replay:diff()}", tractor, StringComparison.Ordinal);

            Assert.DoesNotContain(
                "Backlash",
                localization["HAMMER_MOD_CARD_OVERCHARGE.description"],
                StringComparison.Ordinal);
            Assert.Contains(
                "{IfUpgraded:show:",
                localization["HAMMER_MOD_CARD_CHARGE_AS_YOU_STRIKE.description"],
                StringComparison.Ordinal);
            Assert.Contains(
                "{IfUpgraded:show:\n",
                localization["HAMMER_MOD_CARD_FELYNE_KO_TECHNIQUE.description"],
                StringComparison.Ordinal);
            Assert.Contains(
                "{BonusStun:diff()}",
                localization["HAMMER_MOD_CARD_FELYNE_KO_TECHNIQUE.description"],
                StringComparison.Ordinal);
            Assert.Contains(
                "{StunPerHit:diff()}",
                localization["HAMMER_MOD_CARD_IMPACT_BURST.description"],
                StringComparison.Ordinal);
            Assert.Contains(
                "{IfUpgraded:show:X+1|X}",
                localization["HAMMER_MOD_CARD_TRUE_SPINNING_IMPACT.description"],
                StringComparison.Ordinal);

            Assert.Contains(
                "{Cards:diff()}",
                localization["HAMMER_MOD_CARD_SWITCH_GRIP_SWING.description"],
                StringComparison.Ordinal);
            Assert.Contains(
                "{Cards:diff()}",
                localization["HAMMER_MOD_CARD_WIREBUG_SPIN.description"],
                StringComparison.Ordinal);

            Assert.DoesNotContain("HAMMER_MOD_CARD_OVERCHARGE.backlashDescription", localization.Keys);
            Assert.DoesNotContain("HAMMER_MOD_CARD_FELYNE_KO_TECHNIQUE.upgradeDescription", localization.Keys);
            Assert.DoesNotContain("HAMMER_MOD_CARD_IMPACT_BURST.upgradeDescription", localization.Keys);
            Assert.DoesNotContain("HAMMER_MOD_CARD_TRUE_SPINNING_IMPACT.upgradeDescription", localization.Keys);

            var recoveryMedicine = localization["HAMMER_MOD_CARD_RECOVERY_MEDICINE.description"];
            Assert.Contains("{RegenPower:diff()}", recoveryMedicine, StringComparison.Ordinal);
            Assert.Contains('\n', recoveryMedicine);

            foreach (var card in multilineCards)
            {
                Assert.Contains(
                    '\n',
                    localization[$"HAMMER_MOD_CARD_{card}.description"]);
            }
        }
    }

    [Fact]
    public void RedesignedRelicsPotionsAndPowersUseCurrentLocalizationVariables()
    {
        foreach (var language in new[] { "eng", "zhs" })
        {
            var relics = ReadLocalization(language, "relics");
            foreach (var key in new[]
                     {
                         "HAMMER_MOD_RELIC_SLIDING_BOOST_JEWEL.description",
                         "HAMMER_MOD_RELIC_COUNTERSTRIKE_CHARM.description",
                         "HAMMER_MOD_RELIC_EVASION_MANTLE.description"
                     })
            {
                Assert.Contains("{StrengthPower:diff()}", relics[key], StringComparison.Ordinal);
                Assert.DoesNotContain("{Strength}", relics[key], StringComparison.Ordinal);
            }
            Assert.Contains(
                "{DexterityPower:diff()}",
                relics["HAMMER_MOD_RELIC_SLIDING_BOOST_JEWEL.description"],
                StringComparison.Ordinal);

            var potions = ReadLocalization(language, "potions");
            Assert.Contains("{StrengthPower:diff()}",
                potions["HAMMER_MOD_POTION_MIGHT_SEED.description"],
                StringComparison.Ordinal);
            Assert.Contains("{VulnerablePower:diff()}",
                potions["HAMMER_MOD_POTION_PITFALL.description"],
                StringComparison.Ordinal);

            var powers = ReadLocalization(language, "powers");
            Assert.Contains(
                "{Threshold}",
                powers["HAMMER_MOD_POWER_HAMMER_STUN_POWER.description"],
                StringComparison.Ordinal);
            foreach (var key in new[]
                     {
                         "HAMMER_MOD_POWER_COUNTERSTRIKE_STRENGTH_POWER.description",
                         "HAMMER_MOD_POWER_MIGHT_SEED_POWER.description"
                     })
            {
                Assert.False(string.IsNullOrWhiteSpace(powers[key]));
            }
        }
    }

    private static void AssertCardContract(HammerCard card)
    {
        Assert.NotNull(GetProperty(card, "Type"));
        Assert.NotNull(GetProperty(card, "Rarity"));
        Assert.NotNull(GetProperty(card, "TargetType"));
        Assert.NotNull(GetProperty(card, "DynamicVars"));
        Assert.NotNull(GetProperty(card, "EnergyCost"));

        var canonicalCost = Assert.IsType<int>(GetProperty(card, "CanonicalEnergyCost"));
        Assert.InRange(canonicalCost, 0, 99);

        Assert.IsAssignableFrom<IEnumerable>(GetProperty(card, "CanonicalKeywords"));
        Assert.IsAssignableFrom<IEnumerable>(GetProperty(card, "Keywords"));
    }

    private static void AssertMechanicForMarkup(
        string description,
        string markup,
        HammerCardMechanic mechanics,
        HammerCardMechanic expectedMechanic,
        Type cardType)
    {
        if (!description.Contains(markup, StringComparison.Ordinal))
            return;

        Assert.True(
            mechanics.HasFlag(expectedMechanic),
            $"{cardType.Name} uses {markup} without its hover-tip metadata.");
    }

    private static void AssertOrderedText(string text, params string[] fragments)
    {
        var previousIndex = -1;
        foreach (var fragment in fragments)
        {
            var index = text.IndexOf(fragment, StringComparison.Ordinal);
            Assert.True(index > previousIndex, $"Expected '{fragment}' after index {previousIndex} in: {text}");
            previousIndex = index;
        }
    }

    private static object GetProperty(object instance, string propertyName)
    {
        var property = FindProperty(instance.GetType(), propertyName)
            ?? throw new MissingMemberException(instance.GetType().FullName, propertyName);
        return property.GetValue(instance)
            ?? throw new InvalidOperationException(
                $"{instance.GetType().Name}.{propertyName} returned null.");
    }

    private static void AssertChargeTierUpgrade<TCard>(
        string prefix,
        IReadOnlyList<int> expectedValues)
        where TCard : HammerCard, new()
    {
        var card = new TCard();
        SetProperty(card, "IsMutable", true);
        Invoke(card, "UpgradeInternal");

        var dynamicVars = Assert.IsType<DynamicVarSet>(GetProperty(card, "DynamicVars"));
        for (var tier = 0; tier < expectedValues.Count; tier++)
        {
            var dynamicVar = dynamicVars[$"{prefix}{tier}"];
            Assert.Equal(expectedValues[tier], dynamicVar.BaseValue);
            Assert.True(dynamicVar.WasJustUpgraded);
            Assert.Contains(
                "green",
                dynamicVar.ToHighlightedString(inverse: false),
                StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void AssertCardValues<TCard>(
        int expectedCost,
        params (string Name, int Value)[] expectedVars)
        where TCard : HammerCard, new()
    {
        AssertCardValues(new TCard(), expectedCost, expectedVars);
    }

    private static void AssertUpgradedCardValues<TCard>(
        int expectedCost,
        params (string Name, int Value)[] expectedVars)
        where TCard : HammerCard, new()
    {
        AssertCardValues(CreateUpgradedCard<TCard>(), expectedCost, expectedVars);
    }

    private static void AssertCardValues(
        HammerCard card,
        int expectedCost,
        IEnumerable<(string Name, int Value)> expectedVars)
    {
        var energyCost = Assert.IsType<CardEnergyCost>(GetProperty(card, "EnergyCost"));
        Assert.Equal(expectedCost, energyCost.GetWithModifiers(CostModifiers.None));
        var dynamicVars = Assert.IsType<DynamicVarSet>(GetProperty(card, "DynamicVars"));
        foreach (var (name, value) in expectedVars)
            Assert.Equal(value, dynamicVars[name].BaseValue);
    }

    private static TCard CreateUpgradedCard<TCard>()
        where TCard : HammerCard, new()
    {
        var card = new TCard();
        SetProperty(card, "IsMutable", true);
        Invoke(card, "UpgradeInternal");
        Invoke(card, "FinalizeUpgradeInternal");
        return card;
    }

    private static IReadOnlyCollection<CardKeyword> GetKeywords(HammerCard card)
    {
        return Assert.IsAssignableFrom<IEnumerable<CardKeyword>>(GetProperty(card, "Keywords"))
            .ToArray();
    }

    private static void SetProperty(object instance, string propertyName, object value)
    {
        var property = FindProperty(instance.GetType(), propertyName)
            ?? throw new MissingMemberException(instance.GetType().FullName, propertyName);
        property.SetValue(instance, value);
    }

    private static PropertyInfo? FindProperty(Type type, string propertyName)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            var property = current.GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly);
            if (property is not null)
                return property;
        }

        return null;
    }

    private static void Invoke(object instance, string methodName)
    {
        for (var current = instance.GetType(); current is not null; current = current.BaseType)
        {
            var method = current.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);
            if (method is null)
                continue;

            try
            {
                method.Invoke(instance, null);
                return;
            }
            catch (TargetInvocationException exception) when (exception.InnerException is not null)
            {
                throw exception.InnerException;
            }
        }

        throw new MissingMethodException(instance.GetType().FullName, methodName);
    }

    private static void AssertConditionalDescriptions(
        IReadOnlyDictionary<string, string> localization,
        IEnumerable<Type> cardTypes,
        string conditional)
    {
        foreach (var cardType in cardTypes)
        {
            var prefix = $"HAMMER_MOD_CARD_{ToUpperSnakeCase(cardType.Name)}";
            var description = localization[$"{prefix}.description"];
            var smartDescription = localization[$"{prefix}.smartDescription"];

            Assert.Contains(conditional, description, StringComparison.Ordinal);
            Assert.Equal(description, smartDescription);
        }
    }

    private static IReadOnlyDictionary<string, string> ReadLocalization(
        string language,
        string table = "cards")
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "localization",
            language,
            $"{table}.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement
            .EnumerateObject()
            .ToDictionary(
                static property => property.Name,
                static property => property.Value.GetString() ?? string.Empty,
                StringComparer.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "HammerMod.csproj")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException(
            $"Could not find HammerMod.csproj above {AppContext.BaseDirectory}.");
    }

    private static string ToUpperSnakeCase(string value)
    {
        return WordBoundaryRegex().Replace(value, "$1_$2").ToUpperInvariant();
    }

    [GeneratedRegex("([a-z0-9])([A-Z])", RegexOptions.CultureInvariant)]
    private static partial Regex WordBoundaryRegex();
}
