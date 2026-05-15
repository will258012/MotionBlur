using AlgernonCommons;
using AlgernonCommons.Keybinding;
using AlgernonCommons.Notifications;
using AlgernonCommons.Translation;
using AlgernonCommons.XML;
using ColossalFramework.IO;
using System.IO;
using System.Xml.Serialization;

namespace MotionBlur.Settings;

[XmlRoot("MotionBlur")]
public sealed class ModSettings : SettingsXMLBase
{
    [XmlIgnore]
    private static readonly string SettingsFileName = Path.Combine(DataLocation.localApplicationData, "MotionBlur.xml");

    public static void Load() => XMLFileUtils.Load<ModSettings>(SettingsFileName);
    public static void Save() => XMLFileUtils.Save<ModSettings>(SettingsFileName);

    // Remember edit values here if the settings have edited!
    internal static void ResetToDefaults()
    {
        Translations.CurrentLanguage = "default";
        Logging.DetailLogging = false;
        WhatsNew.LastNotifiedVersionString = "0.0";
        EnabledInGame = true;
        EnabledInMapEditor = true;
        EnabledInAssetEditor = true;
        EnabledInThemeEditor = true;
        EnabledInScenarioEditor = true;
        EnabledInVanillaCamMode = true;
        EnabledInFPSCamera = true;
        EnabledInACMEFPSMode = true;
        FilterType = CameraMotionBlur.MotionBlurFilter.ReconstructionDX11;
        MovementScale = 1f;
        RotationScale = 1f;
        MaxVelocity = 8f;
        MinVelocity = 0.1f;
        VelocityScale = 0.375f;
        VelocityDownsample = 1;
        SoftZDistance = 0.005f;
        Jitter = 0.05f;
        ToggleKey = new Keybinding(
            UnityEngine.KeyCode.B,
            control: true,
            shift: true,
            alt: false);
    }


    [XmlElement(nameof(EnabledInGame))]
    public bool XMLEnabledInGame { get => EnabledInGame; set => EnabledInGame = value; }
    [XmlIgnore]
    internal static bool EnabledInGame = true;

    [XmlElement(nameof(EnabledInMapEditor))]
    public bool XMLEnabledInMapEditor { get => EnabledInMapEditor; set => EnabledInMapEditor = value; }
    [XmlIgnore]
    internal static bool EnabledInMapEditor = true;

    [XmlElement(nameof(EnabledInAssetEditor))]
    public bool XMLEnabledInAssetEditor { get => EnabledInAssetEditor; set => EnabledInAssetEditor = value; }
    [XmlIgnore]
    internal static bool EnabledInAssetEditor = true;

    [XmlElement(nameof(EnabledInThemeEditor))]
    public bool XMLEnabledInThemeEditor { get => EnabledInThemeEditor; set => EnabledInThemeEditor = value; }
    [XmlIgnore]
    internal static bool EnabledInThemeEditor = true;

    [XmlElement(nameof(EnabledInScenarioEditor))]
    public bool XMLEnabledInScenarioEditor { get => EnabledInScenarioEditor; set => EnabledInScenarioEditor = value; }
    [XmlIgnore]
    internal static bool EnabledInScenarioEditor = true;

    [XmlElement(nameof(EnabledInVanillaCamMode))]
    public bool XMLEnabledInVanillaCamMode { get => EnabledInVanillaCamMode; set => EnabledInVanillaCamMode = value; }
    [XmlIgnore]
    internal static bool EnabledInVanillaCamMode = true;

    [XmlElement(nameof(EnabledInFPSCamera))]
    public bool XMLEnabledInFPSCamera { get => EnabledInFPSCamera; set => EnabledInFPSCamera = value; }
    [XmlIgnore]
    internal static bool EnabledInFPSCamera = true;

    [XmlElement(nameof(EnabledInACMEFPSMode))]
    public bool XMLEnabledInACMEFPSMode { get => EnabledInACMEFPSMode; set => EnabledInACMEFPSMode = value; }
    [XmlIgnore]
    internal static bool EnabledInACMEFPSMode = true;

    [XmlElement(nameof(FilterType))]
    public CameraMotionBlur.MotionBlurFilter XMLFilterType { get => FilterType; set => FilterType = value; }
    [XmlIgnore]
    internal static CameraMotionBlur.MotionBlurFilter FilterType = CameraMotionBlur.MotionBlurFilter.ReconstructionDX11;

    [XmlElement(nameof(MovementScale))]
    public float XMLMovementScale { get => MovementScale; set => MovementScale = value; }
    [XmlIgnore]
    internal static float MovementScale = 1f;

    [XmlElement(nameof(RotationScale))]
    public float XMLRotationScale { get => RotationScale; set => RotationScale = value; }
    [XmlIgnore]
    internal static float RotationScale = 1f;

    [XmlElement(nameof(MaxVelocity))]
    public float XMLMaxVelocity { get => MaxVelocity; set => MaxVelocity = value; }
    [XmlIgnore]
    internal static float MaxVelocity = 8f;

    [XmlElement(nameof(MinVelocity))]
    public float XMLMinVelocity { get => MinVelocity; set => MinVelocity = value; }
    [XmlIgnore]
    internal static float MinVelocity = 0.1f;

    [XmlElement(nameof(VelocityScale))]
    public float XMLVelocityScale { get => VelocityScale; set => VelocityScale = value; }
    [XmlIgnore]
    internal static float VelocityScale = 0.375f;

    [XmlElement(nameof(VelocityDownsample))]
    public int XMLVelocityDownsample { get => VelocityDownsample; set => VelocityDownsample = value; }
    [XmlIgnore]
    internal static int VelocityDownsample = 1;

    [XmlElement(nameof(SoftZDistance))]
    public float XMLSoftZDistance { get => SoftZDistance; set => SoftZDistance = value; }
    [XmlIgnore]
    internal static float SoftZDistance = 0.005f;

    [XmlElement(nameof(Jitter))]
    public float XMLJitter { get => Jitter; set => Jitter = value; }
    [XmlIgnore]
    internal static float Jitter = 0.05f;

    [XmlElement(nameof(ToggleKey))]
    public Keybinding XMLToggleKey { get => ToggleKey; set => ToggleKey = value; }
    [XmlIgnore]
    internal static Keybinding ToggleKey = new Keybinding(UnityEngine.KeyCode.B, true, true, false);
}