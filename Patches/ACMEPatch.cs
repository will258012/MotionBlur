using AlgernonCommons;
using HarmonyLib;
using MotionBlur.Settings;
using System;
using System.Reflection;

namespace MotionBlur.Patches;

[HarmonyPatch]
internal class ACMEPatch
{
    private static bool Prepare() => AssemblyUtils.IsAssemblyPresent("ACME");
    private static MethodBase TargetMethod() =>
        AccessTools.Method(AccessTools.TypeByName("ACME.FPSMode, ACME"), "ToggleMode");
    private static void Postfix()
    {
        bool isActive = Traverse.Create(Type.GetType("ACME.FPSMode, ACME"))
            .Field("s_modeActive")
            .GetValue<bool>();

        if (isActive)
        {
            Mod.Instance.ApplySettings(ModSettings.EnabledInACMEFPSMode);
        }
        else
        {
            Mod.Instance.ApplySettings();
        }
    }
}
