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
        const int HP_BAR_TOPLEFT_X = 1542;
        const int HP_BAR_TOPRIGHT_X = 1917;
        const int HP_BAR_TOPLEFT_Y = 1164;

        const int TARGET_BAR_TOPLEFT_X = 1541;
        const int TARGET_BAR_TOPRIGHT_X = 1914;
        const int TARGET_BAR_TOPLEFT_Y = 1012;
        */

        // Laptop coords
        const int HP_BAR_TOPLEFT_X = 1160;
        const int HP_BAR_TOPRIGHT_X = 1668;
        const int HP_BAR_TOPLEFT_Y = 926;

        const int TARGET_BAR_TOPLEFT_X = 1161;
        const int TARGET_BAR_TOPRIGHT_X = 1663;
        const int TARGET_BAR_TOPLEFT_Y = 808;


        public static int healthBarXMin = HP_BAR_TOPLEFT_X;
        public static int healthBarXMax = HP_BAR_TOPRIGHT_X;
        public static int healthBarY = HP_BAR_TOPLEFT_Y;

        public static int petHealthBarXMin = HP_BAR_TOPLEFT_X;
        public static int petHealthBarXMax = HP_BAR_TOPRIGHT_X;
        public static int petHealthBarY = HP_BAR_TOPLEFT_Y + 4;

        public static int manaBarXMin = HP_BAR_TOPLEFT_X;
        public static int manaBarXMax = HP_BAR_TOPRIGHT_X;
        public static int manaBarY = HP_BAR_TOPLEFT_Y + 10;

        public static int targetHealthBarXMin = TARGET_BAR_TOPLEFT_X;
        public static int targetHealthBarXMax = TARGET_BAR_TOPRIGHT_X;
        public static int targetHealthBarY = TARGET_BAR_TOPLEFT_Y;

        public static int targetConX = TARGET_BAR_TOPLEFT_X - 5;
        public static int targetConY = TARGET_BAR_TOPLEFT_Y;

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
