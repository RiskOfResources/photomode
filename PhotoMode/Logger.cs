using UnityEngine;

namespace PhotoMode;

public static class Logger {
   public delegate void LOG(object message);
   public static LOG Log = Debug.Log;
}