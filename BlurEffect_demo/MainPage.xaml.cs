using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace BlurEffect_demo {
    public sealed partial class MainPage : Page {
        public MainPage() {
            this.InitializeComponent();

            this.Loaded += MainPage_Loaded;
        }

        WriteableBitmap wb;
        private async void MainPage_Loaded(object sender, RoutedEventArgs e) {
            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///a.jpg"))
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///b.png"));

            wb = new WriteableBitmap(1000, 1000);
            img.Source = wb;

            // Ensure a file was selected
            if (file != null) {
                // Set the source of the WriteableBitmap to the image stream
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read)) {
                    try {
                        await wb.SetSourceAsync(fileStream);
                    } catch (TaskCanceledException) {
                        // The async action to set the WriteableBitmap's source may be canceled if the user clicks the button repeatedly
                    }
                }
            }

            // 初始值
            ApplyFilter(1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level">[0,40]</param>
        async void ApplyFilter(float level) {

            #region old
            /*
            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                // Scale image to appropriate size 
                BitmapTransform transform = new BitmapTransform()
                {
                    ScaledWidth = Convert.ToUInt32(wb.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(wb.PixelHeight)
                };
                PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8, // WriteableBitmap uses BGRA format 
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation, // This sample ignores Exif orientation 
                    ColorManagementMode.DoNotColorManage
                );


                
                // An array containing the decoded image data, which could be modified before being displayed 
                byte[] sourcePixels = pixelData.DetachPixelData();
                using (Stream stream = wb.PixelBuffer.AsStream())
                {
                    await stream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                }
            }
            */
            #endregion

            // 拷贝
            WriteableBitmap new_bitmap = await Utility.BitmapClone(wb);

            // 添加高斯滤镜效果
            MyImage mi = new MyImage(new_bitmap);
            GaussianBlurFilter filter = new GaussianBlurFilter();
            filter.Sigma = level;
            filter.process(mi);

            // 图片添加完滤镜的 int[] 数组
            int[] array = mi.colorArray;

            // byte[] 数组的长度是 int[] 数组的 4倍
            byte[] result = new byte[array.Length * 4];

            // 通过自加，来遍历 byte[] 数组中的值
            int j = 0;
            for (int i = 0; i < array.Length; i++) {
                // 同时把 int 值中 a、r、g、b 的排列方式，转换为 byte数组中 b、g、r、a 的存储方式 
                result[j++] = (byte)(array[i]);       // Blue
                result[j++] = (byte)(array[i] >> 8);  // Green
                result[j++] = (byte)(array[i] >> 16); // Red
                result[j++] = (byte)(array[i] >> 24); // Alpha
            }

            // Open a stream to copy the image contents to the WriteableBitmap's pixel buffer 
            using (Stream stream = new_bitmap.PixelBuffer.AsStream()) {
                await stream.WriteAsync(result, 0, result.Length);
            }

            img.Source = new_bitmap;// 把 WriteableBitmap 对象赋值给 Image 控件

            // 用像素缓冲区的数据绘制图片
            //new_bitmap.Invalidate();
        }

        private async void MainPage_Loaded2(object sender, RoutedEventArgs e) {
            GaussianBlurFilter filter = new GaussianBlurFilter();
            WriteableBitmap wb = new WriteableBitmap(600, 500);

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///a.jpg"));
            using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read)) {
                try {
                    await wb.SetSourceAsync(fileStream);
                    img.Source = wb;

                    MyImage mi = new MyImage(wb);// filter.process(new MyImage(wb));
                    int[] array = mi.colorArray;

                    byte[] result = new byte[array.Length * 4];

                    int j = 0;
                    for (int i = 0; i < array.Length; i++) {
                        // b g r a   vs  argb
                        result[j++] = (byte)(array[i]); // b
                        result[j++] = (byte)(array[i] >> 8); // g
                        result[j++] = (byte)(array[i] >> 16); // r
                        result[j++] = (byte)(array[i] >> 24); // a
                    }

                    //using (MemoryStream ms = new MemoryStream(result))
                    //{
                    //    BitmapImage src = new BitmapImage();
                    //    src.SetSource(ms.AsRandomAccessStream());
                    //    img.Source = src;
                    //}

                    // 将像素数据写入 WriteableBitmap 对象的像素缓冲区
                    using (Stream stream = wb.PixelBuffer.AsStream()) {
                        await stream.WriteAsync(result, 0, result.Length);
                    }

                    // 用像素缓冲区的数据绘制图片
                    wb.Invalidate();

                    //img.Source = mi.image;
                } catch (Exception ex) {

                    Debug.WriteLine(ex.Message);
                    // The async action to set the WriteableBitmap's source may be canceled if the source is changed again while the action is in progress
                }
            }



            //wb.SetSource()
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
            ApplyFilter((float)e.NewValue);
        }
    }
}
