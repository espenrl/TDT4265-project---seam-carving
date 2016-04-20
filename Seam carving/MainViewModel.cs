using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.CommandWpf;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace erl
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private BitmapSource _inputImage;
        private BitmapSource _biasImage;
        private BitmapSource _energyFunctionImage;
        private BitmapSource _energyFunctionWithBiasImage;
        private BitmapSource _outputImage;
        private bool _carveWidth;
        private bool _carveHeight;
        private string _inputImageInfoStr;
        private int _dimensionPixels;

        private ImageMatrix<byte> _energyMatrix;

        public MainViewModel()
        {
            CarveWidth = true;
        }

        public BitmapSource InputImage
        {
            get { return _inputImage; }
            set
            {
                if (Equals(value, _inputImage)) return;
                _inputImage = value;
                OnPropertyChanged();
            }
        }

        public BitmapSource BiasImage
        {
            get { return _biasImage; }
            set
            {
                if (Equals(value, _biasImage)) return;
                _biasImage = value;
                OnPropertyChanged();
            }
        }

        public BitmapSource EnergyFunctionImage
        {
            get { return _energyFunctionImage; }
            set
            {
                if (Equals(value, _energyFunctionImage)) return;
                _energyFunctionImage = value;
                OnPropertyChanged();
            }
        }

        public BitmapSource EnergyFunctionWithBiasImage
        {
            get { return _energyFunctionWithBiasImage; }
            set
            {
                if (Equals(value, _energyFunctionWithBiasImage)) return;
                _energyFunctionWithBiasImage = value;
                OnPropertyChanged();
            }
        }

        public BitmapSource OutputImage
        {
            get { return _outputImage; }
            set
            {
                if (Equals(value, _outputImage)) return;
                _outputImage = value;
                OnPropertyChanged();
            }
        }

        public bool CarveWidth
        {
            get { return _carveWidth; }
            set
            {
                if (value == _carveWidth) return;
                _carveWidth = value;
                OnPropertyChanged();

                if (value)
                {
                    DimensionPixels = InputImage?.PixelWidth ?? 0;
                    DimensionName = "width";
                }
            }
        }

        public bool CarveHeight
        {
            get { return _carveHeight; }
            set
            {
                if (value == _carveHeight) return;
                _carveHeight = value;
                OnPropertyChanged();

                if (value)
                {
                    DimensionPixels = InputImage?.PixelHeight ?? 0;
                    DimensionName = "height";
                }
            }
        }

        public string InputImageInfoStr
        {
            get { return _inputImageInfoStr; }
            private set
            {
                if (value == _inputImageInfoStr) return;
                _inputImageInfoStr = value;
                OnPropertyChanged();
            }
        }

        public int DimensionPixels
        {
            get { return _dimensionPixels; }
            set
            {
                if (value == _dimensionPixels) return;
                _dimensionPixels = value;
                OnPropertyChanged();
            }
        }

        public string DimensionName
        {
            get { return _dimensionName; }
            set
            {
                if (value == _dimensionName) return;
                _dimensionName = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenImageCommand => new RelayCommand(OpenImage, CanOpenImage);

        private async void OpenImage()
        {
#if DEBUG
            var file = @"D:\test.jpg";
#else
            var dialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                DefaultExt = ".jpeg",
                Filter = "Image files|*.jpg;*.jpeg;*.png;*.gif|All files (*.*)|*.*"
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult != true) return;
            var file = dialog.FileName;
#endif

            _isLoading = true;
            try
            {
                var image = await Task.Run(() =>
                {
                    using (var fileStream = File.Open(file, FileMode.Open))
                    {
                        var img = new BitmapImage();
                        img.BeginInit();
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.StreamSource = fileStream;
                        img.EndInit();
                        img.Freeze();

                        return img;
                    }
                });

                InputImage = image;
                OutputImage = image;

                var imageMatrix = ImageMatrixFactory.FromBitmapSource(InputImage, OptimizeMode.Width);
                var grayscaleImageMatrix = ImageMatrixFactory.Empty<byte>(imageMatrix.Width, imageMatrix.Height, imageMatrix.OptimizeMode);
                imageMatrix.ToGrayScale(grayscaleImageMatrix);
                var energyMatrix = ImageMatrixFactory.Empty<byte>(imageMatrix.Width, imageMatrix.Height, imageMatrix.OptimizeMode);
                SeamCarvingHelper.ComputeEnergyMapBySobel(grayscaleImageMatrix, energyMatrix);

                _energyMatrix = energyMatrix;
                EnergyFunctionImage = energyMatrix.ToBitmapSource();
                EnergyFunctionWithBiasImage = EnergyFunctionImage;

                InputImageInfoStr = $"Width: {image.PixelWidth}, Height: {image.PixelHeight}";

                DimensionPixels = CarveWidth ? image.PixelWidth : image.PixelHeight;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                _isLoading = false;
                ((RelayCommand)OpenImageCommand).RaiseCanExecuteChanged();
            }
        }

        private bool _isLoading;
        private bool CanOpenImage()
        {
            if (_isLoading) return false;

            return true;
        }

        public ICommand SeamCarveCommand => new RelayCommand(SeamCarve, CanSeamCarve);

        private async void SeamCarve()
        {
            _isCarving = true;

            try
            {
                var optimizeMode = CarveWidth ? OptimizeMode.Width : OptimizeMode.Height;
                var imageMatrix = ImageMatrixFactory.FromBitmapSource(InputImage, optimizeMode);

                var optimizedEnergyMatrix = ImageMatrixFactory.Empty<byte>(_energyMatrix.Width, _energyMatrix.Height, optimizeMode);
                for (var x = 0; x < _energyMatrix.Width; x++)
                {
                    for (var y = 0; y < _energyMatrix.Height; y++)
                    {
                        optimizedEnergyMatrix[x, y] = _energyMatrix[x, y];
                    }
                }

                var seamCarve = new SeamCarving(imageMatrix, optimizedEnergyMatrix);

                var direction = CarveWidth ? Direction.Width : Direction.Height;
                var dimensionCount = CarveWidth ? imageMatrix.Width : imageMatrix.Height;
                var pixelRemoveCount = dimensionCount - DimensionPixels;

                var resultImage = await Task.Run(() =>
                {
                    for (var i = 0; i < pixelRemoveCount; i++)
                    {
                        seamCarve.CarveSingle(direction);
                    }
                    return seamCarve.ImageMatrix.ToBitmapSource();
                });

                OutputImage = resultImage;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                _isCarving = false;
                ((RelayCommand)SeamCarveCommand).RaiseCanExecuteChanged();
            }
        }

        private bool _isCarving;
        private string _dimensionName;

        private bool CanSeamCarve()
        {
            if (InputImage == null) return false;
            if (EnergyFunctionWithBiasImage == null) return false;
            if (_isCarving) return false;

            return true;
        }

        public ICommand UpdateBiasCommand => new RelayCommand<InkCanvas>(UpdateBias, CanUpdateBias);

        private void UpdateBias(InkCanvas canvas)
        {
            const double dpi = 96d;
            var width = InputImage.PixelWidth;
            var height = InputImage.PixelHeight;
            var stride = width * 4;
            var size = height * stride;

            // render inkcanvas to bitmap
            var renderTargetBitmap = ToRenderTargetBitmap(canvas, width, height, dpi);

            // convert to bgra32
            var pixelData = new byte[size];
            CopyPixelArrayAsBgra(renderTargetBitmap, pixelData, stride);

            // create bias data
            var biasData = new PixelBias[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var idx = x * 4 + y * width * 4;

                    if (pixelData[idx + 1] > 0)
                    {
                        biasData[x, y] = PixelBias.Include;
                    }
                    else if (pixelData[idx + 2] > 0)
                    {
                        biasData[x, y] = PixelBias.Exclude;
                    }
                    else
                    {
                        biasData[x, y] = PixelBias.None;
                    }
                }
            }

            // create bias image
            var bData = new byte[size];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var idx = x * 4 + y * width * 4;

                    switch (biasData[x, y])
                    {
                        case PixelBias.None:
                            bData[idx] = 255;
                            bData[idx + 1] = 0;
                            bData[idx + 2] = 0;
                            bData[idx + 3] = 0;
                            break;
                        case PixelBias.Include:
                            bData[idx] = 255;
                            bData[idx + 1] = 255;
                            bData[idx + 2] = 255;
                            bData[idx + 3] = 255;
                            break;
                        case PixelBias.Exclude:
                            bData[idx] = 0;
                            bData[idx + 1] = 0;
                            bData[idx + 2] = 0;
                            bData[idx + 3] = 255;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            // create energy function with bias
            var energyData = new byte[size];
            CopyPixelArrayAsBgra(EnergyFunctionImage, energyData, stride);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var idx = x * 4 + y * width * 4;

                    switch (biasData[x, y])
                    {
                        case PixelBias.Include:
                            energyData[idx] = 255;
                            energyData[idx + 1] = 255;
                            energyData[idx + 2] = 255;
                            energyData[idx + 3] = 255;
                            break;
                        case PixelBias.Exclude:
                            energyData[idx] = 0;
                            energyData[idx + 1] = 0;
                            energyData[idx + 2] = 0;
                            energyData[idx + 3] = 255;
                            break;
                    }
                }
            }

            // update energy matrix
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    switch (biasData[x, y])
                    {
                        case PixelBias.Include:
                            _energyMatrix[x, y] = 255;
                            break;
                        case PixelBias.Exclude:
                            _energyMatrix[x, y] = 0;
                            break;
                    }
                }
            }

            BiasImage = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, bData, stride);
            EnergyFunctionWithBiasImage = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, energyData, stride);
        }

        private static void CopyPixelArrayAsBgra(BitmapSource bitmap, byte[] pixelData, int stride)
        {
            var bgraBitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            bgraBitmap.CopyPixels(pixelData, stride, 0);
        }

        private static RenderTargetBitmap ToRenderTargetBitmap(FrameworkElement canvas, int width, int height, double dpi)
        {
            var renderTargetBitmap = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Default);

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var brush = new VisualBrush(canvas);
                context.DrawRectangle(brush,
                    null,
                    new Rect(new Point(), new Size(canvas.ActualWidth, canvas.ActualHeight)));
            }

            visual.Transform = new ScaleTransform(width / canvas.ActualWidth, height / canvas.ActualHeight);

            renderTargetBitmap.Render(visual);
            return renderTargetBitmap;
        }

        private bool CanUpdateBias(InkCanvas canvas)
        {
            if (canvas == null) return false;
            if (InputImage == null) return false;

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private enum PixelBias
        {
            None,
            Include,
            Exclude
        }
    }
}