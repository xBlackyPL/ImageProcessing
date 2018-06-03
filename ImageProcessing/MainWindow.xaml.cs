using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ImageProcessingApp
{
    public partial class MainWindow : Window
    {
        private Bitmap _currentActiveImage;
        private bool _fileHasBeenSaved = true;
        private Bitmap _imageWithoutChanges;
        private bool _isMonochromatic;
        private TextBox _lineElementAngle;
        private TextBox _lineElementLength;
        private TextBox _maskSizeTextBox;
        private TextBox _orderdNumberTextBox;
        private int _selected;
        private TextBox _stdDeviationTextBox;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Img.Height = Height - 5;
        }

        private void LoadFile_MenuItemClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog {Filter = ""};

            var codecs = ImageCodecInfo.GetImageEncoders();
            var sep = string.Empty;

            foreach (var c in codecs)
            {
                var codecName = c.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                dlg.Filter = string.Format("{0}{1}{2} ({3})|{3}", dlg.Filter, sep, codecName, c.FilenameExtension);
                sep = "|";
            }

            dlg.DefaultExt = ".png";

            if (_fileHasBeenSaved)
            {
                if (dlg.ShowDialog() == true)
                {
                    var fileName = dlg.FileName;

                    try
                    {
                        _currentActiveImage = new Bitmap(fileName);
                        Img.Source = new BitmapImage(new Uri(fileName));
                        SaveImageMenuItem.IsEnabled = true;
                        _fileHasBeenSaved = false;
                        _isMonochromatic = ImageProcessing.MonochromaticValidation(_currentActiveImage);
                    }
                    catch (NotSupportedException)
                    {
                        MessageBox.Show("Unknown image format", "Warning", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                if (MessageBox.Show("Open new image without saving changes on current one?", "Warning",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (dlg.ShowDialog() != true) return;
                    var fileName = dlg.FileName;

                    try
                    {
                        _currentActiveImage = new Bitmap(fileName);
                        Img.Source = new BitmapImage(new Uri(fileName));
                        SaveImageMenuItem.IsEnabled = true;
                        _fileHasBeenSaved = false;
                        _isMonochromatic = ImageProcessing.MonochromaticValidation(_currentActiveImage);
                    }
                    catch (NotSupportedException)
                    {
                        MessageBox.Show("Unknown image format", "Warning", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                else
                {
                    SaveCurrentActiveImage();
                }
            }

            Apply.IsEnabled = true;
            _imageWithoutChanges = ImageProcessing.CopyImage(_currentActiveImage);
            Revert.IsEnabled = true;
        }

        private void Exit_MenuItemClick(object sender, RoutedEventArgs e)
        {
            if (!_fileHasBeenSaved)
                if (MessageBox.Show("Do you want to exit without saving image?", "Exit without saving",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    Application.Current.Shutdown();
                else
                    SaveCurrentActiveImage();
            else
                Application.Current.Shutdown();
        }

        private void SaveImage_MenuItemClick(object sender, RoutedEventArgs e)
        {
            SaveCurrentActiveImage();
        }

        private void SaveCurrentActiveImage()
        {
            var dlg = new SaveFileDialog {Filter = ""};

            var codecs = ImageCodecInfo.GetImageEncoders();
            var sep = string.Empty;

            foreach (var c in codecs)
            {
                var codecName = c.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                dlg.Filter = string.Format("{0}{1}{2} ({3})|{3}", dlg.Filter, sep, codecName, c.FilenameExtension);
                sep = "|";
            }

            dlg.DefaultExt = ".png";

            if (dlg.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();

                using (var outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(Convert(_currentActiveImage)));
                    enc.Save(outStream);
                    var bitmap = new Bitmap(outStream);
                }

                encoder.Frames.Add(BitmapFrame.Create(Convert(_currentActiveImage)));
                using (var stream = dlg.OpenFile())
                {
                    encoder.Save(stream);
                }

                _fileHasBeenSaved = true;
            }
        }

        private static BitmapImage Convert(Bitmap bmp)
        {
            if (bmp == null) throw new ArgumentNullException(nameof(bmp));
            using (var memory = new MemoryStream())
            {
                bmp.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private void EffectsComboBox_DropDownClosed(object sender, EventArgs e)
        {
            var selectedOption = EffectsComboBox.SelectionBoxItem.ToString();
            EffectOptions.Children.Clear();

            switch (selectedOption)
            {
                case "Histogram equalization to Gaussian function":
                    _selected = 1;
                    var stdDevLabel = new Label {Content = "Standard deviation:"};

                    EffectOptions.Children.Add(stdDevLabel);

                    _stdDeviationTextBox = new TextBox();
                    _stdDeviationTextBox.PreviewTextInput += AllowsOnlyNumeric;
                    EffectOptions.Children.Add(_stdDeviationTextBox);
                    break;
                case "Ordfilt2":
                    _selected = 2;
                    var maskSizeLabel = new Label {Content = "Mask size:"};
                    EffectOptions.Children.Add(maskSizeLabel);

                    _maskSizeTextBox = new TextBox();
                    _maskSizeTextBox.PreviewTextInput += AllowsOnlyNumeric;
                    EffectOptions.Children.Add(_maskSizeTextBox);


                    var orderNumLabel = new Label {Content = "Order number:"};
                    EffectOptions.Children.Add(orderNumLabel);

                    _orderdNumberTextBox = new TextBox();
                    _orderdNumberTextBox.PreviewTextInput += AllowsOnlyNumeric;
                    EffectOptions.Children.Add(_orderdNumberTextBox);
                    break;
                case "Opening by line as structuring element":
                    _selected = 3;
                    var lineElementLengthLabel = new Label {Content = "Line element length:"};
                    EffectOptions.Children.Add(lineElementLengthLabel);

                    _lineElementLength = new TextBox();
                    _lineElementLength.PreviewTextInput += AllowsOnlyNumeric;
                    EffectOptions.Children.Add(_lineElementLength);

                    var lineElementAngleLabel = new Label {Content = "Line element angle:"};
                    EffectOptions.Children.Add(lineElementAngleLabel);

                    _lineElementAngle = new TextBox();
                    _lineElementAngle.PreviewTextInput += AllowsOnlyNumeric;
                    EffectOptions.Children.Add(_lineElementAngle);
                    break;
                case "Filling the gaps in image's objects":
                    _selected = 4;
                    break;
                default:
                    _selected = 0;
                    break;
            }
        }

        private static void AllowsOnlyNumeric(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            var regex = new Regex("[^0-9,]+");
            textCompositionEventArgs.Handled = regex.IsMatch(textCompositionEventArgs.Text);
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            switch (_selected)
            {
                case 1:
                    double.TryParse(_stdDeviationTextBox.Text, out var stdDeviation);
                    if (_isMonochromatic)
                    {
                        var result =
                            ImageProcessing.ImageHistogramGaussNormalizationMonochromatic(_currentActiveImage,
                                stdDeviation,
                                8);
                        Img.Source = Convert(result);
                        _currentActiveImage = result;
                    }
                    else
                    {
                        var result =
                            ImageProcessing.ImageHistogramGaussianNormalizationRGB(_currentActiveImage, stdDeviation,
                                8);
                        Img.Source = Convert(result);
                        _currentActiveImage = result;
                    }

                    break;
                case 2:
                    int.TryParse(_maskSizeTextBox.Text, out var maskSize);
                    int.TryParse(_orderdNumberTextBox.Text, out var orderNumber);

                    if (orderNumber == 0 || maskSize == 0)
                    {
                        MessageBox.Show(
                            "Invalid value of order number or mask size.\nValues must be greater than zero.",
                            "Value Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (orderNumber > maskSize * maskSize)
                    {
                        MessageBox.Show(
                            "Invalid value of order number.\nValue of order number must be within the range of mask matrix range.",
                            "Value Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    if (maskSize % 2 == 0)
                    {
                        MessageBox.Show("Invalid mask size.\nSize of mask must be odd number.", "Value Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    if (_isMonochromatic)
                    {
                        var result =
                            ImageProcessing.ImageOrdfilt2Monochromatic(_currentActiveImage, maskSize, orderNumber);
                        Img.Source = Convert(result);
                        _currentActiveImage = result;
                    }
                    else
                    {
                        var result = ImageProcessing.ImageOrdfilt2RBG(_currentActiveImage, maskSize, orderNumber);
                        Img.Source = Convert(result);
                        _currentActiveImage = result;
                    }

                    break;
                case 3:
                {
                    if (!_isMonochromatic)
                    {
                        _currentActiveImage = ImageProcessing.Monochromatic(_currentActiveImage);
                        _isMonochromatic = true;
                    }

                    int.TryParse(_lineElementAngle.Text, out var lineAngel);
                    int.TryParse(_lineElementLength.Text, out var lineLength);
                    var result =
                        ImageProcessing.ImageOpeningByLineStructuralElement(_currentActiveImage, lineAngel, lineLength);
                    Img.Source = Convert(result);
                    _currentActiveImage = result;
                    break;
                }
                case 4:
                {
                    if (!_isMonochromatic)
                    {
                        _currentActiveImage = ImageProcessing.Monochromatic(_currentActiveImage);
                        _isMonochromatic = true;
                    }

                    var result = ImageProcessing.ImageFillHoles(_currentActiveImage);
                    Img.Source = Convert(result);
                    _currentActiveImage = result;
                    break;
                }
                case 0:
                    MessageBox.Show("Please select editing option", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    break;
                default:
                    var messageBoxResult =
                        MessageBox.Show("Unknown Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (messageBoxResult == MessageBoxResult.OK) Application.Current.Shutdown();
                    break;
            }
        }

        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            _currentActiveImage = ImageProcessing.CopyImage(_imageWithoutChanges);
            Img.Source = Convert(_currentActiveImage);
        }
    }
}