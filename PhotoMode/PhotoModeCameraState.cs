using System;
using RoR2;
using UnityEngine;

namespace PhotoMode;

public struct PhotoModeCameraState {
   public CameraState State;

   public Quaternion rotation {
      get => State.rotation;
      set => State.rotation = value;
   }
   public Vector3 position {
      get => State.position;
      set => State.position = value;
   }
   public float fov {
      get => State.fov;
      set => State.fov = value;
   }
   
    public static PhotoModeCameraState Lerp(ref PhotoModeCameraState a, ref PhotoModeCameraState b, float t)
    {
      return new PhotoModeCameraState {
        position = Vector3.LerpUnclamped(a.position, b.position, t),
        rotation = Quaternion.SlerpUnclamped(a.rotation, b.rotation, t),
        fov = Mathf.LerpUnclamped(a.fov, b.fov, t),
        FocusDistance = Mathf.LerpUnclamped(a.FocusDistance, b.FocusDistance, t)
      };
    }

   public float FocusDistance;
   
   // for multipoint dolly, so we don't lose track of the checkpoints
   public Tuple<PhotoModeCameraState, PhotoModeCameraState> ControlPoints;
}