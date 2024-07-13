﻿using BepInEx;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using RoR2.UI.SkinControllers;
using UnityEngine;
using UnityEngine.UI;

namespace PhotoMode;

[BepInPlugin("com.riskofresources.discohatesme.photomode", "PhotoMode", "3.0.11")]
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

		On.RoR2.CameraRigController.OnEnable += OnCameraRigControllerOnOnEnable;
		On.RoR2.CameraRigController.OnDisable += OnCameraRigControllerOnOnDisable;
		On.RoR2.UI.PauseScreenController.Awake += OnPauseScreenControllerOnAwake;

		if (Options.HasRiskOfOptions) {
			Options.AddWebUIAction(() => {
				_webService.Startup();
				Application.OpenURL($"http://localhost:{_webService.NegotiatedPort}/index.html");
			});
		}
	}
	
	private void OnDestroy() {
		On.RoR2.CameraRigController.OnEnable -= OnCameraRigControllerOnOnEnable;
		On.RoR2.CameraRigController.OnDisable -= OnCameraRigControllerOnOnDisable;
		On.RoR2.UI.PauseScreenController.Awake -= OnPauseScreenControllerOnAwake;
	}

	private void OnApplicationQuit() {
		_webService?.Shutdown();
	}

	private void SetupPhotoModeButton(PauseScreenController pauseScreenController) {
		var buttonSkinController = pauseScreenController.GetComponentInChildren<ButtonSkinController>();
		if (!buttonSkinController) {
			Debug.LogWarning("Failed to get button	skin controller");
			return;
		}
	
		var buttonGameObject = buttonSkinController.gameObject;
		var photoModeButton = Instantiate(buttonGameObject, buttonGameObject.transform.parent);
		photoModeButton.name = "GenericMenuButton (Photo mode)";
		photoModeButton.transform.SetSiblingIndex(1);
		if (photoModeButton.TryGetComponent<LanguageTextMeshController>(out var languageTextMeshController)) {
			languageTextMeshController.token = "Photo mode";
		}

		if (!photoModeButton.TryGetComponent<HGButton>(out var component)) {
			Debug.LogWarning("Failed to setup photo mode button");
			Destroy(buttonGameObject);
			return;
		}
	
		component.interactable = cameraRigController && cameraRigController.localUserViewer != null;
		component.onClick = new Button.ButtonClickedEvent();
		component.onClick.AddListener(() => {
			pauseScreenController.gameObject.SetActive(false);
			EnterPhotoMode();
		});
	}

	private void EnterPhotoMode() {
		var pmGo = new GameObject("PhotoModeController");
		var controller = pmGo.AddComponent<PhotoModeController>();
		controller.EnterPhotoMode(_settings, cameraRigController);
	}

	private void OnPauseScreenControllerOnAwake(On.RoR2.UI.PauseScreenController.orig_Awake orig, PauseScreenController self) {
		orig.Invoke(self);
		SetupPhotoModeButton(self);
	}

	private void OnCameraRigControllerOnOnDisable(On.RoR2.CameraRigController.orig_OnDisable orig, CameraRigController self) {
		orig.Invoke(self);
		cameraRigController = null;
	}

	private void OnCameraRigControllerOnOnEnable(On.RoR2.CameraRigController.orig_OnEnable orig, CameraRigController self) {
		orig.Invoke(self);
		cameraRigController = self;
	}
}