using BepInEx;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using RoR2.UI.SkinControllers;
using UnityEngine;
using UnityEngine.UI;

namespace PhotoMode;

[BepInPlugin("com.riskofresources.discohatesme.photomode", "PhotoMode", "3.0.3")]
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

		On.RoR2.CameraRigController.OnEnable += (orig, self) => {
			orig.Invoke(self);
			cameraRigController = self;
		};
		On.RoR2.CameraRigController.OnDisable += (orig, self) => {
			orig.Invoke(self);
			cameraRigController = null;
		};
		On.RoR2.UI.PauseScreenController.Awake += (orig, self) => {
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
			pauseScreenController.gameObject.SetActive(false);
			EnterPhotoMode();
		});
	}

	private void EnterPhotoMode() {
		var pmGo = new GameObject("PhotoModeController");
		var controller = pmGo.AddComponent<PhotoModeController>();
		controller.EnterPhotoMode(_settings, cameraRigController);
	}
}