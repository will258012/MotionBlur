using AlgernonCommons;
using AlgernonCommons.Patching;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using FPSCamera.Cam.Controller;
using ICities;
using MotionBlur.Settings;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

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

            bundle.Unload(false);

            if (cameraMotionBlur.shader == null || cameraMotionBlur.dx11MotionBlurShader == null || cameraMotionBlur.noiseTexture == null)
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

        ApplyMotionBlurParameters();
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
    private void ApplyMotionBlurParameters()
    {
        cameraMotionBlur!.filterType = ModSettings.FilterType;
        cameraMotionBlur.movementScale = ModSettings.MovementScale;
        cameraMotionBlur.rotationScale = ModSettings.RotationScale;
        cameraMotionBlur.maxVelocity = ModSettings.MaxVelocity;
        cameraMotionBlur.minVelocity = ModSettings.MinVelocity;
        cameraMotionBlur.velocityScale = ModSettings.VelocityScale;
        cameraMotionBlur.velocityDownsample = ModSettings.VelocityDownsample;
        cameraMotionBlur.softZDistance = ModSettings.SoftZDistance;
        cameraMotionBlur.jitter = ModSettings.Jitter;
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

    private void OnFPSCameraEnabled() => ApplySettings(ModSettings.EnabledInFPSCamera);
    private void OnFPSCameraDisabled() => ApplySettings();


    public Camera MainCamera => RenderManager.instance.CurrentCameraInfo.m_camera;
    private CameraMotionBlur? cameraMotionBlur;
    public AppMode LoadingMode { get; internal set; }
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