using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
   private RingBuffer<NativeArray<byte>> _natives;
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
      _natives = new RingBuffer<NativeArray<byte>>((int) (_duration * framerate));

      // Options.AddSaveReplayBuffer(() => {
      //    StartCoroutine(WriteFiles());
      // });
      enabled = true;
      _replayBufferCoroutine = StartReplayBuffer();
      StartCoroutine(_replayBufferCoroutine);
   }

   private void OnDestroy() {
      StopCoroutine(_replayBufferCoroutine);
      foreach (NativeArray<byte> buffer in _natives) {
         buffer.Dispose();
      }
   }

   private IEnumerator StartReplayBuffer() {
      while (true) {
         yield return new WaitForEndOfFrame();
         var (scale, offs) = (new Vector2(1, -1), new Vector2(0, 1));
         var (grab, flip) = (CreateRenderTexture(), CreateRenderTexture());
         ScreenCapture.CaptureScreenshotIntoRenderTexture(grab);
         Graphics.Blit(grab, flip, scale * _resolutionScale, offs);
         var size = grab.width * grab.height * 4;
         var native = new NativeArray<byte>(size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
         _natives.Put(native);
 
         AsyncGPUReadback.RequestIntoNativeArray(ref native, flip, 0, request => {
            if (request.hasError) {
               Logger.Log("GPU readback error detected.");
            }
            else {
               Logger.Log("saved?");
            }
         
            grab.Release();
            flip.Release();
         });

         yield return new WaitForSecondsRealtime(_interval);
      }
   }

   private RenderTexture CreateRenderTexture() {
      var (w, h) = ((int) (Screen.width * _resolutionScale), (int) (Screen.height * _resolutionScale));
      
      // TODO check if it's better to use sRGB for resolve's Unity sRGB to linear LUT
      var format = _exportLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;
      
      // TODO replace with RenderTexture.GetTemporary()?
      return new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, format);
   }

   private void WriteFiles() {
      var path = Application.dataPath;
      Logger.Log($"saving files to {path}");
      if (!Directory.Exists($"{path}/recordings")) {
         Directory.CreateDirectory($"{path}/recordings");
      }

      int i = 0;
      foreach (NativeArray<byte> buffer in _natives) {
         if (buffer.Length == 0) {
            continue;
         }

         var rt = CreateRenderTexture();
         try {
            using var encoded = ImageConversion.EncodeNativeArrayToPNG(buffer, rt.graphicsFormat, (uint)rt.width, (uint)rt.height);
            File.WriteAllBytes($"{path}/recordings/frame-{i++}.png", encoded.ToArray());
            buffer.Dispose();
            Thread.Sleep(100);
         }
         catch (Exception e) {
            Logger.Log($"failed to encode png {e}");
         }
      }

      Logger.Log($"Wrote {i} files to {path}/recordings");
      _natives.Reset();
   }
   
   
   private readonly object _writeLock = new();
   private bool _writing;

   private void LateUpdate() {
      if (Input.GetKeyDown(KeyCode.F9)) {
         StopCoroutine(_replayBufferCoroutine);
         if (_writing) {
            Logger.Log("Still writing previous recording, try again later");
            return;
         }

         lock (_writeLock) {
            _writing = true;

            Task.Run(() => {
               try {
                  WriteFiles();
               }
               catch (Exception e) {
                  Logger.Log($"write error {e}");
               }

               _replayBufferCoroutine = StartReplayBuffer();
               StartCoroutine(_replayBufferCoroutine);
               _writing = false;
            });
         }
      } 
   }
   

   private class RingBuffer<T>(int size) : IEnumerator<T>, IEnumerable<T> where T : IDisposable {
      private readonly T[] _buffer = new T[size];
      private int _startIndex, _endIndex;
      private int _currentIndex = -1;

      public void Put(T item) {
         _endIndex++;
         _endIndex %= _buffer.Length;

         if (_buffer[_endIndex] != null) {
            _buffer[_endIndex].Dispose();
         }

         _buffer[_endIndex] = item;

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
         _currentIndex = -1;
      }

      IEnumerator<T> IEnumerable<T>.GetEnumerator() {
         return this;
      }

      public IEnumerator GetEnumerator() {
         return this;
      }
   }
}