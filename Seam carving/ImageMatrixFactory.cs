using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace erl
{
    public static class ImageMatrixFactory
    {
        public static ImageMatrix<T> Empty<T>(int width, int height, OptimizeMode optimizeMode)
        {
            if (optimizeMode == OptimizeMode.Width)
            {
                var data = new T[height][];
                for (var y = 0; y < height; y++)
                {
                    data[y] = new T[width];
                }
                return new WidthOptimizedImageMatrix<T>(data, width, height);
            }
            else
            {
                var data = new T[width][];
                for (var x = 0; x < width; x++)
                {
                    data[x] = new T[height];
                }
                return new HeightOptimizedImageMatrix<T>(data, width, height);
            }

        }

        public static ImageMatrix<Color> FromBitmapSource(BitmapSource bitmap, OptimizeMode optimizeMode)
        {
            var height = bitmap.PixelHeight;
            var width = bitmap.PixelWidth;
            var stride = bitmap.PixelWidth * 4;
            var size = bitmap.PixelHeight * stride;

            var pixelData = new byte[size];
            bitmap.CopyPixels(pixelData, stride, 0);



            var matrix = Empty<Color>(width, height, optimizeMode);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var idx = x * 4 + y * width * 4;

                    var blue = pixelData[idx];
                    var green = pixelData[idx + 1];
                    var red = pixelData[idx + 2];
                    var alpha = pixelData[idx + 3];

                    matrix[x, y] = Color.FromArgb(alpha, red, green, blue);
                }
            }

            return matrix;
        }
    }
}