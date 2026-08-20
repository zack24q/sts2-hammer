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
    internal static readonly Vector2 ChargeCounterPosition = new(122f, 16f);
    internal static readonly Vector2 ChargeCounterScale = new(0.8f, 0.8f);
    internal static readonly Vector2 ChargeAmountLabelOffset = new(0f, 78f);
    internal static readonly Color ChargeLevelOneGlowColor = new(1f, 0.08f, 0.04f, 0.96f);
    internal static readonly Color ChargeLevelTwoGlowColor = new(1f, 0.46f, 0.04f, 0.98f);
    internal static readonly Color ChargeLevelThreeGlowColor = new(1f, 1f, 1f, 1f);
    internal const float ChargeLevelOneGlowScale = 1f;
    internal const float ChargeLevelTwoGlowScale = 1.03f;
    internal const float ChargeLevelThreeGlowScale = 1.06f;
    internal const float ChargeGainGhostEndScale = 1.1f;
    internal const float ChargeGainGhostAlpha = 0.22f;
    internal const double ChargeGainGhostDuration = 0.3;
    private const string ChargeCounterLocalId = "charge_counter";
    private const string ChargeGlowNodeName = "Glow";

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
        GainFeedback = SecondaryResourceCounterGainFeedback.None,
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
        registry.RegisterCombatUi<NSecondaryResourceCounter>(
            ChargeCounterLocalId,
            static _ => CreateChargeCounter(),
            static context =>
            {
                context.Node.Bind(context.Player);
                ApplyChargePresentation(
                    context.Node,
                    context.Player is null ? 0 : GetCharge(context.Player),
                    playGainFeedback: false);
            },
            static context =>
            {
                if (string.Equals(
                        context.Definition.Id,
                        Charge.Id,
                        StringComparison.OrdinalIgnoreCase))
                    ApplyChargePresentation(
                        context.Node,
                        context.NewAmount,
                        playGainFeedback: context.Delta > 0);
            },
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

        // Match the Regent star counter's size and offset relative to the energy HUD.
        counter.Position = ChargeCounterPosition;
        counter.Scale = ChargeCounterScale;
        counter.ClipContents = false;
        counter.AddChild(CreateChargeTexture(
            ChargeGlowNodeName,
            ChargeCounterGlowFileName,
            zIndex: 0));
        return counter;
    }

    private static TextureRect CreateChargeTexture(
        string name,
        string fileName,
        int zIndex)
    {
        return new TextureRect
        {
            Name = name,
            CustomMinimumSize = ChargeCounterStyle.IconSize,
            Size = ChargeCounterStyle.IconSize,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Texture = ResourceLoader.Load<Texture2D>(
                $"{Entry.ResPath}/images/characters/{fileName}"),
            ZIndex = zIndex,
        };
    }

    private static void ApplyChargePresentation(
        Control presentation,
        int charge,
        bool playGainFeedback)
    {
        var clampedCharge = Math.Clamp(charge, 0, MaxCharge);
        var color = GetChargeGlowColor(clampedCharge);
        var glow = presentation.GetNode<TextureRect>(ChargeGlowNodeName);
        glow.Visible = color.HasValue;
        glow.SelfModulate = color ?? Colors.Transparent;
        var glowScale = GetChargeGlowScale(clampedCharge);
        glow.Scale = new Vector2(glowScale, glowScale);
        glow.Position = GetChargeGlowPosition(
            ChargeCounterStyle.CounterSize,
            ChargeCounterStyle.IconSize,
            clampedCharge);

        if (playGainFeedback)
            PlayChargeGainFeedback(presentation);
    }

    private static void PlayChargeGainFeedback(Control presentation)
    {
        var ghost = CreateChargeTexture(
            "GainGhost",
            ChargeCounterIconFileName,
            zIndex: 0);
        ghost.PivotOffset = ChargeCounterStyle.IconSize * 0.5f;
        ghost.SelfModulate = new Color(1f, 1f, 1f, ChargeGainGhostAlpha);
        presentation.AddChild(ghost);

        var tween = ghost.CreateTween().SetParallel();
        tween.TweenProperty(
                ghost,
                "scale",
                new Vector2(ChargeGainGhostEndScale, ChargeGainGhostEndScale),
                ChargeGainGhostDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(
                ghost,
                "self_modulate:a",
                0f,
                ChargeGainGhostDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        tween.Chain().TweenCallback(Callable.From(ghost.QueueFree));
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

    internal static Vector2 GetChargeGlowHudPosition(int charge)
    {
        var localOffset = GetChargeGlowPosition(
            ChargeCounterStyle.CounterSize,
            ChargeCounterStyle.IconSize,
            charge);
        return ChargeCounterPosition + new Vector2(
            localOffset.X * ChargeCounterScale.X,
            localOffset.Y * ChargeCounterScale.Y);
    }
}
