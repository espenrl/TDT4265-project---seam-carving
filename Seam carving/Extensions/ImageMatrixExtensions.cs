using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace erl
{
    public static class ImageMatrixExtensions
    {
        public static void CopyRowOrColumnTo<T>(this ImageMatrix<T> sourceMatrix, ImageMatrix<T> targetMatrix, int rowOrColumnIndex, int count, int sourceIndex, int targetIndex)
        {
            if (count == 0) return;

            Array.Copy(sourceMatrix[rowOrColumnIndex], sourceIndex, targetMatrix[rowOrColumnIndex], targetIndex, count);
        }

        public static void Map<TS, TT>(this ImageMatrix<TS> sourceMatrix, ImageMatrix<TT> targetMatrix, Func<TS, TT> valueTransform)
        {
            for (var x = 0; x < sourceMatrix.Width; x++)
            {
                for (var y = 0; y < sourceMatrix.Height; y++)
                {
                    targetMatrix[x, y] = valueTransform(sourceMatrix[x, y]);
                }
            }
        }

        public static void RemoveSeam<T>(this ImageMatrix<T> matrix, Seam seam) where T : struct, IEquatable<T>, IFormattable
        {
            switch (seam.Direction)
            {
                case Direction.Height:
                    Parallel.ForEach(seam.Pixels, p => { matrix.CopyRowOrColumnTo(matrix, p.X, matrix.Height - p.Y - 1, p.Y + 1, p.Y); });
                    break;
                case Direction.Width:
                    Parallel.ForEach(seam.Pixels, p => { matrix.CopyRowOrColumnTo(matrix, p.Y, matrix.Width - p.X - 1, p.X + 1, p.X); });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static BitmapSource ToBitmapSource(this ImageMatrix<Color> matrix)
        {
            var height = matrix.Height;
            var width = matrix.Width;
            var dpi = 96;

            var pixelData = new byte[height*width*4];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var color = matrix[x, y];
                    var idx = x*4 + y*width*4;

                    pixelData[idx] = color.B;
                    pixelData[idx + 1] = color.G;
                    pixelData[idx + 2] = color.R;
                    pixelData[idx + 3] = 255;
                }
            }

            var bitmap = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixelData, width*4);
            bitmap.Freeze();
            return bitmap;
        }

        public static BitmapSource ToBitmapSource(this ImageMatrix<byte> matrix)
        {
            var height = matrix.Height;
            var width = matrix.Width;
            var dpi = 96;

            var pixelData = new byte[height*width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var idx = x + y*width;

                    pixelData[idx] = matrix[x, y];
                }
            }

            var bitmap = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Gray8, null, pixelData, width);
            bitmap.Freeze();
            return bitmap;
        }

        public static void ToGrayScale(this ImageMatrix<Color> sourceMatrix, ImageMatrix<byte> targetMatrix)
        {
            sourceMatrix.Map(targetMatrix, c => (byte) (0.2126*c.R + 0.7152*c.G + 0.0722*c.B));
        }
    }
}