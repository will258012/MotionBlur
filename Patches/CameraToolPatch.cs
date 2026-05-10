using HarmonyLib;
using MotionBlur.Settings;
namespace MotionBlur.Patches;

[HarmonyPatch]
[HarmonyAfter("Will258012.FPSCamera.Continued")]
internal class CameraToolPatch
{

    [HarmonyPatch(typeof(CameraTool), "OnEnable")]
    [HarmonyPostfix]
    private static void OnEnablePatch()
    {
        Mod.Instance.ApplySettings(ModSettings.EnabledInVanillaCamMode);
    }

    [HarmonyPatch(typeof(CameraTool), "OnDisable")]
    [HarmonyPostfix]
    private static void OnDisablePatch()
    {
        Mod.Instance.ApplySettings();
    }
}
