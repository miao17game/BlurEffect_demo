using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace BlurEffect_demo {

    public class MyImage {
        //original bitmap image
        public WriteableBitmap image;
        //public WriteableBitmap destImage;

        //format of image (jpg/png)
        private String formatName;
        //dimensions of image
        private int width, height;
        // RGB Array Color
        public int[] colorArray;

        public MyImage(WriteableBitmap img) {
            this.image = img;// img.Clone(); Silverlight中的 WriteableBitmap 有 Clone()方法，win runtime 中的 WriteableBitmap没有该方法。在调用该构造函数时，可以调用自定义的 Utility.BitmapClone(wb) 方法来实现对原对象的“拷贝”

            formatName = "jpg";
            width = img.PixelWidth;
            height = img.PixelHeight;
            updateColorArray();
        }

        public MyImage clone() {
            return new MyImage(this.image);
        }

        /**
         * Method to reset the image to a solid color
         * @param color - color to rest the entire image to
         */
        public void clearImage(int color) {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    setPixelColor(x, y, color);
                }
            }
        }


        /**
         * Set color array for image - called on initialisation
         * by constructor
         * 
         * @param bitmap
         */
        private void updateColorArray() {
            #region 原
            //colorArray = image.Pixels;

            //int r, g, b;
            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        int index = y * width + x;
            //        r = (colorArray[index] >> 16) & 0xff;
            //        g = (colorArray[index] >> 8) & 0xff;
            //        b = colorArray[index] & 0xff;
            //        colorArray[index] = (255 << 24) | (r << 16) | (g << 8) | b;
            //    }
            //}
            #endregion

            #region 段光磊

            // 戴震军 ： https://github.com/daizhenjun/ImageFilterForWindowsPhone

            // WriteableBitmap.PixelBuffer property ：https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.media.imaging.writeablebitmap.pixelbuffer.aspx

            // WriteableBitmap class : https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.media.imaging.writeablebitmap.aspx

            // WriteableBitmapEx : http://writeablebitmapex.codeplex.com/releases/view/612952

            // Getting Pixels of an Element(WriteableBitmap) // https://social.msdn.microsoft.com/Forums/en-US/39b3c702-caed-47e4-b7d3-b51d75cbca9b/getting-pixels-of-an-element-writeablebitmap?forum=winappswithcsharp

            // Windows 8 WriteableBitmap Pixel Arrays in C# and C++ : http://www.tuicool.com/articles/fQNvUz

            // 各种流转换 ： http://www.cnblogs.com/hebeiDGL/p/3428743.html

            // XAML images sample ： https://code.msdn.microsoft.com/windowsapps/0f5d56ae-5e57-48e1-9cd9-993115b027b9/

            // 重新想象 Windows 8 Store Apps (29) - 图片处理 ： http://www.cnblogs.com/webabcd/archive/2013/05/27/3101069.html

            // 把 uwp 的 WriteableBitmap 对象的 PixelBuffer属性（IBuffer）转换为  byte[] 数组
            byte[] colorBytes = BufferToBytes(image.PixelBuffer);

            // 转换为 silverlight 的 int[] 数组时，长度为 byte[] 数组的四分之一
            colorArray = new int[colorBytes.Length / 4];

            int a, r, g, b;
            int i = 0; // 通过 i 自加，来遍历 byte[] 整个数组

            // 通过图片的宽、高，分别设置 int[] 数组中每个像素的 ARGB 信息
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    // int[] 数组的索引
                    int index = y * width + x;
                    b = colorBytes[i++]; // Blue
                    g = colorBytes[i++]; // Green
                    r = colorBytes[i++]; // Red
                    a = colorBytes[i++]; // Alpha
                    colorArray[index] = (a << 24) | (r << 16) | (g << 8) | b;  // 4个 byte值存储为一个 int 值
                }
            }


            // msdn : https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.media.imaging.writeablebitmap.aspx
            //Stream stream = image.PixelBuffer.AsStream();
            //using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            //{
            //    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
            //    // Scale image to appropriate size 
            //    BitmapTransform transform = new BitmapTransform()
            //    {
            //        ScaledWidth = Convert.ToUInt32(Scenario4WriteableBitmap.PixelWidth),
            //        ScaledHeight = Convert.ToUInt32(Scenario4WriteableBitmap.PixelHeight)
            //    };
            //    PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
            //        BitmapPixelFormat.Bgra8, // WriteableBitmap uses BGRA format 
            //        BitmapAlphaMode.Straight,
            //        transform,
            //        ExifOrientationMode.IgnoreExifOrientation, // This sample ignores Exif orientation 
            //        ColorManagementMode.DoNotColorManage
            //    );

            //    // An array containing the decoded image data, which could be modified before being displayed 
            //    byte[] sourcePixels = pixelData.DetachPixelData();

            //    // Open a stream to copy the image contents to the WriteableBitmap's pixel buffer 
            //    using (Stream stream = Scenario4WriteableBitmap.PixelBuffer.AsStream())
            //    {
            //        await stream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
            //    }
            //}


            //PixelDataProvider dp = await BitmapDecoder.CreateAsync(image.PixelBuffer.a.AsStream());


            //var length = image.PixelWidth * image.PixelHeight;
            //var data = image.PixelBuffer.ToArray();

            //fixed (byte* srcPtr = data)
            //{
            //    fixed (int* dstPtr = colorArray)
            //    {
            //        for (var i = 0; i < length; i++)
            //        {
            //            dstPtr[i] = (srcPtr[i * 4 + 3] << 24)
            //                      | (srcPtr[i * 4 + 2] << 16)
            //                      | (srcPtr[i * 4 + 1] << 8)
            //                      | srcPtr[i * 4 + 0];
            //        }
            //    }
            //}

            #endregion
        }

        static byte[] BufferToBytes(IBuffer buf) {
            using (var dataReader = DataReader.FromBuffer(buf)) {
                var bytes = new byte[buf.Capacity];
                dataReader.ReadBytes(bytes);

                return bytes;
            }
        }

        /**
         * Method to set the color of a specific pixel
         * 
         * @param x
         * @param y
         * @param color
         */
        public void setPixelColor(int x, int y, int color) {
            colorArray[((y * image.PixelWidth + x))] = color;
            //image.setPixel(x, y, color);
            //destImage.setPixel(x, y, colorArray[((y*image.getWidth()+x))]);
        }

        /**
         * Get the color for a specified pixel
         * 
         * @param x
         * @param y
         * @return color
         */
        public int getPixelColor(int x, int y) {
            return colorArray[y * width + x];
        }

        /**
         * Set the color of a specified pixel from an RGB combo
         * 
         * @param x
         * @param y
         * @param c0
         * @param c1
         * @param c2
         */
        public void setPixelColor(int x, int y, int c0, int c1, int c2) {
            int rgbcolor = (255 << 24) + (c0 << 16) + (c1 << 8) + c2;
            colorArray[((y * image.PixelWidth + x))] = rgbcolor;
            //int array = ((y*image.getWidth()+x));

            //vbb.order(ByteOrder.nativeOrder());
            //vertexBuffer = vbb.asFloatBuffer();
            //vertexBuffer.put(vertices);
            //vertexBuffer.position(0);

            //image.setPixel(x, y, colorArray[((y*image.getWidth()+x))]);
        }

        public void copyPixelsFromBuffer() { //从缓冲区中copy数据以加快像素处理速度
                                             // this.destImage.Dispatcher.BeginInvoke(() =>
                                             // {
                                             //var result = new WriteableBitmap(width, height);
                                             //Buffer.BlockCopy(colorArray, 0, destImage.Pixels, 0, colorArray.Length * 4); // 段光磊注释掉
                                             //return result;
                                             // });
        }

        /**
         * Method to get the RED color for the specified 
         * pixel 
         * @param x
         * @param y
         * @return color of R
         */
        public int getRComponent(int x, int y) {
            return (getColorArray()[((y * width + x))] & 0x00FF0000) >> 16;
        }


        /**
         * Method to get the GREEN color for the specified 
         * pixel 
         * @param x
         * @param y
         * @return color of G
         */
        public int getGComponent(int x, int y) {
            return (getColorArray()[((y * width + x))] & 0x0000FF00) >> 8;
        }


        /**
         * Method to get the BLUE color for the specified 
         * pixel 
         * @param x
         * @param y
         * @return color of B
         */
        public int getBComponent(int x, int y) {
            return (getColorArray()[((y * width + x))] & 0x000000FF);
        }




        /**
         * @return the image
         */
        //public WriteableBitmap getImage()
        //{
        //    //return image;
        //    return destImage;
        //}


        /**
         * @param image the image to set
         */
        public void setImage(WriteableBitmap image) {
            this.image = image;
        }


        /**
         * @return the formatName
         */
        public String getFormatName() {
            return formatName;
        }


        /**
         * @param formatName the formatName to set
         */
        public void setFormatName(String formatName) {
            this.formatName = formatName;
        }


        /**
         * @return the width
         */
        public int getWidth() {
            return width;
        }


        /**
         * @param width the width to set
         */
        public void setWidth(int width) {
            this.width = width;
        }


        /**
         * @return the height
         */
        public int getHeight() {
            return height;
        }


        /**
         * @param height the height to set
         */
        public void setHeight(int height) {
            this.height = height;
        }


        /**
         * @return the colorArray
         */
        public int[] getColorArray() {
            return colorArray;
        }


        /**
         * @param colorArray the colorArray to set
         */
        public void setColorArray(int[] colorArray) {
            this.colorArray = colorArray;
        }

        public static int SAFECOLOR(int a) {
            if (a < 0)
                return 0;
            else if (a > 255)
                return 255;
            else
                return a;
        }

        public static int rgb(int r, int g, int b) {
            return (255 << 24) + (r << 16) + (g << 8) + b;
        }


        public static MyImage LoadImage(string path) {
            //Uri uri = new Uri(path, UriKind.Relative);
            //StreamResourceInfo resourceInfo = Application.GetResourceStream(uri);
            //BitmapImage bmp = new BitmapImage();
            //bmp.SetSource(resourceInfo.Stream);
            //return new Image(new WriteableBitmap(bmp));

            return null;
        }

    }

    public interface IImageFilter {
        MyImage process(MyImage imageIn);
    }

    public class GaussianBlurFilter : IImageFilter {
        protected static int Padding = 3;

        /// <summary>
        /// The bluriness factor. 
        /// Should be in the range [0, 40].
        /// </summary>
        public float Sigma = 0.75f;

        protected float[] ApplyBlur(float[] srcPixels, int width, int height) {
            float[] destPixels = new float[srcPixels.Length];
            System.Array.Copy(srcPixels, 0, destPixels, 0, srcPixels.Length);

            int w = width + Padding * 2;
            int h = height + Padding * 2;

            // Calculate the coefficients
            float q = Sigma;
            float q2 = q * q;
            float q3 = q2 * q;

            float b0 = 1.57825f + 2.44413f * q + 1.4281f * q2 + 0.422205f * q3;
            float b1 = 2.44413f * q + 2.85619f * q2 + 1.26661f * q3;
            float b2 = -(1.4281f * q2 + 1.26661f * q3);
            float b3 = 0.422205f * q3;

            float b = 1.0f - ((b1 + b2 + b3) / b0);

            // Apply horizontal pass
            ApplyPass(destPixels, w, h, b0, b1, b2, b3, b);

            // Transpose the array
            float[] transposedPixels = new float[destPixels.Length];
            Transpose(destPixels, transposedPixels, w, h);

            // Apply vertical pass
            ApplyPass(transposedPixels, h, w, b0, b1, b2, b3, b);

            // transpose back
            Transpose(transposedPixels, destPixels, h, w);

            return destPixels;
        }

        protected void ApplyPass(float[] pixels, int width, int height, float b0, float b1, float b2, float b3, float b) {
            float num = 1f / b0;
            int triplewidth = width * 3;
            for (int i = 0; i < height; i++) {
                int steplength = i * triplewidth;
                for (int j = steplength + 9; j < (steplength + triplewidth); j += 3) {
                    pixels[j] = (b * pixels[j]) + ((((b1 * pixels[j - 3]) + (b2 * pixels[j - 6])) + (b3 * pixels[j - 9])) * num);
                    pixels[j + 1] = (b * pixels[j + 1]) + ((((b1 * pixels[(j + 1) - 3]) + (b2 * pixels[(j + 1) - 6])) + (b3 * pixels[(j + 1) - 9])) * num);
                    pixels[j + 2] = (b * pixels[j + 2]) + ((((b1 * pixels[(j + 2) - 3]) + (b2 * pixels[(j + 2) - 6])) + (b3 * pixels[(j + 2) - 9])) * num);
                }
                for (int k = ((steplength + triplewidth) - 9) - 3; k >= steplength; k -= 3) {
                    pixels[k] = (b * pixels[k]) + ((((b1 * pixels[k + 3]) + (b2 * pixels[k + 6])) + (b3 * pixels[k + 9])) * num);
                    pixels[k + 1] = (b * pixels[k + 1]) + ((((b1 * pixels[(k + 1) + 3]) + (b2 * pixels[(k + 1) + 6])) + (b3 * pixels[(k + 1) + 9])) * num);
                    pixels[k + 2] = (b * pixels[k + 2]) + ((((b1 * pixels[(k + 2) + 3]) + (b2 * pixels[(k + 2) + 6])) + (b3 * pixels[(k + 2) + 9])) * num);
                }
            }
        }


        protected void Transpose(float[] input, float[] output, int width, int height) {
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    int index = (j * height) * 3 + (i * 3);
                    int pos = (i * width) * 3 + (j * 3);
                    output[index] = input[pos];
                    output[index + 1] = input[pos + 1];
                    output[index + 2] = input[pos + 2];
                }
            }
        }


        protected float[] ConvertImageWithPadding(MyImage imageIn, int width, int height) {
            int newheight = height + Padding * 2;
            int newwidth = width + Padding * 2;
            float[] numArray = new float[(newheight * newwidth) * 3];
            int index = 0;
            int num = 0;
            for (int i = -3; num < newheight; i++) {
                int y = i;
                if (i < 0) {
                    y = 0;
                } else if (i >= height) {
                    y = height - 1;
                }
                int count = 0;
                int negpadding = -1 * Padding;
                while (count < newwidth) {
                    int x = negpadding;
                    if (negpadding < 0) {
                        x = 0;
                    } else if (negpadding >= width) {
                        x = width - 1;
                    }
                    numArray[index] = imageIn.getRComponent(x, y) * 0.003921569f;
                    numArray[index + 1] = imageIn.getGComponent(x, y) * 0.003921569f;
                    numArray[index + 2] = imageIn.getBComponent(x, y) * 0.003921569f;

                    count++; negpadding++;
                    index += 3;
                }
                num++;
            }
            return numArray;
        }

        //@Override
        public virtual MyImage process(MyImage imageIn) {
            int width = imageIn.getWidth();
            int height = imageIn.getHeight();
            float[] imageArray = ConvertImageWithPadding(imageIn, width, height);
            imageArray = ApplyBlur(imageArray, width, height);
            int newwidth = width + Padding * 2;
            for (int i = 0; i < height; i++) {
                int num = ((i + 3) * newwidth) + 3;
                for (int j = 0; j < width; j++) {
                    int pos = (num + j) * 3;
                    imageIn.setPixelColor(j, i, (byte)(imageArray[pos] * 255f), (byte)(imageArray[pos + 1] * 255f), (byte)(imageArray[pos + 2] * 255f));
                }
            }
            return imageIn;
        }

    }
}
