using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhotoMode;

// TODO refactor these 2 playback methods into a single method 
public class DollyService(bool smoothDolly, float dollyCamSpeed, Easing easing) {
   private float CamSpeed => CameraControl.Instance.GetCameraSpeed(dollyCamSpeed);

   public IEnumerator DollyPlayback(List<PhotoModeCameraState> dollyStates, Action<PhotoModeCameraState> onCameraChange) {
      var dollyIndex = 0;
      var linearCamera = dollyStates[0];

      while (dollyIndex < dollyStates.Count - 1) {
         var prevState = dollyStates[dollyIndex];
         var nextState = dollyStates[dollyIndex + 1];
         var totalDistance = Vector3.Distance(prevState.position, nextState.position);

         // don't divide by 0
         if (Mathf.Approximately(totalDistance, 0)) {
            yield break;
         }

         var distance = CamSpeed * Time.unscaledDeltaTime;
         linearCamera.position = Vector3.MoveTowards(linearCamera.position, nextState.position, distance);
         var linearDistance = Vector3.Distance(linearCamera.position, nextState.position);
         var movePct = (totalDistance - linearDistance) / totalDistance;
         var eased = GetEasedRatio(movePct, easing);
         eased = Mathf.Clamp01(eased);
         var currentState = PhotoModeCameraState.Lerp(ref prevState, ref nextState, eased);
         if (Mathf.Approximately(linearDistance, 0)) {
            dollyIndex++;
         }

         UpdateCamera(currentState, onCameraChange);
         yield return null;
      }
   }

   public IEnumerator MultiPointDollyPlayback(List<PhotoModeCameraState> positionCurve, Action<PhotoModeCameraState> onCameraChange) {
      var segmentLengths = new float[positionCurve.Count - 1];
      var totalLength = 0f;
      for (int i = 0; i < positionCurve.Count - 1; i++) {
         segmentLengths[i] = Vector3.Distance(positionCurve[i].position, positionCurve[i + 1].position);
         totalLength += segmentLengths[i];
      }

      var distanceTraveled = 0f;
      while (distanceTraveled < totalLength) {
         distanceTraveled += CamSpeed * Time.unscaledDeltaTime;
         var linearProgress = Mathf.Clamp01(distanceTraveled / totalLength);
         var easedProgress = GetEasedRatio(linearProgress, easing);
         var targetDistance = easedProgress * totalLength;

         var index = 0;
         var currentSegmentDist = 0f;

         for (int i = 0; i < segmentLengths.Length; i++) {
            if (targetDistance <= currentSegmentDist + segmentLengths[i]) {
               index = i;
               break;
            }

            currentSegmentDist += segmentLengths[i];
            index = i;
         }

         var segmentProgress = (targetDistance - currentSegmentDist) / Mathf.Max(segmentLengths[index], 0.0001f);
         var prevState = positionCurve[index];
         var nextState = positionCurve[index + 1];
         var currentState = PhotoModeCameraState.Lerp(ref prevState, ref nextState, segmentProgress);

         // if we're not using a perfectly smooth dolly we can apply the easings to smooth out rotations
         if (!smoothDolly) {
            var (currentControlPoint, nextControlPoint) = prevState.ControlPoints;
            var controlPointDistance = Vector3.Distance(currentControlPoint.position, nextControlPoint.position);
 
            if (!Mathf.Approximately(controlPointDistance, 0)) {
               var controlPointPct = (controlPointDistance - Vector3.Distance(currentState.position, nextControlPoint.position)) / controlPointDistance;
               var easedRot = Mathf.Clamp01(GetEasedRatio(controlPointPct, easing));
               currentState.rotation = Quaternion.Slerp(currentControlPoint.rotation, nextControlPoint.rotation, easedRot);
            }
         }

         UpdateCamera(currentState, onCameraChange);
         yield return null;
      }
   }

   private void UpdateCamera(PhotoModeCameraState currentState, Action<PhotoModeCameraState> onCameraChange) {
      CameraUpdater.UpdateCameraState(new CameraStateUpdateMessage {
         CameraState = currentState,
         Priority = UpdatePriority.Dolly
      });

      onCameraChange?.Invoke(currentState);
   }

   public static float GetEasedRatio(float x, Easing e) {
      switch (e) {
         case Easing.Linear:
            return x;
         case Easing.SineIn:
            return 1 - (float)Math.Cos(x * Math.PI / 2);
         case Easing.EaseInOutSine:
            return (float)(-(Math.Cos(Math.PI * x) - 1) / 2);
         case Easing.EaseInOutCubic:
            return (float)(x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2);
         case Easing.EaseInOutQuad:
            return (float)(x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2);
         case Easing.SineOut:
            return (float)Math.Sin(x * Math.PI / 2);
         case Easing.QuadIn:
            return x * x;
         case Easing.QuadOut:
            return 1 - (1 - x) * (1 - x);
         case Easing.CubicIn:
            return x * x * x;
         case Easing.CubicOut:
            return 1 - (1 - x) * (1 - x) * (1 - x);
         case Easing.Reverse:
            return 1 - x;
         default:
            Logger.Log("Invalid easing set, falling back to linear");
            return x;
      }
   }
}