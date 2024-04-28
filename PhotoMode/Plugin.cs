using System;
using BepInEx;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using RoR2.UI.SkinControllers;
using UnityEngine;
using UnityEngine.UI;

namespace PhotoMode;

[BepInPlugin("com.riskofresources.discohatesme.photomode", "PhotoMode", "3.0.1")]
[NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
public class PhotoModePlugin : BaseUnityPlugin
{
	public CameraRigController cameraRigController;
	private static PhotoModeSettings _settings;
	private WebService _webService;
	private bool _disablePauseOnExit;
	private bool _allowPhotoModeHotkey;

	public void Awake() {
		_settings = new PhotoModeSettings(Config, Info.Location);
		_webService = new WebService(_settings);

		On.RoR2.CameraRigController.OnEnable += (orig, self) => {
			orig.Invoke(self);
			_allowPhotoModeHotkey = true;
			cameraRigController = self;
		};
		On.RoR2.CameraRigController.OnDisable += (orig, self) => {
			orig.Invoke(self);
			cameraRigController = null;
		};
		On.RoR2.UI.PauseScreenController.Awake += (orig, self) =>
		{
			orig.Invoke(self);
			if (_disablePauseOnExit) {
				_disablePauseOnExit = false;
				Destroy(self.gameObject);
			}
			else {
				SetupPhotoModeButton(self);
			}
		};

		if (Options.HasRiskOfOptions) {
			Options.AddWebUIAction(() => {
				_webService.Startup();
				Application.OpenURL($"http://localhost:{_webService.NegotiatedPort}/index.html");
			});
		}
	}

	public void Update() {
		if (_allowPhotoModeHotkey && cameraRigController?.localUserViewer != null && Input.GetKeyDown(_settings.TogglePhotoMode.Value.MainKey) && !PauseManager.isPaused) {
			EnterPhotoMode();
			_disablePauseOnExit = true;
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
			Destroy(pauseScreenController.gameObject);
			EnterPhotoMode();
		});
	}

	private void EnterPhotoMode() {
		var pmGo = new GameObject("PhotoModeController");
		var controller = pmGo.AddComponent<PhotoModeController>();
		controller.EnterPhotoMode(_settings, cameraRigController);
		_allowPhotoModeHotkey = false;
		controller.OnExit += (_, _) => _allowPhotoModeHotkey = true;
	}
}