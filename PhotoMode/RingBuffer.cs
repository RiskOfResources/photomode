using System;
using System.Collections;
using System.Collections.Generic;

namespace PhotoMode;

public partial class ReplayBuffer {
   public class RingBuffer<T>(int size) : IEnumerator<T>, IEnumerable<T> where T : IDisposable {
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
}