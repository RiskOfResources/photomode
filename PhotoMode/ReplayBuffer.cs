using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhotoMode;

public class ReplayBuffer : MonoBehaviour {
   // settings
   private float _duration;
   private float _resolutionScale;
   private bool _exportLinear;

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
      var framerate = settings.ReplayBufferFramerate.Value;
      _interval = 1 / framerate;

      // Options.AddSaveReplayBuffer(() => {
      //    StartCoroutine(WriteFiles());
      // });
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
      if (Input.GetKeyDown(KeyCode.F9)) {
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

   private class RingBuffer<T>(int size) : IEnumerator<T>, IEnumerable<T> where T : IDisposable {
      private T[] _buffer = new T[size];
      private int _startIndex, _endIndex;
      private int _currentIndex = -1;

      public void Put(T item) {
         if (_buffer[_endIndex] != null) {
            _buffer[_endIndex].Dispose();
         }

         _buffer[_endIndex] = item;
         _endIndex++;
         _endIndex %= _buffer.Length;

         // buffer full
         if (_endIndex <= _startIndex) {
            _startIndex++;
         }

         _startIndex %= _buffer.Length;
      }

      public bool MoveNext() {
         if (_currentIndex == -1) {
            _currentIndex = _startIndex;
            return _startIndex != _endIndex;
         }

         _currentIndex = (_currentIndex + 1) % _buffer.Length;
         return _currentIndex != _endIndex;
      }

      public void Reset() {
         _currentIndex = -1;
      }

      public T Current => _buffer[_currentIndex];

      object IEnumerator.Current => Current;

      public void Dispose() {
         foreach (var item in _buffer) {
            if (item != null) {
               item.Dispose();
            }
         }

         _buffer = new T[size];
         _currentIndex = -1;
      }

      IEnumerator<T> IEnumerable<T>.GetEnumerator() {
         return this;
      }

      public IEnumerator GetEnumerator() {
         return this;
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