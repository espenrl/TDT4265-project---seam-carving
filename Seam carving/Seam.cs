namespace erl
{
    public class Seam
    {
        public Seam(Direction direction, Pixel[] pixels)
        {
            Direction = direction;
            Pixels = pixels;
        }

        public Direction Direction { get; }
        public Pixel[] Pixels { get; }
    }
}