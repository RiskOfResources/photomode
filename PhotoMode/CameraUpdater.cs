using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace PhotoMode;

public class CameraUpdater : MonoBehaviour, ICameraStateProvider {
   private static event EventHandler<CameraStateUpdateMessage> OnCameraStateUpdate; 
   private readonly PriorityQueue<CameraStateUpdateMessage> _cameraUpdates = new();
   private bool _disableAllMovement;
   private bool _followDollyRotation;
   private bool _autoFocus;
   private PhotoModeSettings _settings;
   private CameraRigController _cameraRigController;

   public PhotoModeCameraState Init(CameraRigController cameraRigController, PhotoModeSettings settings) {
      _settings = settings;
      _followDollyRotation = settings.DollyFollowRotation.Value;
      _disableAllMovement = settings.DisableAllMovement.Value;
      _autoFocus = settings.DollyFollowsFocus.Value;
      _cameraRigController = cameraRigController;
      cameraRigController.SetOverrideCam(this, 0f);
      cameraRigController.enableFading = false;

      return new PhotoModeCameraState {
         position = cameraRigController.sceneCam.transform.position,
         rotation = Quaternion.LookRotation(cameraRigController.sceneCam.transform.rotation * Vector3.forward),
         fov = cameraRigController.sceneCam.fieldOfView,
         FocusDistance = _settings.PostProcessFocusDistance.Value,
      };
   }
 
   public static void UpdateCameraState(CameraStateUpdateMessage msg) {
      OnCameraStateUpdate?.Invoke(null, msg);
   }

   private void Awake() {
      OnCameraStateUpdate += CameraStateUpdate;
   }

   private void CameraStateUpdate(object sender, CameraStateUpdateMessage e) {
      _cameraUpdates.Enqueue(e, e.Priority);
      PhotoModeHud.Instance.ShowCameraStatus(e.CameraState);
   }

   private void LateUpdate() {
      if (!_cameraRigController?.sceneCam || _disableAllMovement) {
         return;
      }

      foreach (var updates in _cameraUpdates.Dequeue()) {
         var cameraState = updates.CameraState;
         _cameraRigController.sceneCam.transform.position = cameraState.position;

         if (updates.Priority != UpdatePriority.Dolly || _followDollyRotation) {
            _cameraRigController.sceneCam.transform.rotation = cameraState.rotation;
         }

         if (updates.Priority != UpdatePriority.Dolly || _autoFocus) {
            _settings.PostProcessFocusDistance.Value = cameraState.FocusDistance;
         }
 
         _cameraRigController.sceneCam.fieldOfView = cameraState.fov;
         _cameraRigController.currentCameraState.fov = cameraState.fov;
      }
   }

   private void OnDestroy() {
      OnCameraStateUpdate -= CameraStateUpdate;
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

   private class PriorityQueue<T> {
      private readonly Queue<T>[] _queue;

      public PriorityQueue() {
         var values = Enum.GetValues(typeof(UpdatePriority));
         _queue = new Queue<T>[values.Length];
 
         foreach (var p in values) {
            _queue[(int) p] = new Queue<T>();
         }
      }

      public void Enqueue(T state, UpdatePriority priority) {
         _queue[(int)priority].Enqueue(state);
      }

      public IEnumerable<T> Dequeue() {
         foreach (var p in Enum.GetValues(typeof(UpdatePriority))) {
            var states = _queue[(int)p];
            if (states.Count > 0) {
               yield return states.Dequeue();
            }
         }
      }
   }
}