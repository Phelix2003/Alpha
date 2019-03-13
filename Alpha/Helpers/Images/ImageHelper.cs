using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace Alpha.Helpers.Images
{
    public class ImageHelper
    {
        public byte[] ProcessFileToImage(HttpPostedFileBase dataIn)
        {
            try
            {
                const double FinalImageSize = 150; // in pixels

                int newWidth;
                int newHeight;

                using (Image image = Image.FromStream(dataIn.InputStream, true, true))
                {
                    // Rescaling image size to be done here,  before saving it in the DB. See https://stackoverflow.com/questions/1171696/how-do-you-convert-a-httppostedfilebase-to-an-image
                    //https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp

                    //var destRect = new Rectangle(0, 0, newWidth, newHeight);
                    if (image.Height < image.Width) // Image scaling while conserving the ratio. 
                    {
                        newWidth = (int)FinalImageSize;
                        newHeight = (int)((double)image.Height * (double)FinalImageSize / (double)image.Width);
                    }
                    else
                    {
                        newHeight = (int)FinalImageSize;
                        newWidth = (int)((double)image.Width * (double)FinalImageSize / (double)image.Height);
                    }

                    var destImage = new Bitmap(newWidth, newHeight);
                    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                    using (Graphics g = Graphics.FromImage(destImage))
                    {

                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        using (var wrapMode = new ImageAttributes())
                        {
                            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                            g.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                        }
                        return imageToByteArray(destImage);
                    }
                }
            }
            catch
            {
                return null;
            }

        }

        private byte[] imageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        private Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

    }
}