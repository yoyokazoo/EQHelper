using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;

// taken from http://www.developerfusion.com/code/4630/capture-a-screen-shot/
// heavy modifications done

namespace EQ_helper
{
    /// <summary>
    /// Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public static class ScreenCapture
    {
        public static Color findAverageColor(Point corner, int GEM_WIDTH, int GEM_HEIGHT, Bitmap bm)
        {
            int rSum = 0;
            int gSum = 0;
            int bSum = 0;
            int numColors = 36; // precalced for speed, needs to change if the bounds change

            int lowerBoundX = (GEM_WIDTH / 2) - 3;
            int upperBoundX = (GEM_WIDTH / 2) + 3;

            int lowerBoundY = (GEM_HEIGHT / 2) - 3;
            int upperBoundY = (GEM_HEIGHT / 2) + 3;

            for (int x = lowerBoundX; x < upperBoundX; x++)
            {
                for (int y = lowerBoundY; y < upperBoundY; y++)
                {
                    Color pixel = bm.GetPixel(corner.X + x, corner.Y + y);
                    rSum += pixel.R;
                    gSum += pixel.G;
                    bSum += pixel.B;
                }
            }
            return Color.FromArgb(rSum / numColors, gSum / numColors, bSum / numColors);
        }

        public static Color[] findKeyColorPoints(Point corner, int GEM_WIDTH, int GEM_HEIGHT, Bitmap bm)
        {
            Color[] colors = new Color[9];

            int[] xCoords = { (GEM_WIDTH / 2) - 3, (GEM_WIDTH / 2), (GEM_WIDTH / 2) + 3 };
            int[] yCoords = { (GEM_HEIGHT / 2) - 3, (GEM_HEIGHT / 2), (GEM_HEIGHT / 2) + 3 };

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Color pixel = bm.GetPixel(corner.X + xCoords[x], corner.Y + yCoords[y]);
                    colors[x * 3 + y] = pixel;
                }
            }
            return colors;
        }

        // defaults are set to king.com width and height
        public static Rectangle findTopCornerRectFromRoundedCorner(Bitmap pic, Color outerColor, Color cornerColor, int gameWidth = 775, int gameHeight = 600)
        {
            // we look 1 pixel behind, hence starting at 1
            for (int x = 1; x < pic.Width - 15; x++)
            {
                for (int y = 1; y < pic.Height - 15; y++)
                {
                    if (colorsAlmostMatch(pic.GetPixel(x - 1, y - 1), outerColor) &&
                        colorsAlmostMatch(pic.GetPixel(x - 1, y + 15), outerColor) &&
                        colorsAlmostMatch(pic.GetPixel(x + 15, y - 1), outerColor) &&
                        colorsAlmostMatch(pic.GetPixel(x, y + 15), cornerColor, 35) &&
                        colorsAlmostMatch(pic.GetPixel(x + 15, y), cornerColor, 35))
                    {
                        return new Rectangle(x, y, gameWidth, gameHeight);
                    }
                }
            }

            return new Rectangle(-1, -1, -1, -1);
        }

        public static bool colorsExactlyMatch(Color firstColor, Color secondColor)
        {
            if (firstColor.R == secondColor.R && 
                firstColor.G == secondColor.G && 
                firstColor.B == secondColor.B)
            {
                return true;
            }

            return false;
        }

        public static bool colorsAlmostMatch(Color firstColor, Color secondColor, int threshold = 20)
        {
            int r = Math.Abs(firstColor.R - secondColor.R);
            int g = Math.Abs(firstColor.G - secondColor.G);
            int b = Math.Abs(firstColor.B - secondColor.B);

            if (r > threshold || g > threshold || b > threshold)
            {
                return false;
            }

            return true;
        }

        public static Bitmap getDesktopBitmap()
        {
            try
            {
                Bitmap bm = CaptureScreenBM();

                return bm;
            }
            catch (OutOfMemoryException)
            {

            }
            catch (NullReferenceException)
            {

            }
            return null;
        }

        /// <summary>
        /// Creates an Image object containing a screen shot of the entire desktop
        /// </summary>
        /// <returns></returns>
        public static Bitmap CaptureScreenBM()
        {
            return CaptureWindowBM(User32.GetDesktopWindow());
        }
        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
        /// <returns></returns>
        public static Bitmap CaptureWindowBM(IntPtr handle)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up 
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);

            //// get a .NET image object for it
            //Image img = Image.FromHbitmap(hBitmap);

            Bitmap img = Bitmap.FromHbitmap(hBitmap);

            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
            return img;
        }
        /// <summary>
        /// Creates an Image object containing a screen shot of the entire desktop
        /// </summary>
        /// <returns></returns>
        public static Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }

        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
        /// <returns></returns>
        public static Image CaptureWindow(IntPtr handle)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up 
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
            return img;
        }
        /// <summary>
        /// Captures a screen shot of a specific window, and saves it to a file
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public static void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }
        /// <summary>
        /// Captures a screen shot of the entire desktop, and saves it to a file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public static void CaptureScreenToFile(string filename, ImageFormat format)
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