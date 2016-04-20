using System.Windows.Media;

namespace erl
{
    public class SeamCarving
    {
        private readonly ImageMatrix<byte> _energyMatrix;
        private readonly ImageMatrix<int> _fitnessMatrix;
        
        public SeamCarving(ImageMatrix<Color> imageMatrix, ImageMatrix<byte> energyMatrix)
        {
            ImageMatrix = imageMatrix;
            _energyMatrix = energyMatrix;
            _fitnessMatrix = ImageMatrixFactory.Empty<int>(energyMatrix.Width, energyMatrix.Height, energyMatrix.OptimizeMode);
        }

        public ImageMatrix<Color> ImageMatrix { get; }

        public void CarveSingle(Direction direction)
        {
            var seam = SeamCarvingHelper.GetSeamOfLowestEnergy(_energyMatrix, _fitnessMatrix, direction);

            ImageMatrix.RemoveSeam(seam);
            _energyMatrix.RemoveSeam(seam);
            _fitnessMatrix.RemoveSeam(seam);

            if (direction == Direction.Width)
            {
                ImageMatrix.ReuseAsSubset(ImageMatrix.Width - 1, ImageMatrix.Height);
                _energyMatrix.ReuseAsSubset(_energyMatrix.Width - 1, _energyMatrix.Height);
                _fitnessMatrix.ReuseAsSubset(_fitnessMatrix.Width - 1, _fitnessMatrix.Height);
            }
            else
            {
                ImageMatrix.ReuseAsSubset(ImageMatrix.Width, ImageMatrix.Height - 1);
                _energyMatrix.ReuseAsSubset(_energyMatrix.Width, _energyMatrix.Height - 1);
                _fitnessMatrix.ReuseAsSubset(_fitnessMatrix.Width, _fitnessMatrix.Height - 1);
            }
        }
    }
}