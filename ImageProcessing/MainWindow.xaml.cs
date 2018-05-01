using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace ImageProcessing
{
    public partial class MainWindow : Window
    {
        private bool fileHasBeenSaved = true;
        private BitmapImage currentActiveImage;
        
        public MainWindow()
        {
            InitializeComponent();
            currentActiveImage = new BitmapImage();
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
                    currentActiveImage = new BitmapImage(new Uri(fileName));
                    Img.Source = currentActiveImage;
                    SaveImageMenuItem.IsEnabled = true;
                    RecentlyOpenedMenuItem.IsEnabled = true;
                    fileHasBeenSaved = false;
                }
            }
            else
            {
                if (MessageBox.Show("Chcesz otworzyć nowy obraz bez zapisania starego?", "Bez zapisu", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (dlg.ShowDialog() == true)
                    {
                        var fileName = dlg.FileName;
                        currentActiveImage = new BitmapImage(new Uri(fileName));
                        Img.Source = currentActiveImage;
                        SaveImageMenuItem.IsEnabled = true;
                        RecentlyOpenedMenuItem.IsEnabled = true;
                        fileHasBeenSaved = false;
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
                encoder.Frames.Add(BitmapFrame.Create(currentActiveImage));
                using (var stream = dlg.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
        }
    }
}