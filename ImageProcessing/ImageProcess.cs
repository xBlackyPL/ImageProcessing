using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;


namespace ImageProcess
{
    class ImageProcess
    {
        public static Bitmap ImageHistogramNormalizationRGB(Bitmap sourceImage)
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
    }
}
