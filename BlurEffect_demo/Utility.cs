using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace BlurEffect_demo
{
    class Utility
    {
        /// <summary>
        /// WriteableBitmap 的拷贝
        /// </summary>
        /// <param name="bitmap">原</param>
        /// <returns></returns>
        public static async Task<WriteableBitmap> BitmapClone(WriteableBitmap bitmap)
        {
            WriteableBitmap result = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight);

            byte[] sourcePixels = Get_WriteableBitmap_bytes(bitmap);

            //byte[] resultPixels = new byte[sourcePixels.Length];

            //sourcePixels.CopyTo(resultPixels, 0);

            using (Stream resultStream = result.PixelBuffer.AsStream())
            {
                await resultStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
            }

            return result;
        }

        public static byte[] Get_WriteableBitmap_bytes(WriteableBitmap bitmap)
        {
            // 获取对直接缓冲区的访问，WriteableBitmap 的每个像素都写入直接缓冲区。
            IBuffer bitmapBuffer = bitmap.PixelBuffer;

            //byte[] sourcePixels = new byte[bitmapBuffer.Length];
            //Windows.Security.Cryptography.CryptographicBuffer.CopyToByteArray(bitmapBuffer, out sourcePixels);
            //bitmapBuffer.CopyTo(sourcePixels);

            using (var dataReader = DataReader.FromBuffer(bitmapBuffer))
            {
                var bytes = new byte[bitmapBuffer.Capacity];
                dataReader.ReadBytes(bytes);

                return bytes;
            }
        }
    }
}
