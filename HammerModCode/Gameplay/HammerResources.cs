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
    internal const string ChargeCounterIconFileName = "charge_counter.png";
    internal const string ChargeCounterGlowFileName = "charge_counter_glow.png";
    internal static readonly Vector2 ChargeCounterPosition = new(-36f, 40f);
    internal static readonly Vector2 ChargeCounterScale = new(0.8f, 0.8f);
    internal static readonly Vector2 ChargeAmountLabelOffset = new(0f, 78f);
    internal static readonly Color ChargeLevelOneGlowColor = new(1f, 0.08f, 0.04f, 0.96f);
    internal static readonly Color ChargeLevelTwoGlowColor = new(1f, 0.46f, 0.04f, 0.98f);
    internal static readonly Color ChargeLevelThreeGlowColor = new(1f, 1f, 1f, 1f);
    internal const float ChargeLevelOneGlowScale = 1f;
    internal const float ChargeLevelTwoGlowScale = 1.03f;
    internal const float ChargeLevelThreeGlowScale = 1.06f;
    private const string ChargeCounterLocalId = "charge_counter";

    private static readonly SecondaryResourceCounterStyle ChargeCounterStyle = new()
    {
        CounterSize = new Vector2(128f, 128f),
        IconSize = new Vector2(128f, 128f),
        FontSize = 32,
        OutlineSize = 10,
        PositiveColor = new Color(1f, 0.91f, 0.63f),
        ZeroColor = new Color(0.85f, 0.32f, 0.29f),
        OutlineColor = new Color(0.11f, 0.05f, 0.03f),
        AmountLabelOffset = ChargeAmountLabelOffset,
        AnimateAmountGain = false,
        IconStyle = new SecondaryResourceIconStyle
        {
            Size = new Vector2(128f, 128f),
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
            smallIconPath: $"{Entry.ResPath}/images/characters/{ChargeCounterIconFileName}",
            largeIconPath: $"{Entry.ResPath}/images/characters/{ChargeCounterIconFileName}"));

        registry.AlwaysShowInCombatUiForCharacter<HammerModCharacter>(ChargeLocalId);
        registry.RegisterCombatUi<NHammerChargeCounter>(
            ChargeCounterLocalId,
            static _ => CreateChargeCounter(),
            static context => context.Node.Bind(context.Player),
            static context =>
            {
                if (string.Equals(
                        context.Definition.Id,
                        Charge.Id,
                        StringComparison.OrdinalIgnoreCase))
                    context.Node.SetCharge(context.NewAmount);
            },
            new NodeAttachmentOptions
            {
                Name = "HammerChargeCounter",
                UniqueNameInOwner = true,
                AttachParentSelector = static parent => ((NCombatUi)parent).EnergyCounterContainer
            });
    }

    private static NHammerChargeCounter CreateChargeCounter()
    {
        var counter = NHammerChargeCounter.Create(Charge, ChargeCounterStyle);

        // Match the Regent star counter's size and offset relative to the energy HUD.
        counter.Position = ChargeCounterPosition;
        counter.Scale = ChargeCounterScale;
        return counter;
    }

    public static int GetCharge(Player player)
    {
        return SecondaryResourceCmd.Get(player, Charge.Id);
    }

    internal static Color? GetChargeGlowColor(int charge)
    {
        return charge switch
        {
            <= 0 => null,
            1 => ChargeLevelOneGlowColor,
            2 => ChargeLevelTwoGlowColor,
            _ => ChargeLevelThreeGlowColor,
        };
    }

    internal static float GetChargeGlowScale(int charge)
    {
        return charge switch
        {
            <= 1 => ChargeLevelOneGlowScale,
            2 => ChargeLevelTwoGlowScale,
            _ => ChargeLevelThreeGlowScale,
        };
    }

    internal static Vector2 GetChargeGlowPosition(
        Vector2 counterSize,
        Vector2 iconSize,
        int charge)
    {
        var scale = GetChargeGlowScale(charge);
        return (counterSize - iconSize * scale) * 0.5f;
    }
}

// Layers a charge-colored glow behind RitsuLib's counter without replacing its behavior.
internal sealed partial class NHammerChargeCounter : Control
{
    private NSecondaryResourceCounter? _counter;
    private TextureRect? _glow;
    private Vector2 _counterSize;
    private Vector2 _glowSize;
    private int _charge;

    public static NHammerChargeCounter Create(
        SecondaryResourceDefinition definition,
        SecondaryResourceCounterStyle style)
    {
        var node = new NHammerChargeCounter();
        node.Configure(definition, style);
        return node;
    }

    public void Bind(Player? player)
    {
        _counter?.Bind(player);
        SetCharge(player is null ? 0 : HammerResources.GetCharge(player));
    }

    internal void SetCharge(int charge)
    {
        _charge = Math.Clamp(charge, 0, HammerResources.MaxCharge);
        ApplyGlow();
    }

    private void Configure(
        SecondaryResourceDefinition definition,
        SecondaryResourceCounterStyle style)
    {
        CustomMinimumSize = style.CounterSize;
        Size = style.CounterSize;
        MouseFilter = MouseFilterEnum.Ignore;
        _counterSize = style.CounterSize;
        _glowSize = style.IconSize;

        _glow = new TextureRect
        {
            Position = HammerResources.GetChargeGlowPosition(
                style.CounterSize,
                style.IconSize,
                _charge),
            CustomMinimumSize = style.IconSize,
            Size = style.IconSize,
            MouseFilter = MouseFilterEnum.Ignore,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Texture = ResourceLoader.Load<Texture2D>(
                $"{Entry.ResPath}/images/characters/{HammerResources.ChargeCounterGlowFileName}"),
        };
        AddChild(_glow);

        _counter = NSecondaryResourceCounter.Create(definition, style);
        AddChild(_counter);
        ApplyGlow();
    }

    private void ApplyGlow()
    {
        if (_glow is null)
            return;

        var color = HammerResources.GetChargeGlowColor(_charge);
        _glow.Visible = color.HasValue;
        var scale = HammerResources.GetChargeGlowScale(_charge);
        _glow.Scale = new Vector2(scale, scale);
        _glow.Position = HammerResources.GetChargeGlowPosition(
            _counterSize,
            _glowSize,
            _charge);
        if (color.HasValue)
            _glow.Modulate = color.Value;
    }
}
