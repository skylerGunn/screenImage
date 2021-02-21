using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
namespace ScreenImage
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            //do stuff
            ScreenCapture s = new ScreenCapture();
            //Image img = s.CaptureScreen();
            //img.Save("C:\\Users\\skyle\\Desktop\\imageFolder\\image.png", ImageFormat.Png);
            List<Image2dArray> imagesSplit = new List<Image2dArray>();
            List<Image> imgList = new List<Image>();
            for (int i = 0; i < 500; i++)
            {
                //s.CaptureScreenToFile("C:\\Users\\skyle\\Desktop\\imageFolder\\" + i + ".png", ImageFormat.Png);
                Image img = s.CaptureScreen();
                imgList.Add(img);
                //imagesSplit.Add(new Image2dArray(img, 16));
            }
            Image firI = imgList[0];
            int skipFirst = 0;
            foreach (var a in imgList)
            {
                if (skipFirst == 0)
                {
                    skipFirst = 1;
                } else
                {
                    if (CompareImages(a, firI))
                    {
                        break;
                    }
                }
            }
            /*int f1 = 0;
            int f2 = 0;
            Image a1 = null, a2 = null;
            foreach (var a in imagesSplit)
            {
                int p = 0;
                foreach (var b in a.imageArray)
                {
                    if (f1 == 0) {
                        a1 = b;
                        f1 = 1;
                    } else if (f1 == 1 && f2 == 1)
                    {
                        a2 = b;
                        f2 = 2;
                    }
                    b.Save("C:\\Users\\skyle\\Desktop\\imageFolder\\" + p + ".png", ImageFormat.Png);
                    p++;
                }
                f2 = 1;
            }*/
            //Bitmap bbb = new Bitmap(a1);
            //Bitmap ccc = new Bitmap(a2);
            //bool isEqu = true;
            //bool car = CompareMemCmp(bbb, ccc);
            /*for (int x = 0; x < a1.Width; x++)
            {
                for (int y = 0; y < a1.Height; y++)
                {
                    if (bbb.GetPixel(x, y) == ccc.GetPixel(x, y))
                    {
                        //keep going
                    } else
                    {
                        isEqu = false;
                        break;
                    }
                }
            }*/
            //bool bddd = isEqu;
        }
        public static bool CompareImages(Image img1, Image img2) //helper function for comparing 2 images
        {
            Bitmap bit1 = new Bitmap(img1);
            Bitmap bit2 = new Bitmap(img2);
            
            bool ret = CompareMemCmp(bit1, bit2);
            bit1.Dispose();
            bit2.Dispose();
            return ret;
        }

        [DllImport("msvcrt.dll")] //imported func, faster than using just pixel
        private static extern int memcmp(IntPtr b1, IntPtr b2, long count);
        public static bool CompareMemCmp(Bitmap b1, Bitmap b2)
        {
            if ((b1 == null) != (b2 == null)) return false;
            if (b1.Size != b2.Size) return false;

            var bd1 = b1.LockBits(new Rectangle(new Point(0, 0), b1.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bd2 = b2.LockBits(new Rectangle(new Point(0, 0), b2.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                IntPtr bd1scan0 = bd1.Scan0;
                IntPtr bd2scan0 = bd2.Scan0;

                int stride = bd1.Stride;
                int len = stride * b1.Height;

                return memcmp(bd1scan0, bd2scan0, len) == 0;
            }
            finally
            {
                b1.UnlockBits(bd1);
                b2.UnlockBits(bd2);
            }
        }
    }

    public class Image2dArray
    {
        public List<Image> imageArray;
        public Image2dArray(Image img, int splitBy) //assume splitby always 16 rn and it always works
        {
            imageArray = new List<Image>();
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    int index = (i * 16) + j;
                    imageArray.Add( new Bitmap(240, 135));
                    var graphics = Graphics.FromImage(imageArray[index]);
                    graphics.DrawImage(img, new Rectangle(0, 0, 240, 135), new Rectangle(i * 240, j * 135, 240, 135), GraphicsUnit.Pixel);
                    graphics.Dispose();
                }
            }
        }
    }
    public class ScreenCapture { 
        public Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }
        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
        /// <returns></returns>
        public Image CaptureWindow(IntPtr handle)
        {
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            //int width = windowRect.right - windowRect.left;
            //int height = windowRect.bottom - windowRect.top;
            int width = 3840;
            int height = 2160; //hardcode for now, dimension of my monitor
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            GDI32.SelectObject(hdcDest, hOld);
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            Image img = Image.FromHbitmap(hBitmap);
            GDI32.DeleteObject(hBitmap);
            return img;
        }
        /// <summary>
        /// Captures a screen shot of a specific window, and saves it to a file
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }
        /// <summary>
        /// Captures a screen shot of the entire desktop, and saves it to a file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureScreenToFile(string filename, ImageFormat format)
        {
            Image img = CaptureScreen();
            img.Save(filename, format);
        }

        /// <summary>
        /// Helper class containing Gdi32 API functions
        /// </summary>
        private class GDI32
        {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary>
        /// Helper class containing User32 API functions
        /// </summary>
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        }

    }
}
