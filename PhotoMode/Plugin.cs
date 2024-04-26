using System;
using BepInEx;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using RoR2.UI.SkinControllers;
using UnityEngine;
using UnityEngine.UI;

namespace PhotoMode;

[BepInPlugin("com.riskofresources.discohatesme.photomode", "PhotoMode", "3.0.0")]
[NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
public class PhotoModePlugin : BaseUnityPlugin
{
	public CameraRigController cameraRigController;
	private static PhotoModeSettings _settings;
	private WebService _webService;

	public void Awake() {
		_settings = new PhotoModeSettings(Config, Info.Location);
		_webService = new WebService(_settings);
		// var presetDropdown = Config.Bind("Presets", "Preset", Presets.Default, "Switch between different presets");
		// var presetData = Config.Bind("Presets", "Preset Data", "", "Saved preset data. Don't modify this unless you know what you're doing");
		/*
		Dictionary<Presets, PhotoModeSettings> dict = null;

		if (!String.IsNullOrEmpty(presetData.Value)) {
			var b64 = presetData.Value;
			var bytes = Convert.FromBase64String(b64);
			var json = Encoding.UTF8.GetString(bytes);
			dict = JsonConvert.DeserializeObject<Dictionary<Presets, PhotoModeSettings>>(json, new StringEnumConverter());
		}
	
		dict ??= new();
		if (!dict.ContainsKey(presetDropdown.Value)) {
			dict.Add(presetDropdown.Value, _settings);
		}

		var previousPreset = presetDropdown.Value;
		presetDropdown.SettingChanged += (_, _) => {
			dict[previousPreset] = _settings;
			previousPreset = presetDropdown.Value;
	
			if (dict.TryGetValue(presetDropdown.Value, out var value)) {
				Settings.PopulateFromDict(value);
			}
			else {
				Settings.PresetName.Value = Enum.GetName(typeof(Presets), presetDropdown.Value);
			}

			var jsonString = JsonConvert.SerializeObject(dict, new StringEnumConverter());
			byte[] bytesToEncode = Encoding.UTF8.GetBytes(jsonString);
			string encodedText = Convert.ToBase64String(bytesToEncode);
			presetData.Value = encodedText;
		};
		*/

		On.RoR2.CameraRigController.OnEnable += (orig, self) => {
			orig.Invoke(self);
			cameraRigController = self;
		};
		On.RoR2.CameraRigController.OnDisable += (orig, self) => {
			orig.Invoke(self);
			cameraRigController = null;
		};
		On.RoR2.UI.PauseScreenController.Awake += (orig, self) =>
		{
			orig.Invoke(self);
			SetupPhotoModeButton(self);
		};

		if (Options.HasRiskOfOptions) {
			Options.AddWebUIAction(() => {
				_webService.Startup();
				Application.OpenURL($"http://localhost:{_webService.NegotiatedPort}/index.html");
			});
		}
	}

	private void OnApplicationQuit() {
		_webService?.Shutdown();
	}

	private void SetupPhotoModeButton(PauseScreenController pauseScreenController)
	{
		GameObject buttonGameObject = pauseScreenController.GetComponentInChildren<ButtonSkinController>().gameObject;
		GameObject obj = Instantiate(buttonGameObject, buttonGameObject.transform.parent);
		obj.name = "GenericMenuButton (Photo mode)";
		obj.transform.SetSiblingIndex(1);
		obj.GetComponent<ButtonSkinController>().GetComponent<LanguageTextMeshController>().token = "Photo mode";
		HGButton component = obj.GetComponent<HGButton>();
		component.interactable = cameraRigController.localUserViewer != null;
		component.onClick = new Button.ButtonClickedEvent();
		component.onClick.AddListener(() => {
			var pmGo = new GameObject("PhotoModeController");
			var photoModeController = pmGo.AddComponent<PhotoModeController>();
			photoModeController.EnterPhotoMode(_settings, pauseScreenController, cameraRigController);
		});
	}
}