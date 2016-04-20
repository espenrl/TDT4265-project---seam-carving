using System;
using System.Collections.Generic;

namespace erl
{
    public static class SeamCarvingHelper
    {
        public static Seam GetSeamOfLowestEnergy(ImageMatrix<byte> energyMatrix, ImageMatrix<int> fitnessMatrix, Direction direction)
        {
            switch (direction)
            {
                case Direction.Height:
                    return GetHorizontalSeamOfLowestEnergy(energyMatrix, fitnessMatrix);
                case Direction.Width:
                    return GetVerticalSeamOfLowestEnergy(energyMatrix, fitnessMatrix);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private static Seam GetVerticalSeamOfLowestEnergy(ImageMatrix<byte> energyMatrix, ImageMatrix<int> fitnessMatrix)
        {
            var height = energyMatrix.Height;
            var width = energyMatrix.Width;

            // copy first row
            for (var x = 0; x < width; x++)
            {
                fitnessMatrix[x, 0] = energyMatrix[x, 0];
            }

            // calculate fitness - skip first row
            for (var y = 1; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    fitnessMatrix[x, y] = energyMatrix[x, y];

                    if (x == 0)
                    {
                        fitnessMatrix[x, y] += Math.Min(fitnessMatrix[x, y - 1], fitnessMatrix[x + 1, y - 1]);
                    }
                    else if (x == width - 1)
                    {
                        fitnessMatrix[x, y] += Math.Min(fitnessMatrix[x, y - 1], fitnessMatrix[x - 1, y - 1]);
                    }
                    else
                    {
                        fitnessMatrix[x, y] += Math.Min(fitnessMatrix[x - 1, y - 1], Math.Min(fitnessMatrix[x, y - 1], fitnessMatrix[x + 1, y - 1]));
                    }
                }
            }

            // get min
            var lastRowIndex = height - 1;
            var minValue = int.MaxValue;
            var minIndex = -1;
            for (var x = 0; x < width; x++)
            {
                if (fitnessMatrix[x, lastRowIndex] < minValue)
                {
                    minValue = fitnessMatrix[x, lastRowIndex];
                    minIndex = x;
                }
            }

            // get seam
            var list = new List<Pixel>
            {
                new Pixel(minIndex, lastRowIndex)
            };

            var currentIndex = minIndex;
            for (var y = lastRowIndex - 1; y >= 0; y--)
            {
                var x = currentIndex;
                if (currentIndex == 0)
                {
                    if (fitnessMatrix[currentIndex, y] > fitnessMatrix[currentIndex + 1, y])
                    {
                        x = currentIndex + 1;
                    }
                }
                else if (currentIndex == width - 1)
                {
                    if (fitnessMatrix[currentIndex, y] > fitnessMatrix[currentIndex - 1, y])
                    {
                        x = currentIndex - 1;
                    }
                }
                else
                {
                    if (fitnessMatrix[currentIndex, y] > fitnessMatrix[currentIndex - 1, y] && fitnessMatrix[currentIndex - 1, y] < fitnessMatrix[currentIndex + 1, y])
                    {
                        x = currentIndex - 1;
                    }
                    else if (fitnessMatrix[currentIndex, y] > fitnessMatrix[currentIndex + 1, y])
                    {
                        x = currentIndex + 1;
                    }
                }

                currentIndex = x;
                list.Add(new Pixel(x, y));
            }

            return new Seam(Direction.Width, list.ToArray());
        }

        private static Seam GetHorizontalSeamOfLowestEnergy(ImageMatrix<byte> energyMatrix, ImageMatrix<int> fitnessMatrix)
        {
            var height = energyMatrix.Height;
            var width = energyMatrix.Width;

            // copy first column
            for (var y = 0; y < height; y++)
            {
                fitnessMatrix[0, y] = energyMatrix[0, y];
            }

            // calculate fitness - skip first row
            for (var x = 1; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    fitnessMatrix[x, y] = energyMatrix[x, y];

                    if (y == 0)
                    {
                        fitnessMatrix[x, y] += Math.Min(fitnessMatrix[x - 1, y], fitnessMatrix[x - 1, y + 1]);
                    }
                    else if (y == height - 1)
                    {
                        fitnessMatrix[x, y] += Math.Min(fitnessMatrix[x - 1, y], fitnessMatrix[x - 1, y - 1]);
                    }
                    else
                    {
                        fitnessMatrix[x, y] += Math.Min(fitnessMatrix[x - 1, y - 1], Math.Min(fitnessMatrix[x - 1, y], fitnessMatrix[x - 1, y + 1]));
                    }
                }
            }

            // get min
            var lastColumnIndex = width - 1;
            var minValue = int.MaxValue;
            var minIndex = -1;
            for (var y = 0; y < height; y++)
            {
                if (fitnessMatrix[lastColumnIndex, y] < minValue)
                {
                    minValue = fitnessMatrix[lastColumnIndex, y];
                    minIndex = y;
                }
            }

            // get seam
            var list = new List<Pixel>
            {
                new Pixel(lastColumnIndex, minIndex)
            };

            var currentIndex = minIndex;
            for (var x = lastColumnIndex - 1; x >= 0; x--)
            {
                var y = currentIndex;
                if (currentIndex == 0)
                {
                    if (fitnessMatrix[x, currentIndex] > fitnessMatrix[x, currentIndex + 1])
                    {
                        y = currentIndex + 1;
                    }
                }
                else if (currentIndex == height - 1)
                {
                    if (fitnessMatrix[x, currentIndex] > fitnessMatrix[x, currentIndex - 1])
                    {
                        y = currentIndex - 1;
                    }
                }
                else
                {
                    if (fitnessMatrix[x, currentIndex] > fitnessMatrix[x, currentIndex - 1] && fitnessMatrix[x, currentIndex - 1] < fitnessMatrix[x, currentIndex + 1])
                    {
                        y = currentIndex - 1;
                    }
                    else if (fitnessMatrix[x, currentIndex] > fitnessMatrix[x, currentIndex + 1])
                    {
                        y = currentIndex + 1;
                    }
                }

                currentIndex = y;
                list.Add(new Pixel(x, y));
            }

            return new Seam(Direction.Height, list.ToArray());
        }

        public static void ComputeEnergyMapBySobel(ImageMatrix<byte> grayscaleImageMatrix, ImageMatrix<byte> energyMatrix)
        {
            var height = grayscaleImageMatrix.Height;
            var width = grayscaleImageMatrix.Width;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var p1 = GetPixel(grayscaleImageMatrix, x - 1, y - 1);
                    var p2 = GetPixel(grayscaleImageMatrix, x, y - 1);
                    var p3 = GetPixel(grayscaleImageMatrix, x + 1, y - 1);

                    var p4 = GetPixel(grayscaleImageMatrix, x - 1, y);
                    var p6 = GetPixel(grayscaleImageMatrix, x + 1, y);

                    var p7 = GetPixel(grayscaleImageMatrix, x - 1, y + 1);
                    var p8 = GetPixel(grayscaleImageMatrix, x, y + 1);
                    var p9 = GetPixel(grayscaleImageMatrix, x + 1, y + 1);

                    var sobelX = p1 + p2 + p2 + p3 - p7 - p8 - p8 - p9;
                    var sobelY = p3 + p6 + p6 + p9 - p1 - p4 - p4 - p7;
                    var sobel = (int) Math.Sqrt(sobelX*sobelX + sobelY*sobelY);

                    const byte limit = 255;

                    energyMatrix[x, y] = sobel > limit ? limit : (byte) sobel;
                }
            }
        }

        private static byte GetPixel(ImageMatrix<byte> matrix, int x, int y)
        {
            var width = matrix.Width;
            var height = matrix.Height;

            var x1 = x;
            var y1 = y;

            if (x1 < 0) x1 = 0;
            if (x1 > width - 1) x1 = width - 1;
            if (y1 < 0) y1 = 0;
            if (y1 > height - 1) y1 = height - 1;

            x = x1;
            y = y1;

            return matrix[x, y];
        }
    }
}