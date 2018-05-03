using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageProcessing
{
    public partial class MainWindow : Window
    {
        private bool fileHasBeenSaved = true;
        private Bitmap currentActiveImage;
        
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

        private void EffectsComboBox_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {

        }
    }
}