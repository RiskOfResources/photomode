using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Newtonsoft.Json;
using UnityEngine;

namespace PhotoMode;

public class PhotoModeSettings {
   private readonly ConfigFile _configFile;
 
   [JsonIgnore]
   public string BaseDir { get; }

   [JsonIgnore]
   public List<PhotoModeSetting> Settings { get; } = new();

   [JsonConstructor]
   public PhotoModeSettings() : this(null, "") {
   }

   public PhotoModeSettings(ConfigFile configFile, string baseDir) {
      Logger.Log("Creating new Photo Mode Settings");
      _configFile = configFile;
      BaseDir = baseDir;
      // PresetName = CreatePhotoModeSetting(SettingCategory.General, "Preset Name", "Default", "Name of the preset.");
      CameraSensitivity = CreatePhotoModeSetting(SettingCategory.General, "Camera Sensitivity", 1f, "Sensitivity of the camera movements.", 0);
      CameraPanSpeed = CreatePhotoModeSetting(SettingCategory.General, "Camera Pan Speed", 5f, "Speed of the camera pan.", 0);
      CameraElevationSpeed = CreatePhotoModeSetting(SettingCategory.General, "Camera Elevation Speed", 5f, "Speed of the camera raise/lower.", 0);
      CameraRollSensitivity = CreatePhotoModeSetting(SettingCategory.General, "Camera Roll Sensitivity", 10f, "Sensitivity of the camera roll.", 0);
      CameraFovSensitivity = CreatePhotoModeSetting(SettingCategory.General, "Camera FOV Sensitivity", 10f, "Sensitivity of the camera FOV speed.", 0);
      CameraSprintMultiplier = CreatePhotoModeSetting(SettingCategory.General, "Camera Sprint Multiplier", 5f, "Multiplier for how much the camera increases in speed when held.", min: 1);
      CameraSlowMultiplier = CreatePhotoModeSetting(SettingCategory.General, "Camera Slow Multiplier", 0.3f, "Multiplier for how much the camera decreases in speed when held.", max: 1);
      SnapRollEnabled = CreatePhotoModeSetting(SettingCategory.General, "Snap Roll Enabled", true, "If the roll should snap back to 0 when close to 0 roll angle.");
      CameraMinFov = CreatePhotoModeSetting(SettingCategory.General, "Camera Min FOV", 4f, "Minimum camera field of view.", min: 0, max: 180);
      CameraMaxFov = CreatePhotoModeSetting(SettingCategory.General, "Camera Max FOV", 120f, "Maximum camera field of view.", min: 0, max: 180);
      TimeScaleStep = CreatePhotoModeSetting(SettingCategory.General, "Time Scale Step", 0.1f, "Step value to increase/decrease time scale when key is pressed.", max: 10);
      PhysicsTickRate = CreatePhotoModeSetting(SettingCategory.General, "Physics Tick Rate", 60, "How many times per second physics objects are updated while in Photo Mode. Higher values will smooth out movements but require more processing power and may change physics interactions which is why it defaults to the game default of: 60.", 2, 5000, 1);
      
      SmoothCamera = CreatePhotoModeSetting(SettingCategory.General, "Smooth Camera", true, "Check to smooth the free-cam.");
      PanningSmooth = CreatePhotoModeSetting(SettingCategory.General, "Camera Smooth Pan Speed", 50f, "How fast the smooth pan camera can move.", 0);
      PanningSmoothTime = CreatePhotoModeSetting(SettingCategory.General, "Camera Panning Smoothing Time", 1.5f, "How many seconds to smooth the camera position while panning (inertia after releasing panning keys)", 0);
      MaxSmoothRotationSpeed = CreatePhotoModeSetting(SettingCategory.General, "Smooth Rotation Max Speed", 5f, "How fast the smooth camera can rotate", 0);
      RotationSmoothDecay = CreatePhotoModeSetting(SettingCategory.General, "Smooth Rotation Decay", 0.25f, "How much to decay the rotation speed after stopping mouse movement", 0);
      
      // dolly
      DollyEasingFunction = CreatePhotoModeSetting(SettingCategory.General, "Dolly Easing Function", Easing.Linear, "How the dolly cam transitions between states. In means start slow and end fast, out means start fast and end slow.");
      DollyCamSpeed = CreatePhotoModeSetting(SettingCategory.General, "Dolly Cam Speed", 5f, "Speed at which the dolly cam pans/rotates", 0);
      DollyFollowRotation = CreatePhotoModeSetting(SettingCategory.General, "Dolly Follows Rotation", true, "If the dolly camera should follow the rotation you set at the checkpoints or only follow the path and let the user control the rotation.");
      NumberOfDollyPoints = CreatePhotoModeSetting(SettingCategory.General, "Dolly Path Smoothing", 50, "How many line segments to break the dolly path into to create a smooth path. Only applies when you have at least 1 checkpoint. The more points you add the slower the dolly will move.", min: 2, max: 5000, increment: 1);
      DollyFollowsFocus = CreatePhotoModeSetting(SettingCategory.General, "Dolly Auto-Focus", false, "If the dolly should follow your focus changes.");
      SmoothDolly = CreatePhotoModeSetting(SettingCategory.General, "Smooth Dolly Cam", true, "Smooths out dolly movements by sacrificing some rotational accuracy when using at least 1 checkpoint (at least 3 dolly points total).");
      
      // key bindings
      RaiseCameraKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Raise Camera", new KeyboardShortcut(KeyCode.E));
      LowerCameraKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Lower Camera", new KeyboardShortcut(KeyCode.Q));
      CameraSprintKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Speed Up Camera", new KeyboardShortcut(KeyCode.LeftControl));
      CameraSlowKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Slow Down Camera", new KeyboardShortcut(KeyCode.LeftShift));
      ArcCameraKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Arc Camera On Player", new KeyboardShortcut(KeyCode.Space));
      ToggleRecordingKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Toggle Recording Key", new KeyboardShortcut(KeyCode.R));
      DollyPlaybackKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Dolly Playback Key", new KeyboardShortcut(KeyCode.P));
      DollyCheckpointKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Add Dolly Checkpoint", new KeyboardShortcut(KeyCode.T));
      IncreaseTimeScaleKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Increase Time Scale", new KeyboardShortcut(KeyCode.PageUp));
      DecreaseTimeScaleKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Decrease Time Scale", new KeyboardShortcut(KeyCode.PageDown));
      ToggleTimePausedKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Toggle Pause Key", new KeyboardShortcut(KeyCode.Pause));
      ToggleSmoothCameraKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Toggle Smooth Camera", new KeyboardShortcut(KeyCode.G));
      NextPlayerKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Arc Focus Previous Player", new KeyboardShortcut(KeyCode.LeftArrow));
      PrevPlayerKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Arc Focus Next Player", new KeyboardShortcut(KeyCode.RightArrow));
      // CaptureScreen = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Capture Screen", new KeyboardShortcut(KeyCode.None), "Records RAW textures to PNGs and saves into your Risk of Rain 2 data folder. Huge file size since it captures at 4x resolution and at your current frame rate. Limit the frame rate or change resolution to adjust.");
      SaveReplayBuffer = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Save Replay Buffer", new KeyboardShortcut(KeyCode.F9), "Save the current replay buffer as an image sequence to disk in your risk of rain 2 data/recordings folder.");
      ToggleHud = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Toggle HUD visibility", new KeyboardShortcut(KeyCode.H));
      ScrollWheelModifierKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Scroll Wheel Modifier", new KeyboardShortcut(KeyCode.LeftAlt), "Hold while scrolling to change the focal length instead of the focus distance.");
      ScrollWheelApertureModifierKey = CreatePhotoModeSetting(SettingCategory.KeyBindings, "Scroll Wheel Aperture Modifier", new KeyboardShortcut(KeyCode.LeftAlt, KeyCode.LeftShift), "Hold while scrolling to change the aperture instead of the focus distance.");
 
      // arc settings
      ToggleArcCamera = CreatePhotoModeSetting(SettingCategory.ArcCamera, "Toggle Arc Camera", false, "True to toggle arc camera on keypress instead of needing to hold the button.");
      SmoothArcCamera = CreatePhotoModeSetting(SettingCategory.ArcCamera, "Smooth Arc", true, "Smoothly rotate/pan around the player instead of snapping");
      ArcPanningSmoothTime = CreatePhotoModeSetting(SettingCategory.ArcCamera, "Arc Panning Smooth Time", 1f, "How many seconds to smooth the camera position when the target of an arc camera moves", 0);
      SmoothArcCamSpeed = CreatePhotoModeSetting(SettingCategory.ArcCamera, "Smooth Arc Camera Speed", 5f, "Amount of smoothing for the arc camera", 0);
      RestrictArcPlayers = CreatePhotoModeSetting(SettingCategory.ArcCamera, "Arc Around Players Only", true, "Whether to arc around players or any model in the scene");

      // post processing
      PostProcessing = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Enable Post Processing", true, "Enable post processing effects");
      PostProcessingAntiAliasing = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Anti-aliasing", true, "Enable anti-aliasing");
      PostProcessDepth = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Depth of Field", true, "Enable depth of field");
      PostProcessFocusDistance = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Focus Distance", 3.2f, "Distance to the point of focus. Adjustable on the fly with the scroll wheel", min: 0, max: 1000);
      PostProcessFocalLength = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Focal Length", 50f, "Set the distance between the lens and the film. The larger the value is, the shallower the depth of field is.", min: 1, max: 300);
      PostProcessAperture = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Aperture", 5.6f, "Set the ratio of the aperture (known as f-stop or f-number). The smaller the value is, the shallower the depth of field is.", max: 64);
      PostProcessingFocusDistanceStep = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Focus Distance Step", 0.1f, "Step value to increase/decrease depth of field focus distance as you scroll", 0.01f);
      PostProcessVignette = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Vignette", false, "Enable Vignette");
      PostProcessVignetteColor = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Vignette Color", Color.black, "Vignette Color");

      PostProcessVignetteIntensity = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Vignette Intensity", .35f, "Set the amount of vignetting on screen.", 0, 1);
      PostProcessVignetteSmoothness = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Vignette Smoothness", .5f, "Set the smoothness of the Vignette borders.", 0, 1);
      PostProcessVignetteRoundness = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Vignette Roundness", 1f, "Set the value to round the Vignette. Lower values will make a more squared vignette.", 0, 1);
      PostProcessVignetteRounded = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Vignette Rounded", false, "Enable this checkbox to make the vignette perfectly round. When disabled, the Vignette effect is dependent on the current aspect ratio.");
      BreakBeforeColorGrading = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Break Before Color Grading", false, "Stop applying post-process effects before color grading for exporting to external color grading");
      PostProcessColorGrading = CreatePhotoModeSetting(SettingCategory.PostProcessing, "Color Grading", false, "Enable LUT color grading (requires .cube LUT name specified)");
      LutName = CreatePhotoModeSetting(SettingCategory.PostProcessing, "LUT Name", "", "The name of your .cube LUT to import (full name including .cube extension)");
      
      // other
      TextFadeTime = CreatePhotoModeSetting(SettingCategory.UI, "Text Fade Out Time", 1.5f, "How long to show text before fading out", min: 0, increment: 0.1f);
      ShowHudByDefault = CreatePhotoModeSetting(SettingCategory.UI, "Show HUD", true, "Show the photo mode HUD");
      ShowDollyPath = CreatePhotoModeSetting(SettingCategory.UI, "Show Dolly Path", false, "Show the path of the dolly (mostly for debugging).");
      
      // experimental
      DisableAllMovement = CreatePhotoModeSetting(SettingCategory.Experimental, "Disable All Camera Movement", false, "Disable movement for external control or testing.");
      ExportLinearColorSpace = CreatePhotoModeSetting(SettingCategory.Experimental, "Record linear color space", false, "When recording a RAW screen-capture should the exported images use linear or sRGB color space.");
      EnableReplayBuffer = CreatePhotoModeSetting(SettingCategory.Experimental, "Enable Replay buffer", false, "It's a replay buffer");
      ReplayBufferDuration = CreatePhotoModeSetting(SettingCategory.Experimental, "Replay Buffer Duration", 1f, "Number of seconds to record.");
      ReplayBufferFramerate = CreatePhotoModeSetting(SettingCategory.Experimental, "Replay Buffer Framerate", 60f, "Rate at which to save frames.", 1f, 240f, 1f);
      ReplayBufferResolutionScale = CreatePhotoModeSetting(SettingCategory.Experimental, "Replay Buffer Resolution Scale", 1f, "The scaling for the screen size. ", 0.1f, 4f, 0.1f);
   }

   private PhotoModeSetting<Color> CreatePhotoModeSetting(SettingCategory category, string name, Color defaultValue, string description = "") {
      var setting = new PhotoModeSetting<Color>(category, name, defaultValue, description);
      return AddPhotoModeSetting(setting);
   }
   
   private PhotoModeSetting<Easing> CreatePhotoModeSetting(SettingCategory category, string name, Easing defaultValue, string description = "") {
      var setting = new PhotoModeSetting<Easing>(category, name, defaultValue, description);
      return AddPhotoModeSetting(setting);
   }

   private PhotoModeSetting<float> CreatePhotoModeSetting(SettingCategory category, string name, float defaultValue, string description = "", float min = 0.01f, float max = 100, float increment = 0.01f) {
      var setting = new FloatPhotoModeSetting(category, name, defaultValue, description, min, max, increment);
      return AddPhotoModeSetting(setting);
   }
   
   private PhotoModeSetting<T> CreatePhotoModeSetting<T>(SettingCategory category, string name, T defaultValue, string description = "") {
      var setting = new PhotoModeSetting<T>(category, name, defaultValue, description);
      return AddPhotoModeSetting(setting);
   }

   private PhotoModeSetting<T> AddPhotoModeSetting<T>(PhotoModeSetting<T> setting) {
      Settings.Add(setting);
      
      if(_configFile is not null) {
         var configEntry = _configFile.Bind(setting.Category.Section, setting.Name, setting.DefaultValue, setting.Description);

         if (Options.HasRiskOfOptions) {
            Options.AddOption(configEntry, setting);
         }
         
         setting.AddConfigEntry(configEntry);
      }

      return setting;
   }

   // public readonly PhotoModeSetting<string> PresetName;
   public readonly PhotoModeSetting<float> CameraSensitivity;
   public readonly PhotoModeSetting<float> CameraPanSpeed;
   public readonly PhotoModeSetting<float> CameraElevationSpeed;
   public readonly PhotoModeSetting<float> CameraRollSensitivity;
   public readonly PhotoModeSetting<float> CameraFovSensitivity;
   public readonly PhotoModeSetting<float> CameraSprintMultiplier;
   public readonly PhotoModeSetting<float> CameraSlowMultiplier;
   public readonly PhotoModeSetting<bool> SnapRollEnabled;
   public readonly PhotoModeSetting<float> CameraMinFov;
   public readonly PhotoModeSetting<float> CameraMaxFov;
   public readonly PhotoModeSetting<float> TimeScaleStep;
   public readonly PhotoModeSetting<float> PhysicsTickRate;

   // smooth cam
   public readonly PhotoModeSetting<bool> SmoothCamera;
   public readonly PhotoModeSetting<float> PanningSmooth;
   public readonly PhotoModeSetting<float> PanningSmoothTime;
   public readonly PhotoModeSetting<float> MaxSmoothRotationSpeed;
   public readonly PhotoModeSetting<float> RotationSmoothDecay;

   // dolly
   public readonly PhotoModeSetting<Easing> DollyEasingFunction;
   public readonly PhotoModeSetting<float> DollyCamSpeed;
   public readonly PhotoModeSetting<bool> DollyFollowRotation;
   public readonly PhotoModeSetting<float> NumberOfDollyPoints;
   public readonly PhotoModeSetting<bool> DollyFollowsFocus;
   public readonly PhotoModeSetting<bool> SmoothDolly;

   // key bindings
   public readonly PhotoModeSetting<KeyboardShortcut> RaiseCameraKey;
   public readonly PhotoModeSetting<KeyboardShortcut> LowerCameraKey;
   public readonly PhotoModeSetting<KeyboardShortcut> CameraSprintKey;
   public readonly PhotoModeSetting<KeyboardShortcut> CameraSlowKey;

   public readonly PhotoModeSetting<KeyboardShortcut> ArcCameraKey;
   public readonly PhotoModeSetting<KeyboardShortcut> ToggleRecordingKey;
   public readonly PhotoModeSetting<KeyboardShortcut> DollyPlaybackKey;
   public readonly PhotoModeSetting<KeyboardShortcut> DollyCheckpointKey;
   public readonly PhotoModeSetting<KeyboardShortcut> IncreaseTimeScaleKey;
   public readonly PhotoModeSetting<KeyboardShortcut> DecreaseTimeScaleKey;
   public readonly PhotoModeSetting<KeyboardShortcut> ToggleTimePausedKey;
   public readonly PhotoModeSetting<KeyboardShortcut> ToggleSmoothCameraKey;
   public readonly PhotoModeSetting<KeyboardShortcut> NextPlayerKey;
   public readonly PhotoModeSetting<KeyboardShortcut> PrevPlayerKey;
   // public readonly PhotoModeSetting<KeyboardShortcut> CaptureScreen;
   public readonly PhotoModeSetting<KeyboardShortcut> SaveReplayBuffer;
   public readonly PhotoModeSetting<KeyboardShortcut> ToggleHud;
   public readonly PhotoModeSetting<KeyboardShortcut> ScrollWheelModifierKey;
   public readonly PhotoModeSetting<KeyboardShortcut> ScrollWheelApertureModifierKey;

   // arc settings
   public readonly PhotoModeSetting<bool> ToggleArcCamera;
   public readonly PhotoModeSetting<bool> SmoothArcCamera;
   public readonly PhotoModeSetting<float> ArcPanningSmoothTime;
   public readonly PhotoModeSetting<float> SmoothArcCamSpeed;
   public readonly PhotoModeSetting<bool> RestrictArcPlayers;

   // post processing
   public readonly PhotoModeSetting<bool> PostProcessing;
   public readonly PhotoModeSetting<bool> PostProcessingAntiAliasing;
   public readonly PhotoModeSetting<bool> PostProcessDepth;
   public readonly PhotoModeSetting<float> PostProcessFocusDistance;
   public readonly PhotoModeSetting<float> PostProcessFocalLength;
   public readonly PhotoModeSetting<float> PostProcessAperture;
   public readonly PhotoModeSetting<float> PostProcessingFocusDistanceStep;
   public readonly PhotoModeSetting<bool> PostProcessVignette;
   [JsonIgnore]
   public readonly PhotoModeSetting<Color> PostProcessVignetteColor;
   public readonly PhotoModeSetting<float> PostProcessVignetteIntensity;
   public readonly PhotoModeSetting<float> PostProcessVignetteSmoothness;
   public readonly PhotoModeSetting<float> PostProcessVignetteRoundness;
   public readonly PhotoModeSetting<bool> PostProcessVignetteRounded;
   public readonly PhotoModeSetting<bool> BreakBeforeColorGrading;
   public readonly PhotoModeSetting<bool> PostProcessColorGrading;

   // other
   public readonly PhotoModeSetting<float> TextFadeTime;
   public readonly PhotoModeSetting<bool> ShowHudByDefault;
   public readonly PhotoModeSetting<bool> ShowDollyPath;
   public readonly PhotoModeSetting<bool> DisableAllMovement;
   public readonly PhotoModeSetting<bool> ExportLinearColorSpace;
   public readonly PhotoModeSetting<string> LutName;
   public readonly PhotoModeSetting<bool> EnableReplayBuffer;
   public readonly PhotoModeSetting<float> ReplayBufferDuration;
   public readonly PhotoModeSetting<float> ReplayBufferFramerate;
   public readonly PhotoModeSetting<float> ReplayBufferResolutionScale;
}

public abstract class PhotoModeSetting {
   public PhotoModeSetting(string name) {
      Name = name;
   }

   public abstract void UpdateValue(object obj);
   public abstract void Save();
   public string Name { get; }
}

public class PhotoModeSetting<T> : PhotoModeSetting {
   public PhotoModeSetting(SettingCategory category, string name, T defaultValue, string description) : base(name) {
      Category = category;
      DefaultValue = defaultValue;
      Description = description;
      Value = defaultValue;
   }

   public override void UpdateValue(object obj) {
      if (obj is T value) {
         Value = value;
      }
      else {
         Logger.Log($"Update value is not of correct type. Current ({Value}), New ({obj})");
      }
   }

   public override void Save() {
      if (_configEntry is not null && !_configEntry.Value.Equals(Value)) {
         _configEntry.Value = Value;
      }
   }

   public void AddConfigEntry(ConfigEntry<T> configEntry) {
      _configEntry = configEntry;
      Value = _configEntry.Value;
 
      _configEntry.SettingChanged += (_, _) => {
         Value = _configEntry.Value;
      };
   }
   
   [JsonIgnore]
   private ConfigEntry<T> _configEntry;

   public SettingCategory Category { get; }
   public T DefaultValue { get; }
   public string Description { get; }
   public T Value { get; set; }
}

public class FloatPhotoModeSetting : PhotoModeSetting<float> {
   public FloatPhotoModeSetting(SettingCategory category, string name, float defaultValue,
      string description, float min = 0.01f, float max = 100, float increment = 0.01f) : base(category, name, defaultValue, description) {
      Min = min;
      Max = max;
      Increment = increment;
   }

   public override void UpdateValue(object obj) {
      Value = Convert.ToSingle(obj);
   }

   public float Min { get; }
   public float Max { get; }
   public float Increment { get; }
}

public class SettingCategory {
   public SettingCategory(string section) {
      Section = section;
   }

   public string Section { get; }
   public static SettingCategory General => new("General");
   public static SettingCategory KeyBindings => new("Key Bindings");
   public static SettingCategory ArcCamera => new("Arc Camera");
   public static SettingCategory PostProcessing = new("Post Processing");
   public static SettingCategory UI = new("UI");
   // public static SettingCategory Presets = new("Presets");
   public static SettingCategory Experimental = new("Experimental");
}
