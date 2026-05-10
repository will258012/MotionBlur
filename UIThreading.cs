using AlgernonCommons.Keybinding;
using ColossalFramework;
using ColossalFramework.UI;
using FPSCamera.Cam;
using FPSCamera.Cam.Controller;
using ICities;
using MotionBlur.Settings;
using UnityEngine;

namespace MotionBlur;

public class UIThreading : ThreadingExtensionBase
{
    public AudioClip ToggleSound
    {
        get
        {
            field ??= UIView.GetAView().defaultClickSound;
            return field;
        }
    }
    public AudioClip DisabledToggleSound
    {
        get
        {
            field ??= UIView.GetAView().defaultDisabledClickSound;
            return field;
        }
    }

    public readonly static int CitizensLayer = LayerMask.NameToLayer("Citizens");
    public readonly static int VehicleLayer = LayerMask.NameToLayer("Vehicles");

    private bool triedInitialized = false;
    private int excludeLayers;
    public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
    {

        if (!triedInitialized)
        {
            if (Mod.Instance.MainCamera != null)
            {
                if (Mod.Instance.InitializeMotionBlur())
                {
                    Mod.Instance.ApplySettings();
                }
                triedInitialized = true;
            }
            return;
        }

        if (KeyTriggered(ModSettings.ToggleKey))
        {
            if (Mod.Instance.ToggleMotionBlur().HasValue)
                AudioManager.instance.PlaySound(ToggleSound, 1f);
            else
                AudioManager.instance.PlaySound(DisabledToggleSound, 1f);
        }

        if (!Mod.Instance.IsMotionBlurEnabled) return;

        // Exclude layers based on camera target
        excludeLayers = default;

        if (Mod.Instance.isFPSCameraEnabled)
            CheckFPSCameraTarget();
        else if (ToolsModifierControl.cameraController.GetTarget().Type == InstanceType.Vehicle)
            excludeLayers |= 1 << VehicleLayer;
        else if (ToolsModifierControl.cameraController.GetTarget().Type is InstanceType.Citizen or InstanceType.CitizenInstance)
            excludeLayers |= 1 << CitizensLayer;

        if (excludeLayers != default)
            if (!ToolsModifierControl.cameraController.GetTarget().IsEmpty)
                Shader.SetGlobalFloat("_ClearDistance", Mathf.Clamp(ToolsModifierControl.cameraController.m_targetSize, 15f, 200f));
            else if (FPSCamController.Instance.Status.IsFlagSet(FPSCamController.CamStatus.Enabled))
                Shader.SetGlobalFloat("_ClearDistance", 15f);

        CameraMotionBlur.excludeLayers = excludeLayers;
    }
    private void CheckFPSCameraTarget()
    {
        if (FPSCamController.Instance.FPSCam is IFollowCam followCam)
        {
            if (followCam.FollowInstance.Type == InstanceType.Vehicle)
                excludeLayers |= 1 << VehicleLayer;
            if (followCam.FollowInstance.Type == InstanceType.Citizen)
                excludeLayers |= 1 << CitizensLayer;
        }
        else if (FPSCamController.Instance.Status.IsFlagSet(FPSCamController.CamStatus.PluginEnabled))
        {
            excludeLayers |= 1 << VehicleLayer;
        }
    }
    private static bool KeyTriggered(Keybinding key)
    {
        // Check primary key.
        if (!Input.GetKeyDown((KeyCode)key.Key))
        {
            return false;
        }

        // Check modifier keys.
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) != key.Control)
        {
            return false;
        }

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) != key.Shift)
        {
            return false;
        }

        if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr)) != key.Alt)
        {
            return false;
        }

        // If we got here, all checks passed.
        return true;
    }
}