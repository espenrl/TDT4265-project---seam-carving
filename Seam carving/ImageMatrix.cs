using System;
using System.Diagnostics;

namespace erl
{
    [DebuggerDisplay("{Width}, {Height}")]
    public abstract class ImageMatrix<T>
    {
        protected readonly T[][] Data;

        protected ImageMatrix(T[][] data, int width, int height)
        {
            Data = data;
            Width = width;
            Height = height;
        }

        public OptimizeMode OptimizeMode { get; protected set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public abstract T this[int x, int y] { get; set; }

        public T[] this[int rowOrcolumnIndex] => Data[rowOrcolumnIndex];

        public void ReuseAsSubset(int width, int height)
        {
            if(width > Width) throw new InvalidOperationException();
            if(height > Height) throw new InvalidOperationException();

            Width = width;
            Height = height;
        }
    }

    public class WidthOptimizedImageMatrix<T> : ImageMatrix<T>
    {
        public WidthOptimizedImageMatrix(T[][] data, int width, int height) : base(data, width, height)
        {
            OptimizeMode = OptimizeMode.Width;
        }

        public override T this[int x, int y]
        {
            get { return Data[y][x]; }
            set { Data[y][x] = value; }
        }
    }

    public class HeightOptimizedImageMatrix<T> : ImageMatrix<T>
    {
        public HeightOptimizedImageMatrix(T[][] data, int width, int height) : base(data, width, height)
        {
            OptimizeMode = OptimizeMode.Height;
        }

        public override T this[int x, int y]
        {
            get { return Data[x][y]; }
            set { Data[x][y] = value; }
        }
    }
}