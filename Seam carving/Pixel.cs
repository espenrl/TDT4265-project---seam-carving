using System.Diagnostics;

namespace erl
{
    [DebuggerDisplay("{X,nq}, {Y,nq}")]
    public class Pixel
    {
        public Pixel(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }
}