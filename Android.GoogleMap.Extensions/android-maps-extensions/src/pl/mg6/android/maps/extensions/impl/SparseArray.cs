using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace J2i.Net.Sparse
{
    public class SparseArrayInt : SparseArray<int>
    {
        public SparseArrayInt(int chunkSize):base(chunkSize)
        {
            IsEmptyValue = (x) => x == 0;
        }

        public SparseArrayInt():this(256)
        {
        }
    }

    public class SparseArrayDouble:SparseArray<double>
    {
        public SparseArrayDouble(int chunkSize):base(chunkSize)
        {
            IsEmptyValue = (x) => x == 0d;
        }

        public SparseArrayDouble():this(256)
        {
            
        }
    }

    public class SparseArray<T> where T : new()
    {

        public int NewChunkSize { get; set;  }

        public int ChunkSize { get; private set;  }

        private int _lastAccessedIndex = -1;

        public int Length { get; private set; }

        public static T EmptyValue { get; private set; }

        public delegate bool IsEmptyValueDelegate(T v);

        public IsEmptyValueDelegate IsEmptyValue;

        static SparseArray()
        {
            EmptyValue = new T();            
        }

        public SparseArray():this(256)
        {
        }

        public SparseArray(int chunkSize)
        {
            Length = -0;
            ChunkSize = chunkSize;
            IsEmptyValue = (x) => { return EmptyValue.Equals(x); };
        }

        public SparseArray(int chunkSize, int initialChunkCount):this(chunkSize)
        {
            ChunkList = new List<ArrayChunk>(initialChunkCount);
        }

        private ArrayChunk _currentArrayChunk = null;

        public int ChunkCount
        {
            get { return ChunkList.Count; }
        }


        public class ArrayChunk
        {
            //internal bool IsDirty { get; set;  }
            public int StartIndex { get; set;  }

            public int EndIndex
            {
                get
                {
                    if (Data == null)
                        return -1;
                    return StartIndex +  Data.Length-1;
                }
            }
            public T[] Data { get; set;  }
        }

        private List<ArrayChunk> ChunkList { get; set;  }

        public T this[int index]
        {
            get 
            { 
                
                var workingChunk = FindChunkContainingIndex(index);
                if (workingChunk != null)
                    return workingChunk.Data[index - workingChunk.StartIndex];
                return EmptyValue;
            }
            set 
            { 
                var workingChunk = FindChunkContainingIndex(index);
                if((workingChunk==null)&&(EmptyValue.Equals(value)))
                    return;
                workingChunk = FindChunkContainingIndex(index, true);
                workingChunk.Data[index - workingChunk.StartIndex] = value;

                if (index > (Length-1))
                    Length = index + 1;
            }
        }

        protected ArrayChunk FindChunkContainingIndex(int index, bool createIfNeeded = false)
        {
            if ((ChunkList == null) || (ChunkList.Count == 0))
            {
                if(createIfNeeded)
                    (ChunkList  ?? (ChunkList = new List<ArrayChunk>())).Add(new ArrayChunk(){Data=new T[ChunkSize], StartIndex = (index - index % ChunkSize)});
                else
                    return null;
            }
            if ((_lastAccessedIndex == -1) || (_currentArrayChunk == null))
            {
                _currentArrayChunk = ChunkList[_lastAccessedIndex = 0];
            }

            if ((index >= _currentArrayChunk.StartIndex) && (index <= _currentArrayChunk.EndIndex))
                return _currentArrayChunk;
            else if (index < _currentArrayChunk.StartIndex)
            {
                var currentIndex = _lastAccessedIndex;
                while (true)// (index < ChunkList[currentIndex].StartIndex)
                {
                    if(currentIndex==0)
                    {
                        if(createIfNeeded)
                        {
                            _lastAccessedIndex = currentIndex;
                            _currentArrayChunk = new ArrayChunk() { Data = new T[ChunkSize], StartIndex = index - (index % ChunkSize) };
                            ChunkList.Insert(_lastAccessedIndex, _currentArrayChunk);
                            return _currentArrayChunk;                            
                        }
                        return null;
                    }
                    --currentIndex;
                    if (currentIndex < 0)
                        return null;
                    if ((ChunkList[currentIndex].StartIndex <= index) && (ChunkList[currentIndex].EndIndex >= index))
                    {
                        _lastAccessedIndex = currentIndex;
                        _currentArrayChunk = ChunkList[currentIndex];
                        return _currentArrayChunk;
                    }
                    if(ChunkList[currentIndex].EndIndex<index)
                    {
                        if (createIfNeeded)
                        {
                            _lastAccessedIndex = currentIndex + 1;
                            _currentArrayChunk = new ArrayChunk() { Data = new T[ChunkSize], StartIndex = index - (index % ChunkSize) };
                            ChunkList.Insert(_lastAccessedIndex, _currentArrayChunk);
                            return _currentArrayChunk;
                        }
                        else
                            return null;
                    }
                    if ((ChunkList[currentIndex].StartIndex > index)&&(currentIndex==0))
                    {
                        if(createIfNeeded)
                        {
                            _lastAccessedIndex = currentIndex ;
                            _currentArrayChunk = new ArrayChunk() { Data = new T[ChunkSize] , StartIndex = index - (index % ChunkSize)};
                            ChunkList.Insert(_lastAccessedIndex, _currentArrayChunk);
                            return _currentArrayChunk;
                        }
                        else
                            return null;
                    }
                        
                }
            }
            else if (index > _currentArrayChunk.EndIndex)
            {
                var currentIndex = _lastAccessedIndex;
                while (true)//(index > ChunkList[currentIndex].EndIndex)
                {
                    ++currentIndex;
                    if (currentIndex > ChunkList.Count - 1)
                    {
                        if (createIfNeeded)
                        {
                            _lastAccessedIndex = currentIndex ;
                            _currentArrayChunk = new ArrayChunk() { Data = new T[ChunkSize], StartIndex = index - (index % ChunkSize)};
                            ChunkList.Insert(_lastAccessedIndex, _currentArrayChunk);
                            return _currentArrayChunk;
                        }
                        else
                            return null;
                    }
                    if ((ChunkList[currentIndex].StartIndex <= index) && (ChunkList[currentIndex].EndIndex >= index))
                    {
                        _lastAccessedIndex = currentIndex;
                        _currentArrayChunk = ChunkList[currentIndex];
                        return _currentArrayChunk;
                    }
                    if ((ChunkList[currentIndex].StartIndex>index))
                    {
                        _lastAccessedIndex = currentIndex;
                        _currentArrayChunk = new ArrayChunk() { Data = new T[ChunkSize], StartIndex = index - (index % ChunkSize) };
                        ChunkList.Insert(_lastAccessedIndex, _currentArrayChunk);
                        return _currentArrayChunk;
                        
                    }
                    if (ChunkList[currentIndex].EndIndex < index)
                    {
                        if(createIfNeeded)
                        {
                            _lastAccessedIndex = currentIndex;
                            _currentArrayChunk = new ArrayChunk() { Data = new T[ChunkSize], StartIndex = index - (index % ChunkSize)};
                            ChunkList.Insert(_lastAccessedIndex, _currentArrayChunk);
                            return _currentArrayChunk;
                        }
                        else
                            return null;
                    }
                        
                }
            }
            return null; //never called
        }

        public void Condense()
        {
            for(int i=ChunkList.Count-1;i>=0;--i)
            {
                bool removeCurrentChunk = true;
                var currentChunk = ChunkList[i];
                for(int k=0;(k<currentChunk.Data.Length)&&(removeCurrentChunk);++k)
                {
                    if (!IsEmptyValue(currentChunk.Data[k]))
                        removeCurrentChunk = false;
                }
                if(removeCurrentChunk)
                {
                    ChunkList.RemoveAt(i);
                }
            }
            _lastAccessedIndex = 0;
            _currentArrayChunk = null;
        }

    }
}
