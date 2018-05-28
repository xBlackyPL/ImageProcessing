using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageProcessing
{
    internal class ImageProcess
    {
        public static bool MonochromaticValidation(Bitmap sourceImage)
        {
            for (var i = 0; i < sourceImage.Height - 1; i++)
            for (var j = 0; j < sourceImage.Width - 1; j++)
            {
                var pixel = sourceImage.GetPixel(j, i);
                if (pixel.R == pixel.G && pixel.R == pixel.B)
                    continue;
                return false;
            }

            return true;
        }

        public static Bitmap Monochromatic(Bitmap sourceImage)
        {
            var result = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (var i = 0; i < sourceImage.Height - 1; i++)
            for (var j = 0; j < sourceImage.Width - 1; j++)
            {
                var pixel = sourceImage.GetPixel(j, i);
                var colorValue = (int) (0.3 * pixel.R + 0.6 * pixel.G + 0.1 * pixel.B);
                var newColor = Color.FromArgb(colorValue, colorValue, colorValue);
                result.SetPixel(j, i, newColor);
            }

            return result;
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

        private static double[] GaussValuesPerColor(double stdDeviation)
        {
            var gaussValues = new double[256];
            double sum = 0.0;

            for (var i = 0; i < 256; i++)
            {
                gaussValues[i] = Math.Exp((-1)*((((double)i - 0.5) * ((double)i - 0.5)) /
                                          (2 * (stdDeviation * stdDeviation))));

            }



            return gaussValues;
        }

        public static Bitmap ImageHistogramGaussNormalizationMonochromatic(Bitmap sourceImage, double stdDeviation)
        {
            var result = new Bitmap(sourceImage.Width, sourceImage.Height);
            var sourceHistogram = GetImageHistogramMonochromatic(sourceImage);
            var sourceHistogramCumulant = GetHistogramCumulant(sourceHistogram);
            var gaussValuesForEachColor = GaussValuesPerColor(stdDeviation);

            foreach (var element in gaussValuesForEachColor)
            {
                Console.WriteLine(element);
            }


            var numberOfColorClasses = 16;

            
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

        public static Bitmap ImageOpeningByLineStructuralElement(Bitmap sourceImage, int angle, int size)
        {
            var structElement = LineStructuralElementGenerator(angle, size);

            foreach (var row in structElement)
            {
                foreach (var element in row) Console.Write(element + " ");
                Console.WriteLine();
            }

            var result = ImageErode(sourceImage, structElement);
            result = ImageDilate(result, structElement);
            return result;
        }

        private static int[][] LineStructuralElementGenerator(int angle, int structuralElementSize)
        {
            angle %= 180;
            var above90Deg = angle > 90;
            angle %= 91;

            var alpha = angle * Math.PI / 180;
            var columns = (int) Math.Ceiling(structuralElementSize * Math.Cos(alpha)) + 1;
            var rows = (int) Math.Ceiling(structuralElementSize * Math.Sin(alpha)) + 1;

            columns -= columns % 2 == 0 ? 1 : 0;
            rows -= rows % 2 == 0 ? 1 : 0;

            var structuralElement = new int[rows][];
            for (var i = 0; i < rows; i++) structuralElement[i] = new int[columns];

            var lineGradient = ((double) rows - 1) / ((double) columns - 1);

            if (!double.IsInfinity(lineGradient))
                for (var i = 0; i < columns; i++)
                {
                    var y = rows - lineGradient * i;
                    structuralElement[(int) y - 1][i] = 1;
                }
            else
                for (var i = 0; i < rows; i++)
                    structuralElement[i][0] = 1;

            for (var i = 0; i < rows; i++)
            {
                var correctLine = false;

                for (var j = 0; j < columns; j++)
                    if (structuralElement[i][j] == 1)
                        correctLine = true;

                if (correctLine) continue;
                var correctColumn = 0;

                if (i <= rows / 2)
                    for (var j = 0; j < columns; j++)
                    {
                        if (structuralElement[i - 1][j] != 1) continue;
                        correctColumn = j;
                        break;
                    }
                else
                    for (var j = 0; j < columns; j++)
                    {
                        if (structuralElement[i + 1][j] != 1) continue;
                        correctColumn = j;
                        break;
                    }

                structuralElement[i][correctColumn] = 1;
            }

            if (!above90Deg) return structuralElement;

            for (var i = 0; i < rows; i++)
            for (var j = 0; j < columns / 2; j++)
            {
                var tmp = structuralElement[i][j];
                structuralElement[i][j] = structuralElement[i][columns - 1 - j];
                structuralElement[i][columns - 1 - j] = tmp;
            }

            return structuralElement;
        }

        public static Bitmap ImageErode(Bitmap sourceImage, int[][] structuralElement)
        {
            var result = new Bitmap(sourceImage.Width, sourceImage.Height);
            var rows = structuralElement.Length;
            var columns = structuralElement[0].Length;

            for (var i = rows / 2 + 1; i < sourceImage.Height - rows; i++)
            for (var j = columns / 2 + 1; j < sourceImage.Width - columns; j++)
            {
                var centralPoint = new Point(j, i);
                result.SetPixel(j, i, ImageErodeGetPixelValue(sourceImage, structuralElement, centralPoint));
            }

            return result;
        }

        public static Bitmap ImageDilate(Bitmap sourceImage, int[][] structuralElement)
        {
            var result = new Bitmap(sourceImage.Width, sourceImage.Height);

            var rows = structuralElement.Length;
            var columns = structuralElement[0].Length;

            for (var i = rows / 2 + 1; i < sourceImage.Height - rows; i++)
            for (var j = columns / 2 + 1; j < sourceImage.Width - columns; j++)
            {
                var centralPoint = new Point(j, i);
                result.SetPixel(j, i, ImageDilateGetPixelValue(sourceImage, structuralElement, centralPoint));
            }

            return result;
        }

        private static Color ImageErodeGetPixelValue(Bitmap sourceImage, int[][] mask, Point startingPoint)
        {
            var pixelValues = new List<int>();
            var rows = mask.Length;
            var columns = mask.First().Length;

            for (var i = startingPoint.Y - rows / 2; i <= startingPoint.Y + rows / 2; i++)
            for (var j = startingPoint.X - columns / 2; j <= startingPoint.X + columns / 2; j++)
            {
                Color subtractedColor;
                if (mask[i - (startingPoint.Y - rows / 2)][j - (startingPoint.X - columns / 2)] != 1) continue;

                subtractedColor = sourceImage.GetPixel(j, i);
                pixelValues.Add(subtractedColor.R);
            }

            pixelValues.Sort();
            var newColor = Color.FromArgb(pixelValues.First(), pixelValues.First(), pixelValues.First());

            return newColor;
        }

        private static Color ImageDilateGetPixelValue(Bitmap sourceImage, int[][] mask,
            Point startingPoint)
        {
            var pixelValues = new List<int>();
            var rows = mask.Length;
            var columns = mask.First().Length;

            for (var i = startingPoint.Y - rows / 2; i <= startingPoint.Y + rows / 2; i++)
            for (var j = startingPoint.X - columns / 2; j <= startingPoint.X + columns / 2; j++)
            {
                Color subtractedColor;
                if (mask[i - (startingPoint.Y - rows / 2)][j - (startingPoint.X - columns / 2)] != 1) continue;

                subtractedColor = sourceImage.GetPixel(j, i);
                pixelValues.Add(subtractedColor.R);
            }

            pixelValues.Sort();
            var newColor = Color.FromArgb(pixelValues.Last(), pixelValues.Last(), pixelValues.Last());

            return newColor;
        }
    }
}