﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C5
{
    namespace Linq
    {
        public static class EnumeratorExtension
        {
            static EnumeratorExtension()
            {
                
            }
         
            
        }
    }

    [Serializable]
    public class Enumerator<T> : IEnumerator<T>,
        IEnumerable<T>
    {
        private ArrayBase<T> _internalList;

        private int _index;
        private int _theStamp;
        private int _end;
        private T current;

        private static int mainThreadId;
        // If called in the non main thread, will return false;
        private static bool IsMainThread
        {
            get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId; }
        }

        public Enumerator(ArrayBase<T> list)
        {
            _internalList = list;
            mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        internal void UpdateReference(ArrayBase<T> list, int start, int end, int theStamp)
        {
            _index = start;
            _end = end;
            _internalList = list;
            current = default(T);
            _theStamp = theStamp;
        }


        public void Dispose()
        {
            //Do nothing
        }

        //public override int GetHashCode()
        //{
        //    uint h1 = (uint) _internalList.GetHashCode();

        //    int h = (int) ((h1*1529784657 + 1) ^ (h1*2912831877) ^ (h1*1118771817 + 2));
        //    return h;
        //}

        public bool MoveNext()
        {
            ArrayBase<T> list = _internalList;

            if (list.stamp != _theStamp)
                throw new CollectionModifiedException();

            if (_index < _end)
            {
                current = list[_index];
                _index++;

                return true;
            }

            current = default(T);
            return false;
        }

        public void Reset()
        {
            _index = 0;
            current = default(T);
            _end = 0;
        }

        public T Current
        {
            get { return current; }
        }

        object IEnumerator.Current
        {
            get { return current; }
        }

        public Enumerator<T> Clone()
        {
            var enumerator = new Enumerator<T>(_internalList)
            {
                current = default(T),

            };
            return enumerator;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var enumerator = !IsMainThread ? Clone() : this;
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [Serializable]
    public class WhereEnumerator<T> : EnumerableBase<T>, IEnumerator<T>, IEnumerable<T>
    {
        private ArrayBase<T> _internalList;

        private T current;
        private int _index;
        private int _theStamp;
        private int _end;

        private int _state;

        private Enumerator<T> _wrapperEnumerator;
        private Func<T, bool> _predicate;

        static int mainThreadId;
        // If called in the non main thread, will return false;
        static bool IsMainThread
        {
            get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId; }
        }

        public WhereEnumerator()
        {
            mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            _state = -2;
        }
        internal void UpdateReference(ArrayBase<T> list, int start, int end, int theStamp, Func<T, bool> predicate)
        {
            _predicate = predicate;

            _internalList = list;
            _index = start;
            _end = end;

            current = default(T);
            _theStamp = theStamp;
        }


        public void Dispose()
        {
            //Do nothing
        }

        public bool MoveNext()
        {
            var list = _internalList;

            if (list.stamp != _theStamp)
                throw new CollectionModifiedException();

            if (_state == -2)
                _wrapperEnumerator = (Enumerator<T>)list.GetEnumerator();
            else
                _wrapperEnumerator.UpdateReference(list, _index, _end, _theStamp);



            if (_index < _end)
            {
                _state = 1;
                while (_wrapperEnumerator.MoveNext())
                {
                    var temp = _wrapperEnumerator.Current;
                    _index++;
                    if (_predicate(temp))
                    {
                        current = temp;
                        return true;
                    }
                }
            }

            current = default(T);
            return false;
        }

        public void Reset()
        {
            _index = 0;
            current = default(T);
            _end = 0;
        }

        public T Current { get { return current; } }

        object IEnumerator.Current
        {
            get { return current; }
        }

        public WhereEnumerator<T> Clone()
        {
            var enumerator = new WhereEnumerator<T>
            {
                _internalList = _internalList,
                current = default(T),

            };
            return enumerator;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            var enumerator = !IsMainThread ? Clone() : this;
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
