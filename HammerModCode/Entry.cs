using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using HammerMod.Gameplay;
using STS2RitsuLib;
using STS2RitsuLib.Interop;

namespace HammerMod;

[ModInitializer(nameof(Initialize))]
public partial class Entry
{
    public const string ModId = "HammerMod";
    public const string ResPath = $"res://{ModId}";

    public static Logger Logger { get; private set; } = null!;

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        Logger = RitsuLibFramework.CreateLogger(ModId);
        HammerResources.Register();
        HammerKeywords.Register();
        HammerTargetTypes.Register();
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        Logger.Info("HammerMod content initialized.");
    }
}
