using AlgernonCommons;
using AlgernonCommons.Keybinding;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using System;
using UnityEngine;

namespace MotionBlur.Settings;

public class OptionsPanel : OptionsPanelBase
{
    private const float Margin = 5f;
    private const float LeftMargin = 24f;
    private const float TitleMargin = 50f;
    private const float SliderMargin = 60f;

    private UIScrollablePanel scrollPanel;
    protected override void Setup()
    {
        float headerWidth = width - (Margin * 2f);
        float currentY = LeftMargin;

        // Scroll panel
        scrollPanel = AddUIComponent<UIScrollablePanel>();
        scrollPanel.relativePosition = new Vector2(0f, Margin);
        scrollPanel.width = width - 15f;
        scrollPanel.height = height - 15f;
        scrollPanel.autoLayout = false;
        scrollPanel.clipChildren = true;
        scrollPanel.builtinKeyNavigation = true;
        scrollPanel.scrollWheelDirection = UIOrientation.Vertical;

        scrollPanel.eventVisibilityChanged += (_, visible) =>
        {
            if (visible)
            {
                scrollPanel.Reset();
            }
        };

        UIScrollbars.AddScrollbar(this, scrollPanel);

        var language_DropDown = UIDropDowns.AddPlainDropDown(
             scrollPanel,
             LeftMargin,
             currentY,
             Translations.Translate("LANGUAGE_CHOICE"),
             Translations.LanguageList,
             Translations.Index);

        language_DropDown.eventSelectedIndexChanged += (_, index) =>
        {
            Translations.Index = index;
            OptionsPanelManager<OptionsPanel>.LocaleChanged();
        };

        language_DropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
        currentY += language_DropDown.parent.height + Margin;


        var logging_CheckBox = UICheckBoxes.AddPlainCheckBox(
             scrollPanel,
             LeftMargin,
             currentY,
             Translations.Translate("DETAIL_LOGGING"));

        logging_CheckBox.isChecked = Logging.DetailLogging;
        logging_CheckBox.eventCheckChanged += (_, value) =>
        {
            Logging.DetailLogging = value;
        };

        currentY += logging_CheckBox.height + LeftMargin;

        UISpacers.AddTitleSpacer(
            scrollPanel,
            Margin,
            currentY,
            headerWidth,
            Translations.Translate("ENABLE_MOTION_BLUR_IN"));

        currentY += TitleMargin;

        CreateCheckBox(Translations.Translate("GAME"), ref currentY,
            ModSettings.EnabledInGame,
            value => ModSettings.EnabledInGame = value);

        CreateCheckBox(Translations.Translate("MAP_EDITOR"), ref currentY,
            ModSettings.EnabledInMapEditor,
            value => ModSettings.EnabledInMapEditor = value);

        CreateCheckBox(Translations.Translate("ASSET_EDITOR"), ref currentY,
            ModSettings.EnabledInAssetEditor,
            value => ModSettings.EnabledInAssetEditor = value);

        CreateCheckBox(Translations.Translate("THEME_EDITOR"), ref currentY,
            ModSettings.EnabledInThemeEditor,
            value => ModSettings.EnabledInThemeEditor = value);

        CreateCheckBox(Translations.Translate("SCENARIO_EDITOR"), ref currentY,
            ModSettings.EnabledInScenarioEditor,
            value => ModSettings.EnabledInScenarioEditor = value);

        CreateCheckBox(Translations.Translate("VANILLA_CAM"), ref currentY,
            ModSettings.EnabledInVanillaCamMode,
            value => ModSettings.EnabledInVanillaCamMode = value);

        CreateCheckBox(Translations.Translate("FPC"), ref currentY,
            ModSettings.EnabledInFPSCamera,
            value => ModSettings.EnabledInFPSCamera = value);

        CreateCheckBox(Translations.Translate("ACME_FPS"), ref currentY,
            ModSettings.EnabledInACMEFPSMode,
            value => ModSettings.EnabledInACMEFPSMode = value);

        currentY += LeftMargin;

        UISpacers.AddTitleSpacer(
            scrollPanel,
            Margin,
            currentY,
            headerWidth,
            Translations.Translate("PARAM"));

        currentY += TitleMargin;

        var filterDropdown = UIDropDowns.AddPlainDropDown(
    scrollPanel,
    LeftMargin,
    currentY,
    Translations.Translate("ALGO"),
    Enum.GetNames(typeof(CameraMotionBlur.MotionBlurFilter)),
    (int)ModSettings.FilterType);
        filterDropdown.tooltip = Translations.Translate("ALGO_TOOLTIP");
        filterDropdown.eventSelectedIndexChanged += (_, value) =>
        {
            ModSettings.FilterType = (CameraMotionBlur.MotionBlurFilter)value;
            OptionsPanelManager<OptionsPanel>.LocaleChanged();
        };
        filterDropdown.parent.relativePosition =
            new Vector2(LeftMargin, currentY);

        currentY += filterDropdown.parent.height + LeftMargin;

        if (ModSettings.FilterType == CameraMotionBlur.MotionBlurFilter.CameraMotion)
        {
            var label1 = UILabels.AddLabel(scrollPanel, LeftMargin, currentY, string.Format(Translations.Translate("AVAILABLE_ONLY_TIP"), Translations.Translate("ALGO"), CameraMotionBlur.MotionBlurFilter.CameraMotion.ToString()), headerWidth);
            currentY += label1.height + Margin;
            CreateSlider(
                Translations.Translate("MOVEMENT_SCALE"),
                ref currentY,
                0f, 5f, .1f,
                ModSettings.MovementScale,
                value => ModSettings.MovementScale = value,
                Translations.Translate("MOVEMENT_SCALE_TOOLTIP"));

            var label2 = UILabels.AddLabel(scrollPanel, LeftMargin, currentY, string.Format(Translations.Translate("AVAILABLE_ONLY_TIP"), Translations.Translate("ALGO"), CameraMotionBlur.MotionBlurFilter.CameraMotion.ToString()), headerWidth);
            currentY += label2.height + Margin;
            CreateSlider(
                Translations.Translate("ROTATION_SCALE"),
                ref currentY,
                0f, 5f, .1f,
                ModSettings.RotationScale,
                value => ModSettings.RotationScale = value,
                Translations.Translate("ROTATION_SCALE_TOOLTIP"));
        }
        if (ModSettings.FilterType is CameraMotionBlur.MotionBlurFilter.Reconstruction
            or CameraMotionBlur.MotionBlurFilter.ReconstructionDX11
            or CameraMotionBlur.MotionBlurFilter.ReconstructionDisc)
        {
            var label3 = UILabels.AddLabel(scrollPanel, LeftMargin, currentY, string.Format(Translations.Translate("AVAILABLE_ONLY_TIP"), Translations.Translate("ALGO"), $"{CameraMotionBlur.MotionBlurFilter.Reconstruction}, {CameraMotionBlur.MotionBlurFilter.ReconstructionDX11}, {CameraMotionBlur.MotionBlurFilter.ReconstructionDisc}"), headerWidth);
            currentY += label3.height + Margin;

            CreateSlider(
                Translations.Translate("SOFT_Z_DISTANCE"),
                ref currentY,
                .001f, .1f, .001f,
                ModSettings.SoftZDistance,
                value => ModSettings.SoftZDistance = value,
                Translations.Translate("SOFT_Z_DISTANCE_TOOLTIP"));
        }
        CreateSlider(
            Translations.Translate("MAX_VELOCITY"),
            ref currentY,
            2f, 50f, .1f,
            ModSettings.MaxVelocity,
            value => ModSettings.MaxVelocity = value,
            Translations.Translate("MAX_VELOCITY_TOOLTIP"),
            new UISliders.SliderValueFormat(1, .1f, "N", "px"));

        CreateSlider(
            Translations.Translate("MIN_VELOCITY"),
            ref currentY,
            0f, 1f, .05f,
            ModSettings.MinVelocity,
            value => ModSettings.MinVelocity = value,
            Translations.Translate("MIN_VELOCITY_TOOLTIP"),
            new UISliders.SliderValueFormat(1, .1f, "N", "px"));

        CreateSlider(
            Translations.Translate("VELOCITY_SCALE"),
            ref currentY,
            .1f, 2f, .05f,
            ModSettings.VelocityScale,
            value => ModSettings.VelocityScale = value,
            Translations.Translate("VELOCITY_SCALE_TOOLTIP"));

        var downSample = UISliders.AddPlainSliderWithIntegerValue(
            scrollPanel,
            LeftMargin,
            currentY,
            Translations.Translate("VELOCITY_DOWNSAMPLE"),
            1f,
            4f,
            1f,
            ModSettings.VelocityDownsample);

        downSample.thumbObject.area = Vector4.zero; // Fix thumb disappeared issues
        downSample.tooltip = Translations.Translate("VELOCITY_DOWNSAMPLE_TOOLTIP");
        downSample.eventValueChanged += (_, value) => ModSettings.VelocityDownsample = (int)value;
        currentY += downSample.height + SliderMargin;

        CreateSlider(
            Translations.Translate("JITTER"),
            ref currentY,
            0f, .5f, .01f,
            ModSettings.Jitter,
            value => ModSettings.Jitter = value,
            Translations.Translate("JITTER_TOOLTIP"));

        var keymapping = OptionsKeymapping.AddKeymapping(scrollPanel, 5f, currentY, Translations.Translate("TOGGLE_MOTION_BLUR"), ModSettings.ToggleKey);
        keymapping.Panel.tooltip = Translations.Translate("TOGGLE_MOTION_BLUR_TOOLTIP");
        currentY += keymapping.Panel.height + LeftMargin;

        var defaults_Button = UIButtons.AddButton(
            scrollPanel,
            LeftMargin,
            currentY,
            Translations.Translate("RESET_SETTINGS"),
            width: 300);

        defaults_Button.eventClicked += (_, _) =>
        {
            ModSettings.ResetToDefaults();
            OptionsPanelManager<OptionsPanel>.LocaleChanged();
        };
    }


    private void CreateCheckBox(
        string text,
        ref float y,
        bool defaultValue,
        Action<bool> onChanged)
    {
        var checkbox = UICheckBoxes.AddPlainCheckBox(
            scrollPanel,
            LeftMargin,
            y,
            text);

        checkbox.isChecked = defaultValue;
        checkbox.eventCheckChanged += (_, value) => onChanged(value);

        y += checkbox.height + Margin;
    }


    private void CreateSlider(
        string text,
        ref float y,
        float min,
        float max,
        float step,
        float defaultValue,
        Action<float> onChanged,
        string? tooltip = null,
        UISliders.SliderValueFormat? format = null)
    {
        UISlider slider;
        if (format.HasValue)
            slider = UISliders.AddPlainSliderWithValue(
                 scrollPanel,
                 LeftMargin,
                 y,
                 text,
                 min,
                 max,
                 step,
                 defaultValue,
                 format.Value);
        else
            slider = UISliders.AddPlainSliderWithValue(
                scrollPanel,
                LeftMargin,
                y,
                text,
                min,
                max,
                step,
                defaultValue);

        slider.tooltip = tooltip;
        slider.eventValueChanged += (_, value) => onChanged(value);

        y += slider.height + SliderMargin;
    }
}