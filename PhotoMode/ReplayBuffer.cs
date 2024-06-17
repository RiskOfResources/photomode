using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhotoMode;

public partial class ReplayBuffer : MonoBehaviour {
   // settings
   private float _duration;
   private float _resolutionScale;
   private bool _exportLinear;
   private KeyCode _saveReplayKey;

   // timing
   private float _interval;
   private RingBuffer<Frame> _replayBuffer;
   private IEnumerator _replayBufferCoroutine;
 
   public void Init(PhotoModeSettings settings) {
      enabled = false;
      if (!Options.HasRiskOfOptions || !settings.EnableReplayBuffer.Value) {
         return;
      }

      _duration = settings.ReplayBufferDuration.Value;
      _resolutionScale = settings.ReplayBufferResolutionScale.Value;
      _exportLinear = settings.ExportLinearColorSpace.Value;
      _interval = 1 / settings.ReplayBufferFramerate.Value;
      _saveReplayKey = settings.SaveReplayBuffer.Value.MainKey;

      enabled = true;
      _replayBufferCoroutine = StartReplayBuffer();
      StartCoroutine(_replayBufferCoroutine);
   }

   private void OnDestroy() {
      StopCoroutine(_replayBufferCoroutine);

      lock (_writeLock) {
         try {
            _replayBuffer.Dispose();
         }
         catch (Exception e) {
            // ignore
            Logger.Log($"error freeing memory: {e}");
         }
      }
   }

   private IEnumerator StartReplayBuffer() {
      _replayBuffer?.Dispose();
      _replayBuffer = new RingBuffer<Frame>((int) (_duration / _interval));
      while (true) {
         yield return new WaitForEndOfFrame();
         var (scale, offs) = (new Vector2(1, -1), new Vector2(0, 1));
         var (grab, flip) = (CreateRenderTexture(), CreateRenderTexture(_resolutionScale));
         ScreenCapture.CaptureScreenshotIntoRenderTexture(grab);
         Graphics.Blit(grab, flip, scale, offs);

         var size = flip.width * flip.height * 4;
         var native = new NativeArray<byte>(size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
         var copy = native;
 
         AsyncGPUReadback.RequestIntoNativeArray(ref native, flip, 0, request => {
            if (request.hasError) {
               Logger.Log("GPU readback error detected.");
            }
            else {
               _replayBuffer.Put(new Frame(false, copy));
            }
         
            RenderTexture.ReleaseTemporary(grab);
            RenderTexture.ReleaseTemporary(flip);
         });

         yield return new WaitForSecondsRealtime(_interval);
      }
   }

   private RenderTexture CreateRenderTexture(float scale = 1, bool temp = true) {
      var (w, h) = ((int) (Screen.width * scale), (int) (Screen.height * scale));
      
      // TODO check if it's better to use sRGB for resolve's Unity sRGB to linear LUT
      var format = _exportLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;
      
      return temp ? RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32, format) :  new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, format);
   }

   private void WriteFiles() {
      var path = Application.dataPath;
      Logger.Log($"saving files to {path}");
      if (!Directory.Exists($"{path}/recordings")) {
         Directory.CreateDirectory($"{path}/recordings");
      }

      int i = 0;
      var rt = CreateRenderTexture(_resolutionScale, false);
      foreach (Frame frame in _replayBuffer) {
         try {
            using var encoded = ImageConversion.EncodeNativeArrayToPNG(frame.Data, rt.graphicsFormat, (uint)rt.width, (uint)rt.height);
            File.WriteAllBytes($"{path}/recordings/frame-{i++}.png", encoded.ToArray());
         }
         catch (Exception e) {
            Logger.Log($"failed to encode png {e}");
         }
      }

      Destroy(rt);
      Logger.Log($"Wrote {i} files to {path}/recordings");
   }
   
   private readonly object _writeLock = new();
   private bool _writing;
   private bool _restartBuffer;

   private void LateUpdate() {
      if (Input.GetKeyDown(_saveReplayKey)) {
         StopCoroutine(_replayBufferCoroutine);
         if (_writing) {
            Logger.Log("Still writing previous recording, try again later");
            return;
         }


         Task.Run(() => {
            lock (_writeLock) {
               _writing = true;
               try {
                  WriteFiles();
               }
               catch (Exception e) {
                  Logger.Log($"write error {e}");
               }

               _writing = false;
               _restartBuffer = true;
            }
         });
      }
      else if(_restartBuffer) {
         _restartBuffer = false;
         _replayBufferCoroutine = StartReplayBuffer();
         StartCoroutine(_replayBufferCoroutine);
      }
   }

   private struct Frame(bool isDisposed, NativeArray<byte> data) : IDisposable {
      public NativeArray<byte> Data { get; } = data;

      public void Dispose() {
         if (!isDisposed) {
            Data.Dispose();
            isDisposed = true;
         }
      }
   }
}