using Godot;
using HammerMod.Characters;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace HammerMod.Gameplay;

public static class HammerResources
{
    public const string ChargeLocalId = "charge";
    public const int MaxCharge = 3;
    private const string ChargeCounterLocalId = "charge_counter";

    private static readonly SecondaryResourceCounterStyle ChargeCounterStyle = new()
    {
        CounterSize = new Vector2(96f, 96f),
        IconSize = new Vector2(88f, 88f),
        FontSize = 52,
        OutlineSize = 12,
        PositiveColor = new Color(1f, 0.91f, 0.63f),
        ZeroColor = new Color(0.85f, 0.32f, 0.29f),
        OutlineColor = new Color(0.11f, 0.05f, 0.03f),
        AmountLabelOffset = new Vector2(0f, -1f),
        AnimateAmountGain = true,
        AmountGainSmoothTime = 0.1f,
        IconStyle = new SecondaryResourceIconStyle
        {
            Size = new Vector2(88f, 88f),
            IconOffset = Vector2.Zero,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            HoverTip = SecondaryResourceHoverTipStyle.Default
        },
        GainFeedback = SecondaryResourceCounterGainFeedback.StarCounterLike,
        FormatAmount = static (amount, _) => amount.ToString()
    };

    public static SecondaryResourceDefinition Charge { get; private set; } = null!;

    public static void Register()
    {
        var registry = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);

        Charge = registry.Register(ChargeLocalId, new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: MaxCharge,
            hardMaxAmount: MaxCharge,
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
            locTable: "static_hover_tips",
            titleKey: "HAMMER_MOD_RESOURCE_CHARGE.title",
            descriptionKey: "HAMMER_MOD_RESOURCE_CHARGE.description",
            smallIconPath: $"{Entry.ResPath}/images/characters/charge_counter.svg",
            largeIconPath: $"{Entry.ResPath}/images/characters/charge_counter.svg"));

        registry.AlwaysShowInCombatUiForCharacter<HammerModCharacter>(ChargeLocalId);
        registry.RegisterCombatUi<NSecondaryResourceCounter>(
            ChargeCounterLocalId,
            static _ => CreateChargeCounter(),
            static context => context.Node.Bind(context.Player),
            new NodeAttachmentOptions
            {
                Name = "HammerChargeCounter",
                UniqueNameInOwner = true,
                AttachParentSelector = static parent => ((NCombatUi)parent).EnergyCounterContainer
            });
    }

    private static NSecondaryResourceCounter CreateChargeCounter()
    {
        var counter = NSecondaryResourceCounter.Create(Charge, ChargeCounterStyle);

        // Keep the resource attached to the energy HUD instead of a fixed screen coordinate.
        counter.Position = new Vector2(122f, 16f);
        return counter;
    }

    public static int GetCharge(Player player)
    {
        return SecondaryResourceCmd.Get(player, Charge.Id);
    }
}
