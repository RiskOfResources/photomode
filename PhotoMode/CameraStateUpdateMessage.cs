namespace PhotoMode;

public struct CameraStateUpdateMessage {
   public PhotoModeCameraState CameraState;
   public UpdatePriority Priority;
}

public enum UpdatePriority {
   FreeLook,
   Dolly,
}