using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.MessageBox;

namespace ImageProcessing
{
    public partial class MainWindow : Window
    {
        private bool fileHasBeenSaved = true;
        private Bitmap currentActiveImage;
        bool isMonochromatic = false;
        int selected = 0;
        
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
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "";

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            string sep = string.Empty;

            foreach (var c in codecs)
            {
                string codecName = c.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                dlg.Filter = String.Format("{0}{1}{2} ({3})|{3}", dlg.Filter, sep, codecName, c.FilenameExtension);
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
                        RecentlyOpenedMenuItem.IsEnabled = true;
                        fileHasBeenSaved = false;
                        isMonochromatic = ImageProcess.monochromaticValidation(currentActiveImage);
                    }
                    catch (NotSupportedException)
                    {
                        MessageBox.Show("Nieobsługiwanny format obrazu", "Ostrzeżenie", MessageBoxButton.OK,MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                if (MessageBox.Show("Chcesz otworzyć nowy obraz bez zapisania starego?", "Bez zapisu", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (dlg.ShowDialog() == true)
                    {
                        var fileName = dlg.FileName;

                        try
                        {
                            currentActiveImage = new Bitmap(fileName);
                            Img.Source = new BitmapImage(new Uri(fileName));
                            SaveImageMenuItem.IsEnabled = true;
                            RecentlyOpenedMenuItem.IsEnabled = true;
                            fileHasBeenSaved = false;
                            isMonochromatic = ImageProcess.monochromaticValidation(currentActiveImage);
                        }
                        catch (NotSupportedException)
                        {
                            MessageBox.Show("Nieobsługiwanny format obrazu", "Ostrzeżenie", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    SaveCurrentActiveImage();
                }
            }

        }

        private void Exit_MenuItemClick(object sender, RoutedEventArgs e)
        {
            if(!fileHasBeenSaved)
                if (MessageBox.Show("Chcesz wyjść bez zapisania pliku?", "Wyjście bez zapisu", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    SaveCurrentActiveImage();
                }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void SaveImage_MenuItemClick(object sender, RoutedEventArgs e)
        {
            SaveCurrentActiveImage();
        }

        private void SaveCurrentActiveImage()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "";

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            string sep = string.Empty;

            foreach (var c in codecs)
            {
                string codecName = c.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                dlg.Filter = String.Format("{0}{1}{2} ({3})|{3}", dlg.Filter, sep, codecName, c.FilenameExtension);
                sep = "|";
            }
            
            dlg.DefaultExt = ".png";

            if (dlg.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();

                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(Convert(currentActiveImage)));
                    enc.Save(outStream);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);
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
                var stdDevLabel = new Label();
                
                stdDevLabel.Content = "Standard deviation:";
                EffectOptions.Children.Add(stdDevLabel);

                var stdDev = new TextBox();
                stdDev.PreviewTextInput += AllowsOnlyNumeric;
                EffectOptions.Children.Add(stdDev);
            }

            if (selectedOption == "Ordfilt2")
            {
                selected = 2;
                var maskSizeLabel = new Label();
                maskSizeLabel.Content = "Mask size:";
                EffectOptions.Children.Add(maskSizeLabel);

                var maskSize = new TextBox();
                maskSize.PreviewTextInput += AllowsOnlyNumeric;
                EffectOptions.Children.Add(maskSize);

                
                var orderNumLabel = new Label();
                orderNumLabel.Content = "Order number:";
                EffectOptions.Children.Add(orderNumLabel);

                var orderdNum = new TextBox();
                orderdNum.PreviewTextInput += AllowsOnlyNumeric;
                EffectOptions.Children.Add(orderdNum);
            }

            if (selectedOption == "Opening with circle structuring element")
            {
                selected = 3;
                var circleRadiusLabel = new Label();
                circleRadiusLabel.Content = "Circle radius:";
                EffectOptions.Children.Add(circleRadiusLabel);

                var circleRadius = new TextBox();
                circleRadius.PreviewTextInput += AllowsOnlyNumeric;
                EffectOptions.Children.Add(circleRadius);
            }

            if (selectedOption == "Image segmentation")
            {
                selected = 4;               
            }
            Apply.IsEnabled = true;
        }

        private void AllowsOnlyNumeric(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            Regex regex = new Regex("[^0-9\\.,]+");
            textCompositionEventArgs.Handled = regex.IsMatch(textCompositionEventArgs.Text);
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if(selected == 1)
            {
                if (isMonochromatic)
                {
                    var result = ImageProcess.ImageHistogramGaussNormalizationMonochromatic(currentActiveImage, 0.01);
                    Img.Source = Convert(result);
                }
                else
                {
                    var result = ImageProcess.ImageHistogramGaussianNormalizationRGB(currentActiveImage, 0.01);
                    Img.Source = Convert(result);
                }
            }
            else if (selected == 2)
            {

            }
            else if (selected == 3)
            {

            }
            else if (selected == 4)
            {

            }
            else
            {
                /**
                 * Should never happend
                 */

                var messageBoxResult = MessageBox.Show("Nieznany błąd", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
}