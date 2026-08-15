using HammerMod.Characters;
using HammerMod.Gameplay;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
[RegisterCharacterStarterCard(typeof(HammerModCharacter), 1, Order = 30)]
public sealed class Charge : ModCardTemplate
{
    private const int ChargeGain = 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/placeholders/{nameof(Charge)}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Charge", ChargeGain)
    ];

    public Charge() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SecondaryResourceCmd.Gain(
            Owner,
            HammerResources.Charge.Id,
            ChargeGain,
            source: this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
