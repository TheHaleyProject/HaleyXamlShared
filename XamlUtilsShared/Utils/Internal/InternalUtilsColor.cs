using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Haley.Models;
using System.Windows.Media.Imaging;
using Haley.Enums;
using System.Windows.Media;
using Haley.Utils;

#if HWPFR
using Isolated.Haley.WpfIconPack;
#elif HMVVM
using Isolated.Haley.MVVM;
#endif

//STRIDE: The stride is the width of a single row of pixels (a scan line), rounded up to a four-byte boundary. If the stride is positive, the bitmap is top-down. If the stride is negative, the bitmap is bottom-up.

// To calculate stride, the formula we generally use is, (Bitsperpixel + 7 )/8 . If we see carefully, this gives us a decimal value. But our main idea is to get integer.
// We have 
// 1 byte (Mono chrome, 8 bits)
// 2 byte(Dual Color 16 bits)
// 3 byte(24 bits)
// 4 byte(32 bits)..We can also have 64 bits, 128 bits etc..

// Example:

// Monochrome: 8 bits = (8 + 7) / 8 = 1.875 = 1 byte
//     Dual : 16 bits = (16 + 7) / 8 = 2.875 = 2 byte.


namespace Haley.Utils
{
    internal class InternalUtilsColor
    {

#region ColorChanger
        public static ImageInfo GetImageInfo(ImageSource source)
        {
            try
            {
                BitmapSource _input = source as BitmapSource;
                ImageInfo res = new ImageInfo();
                res.source = _input;
                res.stride = _input.PixelWidth * ((_input.Format.BitsPerPixel + 7) / 8);
                res.length = res.stride * _input.PixelHeight; //Width  * height ( in pixel)
                res.pixel_height = _input.PixelHeight;
                res.pixel_width = _input.PixelWidth;
                res.dpiX = _input.DpiX;
                res.dpiY = _input.DpiY;
                res.format = _input.Format;
                //res.metadata = _input?.Metadata as BitmapMetadata;
                return res;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static ImageSource CloneImage(ImageSource source, byte[] new_array = null)
        {
            //we can use better clone directly
            //return source.Clone(); //Check if it has any side effects.
            return CloneImage(GetImageInfo(source), new_array);
        }

        public static ImageSource CloneImage(ImageInfo source, byte[] new_array = null)
        {
            try
            {
                if (new_array == null)
                {
                    new_array = new byte[source.length]; //matching the length of the source. //THIS IS AN EMPTY ARRAY
                }
                //The new array should match the length of the source. if not throw exception.
                BitmapSource res = BitmapSource.Create(source.pixel_width, source.pixel_height, source.dpiX, source.dpiY, source.format, null, new_array, source.stride);
                return res;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static ImageSource ChangeImageColor(ImageInfo source, int red,int green, int blue)
        {
            try
            {
                //Ensure that the color values are with in the allowed range.
                InternalUtilsCommon.ClampLimits(ref red);
                InternalUtilsCommon.ClampLimits(ref green);
                InternalUtilsCommon.ClampLimits(ref blue);

                //Now using the imageinfo source, create a new array and fill it with the input color.
                byte[] newimage = new byte[source.length];

                //We are merely going to refill all the RGB values with the input colors. But remember, we are not changing the transparency. So, it is better to retain the transparency from the parent. So, copy the pixels (only for the transparency values).

                source.source.CopyPixels(newimage, source.stride, 0); //This copies all values. but we replace all the colors below.

                for (int i = 0; i < newimage.Length; i+=4) //increment by 4 number
                {
                    newimage[i + (int)RGB.Red] = Convert.ToByte(red);
                    newimage[i + (int)RGB.Green] = Convert.ToByte(green);
                    newimage[i + (int)RGB.Blue] = Convert.ToByte(blue);
                }

                //At this point, we have obtained an array with transparency (if available).

                return CloneImage(source, newimage);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static ImageSource ChangeImageColor(ImageSource source, System.Windows.Media.Brush brush)
        {
            try
            {
                ImageSource result = null;
                if (source is BitmapSource) {
                    var scb = brush as SolidColorBrush;
                    if (scb == null) scb = System.Windows.Media.Brushes.Black; //for direclty color change on the bitmap we can only process the solid color brush
                    var newcolor = scb.Color;
                    var imageinfo = GetImageInfo(source);
                    result = ChangeImageColor(imageinfo, int.Parse(newcolor.R.ToString()), int.Parse(newcolor.G.ToString()), int.Parse(newcolor.B.ToString()));
                } else if (source is DrawingImage dwgImage) {
                    //Handle drawing image differently
                    return ChangeDrawingColor(dwgImage, brush);
                }
                return result;
            }
            catch (Exception)
            {
                return source; //In case of error, just reuse the source image itself.
            }
        }

        public static ImageSource ChangeDrawingColor(DrawingImage source, System.Windows.Media.Brush brush) {
            if (source == null) return source;
            //We return a new image source.
            DrawingImage target = source.Clone(); //Always clone the source, or else we will end up changing the value of the source directly and when we try to change hover, we will change both the colors
            if (source.IsFrozen) {
                //if we have already cloned, this step might not be required.
                target = source.Clone();
            }

            //A drawing image may directly contain geometry drawing or can contain any level of drawing groups
            ChangeGeomColor(target.Drawing, brush); //recursive change
            return target;
        }


        static void ChangeGeomColor(Drawing drawing, System.Windows.Media.Brush brush) {

            if (drawing is DrawingGroup dgroup) {
                foreach (var child in dgroup.Children) {
                    ChangeGeomColor(child, brush);
                }
            } else if (drawing is GeometryDrawing dwg_geom) {
                if (dwg_geom.Brush != null && dwg_geom.Brush.IsFrozen) {
                    dwg_geom.Brush = dwg_geom.Brush.Clone();
                }

                dwg_geom.SetCurrentValue(GeometryDrawing.BrushProperty, brush);
            }
        }

#endregion
    }
}
