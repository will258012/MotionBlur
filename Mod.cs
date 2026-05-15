using AlgernonCommons;
using AlgernonCommons.Patching;
using AlgernonCommons.Translation;
using FPSCamera.Cam.Controller;
using ICities;
using MotionBlur.Settings;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MotionBlur;

public class Mod : PatcherMod<OptionsPanel, PatcherBase>, IUserMod
{
    public Mod() => Instance = this;
    public override string BaseName => "Motion Blur";
    public string Description => Translations.Translate("MOD_DESCRIPTION");
    public override string HarmonyID => "Will258012.MotionBlur";
    public override void LoadSettings() => ModSettings.Load();
    public override void SaveSettings() { ModSettings.Save(); ApplySettings(); }
    public new static Mod Instance { get; private set; }
    public AppMode LoadingMode { get; internal set; }
    /// <summary>
    /// Toggles the motion blur effect on or off.
    /// </summary>
    /// <returns><c>True</c> if motion blur is enabled after toggling, <c>null</c> if motion blur is not ready.</returns>
    public bool? ToggleMotionBlur()
    {
        if (!IsMotionBlurReady)
        {
            Logging.Error("Failed to toggle due to motion blur is not ready");
            return null;
        }
        cameraMotionBlur!.enabled = !cameraMotionBlur.enabled;
        return cameraMotionBlur.enabled;
    }
    /// <summary>
    /// Indicates whether the motion blur component has been initialized and is ready for use.
    /// </summary>
    public bool IsMotionBlurReady => cameraMotionBlur != null;

    /// <summary>
    /// Indicates whether the motion blur component has been enabled.
    /// </summary>
    public bool IsMotionBlurEnabled => cameraMotionBlur?.enabled ?? false;
    internal bool InitializeMotionBlur()
    {
        if (MainCamera == null)
        {
            Logging.Error("MainCamera is null, cannot initialize motion blur");
            return false;
        }
        cameraMotionBlur = MainCamera.gameObject.GetComponent<CameraMotionBlur>()
                        ?? MainCamera.gameObject.AddComponent<CameraMotionBlur>();
        if (!LoadMotionBlurShaders())
        {
            Logging.Error("Failed to load motion blur shaders, removing component");
            GameObject.DestroyImmediate(cameraMotionBlur);
            cameraMotionBlur = null;
            return false;
        }

        cameraMotionBlur.enabled = false;
        Logging.KeyMessage("Motion blur initialized successfully");
        return true;
    }
    public override void OnEnabled()
    {
        base.OnEnabled();
        if (FPSCameraAPI.Helper.IsFPSCameraInstalledAndEnabled)
            RegisterInFPSCamera();
    }
    public override void OnDisabled()
    {
        base.OnDisabled();
        if (FPSCameraAPI.Helper.IsFPSCameraInstalledAndEnabled)
            UnregisterFromFPSCamera();
    }


    private bool LoadMotionBlurShaders()
    {
        try
        {
            string platformFolder;
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    platformFolder = "Windows64";
                    break;
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    platformFolder = "MacOSX";
                    break;
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    platformFolder = "Linux64";
                    break;
                default:
                    platformFolder = "Windows64";
                    break;
            }

            string bundlePath = Path.Combine(AssemblyUtils.AssemblyPath, $@"Resources\{platformFolder}\motionblur");

            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Logging.Error("Failed to load motionblur AssetBundle");
                return false;
            }

            cameraMotionBlur!.shader = bundle.LoadAsset<Shader>("CameraMotionBlur");
            cameraMotionBlur.dx11MotionBlurShader = bundle.LoadAsset<Shader>("CameraMotionBlurDX11");
            cameraMotionBlur.noiseTexture = bundle.LoadAsset<Texture2D>("MotionBlurJitter");
            cameraMotionBlur.replacementClear = bundle.LoadAsset<Shader>("MotionBlurClear");

            bundle.Unload(false);

            if (!cameraMotionBlur.shader || !cameraMotionBlur.dx11MotionBlurShader || !cameraMotionBlur.noiseTexture || !cameraMotionBlur.replacementClear)
            {
                Logging.Error("Shaders or/and noise texture not found in the AssetBundle");
                return false;
            }

            return true;
        }
        catch (System.Exception e)
        {
            Logging.LogException(e);
            return false;
        }
    }
    /// <summary>
    /// Applies all motion blur settings to the component.
    /// </summary>
    /// <param name="forceSet">Optional. If provided, overrides the enable state of the motion blur component.</param>
    public void ApplySettings(bool? forceSet = null)
    {
        if (!IsMotionBlurReady)
        {
            Logging.Error("Failed to apply settings due to motion blur is not ready");
            return;
        }

        if (forceSet.HasValue)
            cameraMotionBlur?.enabled = forceSet.Value;
        else
            ApplyLoadingModeSettings();
    }
    private void ApplyLoadingModeSettings()
    {
        switch (LoadingMode)
        {
            case AppMode.Game when ModSettings.EnabledInGame:
            case AppMode.MapEditor when ModSettings.EnabledInMapEditor:
            case AppMode.AssetEditor when ModSettings.EnabledInAssetEditor:
            case AppMode.ThemeEditor when ModSettings.EnabledInThemeEditor:
            case AppMode.ScenarioEditor when ModSettings.EnabledInScenarioEditor:
                cameraMotionBlur?.enabled = true;
                break;
            default:
                cameraMotionBlur?.enabled = false;
                break;
        }
    }

    private void RegisterInFPSCamera()
    {
        UnregisterFromFPSCamera();
        FPSCamController.OnCameraEnabled += OnFPSCameraEnabled;
        FPSCamController.OnCameraDisabled += OnFPSCameraDisabled;
        Logging.Message("Registered for FPS Camera events");
    }
    private void UnregisterFromFPSCamera()
    {
        FPSCamController.OnCameraEnabled -= OnFPSCameraEnabled;
        FPSCamController.OnCameraDisabled -= OnFPSCameraDisabled;
    }

    private void OnFPSCameraEnabled() { isFPSCameraEnabled = true; ApplySettings(ModSettings.EnabledInFPSCamera); }
    private void OnFPSCameraDisabled() { isFPSCameraEnabled = false; ApplySettings(); }


    public Camera MainCamera => ToolsModifierControl.cameraController.m_camera;
    private CameraMotionBlur? cameraMotionBlur;

    internal bool isFPSCameraEnabled;

}
public class Loading : PatcherLoadingBase<OptionsPanel, PatcherBase>
{
    protected override List<AppMode> PermittedModes =>
    [
        AppMode.Game,
        AppMode.MapEditor,
        AppMode.AssetEditor,
        AppMode.ThemeEditor,
        AppMode.ScenarioEditor
    ];

    protected override void CreatedActions(ILoading loading)
    {
        base.CreatedActions(loading);
        Mod.Instance.LoadingMode = loading.currentMode;
    }
}