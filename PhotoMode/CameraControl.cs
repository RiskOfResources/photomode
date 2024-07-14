using UnityEngine;

namespace PhotoMode;

public class CameraControl : MonoBehaviour, IPhotoModeUnityComponentSingleton {
   private PhotoModeSettings _settings;
   public bool IsSprinting { get; set; }
   public bool IsSlowing { get; set; }
   public static CameraControl Instance;

   public void Init(PhotoModeSettings photoModeSettings) {
      _settings = photoModeSettings;
   }

   public float GetCameraSpeed(float baseSpeed) {
      if (IsSprinting) {
         return baseSpeed * _settings.CameraSprintMultiplier.Value;
      }

      if (IsSlowing) {
         return baseSpeed * _settings.CameraSlowMultiplier.Value;
      }

      return baseSpeed;
   }

   private void Awake() {
      Instance = this;
   }

   private void Update() {
      IsSprinting = Input.GetKey(_settings.CameraSprintKey.Value.MainKey);
      IsSlowing = Input.GetKey(_settings.CameraSlowKey.Value.MainKey);
 
      if (Input.GetKeyDown(_settings.ToggleSmoothCameraKey.Value.MainKey)) {
         var newSmooth = !_settings.SmoothCamera.Value;
         _settings.SmoothCamera.Value = newSmooth;
         PhotoModeHud.Instance.DisplayAndFadeOutText($"Smooth Camera: {newSmooth}");
      }
   }

   private void OnDestroy() {
      Instance = null;
   }
}