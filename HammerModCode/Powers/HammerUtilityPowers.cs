using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Powers;

[RegisterPower]
public sealed class FreeMealPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task BeforePotionUsed(PotionModel potion, Creature? target)
    {
        if (potion.Owner.Creature != Owner)
            return;

        Flash();
        await PowerCmd.Remove(this);
    }
}

[RegisterPower]
public sealed class LuckyVoucherPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}
