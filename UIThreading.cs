using AlgernonCommons.Keybinding;
using ColossalFramework.UI;
using ICities;
using MotionBlur.Settings;
using UnityEngine;

namespace MotionBlur;

public class UIThreading : ThreadingExtensionBase
{
    private bool _initialized = false;
    public AudioClip ToggleSound
    {
        get
        {
            if (field == null)
                field = UIView.GetAView().defaultClickSound;
            return field;
        }
    }
    public AudioClip DisabledToggleSound
    {
        get
        {
            if (field == null)
                field = UIView.GetAView().defaultDisabledClickSound;
            return field;
        }
    }
    public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
    {
        if (!_initialized)
        {
            if (!Mod.Instance.IsMotionBlurReady && Mod.Instance.MainCamera != null)
            {
                if (Mod.Instance.InitializeMotionBlur())
                {
                    Mod.Instance.ApplySettings();
                    _initialized = true;
                }
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