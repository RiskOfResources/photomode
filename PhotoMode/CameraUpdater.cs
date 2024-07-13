using System;
using RoR2;
using UnityEngine;

namespace PhotoMode;

public class CameraUpdater : MonoBehaviour, ICameraStateProvider {
   private static event EventHandler<PhotoModeCameraState> OnCameraStateUpdate; 
   private PhotoModeCameraState _cameraState;
   private bool _disableAllMovement;
   private CameraRigController _cameraRigController;

   public PhotoModeCameraState Init(CameraRigController cameraRigController, PhotoModeSettings settings) {
      _cameraRigController = cameraRigController;
      cameraRigController.SetOverrideCam(this, 0f);
      cameraRigController.enableFading = false;
      _disableAllMovement = settings.DisableAllMovement.Value;

      return new PhotoModeCameraState {
         position = cameraRigController.sceneCam.transform.position,
         rotation = Quaternion.LookRotation(cameraRigController.sceneCam.transform.rotation * Vector3.forward),
         fov = cameraRigController.sceneCam.fieldOfView
      };
   }
 
   public static void UpdateCameraState(CameraStateUpdateMessage msg) {
      OnCameraStateUpdate?.Invoke(null, msg.CameraState);
   }

   private void Awake() {
      OnCameraStateUpdate += OnOnCameraStateUpdate;
   }

   private void OnOnCameraStateUpdate(object sender, PhotoModeCameraState e) {
      _cameraState = e;
   }

   private void Update() {
      if (!_cameraRigController?.sceneCam || _disableAllMovement) {
         return;
      }
 
      _cameraRigController.sceneCam.transform.position = _cameraState.position;
      _cameraRigController.sceneCam.transform.rotation = _cameraState.rotation;
      _cameraRigController.sceneCam.fieldOfView = _cameraState.fov;
   }

   private void OnDestroy() {
      OnCameraStateUpdate -= OnCameraStateUpdate;
      _cameraRigController.enableFading = true;
      _cameraRigController.SetOverrideCam(null);
      var cameraTransform = _cameraRigController.sceneCam.transform;
      cameraTransform.localPosition = Vector3.zero;
      cameraTransform.localRotation = Quaternion.identity;
   }
 
   public void GetCameraState(CameraRigController _, ref CameraState cameraState)
   {
   }

   public bool IsHudAllowed(CameraRigController _)
   {
      return false;
   }

   public bool IsUserControlAllowed(CameraRigController _)
   {
      return false;
   }

   public bool IsUserLookAllowed(CameraRigController _)
   {
      return false;
   }
}