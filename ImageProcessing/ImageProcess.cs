using System.Collections.Generic;
using System.Drawing;

namespace ImageProcessing
{
    internal class ImageProcess
    {
        public static bool MonochromaticValidation(Bitmap sourceImage)
        {
            for (var i = 0; i < sourceImage.Height; i++)
            for (var j = 0; i < sourceImage.Width; i++)
            {
                var pixel = sourceImage.GetPixel(j, i);
                if (pixel.R == pixel.G && pixel.R == pixel.B)
                    continue;
                return false;
            }

            return true;
        }

        public static int[] GetImageHistogramMonochromatic(Bitmap sourceImage)
        {
            var histogram = new int[256];
            for (var i = 0; i < sourceImage.Height; i++)
            for (var j = 0; j < sourceImage.Width; j++)
            {
                var pixel = sourceImage.GetPixel(j, i);
                histogram[pixel.R]++;
            }

            return histogram;
        }

        private static int[] GetHistogramCumulant(int[] sourceHistogram)
        {
            var histogramCumulant = new int[256];
            histogramCumulant[0] = sourceHistogram[0];

            for (var i = 1; i < sourceHistogram.Length; i++)
                histogramCumulant[i] = sourceHistogram[i] + histogramCumulant[i - 1];

            return histogramCumulant;
        }

        public static Bitmap ImageHistogramGaussNormalizationMonochromatic(Bitmap sourceImage, double stdDeviation)
        {
            var result = new Bitmap(sourceImage.Width, sourceImage.Height);
            var sourceHistogram = GetImageHistogramMonochromatic(sourceImage);
            var sourceHistogramCumulant = GetHistogramCumulant(sourceHistogram);

            var numberOfColorClasses = 32;

            for (var i = 0; i < 256; i++)
            {
            }

            return result;
        }

        public static Bitmap ImageHistogramGaussianNormalizationRGB(Bitmap sourceImage, double stdDeviation)
        {
            var result = new Bitmap(sourceImage.Width, sourceImage.Height);
            return result;
        }

        public static Bitmap ImageOrdfilt2Monochromatic(Bitmap sourceImage, int maskSize, int orderNumber)
        {
            var result = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (var i = 0; i < sourceImage.Height - 1; i++)
            for (var j = 0; j < sourceImage.Width - 1; j++)
            {
                var centralPoint = new Point(j, i);
                result.SetPixel(j, i, Ordfilt2GetColorMonochromatic(sourceImage, maskSize, orderNumber, centralPoint));
            }

            return result;
        }

        public static Bitmap ImageOrdfilt2RBG(Bitmap sourceImage, int maskSize, int orderNumber)
        {
            var result = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (var i = 0; i < sourceImage.Height - 1; i++)
            for (var j = 0; j < sourceImage.Width - 1; j++)
            {
                var centralPoint = new Point(j, i);
                result.SetPixel(j, i, Ordfilt2GetColorRGB(sourceImage, maskSize, orderNumber, centralPoint));
            }

            return result;
        }


        private static Color Ordfilt2GetColorMonochromatic(Bitmap sourceImage, int maskSize, int orderNumber,
            Point startingPoint)
        {
            var colorValues = new List<int>();

            for (var i = startingPoint.Y; i < startingPoint.Y + maskSize; i++)
            for (var j = startingPoint.X; j < startingPoint.X + maskSize; j++)
            {
                var colorValue = 0;

                if (j > sourceImage.Width - 1 && i > sourceImage.Height - 1)
                    colorValue = sourceImage
                        .GetPixel(j - (j - (sourceImage.Width - 1)), i - (i - (sourceImage.Height - 1))).R;
                else if (j > sourceImage.Width - 1)
                    colorValue = sourceImage.GetPixel(j - (j - (sourceImage.Width - 1)), i).R;
                else if (i > sourceImage.Height - 1)
                    colorValue = sourceImage.GetPixel(j, i - (i - (sourceImage.Height - 1))).R;
                else if (i < maskSize && j < maskSize)
                    colorValue = sourceImage.GetPixel(j + maskSize / 2, i + maskSize / 2).R;
                else if (i < maskSize)
                    colorValue = sourceImage.GetPixel(j, i + maskSize / 2).R;
                else if (j < maskSize)
                    colorValue = sourceImage.GetPixel(j + maskSize / 2, i).R;
                else
                    colorValue = sourceImage.GetPixel(j, i).R;

                colorValues.Add(colorValue);
            }

            colorValues.Sort();
            var newColor = Color.FromArgb(colorValues[orderNumber - 1], colorValues[orderNumber - 1],
                colorValues[orderNumber - 1]);
            return newColor;
        }

        private static Color Ordfilt2GetColorRGB(Bitmap sourceImage, int maskSize, int orderNumber, Point startingPoint)
        {
            var colorValuesR = new List<int>();
            var colorValuesG = new List<int>();
            var colorValuesB = new List<int>();

            for (var i = startingPoint.Y; i < startingPoint.Y + maskSize; i++)
            for (var j = startingPoint.X; j < startingPoint.X + maskSize; j++)
            {
                Color subtractedColor;

                if (j > sourceImage.Width - 1 && i > sourceImage.Height - 1)
                    subtractedColor = sourceImage.GetPixel(j - (j - (sourceImage.Width - 1)),
                        i - (i - (sourceImage.Height - 1)));
                else if (j > sourceImage.Width - 1)
                    subtractedColor = sourceImage.GetPixel(j - (j - (sourceImage.Width - 1)), i);
                else if (i > sourceImage.Height - 1)
                    subtractedColor = sourceImage.GetPixel(j, i - (i - (sourceImage.Height - 1)));
                else if (i < maskSize && j < maskSize)
                    subtractedColor = sourceImage.GetPixel(j + maskSize / 2, i + maskSize / 2);
                else if (i < maskSize)
                    subtractedColor = sourceImage.GetPixel(j, i + maskSize / 2);
                else if (j < maskSize)
                    subtractedColor = sourceImage.GetPixel(j + maskSize / 2, i);
                else
                    subtractedColor = sourceImage.GetPixel(j, i);

                colorValuesR.Add(subtractedColor.R);
                colorValuesG.Add(subtractedColor.G);
                colorValuesB.Add(subtractedColor.B);
            }

            colorValuesR.Sort();
            colorValuesG.Sort();
            colorValuesB.Sort();

            var newColor = Color.FromArgb(colorValuesR[orderNumber - 1], colorValuesG[orderNumber - 1],
                colorValuesB[orderNumber - 1]);
            return newColor;
        }
    }
}