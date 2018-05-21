using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;


namespace ImageProcessing
{
    class ImageProcess
    {
        public static int[] GetImageHistogramMonochromatic(Bitmap sourceImage)
        {
            var histogram = new int[256];
            for (var i = 0; i < sourceImage.Height; i++)
            {
                for (var j = 0; j < sourceImage.Width; j++)
                {
                    var pixel = sourceImage.GetPixel(j, i);
                    histogram[pixel.R]++;
                }
            }

            return histogram;
        }

        private static int[] getHistogramCumulant(int[] sourceHistogram)
        {
            var histogramCumulant = new int[256];

            for (var i = 1; i < sourceHistogram.Length; i++)
            {
                histogramCumulant[i] = sourceHistogram[i] + sourceHistogram[i - 1];
            }

            return histogramCumulant;
        }
        
        public static Bitmap ImageHistogramGaussNormalizationMonochromatic(Bitmap sourceImage, double stdDeviation)
        {
            var result = new Bitmap(sourceImage.Width, sourceImage.Height);

            var sourceHistogram = GetImageHistogramMonochromatic(sourceImage);


            var sourceHistogramCumulant = new int[256];

            

            



        }

        public static Bitmap ImageHistogramGaussianNormalizationRGB(Bitmap sourceImage, double stdDeviation)
        {
            Bitmap result = new Bitmap(sourceImage.Width, sourceImage.Height);
            var pixels = new Color[sourceImage.Width, sourceImage.Height];


            var stdDev = 0.4;


            var histR = new int[256];
            var histG = new int[256];
            var histB = new int[256];
            var expectedValueR = 0;
            var expectedValueG = 0;
            var expectedValueB = 0;

            var numberOfAllPixels = sourceImage.Width * sourceImage.Height;

            for (int i = 0; i < sourceImage.Width - 1; i++)
                for (int j = 0; j < sourceImage.Height - 1; j++)
                {
                    var pixel = sourceImage.GetPixel(i, j);
                    histR[pixel.R]++;
                    histG[pixel.G]++;
                    histB[pixel.B]++;
                    pixels[i, j] = pixel;
                }

            for (int i = 0; i < 256; i++)
            {
                expectedValueR += i * histR[i];
                expectedValueG += i * histG[i];
                expectedValueB += i * histB[i];
            }

            expectedValueR /= numberOfAllPixels;
            expectedValueG /= numberOfAllPixels;
            expectedValueB /= numberOfAllPixels;

            for (int i = 0; i < sourceImage.Width - 1; i++)
                for (int j = 0; j < sourceImage.Height - 1; j++)
                {
                    var newPixel = new Color();
                    var R = (1 / Math.Sqrt(2 * Math.PI * stdDev)) * (Math.Exp((-1) * Math.Pow((pixels[i, j].R - expectedValueR), 2) / stdDev));
                    var G = (1 / Math.Sqrt(2 * Math.PI * stdDev)) * (Math.Exp((-1) * Math.Pow((pixels[i, j].G - expectedValueG), 2) / stdDev));
                    var B = (1 / Math.Sqrt(2 * Math.PI * stdDev)) * (Math.Exp((-1) * Math.Pow((pixels[i, j].B - expectedValueB), 2) / stdDev));
                    R *= 255;
                    G *= 255;
                    B *= 255;
                    newPixel = Color.FromArgb((int)R, (int)G, (int)B);
                    result.SetPixel(i, j, newPixel); 
                } 

            return result;
        }
        
        public static bool monochromaticValidation(Bitmap sourceImage)
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
    }
}
