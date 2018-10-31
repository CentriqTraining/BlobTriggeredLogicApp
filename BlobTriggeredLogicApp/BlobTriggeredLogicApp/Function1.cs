using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace BlobTriggeredLogicApp
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run(
            [BlobTrigger("samples-workitems/{name}", Connection = "")]Stream myBlob,
            [Blob("samples-workitems-complete/{name}", FileAccess.Write)]Stream altered,
            string name, TraceWriter log)
        {
            /*  Params need to be changed to:
               
               [BlobTrigger("samples-workitems/{name}", Connection = "")]Stream myBlob,
               [Blob("samples-workitems-complete/{name}", FileAccess.Write)]Stream altered,
               string name, TraceWriter log

            */
            try
            {
                using (Image FromImage = Image.FromStream(myBlob))
                using (Image Watermark =  Properties.Resources.NagelSeries2_10)
                {
                    //  Create a new image to write to
                    Bitmap TargetImage = new Bitmap(FromImage.Width, FromImage.Height, PixelFormat.Format32bppArgb);

                    // Get a color matrix that will set specific attributes of the images
                    ColorMatrix ColorMatrixForWatermark = new ColorMatrix();
                    ColorMatrixForWatermark.Matrix33 = .3f;  //  opacity

                    ColorMatrix ColorMatrixForOriginal = new ColorMatrix();
                    ColorMatrixForOriginal.Matrix33 = .7f;  //  opacity

                    //  Set the attributes using the matrix
                    ImageAttributes AttributesForWatermark = new ImageAttributes();
                    ImageAttributes AttributesForOriginal = new ImageAttributes();
                    AttributesForWatermark.SetColorMatrix(ColorMatrixForWatermark, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    AttributesForOriginal.SetColorMatrix(ColorMatrixForOriginal, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    using (Graphics Gfx = Graphics.FromImage(TargetImage))
                    {
                        //  Draw the original image onto our new image at specific opacity
                        Gfx.DrawImage(FromImage,
                            new Rectangle(0, 0, FromImage.Width, FromImage.Height),
                            0, 0, FromImage.Width, FromImage.Height,
                            GraphicsUnit.Pixel, AttributesForOriginal);

                        //  Draw the Watermark as well at 40%
                        Gfx.DrawImage(Watermark,
                            new Rectangle(0, 0, TargetImage.Width, TargetImage.Height),
                                            0, 0, Watermark.Width, Watermark.Height,
                                            GraphicsUnit.Pixel, AttributesForWatermark);
                    }

                    TargetImage.Save(altered, ImageFormat.Jpeg);
                }


                log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            }
            catch (System.Exception ex)
            {
                log.Error(ex.ToString());
            }

        }
    }
}
