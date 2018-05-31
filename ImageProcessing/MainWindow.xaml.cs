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
        private Bitmap currentActiveImage;
        private Bitmap imageWithoutChanges;
        private bool fileHasBeenSaved = true;
        private bool isMonochromatic;
        private TextBox lineElementAngle;
        private TextBox lineElementLength;
        private TextBox maskSizeTextBox;
        private TextBox orderdNumberTextBox;
        private int selected = 0;
        private TextBox stdDeviationTextBox;

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
            var dlg = new OpenFileDialog();
            dlg.Filter = "";

            var codecs = ImageCodecInfo.GetImageEncoders();
            var sep = string.Empty;

            foreach (var c in codecs)
            {
                var codecName = c.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                dlg.Filter = string.Format("{0}{1}{2} ({3})|{3}", dlg.Filter, sep, codecName, c.FilenameExtension);
                sep = "|";
            }

            dlg.DefaultExt = ".png";

            if (fileHasBeenSaved)
            {
                if (dlg.ShowDialog() == true)
                {
                    var fileName = dlg.FileName;

                    try
                    {
                        currentActiveImage = new Bitmap(fileName);
                        Img.Source = new BitmapImage(new Uri(fileName));
                        SaveImageMenuItem.IsEnabled = true;
                        fileHasBeenSaved = false;
                        isMonochromatic = ImageProcessing.MonochromaticValidation(currentActiveImage);
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
                        currentActiveImage = new Bitmap(fileName);
                        Img.Source = new BitmapImage(new Uri(fileName));
                        SaveImageMenuItem.IsEnabled = true;
                        fileHasBeenSaved = false;
                        isMonochromatic = ImageProcessing.MonochromaticValidation(currentActiveImage);
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
            imageWithoutChanges = ImageProcessing.CopyImage(currentActiveImage);
            Revert.IsEnabled = true;
        }

        private void Exit_MenuItemClick(object sender, RoutedEventArgs e)
        {
            if (!fileHasBeenSaved)
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
                    enc.Frames.Add(BitmapFrame.Create(Convert(currentActiveImage)));
                    enc.Save(outStream);
                    var bitmap = new Bitmap(outStream);
                }

                encoder.Frames.Add(BitmapFrame.Create(Convert(currentActiveImage)));
                using (var stream = dlg.OpenFile())
                {
                    encoder.Save(stream);
                }

                fileHasBeenSaved = true;
            }
        }

        private BitmapImage Convert(Bitmap bmp)
        {
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

            if (selectedOption == "Histogram equalization to Gaussian function")
            {
                selected = 1;
                var stdDevLabel = new Label {Content = "Standard deviation:"};

                EffectOptions.Children.Add(stdDevLabel);

                stdDeviationTextBox = new TextBox();
                stdDeviationTextBox.PreviewTextInput += AllowsOnlyNumeric;
                EffectOptions.Children.Add(stdDeviationTextBox);
            }
            else if (selectedOption == "Ordfilt2")
            {
                selected = 2;
                var maskSizeLabel = new Label {Content = "Mask size:"};
                EffectOptions.Children.Add(maskSizeLabel);

                maskSizeTextBox = new TextBox();
                maskSizeTextBox.PreviewTextInput += AllowsOnlyNumeric;
                EffectOptions.Children.Add(maskSizeTextBox);


                var orderNumLabel = new Label {Content = "Order number:"};
                EffectOptions.Children.Add(orderNumLabel);

                orderdNumberTextBox = new TextBox();
                orderdNumberTextBox.PreviewTextInput += AllowsOnlyNumeric;
                EffectOptions.Children.Add(orderdNumberTextBox);
            }
            else if (selectedOption == "Opening by line as structuring element")
            {
                selected = 3;
                var lineElementLengthLabel = new Label {Content = "Line element length:"};
                EffectOptions.Children.Add(lineElementLengthLabel);

                lineElementLength = new TextBox();
                lineElementLength.PreviewTextInput += AllowsOnlyNumeric;
                EffectOptions.Children.Add(lineElementLength);

                var lineElementAngleLabel = new Label {Content = "Line element angle:"};
                EffectOptions.Children.Add(lineElementAngleLabel);

                lineElementAngle = new TextBox();
                lineElementAngle.PreviewTextInput += AllowsOnlyNumeric;
                EffectOptions.Children.Add(lineElementAngle);
            }
            else if (selectedOption == "Filling the gaps in image's objects")
            {
                selected = 4;
            }
        }

        private static void AllowsOnlyNumeric(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            var regex = new Regex("[^0-9\\.,]+");
            textCompositionEventArgs.Handled = regex.IsMatch(textCompositionEventArgs.Text);
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (selected == 1)
            {
                double.TryParse(stdDeviationTextBox.Text, out var stdDeviation);
                if (isMonochromatic)
                {
                    var result =
                        ImageProcessing.ImageHistogramGaussNormalizationMonochromatic(currentActiveImage, stdDeviation, 8);
                    Img.Source = Convert(result);
                }
                else
                {
                    var result = ImageProcessing.ImageHistogramGaussianNormalizationRGB(currentActiveImage, stdDeviation, 8);
                    Img.Source = Convert(result);
                }
            }
            else if (selected == 2)
            {
                int.TryParse(maskSizeTextBox.Text, out var maskSize);
                int.TryParse(orderdNumberTextBox.Text, out var orderNumber);

                if (orderNumber == 0 || maskSize == 0)
                {
                    MessageBox.Show("Invalid value of order number or mask size.\nValues must be greater than zero.", "Value Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (orderNumber > maskSize * maskSize)
                {
                    MessageBox.Show("Invalid value of order number.\nValue of order number must be within the range of mask matrix range.", "Value Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (maskSize % 2 == 0)
                {
                    MessageBox.Show("Invalid mask size.\nSize of mask must be odd number.", "Value Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (isMonochromatic)
                {
                    var result = ImageProcessing.ImageOrdfilt2Monochromatic(currentActiveImage, maskSize, orderNumber);
                    Img.Source = Convert(result);
                    currentActiveImage = result;
                }
                else
                {
                    var result = ImageProcessing.ImageOrdfilt2RBG(currentActiveImage, maskSize, orderNumber);
                    Img.Source = Convert(result);
                    currentActiveImage = result;
                }
            }
            else if (selected == 3)
            {
                if (!isMonochromatic)
                {
                    currentActiveImage = ImageProcessing.Monochromatic(currentActiveImage);
                    isMonochromatic = true;
                }
                int.TryParse(lineElementAngle.Text, out var lineAngel);
                int.TryParse(lineElementLength.Text, out var lineLength);
                var result = ImageProcessing.ImageOpeningByLineStructuralElement(currentActiveImage, lineAngel, lineLength);
                Img.Source = Convert(result);
                currentActiveImage = result;
            }
            else if (selected == 4)
            {
            }
            else if (selected == 0)
            {
                MessageBox.Show("Please select editing option", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var messageBoxResult =
                    MessageBox.Show("Unknown Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (messageBoxResult == MessageBoxResult.OK) Application.Current.Shutdown();
            }
        }

        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            currentActiveImage = ImageProcessing.CopyImage(imageWithoutChanges);
            Img.Source = Convert(currentActiveImage);
        }
    }
}