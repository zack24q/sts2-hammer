namespace HammerMod.Gameplay;

internal sealed class ChargeReleaseSnapshot
{
    private int? _charge;

    internal bool IsActive => _charge.HasValue;

    internal int Begin(int currentCharge, bool isFirstInSeries)
    {
        if (isFirstInSeries)
            _charge = currentCharge;

        return _charge ?? currentCharge;
    }

    internal bool ShouldRelease(bool isLastInSeries)
    {
        return !IsActive || isLastInSeries;
    }

    internal void Finish(bool isLastInSeries)
    {
        if (IsActive && isLastInSeries)
            _charge = null;
    }
}
