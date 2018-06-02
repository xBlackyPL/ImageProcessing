using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageProcessingApp
{
    internal static class ImageProcessing
    {
        public static bool MonochromaticValidation(Bitmap sourceImage)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));

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
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));

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

        public static Bitmap CopyImage(Bitmap sourceImage)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));

            var result = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (var i = 0; i < sourceImage.Height - 1; i++)
            for (var j = 0; j < sourceImage.Width - 1; j++)
                result.SetPixel(j, i, sourceImage.GetPixel(j, i));

            return result;
        }

        public static int[] GetImageHistogram(Bitmap sourceImage, char layer)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (layer <= 0) throw new ArgumentOutOfRangeException(nameof(layer));

            var histogram = new int[256];
            for (var i = 0; i < sourceImage.Height; i++)
            for (var j = 0; j < sourceImage.Width; j++)
            {
                var pixel = sourceImage.GetPixel(j, i);
                switch (layer)
                {
                    case 'R':
                    case 'r':
                        histogram[pixel.R]++;
                        break;
                    case 'G':
                    case 'g':
                        histogram[pixel.G]++;
                        break;

                    case 'B':
                    case 'b':
                        histogram[pixel.B]++;
                        break;
                    default:
                        Console.WriteLine(@"Invalide use of GetImageHistogram, invalid layer parameter");
                        return histogram;
                }
            }

            return histogram;
        }

        private static double[] GetHistogramCumulant(int[] sourceHistogram, double normalization)
        {
            if (sourceHistogram == null) throw new ArgumentNullException(nameof(sourceHistogram));
            if (normalization <= 0) throw new ArgumentOutOfRangeException(nameof(normalization));

            var histogramCumulant = new double[256];
            histogramCumulant[0] = sourceHistogram[0];

            for (var i = 1; i < sourceHistogram.Length; i++)
                histogramCumulant[i] = sourceHistogram[i] + histogramCumulant[i - 1];

            for (var i = 0; i < sourceHistogram.Length; i++)
                histogramCumulant[i] /= normalization;

            return histogramCumulant;
        }

        private static double[] GaussValuesPerColor(double stdDeviation)
        {
            if (stdDeviation <= 0) throw new ArgumentOutOfRangeException(nameof(stdDeviation));
            var gaussValues = new double[256];

            for (var i = 0; i < 256; i++)
                gaussValues[i] = 1 / (stdDeviation * Math.Sqrt(2 * Math.PI)) *
                                 Math.Exp(-((i / 255.0 - 0.5) * (i / 255.0 - 0.5)) /
                                          (2 * (stdDeviation * stdDeviation)));

            return gaussValues;
        }

        public static Bitmap ImageHistogramGaussNormalizationMonochromatic(Bitmap sourceImage, double stdDeviation,
            int numberOfColorClasses)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (stdDeviation <= 0) throw new ArgumentOutOfRangeException(nameof(stdDeviation));
            if (numberOfColorClasses <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfColorClasses));

            var result = new Bitmap(sourceImage.Width, sourceImage.Height);
            double pixels = sourceImage.Width * sourceImage.Height;
            var gaussValuesForEachColor = GaussValuesPerColor(stdDeviation);

            var classBorder = new double[numberOfColorClasses];
            var index = 0;
            var classNumber = 0;
            double area = 0;
            const double step = 1 / 255.0;
            var totalArea = 0.0;

            for (var i = 0; i < 256; i++) totalArea += gaussValuesForEachColor[i] * step;

            while (index < 256)
            {
                area += gaussValuesForEachColor[index] * step;
                if (area < totalArea * (classNumber + 1) / numberOfColorClasses)
                {
                    index++;
                    continue;
                }

                if (classNumber == numberOfColorClasses) break;
                classBorder[classNumber++] = (double) index / 255;
            }

            classBorder[numberOfColorClasses - 1] = 1;
            var newPixelValues = newPixelValuesHistogramEqualization(numberOfColorClasses, classBorder,
                GetHistogramCumulant(GetImageHistogram(sourceImage, 'R'), pixels));

            for (var i = 0; i < sourceImage.Height - 1; i++)
            for (var j = 0; j < sourceImage.Width - 1; j++)
            {
                var sourcePixelValue = sourceImage.GetPixel(j, i);
                var newPixelValue = newPixelValues[sourcePixelValue.R];
                result.SetPixel(j, i, Color.FromArgb(newPixelValue, newPixelValue, newPixelValue));
            }

            return result;
        }

        public static Bitmap ImageHistogramGaussianNormalizationRGB(Bitmap sourceImage, double stdDeviation,
            int numberOfColorClasses)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (stdDeviation <= 0) throw new ArgumentOutOfRangeException(nameof(stdDeviation));
            if (numberOfColorClasses <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfColorClasses));

            var result = new Bitmap(sourceImage.Width, sourceImage.Height);
            double pixels = sourceImage.Width * sourceImage.Height;
            var gaussValuesForEachColor = GaussValuesPerColor(stdDeviation);


            var classBorder = new double[numberOfColorClasses];
            var index = 0;
            var classNumber = 0;
            double area = 0;
            const double step = 1 / 255.0;
            var totalArea = 0.0;

            for (var i = 0; i < 256; i++) totalArea += gaussValuesForEachColor[i] * step;

            while (index < 256)
            {
                area += gaussValuesForEachColor[index] * step;
                if (area < totalArea * (classNumber + 1) / numberOfColorClasses)
                {
                    index++;
                    continue;
                }

                if (classNumber == numberOfColorClasses) break;
                classBorder[classNumber++] = (double) index / 255;
            }

            classBorder[numberOfColorClasses - 1] = 1;

            var newPixelValuesR =
                newPixelValuesHistogramEqualization(numberOfColorClasses, classBorder,
                    GetHistogramCumulant(GetImageHistogram(sourceImage, 'R'), pixels));
            var newPixelValuesG =
                newPixelValuesHistogramEqualization(numberOfColorClasses, classBorder,
                    GetHistogramCumulant(GetImageHistogram(sourceImage, 'G'), pixels));
            var newPixelValuesB =
                newPixelValuesHistogramEqualization(numberOfColorClasses, classBorder,
                    GetHistogramCumulant(GetImageHistogram(sourceImage, 'B'), pixels));

            for (var i = 0; i < sourceImage.Height - 1; i++)
            for (var j = 0; j < sourceImage.Width - 1; j++)
            {
                var sourcePixelValue = sourceImage.GetPixel(j, i);
                var newPixelValueR = newPixelValuesR[sourcePixelValue.R];
                var newPixelValueG = newPixelValuesG[sourcePixelValue.G];
                var newPixelValueB = newPixelValuesB[sourcePixelValue.B];
                result.SetPixel(j, i, Color.FromArgb(newPixelValueR, newPixelValueG, newPixelValueB));
            }

            return result;
        }

        private static int[] newPixelValuesHistogramEqualization(int numberOfColorClasses, double[] classBorder,
            double[] sourceHistogramCumulant)
        {
            if (classBorder == null) throw new ArgumentNullException(nameof(classBorder));
            if (sourceHistogramCumulant == null) throw new ArgumentNullException(nameof(sourceHistogramCumulant));
            if (numberOfColorClasses <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfColorClasses));

            var index = 0;
            var newPixelValues = new int[sourceHistogramCumulant.Length];

            for (var i = 0; i < numberOfColorClasses; i++)
            {
                var pixelValue = index;
                while (index < 255)
                {
                    if (sourceHistogramCumulant[index] <= classBorder[i])
                    {
                        newPixelValues[index++] = pixelValue;
                        continue;
                    }

                    break;
                }
            }

            return newPixelValues;
        }

        public static Bitmap ImageOrdfilt2Monochromatic(Bitmap sourceImage, int maskSize, int orderNumber)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (maskSize <= 0) throw new ArgumentOutOfRangeException(nameof(maskSize));
            if (orderNumber <= 0) throw new ArgumentOutOfRangeException(nameof(orderNumber));

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
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (maskSize <= 0) throw new ArgumentOutOfRangeException(nameof(maskSize));
            if (orderNumber <= 0) throw new ArgumentOutOfRangeException(nameof(orderNumber));

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
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (maskSize <= 0) throw new ArgumentOutOfRangeException(nameof(maskSize));
            if (orderNumber <= 0) throw new ArgumentOutOfRangeException(nameof(orderNumber));

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
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (maskSize <= 0) throw new ArgumentOutOfRangeException(nameof(maskSize));
            if (orderNumber <= 0) throw new ArgumentOutOfRangeException(nameof(orderNumber));

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
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (angle <= 0) throw new ArgumentOutOfRangeException(nameof(angle));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

            var structElement = LineStructuralElementGenerator(angle, size);
            var result = ImageDilate(ImageErode(sourceImage, structElement), structElement);
            return result;
        }

        private static int[][] LineStructuralElementGenerator(int angle, int structuralElementSize)
        {
            if (angle <= 0) throw new ArgumentOutOfRangeException(nameof(angle));
            if (structuralElementSize <= 0) throw new ArgumentOutOfRangeException(nameof(structuralElementSize));

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
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (structuralElement == null) throw new ArgumentNullException(nameof(structuralElement));

            var result = new Bitmap(sourceImage.Width, sourceImage.Height);
            var rows = structuralElement.Length;
            var columns = structuralElement[0].Length;

            for (var i = rows / 2 + 1; i < sourceImage.Height - rows; i++)
            for (var j = columns / 2 + 1; j < sourceImage.Width - columns; j++)
                result.SetPixel(j, i, ImageErodeGetPixelValue(sourceImage, structuralElement, new Point(j, i)));

            return result;
        }

        public static Bitmap ImageDilate(Bitmap sourceImage, int[][] structuralElement)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (structuralElement == null) throw new ArgumentNullException(nameof(structuralElement));

            var result = new Bitmap(sourceImage.Width, sourceImage.Height);

            var rows = structuralElement.Length;
            var columns = structuralElement[0].Length;

            for (var i = rows / 2 + 1; i < sourceImage.Height - rows; i++)
            for (var j = columns / 2 + 1; j < sourceImage.Width - columns; j++)
                result.SetPixel(j, i, ImageDilateGetPixelValue(sourceImage, structuralElement, new Point(j, i)));

            return result;
        }

        private static Color ImageErodeGetPixelValue(Bitmap sourceImage, int[][] mask, Point startingPoint)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (mask == null) throw new ArgumentNullException(nameof(mask));

            var pixelValues = new List<int>();

            var rows = mask.Length;
            var columns = mask.First().Length;

            for (var i = startingPoint.Y - rows / 2; i <= startingPoint.Y + rows / 2; i++)
            for (var j = startingPoint.X - columns / 2; j <= startingPoint.X + columns / 2; j++)
            {
                if (mask[i - (startingPoint.Y - rows / 2)][j - (startingPoint.X - columns / 2)] != 1) continue;

                pixelValues.Add(sourceImage.GetPixel(j, i).R);
            }

            pixelValues.Sort();
            var newColor = Color.FromArgb(pixelValues.First(), pixelValues.First(), pixelValues.First());

            return newColor;
        }

        private static Color ImageDilateGetPixelValue(Bitmap sourceImage, int[][] mask,
            Point startingPoint)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
            if (mask == null) throw new ArgumentNullException(nameof(mask));

            var pixelValues = new List<int>();
            var rows = mask.Length;
            var columns = mask.First().Length;

            for (var i = startingPoint.Y - rows / 2; i <= startingPoint.Y + rows / 2; i++)
            for (var j = startingPoint.X - columns / 2; j <= startingPoint.X + columns / 2; j++)
            {
                if (mask[i - (startingPoint.Y - rows / 2)][j - (startingPoint.X - columns / 2)] != 1) continue;
                pixelValues.Add(sourceImage.GetPixel(j, i).R);
            }

            pixelValues.Sort();
            var newColor = Color.FromArgb(pixelValues.Last(), pixelValues.Last(), pixelValues.Last());

            return newColor;
        }

        public static Bitmap ImageFillHoles(Bitmap sourceImage)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));

            var result = CopyImage(sourceImage);

            for (var i = 0; i < result.Height - 1; i++)
            {
                if (result.GetPixel(0, i).Equals(Color.FromArgb(255, 0, 0, 0)))
                    FloodPoints(result, new Point(0, i));

                if (result.GetPixel(result.Width - 1, i).Equals(Color.FromArgb(255, 0, 0, 0)))
                    FloodPoints(result, new Point(result.Width - 1, i));
            }

            for (var i = 0; i < result.Width - 1; i++)
            {
                if (result.GetPixel(i, 0).Equals(Color.FromArgb(255, 0, 0, 0)))
                    FloodPoints(result, new Point(i, 0));

                if (result.GetPixel(i, result.Height - 1).Equals(Color.FromArgb(255, 0, 0, 0)))
                    FloodPoints(result, new Point(i, result.Height - 1));
            }

            for (var i = 0; i < result.Height - 1; i++)
            for (var j = 0; j < result.Width - 1; j++)
                if (result.GetPixel(j, i).Equals(Color.FromArgb(255, 0, 255, 0)))
                    result.SetPixel(j, i, Color.Black);
                else if (result.GetPixel(j, i).Equals(Color.FromArgb(255, 0, 0, 0))) result.SetPixel(j, i, Color.White);

            return result;
        }

        private static void FloodPoints(Bitmap image, Point startingPoint)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            image.SetPixel(startingPoint.X, startingPoint.Y, Color.FromArgb(255, 0, 255, 0));
            var pendingPoints = new Queue<Point>();
            pendingPoints.Enqueue(startingPoint);

            while (pendingPoints.Count > 0)
            {
                var currentPoint = pendingPoints.Dequeue();

                if (currentPoint.Y > 0 &&
                    image.GetPixel(currentPoint.X, currentPoint.Y - 1).Equals(Color.FromArgb(255, 0, 0, 0)))
                {
                    image.SetPixel(currentPoint.X, currentPoint.Y - 1, Color.FromArgb(255, 0, 255, 0));
                    pendingPoints.Enqueue(new Point(currentPoint.X, currentPoint.Y - 1));
                }

                if (currentPoint.Y + 1 < image.Height - 1 &&
                    image.GetPixel(currentPoint.X, currentPoint.Y + 1).Equals(Color.FromArgb(255, 0, 0, 0)))
                {
                    image.SetPixel(currentPoint.X, currentPoint.Y + 1, Color.FromArgb(255, 0, 255, 0));
                    pendingPoints.Enqueue(new Point(currentPoint.X, currentPoint.Y + 1));
                }

                if (currentPoint.X > 0 &&
                    image.GetPixel(currentPoint.X - 1, currentPoint.Y).Equals(Color.FromArgb(255, 0, 0, 0)))
                {
                    image.SetPixel(currentPoint.X - 1, currentPoint.Y, Color.FromArgb(255, 0, 255, 0));
                    pendingPoints.Enqueue(new Point(currentPoint.X - 1, currentPoint.Y));
                }

                if (currentPoint.X + 1 < image.Width - 1 &&
                    image.GetPixel(currentPoint.X + 1, currentPoint.Y).Equals(Color.FromArgb(255, 0, 0, 0)))
                {
                    image.SetPixel(currentPoint.X + 1, currentPoint.Y, Color.FromArgb(255, 0, 255, 0));
                    pendingPoints.Enqueue(new Point(currentPoint.X + 1, currentPoint.Y));
                }
            }
        }
    }
}