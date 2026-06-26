using System;
using System.Collections;
using BepInEx;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using RoR2.UI.SkinControllers;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Permissions;

[assembly: SecurityPermission( SecurityAction.RequestMinimum, SkipVerification = true )]
namespace PhotoMode;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
public class PhotoModePlugin : BaseUnityPlugin
{
	public CameraRigController cameraRigController;
	private static PhotoModeSettings _settings;
	private WebService _webService;
	private GameObject _pmGo;

	public void Awake() {
		try {
			_settings ??= new PhotoModeSettings(Config, Info.Location);
		}
		catch (Exception e) {
			Logger.LogWarning($"Failed to create settings PhotoMode may not work properly: {e}");
		}
		
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

	public void Update() {
		if (cameraRigController && Input.GetKeyDown(_settings.TogglePhotoModeKey.Value.MainKey)) {
			StartPhotoMode(Time.timeScale);
		}
	}

	private void OnApplicationQuit() {
		_webService?.Shutdown();
	}

	public void StartPhotoMode(float timeScale) {
		if (!cameraRigController || cameraRigController.localUserViewer == null) {
			return;
		}

		if (_pmGo) {
			Debug.LogWarning("Photo mode is already open");
			return;
		}
		
		var fixedDeltaTime = Time.fixedDeltaTime;

		_pmGo = new GameObject("PhotoModeController");
		_pmGo.SetActive(false);
		var controller = _pmGo.AddComponent<PhotoModeController>();
		controller.OnExit += (_, _) => {
			Destroy(_pmGo);
			Time.timeScale = timeScale;
			Time.fixedDeltaTime = fixedDeltaTime;
			PauseManager.ClosePauseScreen(true);
		};

		controller.EnterPhotoMode(_settings, cameraRigController);
		_pmGo.SetActive(true);
		StartCoroutine(PauseAtEndOfFrame());

		IEnumerator PauseAtEndOfFrame() {
			yield return new WaitForEndOfFrame();
			Time.timeScale = 0;

			var physicsTickRate = _settings.PhysicsTickRate.Value;
			if (physicsTickRate > 0) {
				var physicsTickInterval = 1 / physicsTickRate;
				if(!Mathf.Approximately(physicsTickInterval, fixedDeltaTime)) {
					Time.fixedDeltaTime = physicsTickInterval;
				}
			}
		}
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
		photoModeButton.transform.SetSiblingIndex(2);
		if (photoModeButton.TryGetComponent<LanguageTextMeshController>(out var languageTextMeshController)) {
			languageTextMeshController.token = "Photo mode";
		}

		if (!photoModeButton.TryGetComponent<HGButton>(out var component)) {
			Debug.LogWarning("Failed to setup photo mode button");
			Destroy(photoModeButton);
			return;
		}
	
		// time scale before pausing
		var timeScale = Time.timeScale;
		component.interactable = cameraRigController && cameraRigController.localUserViewer != null;
		component.onClick = new Button.ButtonClickedEvent();
		component.onClick.AddListener(() => {
			pauseScreenController.DestroyPauseScreen(true);
			StartPhotoMode(timeScale);
		});
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