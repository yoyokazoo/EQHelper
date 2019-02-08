using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace EQ_helper
{
    public static class EQScreen
    {
        // TODO: Swap coords based on checkbox
        // Desktop coords
        /*
        public static int manaBarXMin = 1541;
        public static int manaBarXMax = 1916;
        public static int manaBarY = 1175;

        public static int healthBarXMin = 1541;
        public static int healthBarXMax = 1916;
        public static int healthBarY = 1165;

        public static int targetHealthBarXMin = 1541;
        public static int targetHealthBarXMax = 1914;
        public static int targetHealthBarY = 1014;

        public static int petHealthBarXMin = 1541;
        public static int petHealthBarXMax = 1916;
        public static int petHealthBarY = 1169;

        public static int targetConX = 1536;
        public static int targetConY = 1014;
        */

            // Laptop coords
        public static int manaBarXMin = 1160;
        public static int manaBarXMax = 1668;
        public static int manaBarY = 936;

        public static int healthBarXMin = 1160;
        public static int healthBarXMax = 1668;
        public static int healthBarY = 926;

        public static int petHealthBarXMin = 1160;
        public static int petHealthBarXMax = 1668;
        public static int petHealthBarY = 930;

        public static int targetHealthBarXMin = 1161;
        public static int targetHealthBarXMax = 1663;
        public static int targetHealthBarY = 808;

        public static int targetConX = 1156;
        public static int targetConY = 808;

        // for reference
        private static Color EMPTY_BAR_COLOR = Color.FromArgb(57, 60, 57);
        private static Color WHITE_TEXT_COLOR = Color.FromArgb(255, 255, 255);
        private static Color MANA_COLOR = Color.FromArgb(0, 114, 231);
        private static Color HEALTH_COLOR = Color.FromArgb(217, 0, 0);
        private static Color PET_HEALTH_COLOR = Color.FromArgb(32, 118, 32);

        public static Color SITTING_CHARACTER_COLOR = Color.FromArgb(247, 247, 222);
        public static Color STANDING_CHARACTER_COLOR = Color.FromArgb(82, 117, 148);
        public static Color COMBAT_CHARACTER_COLOR = Color.FromArgb(198, 195, 198);

        private static IntPtr eqWindowHandle = IntPtr.Zero;

        public static Bitmap GetEQBitmap()
        {
            return ScreenCapture.CaptureWindowBM(GetEQWindowHandle());
        }

        public static IntPtr GetEQWindowHandle()
        {
            if(eqWindowHandle != IntPtr.Zero) { return eqWindowHandle; }

            Process p = Process.GetProcessesByName("eqgame").FirstOrDefault();
            eqWindowHandle = p.MainWindowHandle;

            return eqWindowHandle;
        }

        // checks if the bar is some shade of grey
        public static bool IsEmptyBarColor(Color colorToCheck)
        {
            return Math.Abs(colorToCheck.R - colorToCheck.G) <= 10 && Math.Abs(colorToCheck.R - colorToCheck.B) <= 10;
        }
        // check if the bar is mostly red/blue/green or white (% text)
        public static bool IsHealthColor(Color colorToCheck)
        {
            return (colorToCheck.R - colorToCheck.G > 15 && colorToCheck.R - colorToCheck.B > 15) || Color.Equals(colorToCheck, WHITE_TEXT_COLOR);
        }
        public static bool IsPetHealthColor(Color colorToCheck)
        {
            return (colorToCheck.G - colorToCheck.B > 15 && colorToCheck.G - colorToCheck.R > 15) || Color.Equals(colorToCheck, WHITE_TEXT_COLOR);
        }
        public static bool IsManaColor(Color colorToCheck)
        {
            return (colorToCheck.B - colorToCheck.R > 15 && colorToCheck.B - colorToCheck.G > 15) || Color.Equals(colorToCheck, WHITE_TEXT_COLOR);
        }

        public static float GetBarPercentFilled(Bitmap bm, int minX, int maxX, int y, Func<Color, bool> colorCheckMethod)
        {
            int filledBarX = minX;
            for (int x = minX; x <= maxX; x++)
            {
                Color pixelToCheck = bm.GetPixel(x, y);

                if (colorCheckMethod(pixelToCheck))
                {
                    filledBarX = x;
                }
                else if (IsEmptyBarColor(pixelToCheck))
                {
                    break;
                }
            }

            float barFilled = (filledBarX - minX);
            float barTotal = (maxX - minX);
            float barPercentFilled = barFilled / barTotal;

            return barPercentFilled;
        }
    }
}
