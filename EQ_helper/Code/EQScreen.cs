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
        // Laptop coords
        public static int HP_BAR_TOPLEFT_X = 1160;
        public static int HP_BAR_TOPRIGHT_X = 1668;
        public static int HP_BAR_TOPLEFT_Y = 926;

        public static int TARGET_BAR_TOPLEFT_X;
        public static int TARGET_BAR_TOPRIGHT_X;
        public static int TARGET_BAR_TOPLEFT_Y;

        public static int healthBarXMin;
        public static int healthBarXMax;
        public static int healthBarY;

        public static int petHealthBarXMin;
        public static int petHealthBarXMax;
        public static int petHealthBarY;

        public static int manaBarXMin;
        public static int manaBarXMax;
        public static int manaBarY;

        public static int characterStateX;
        public static int characterStateY;

        public static int targetHealthBarXMin;
        public static int targetHealthBarXMax;
        public static int targetHealthBarY;

        public static int targetConX;
        public static int targetConY;

        public static int nameCoordX1;
        public static int nameCoordY1;
        public static int nameCoordX2;
        public static int nameCoordY2;
        public static int nameCoordX3;
        public static int nameCoordY3;
        public static int nameCoordX4;
        public static int nameCoordY4;

        // for reference
        private static Color EMPTY_BAR_COLOR = Color.FromArgb(57, 60, 57);
        private static Color WHITE_TEXT_COLOR = Color.FromArgb(255, 255, 255);
        private static Color MANA_COLOR = Color.FromArgb(0, 114, 231);
        private static Color HEALTH_COLOR = Color.FromArgb(217, 0, 0);
        private static Color PET_HEALTH_COLOR = Color.FromArgb(32, 118, 32);

        public static Color SITTING_CHARACTER_COLOR = Color.FromArgb(247, 247, 222);
        public static Color STANDING_CHARACTER_COLOR = Color.FromArgb(82, 117, 148);
        public static Color COMBAT_CHARACTER_COLOR = Color.FromArgb(198, 195, 198);
        public static Color POISONED_CHARACTER_COLOR = Color.FromArgb(148, 215, 0);

        public static Color NAME_WHITE = Color.FromArgb(240, 240, 240);
        public static Color NAME_GREY = Color.FromArgb(210, 210, 211);

        private static IntPtr eqWindowHandle = IntPtr.Zero;

        public static void SetComputer(bool laptop)
        {
            // TODO: clean this up, pass in a struct
            if(laptop)
            {
                HP_BAR_TOPLEFT_X = 1160;
                HP_BAR_TOPRIGHT_X = 1668;
                HP_BAR_TOPLEFT_Y = 926;

                TARGET_BAR_TOPLEFT_X = 1161;
                TARGET_BAR_TOPRIGHT_X = 1663;
                TARGET_BAR_TOPLEFT_Y = 808;   
            }
            else
            {
                HP_BAR_TOPLEFT_X = 1572;
                HP_BAR_TOPRIGHT_X = 1945; // + 373?
                HP_BAR_TOPLEFT_Y = 1012;

                TARGET_BAR_TOPLEFT_X = 1541;
                TARGET_BAR_TOPRIGHT_X = 1914;
                TARGET_BAR_TOPLEFT_Y = 1012;
            }

            healthBarXMin = HP_BAR_TOPLEFT_X;
            healthBarXMax = HP_BAR_TOPRIGHT_X;
            healthBarY = HP_BAR_TOPLEFT_Y;

            petHealthBarXMin = HP_BAR_TOPLEFT_X;
            petHealthBarXMax = HP_BAR_TOPRIGHT_X;
            petHealthBarY = HP_BAR_TOPLEFT_Y + 4;

            manaBarXMin = HP_BAR_TOPLEFT_X;
            manaBarXMax = HP_BAR_TOPRIGHT_X;
            manaBarY = HP_BAR_TOPLEFT_Y + 10;

            characterStateX = HP_BAR_TOPRIGHT_X - 9;
            characterStateY = HP_BAR_TOPLEFT_Y - 25;

            targetHealthBarXMin = TARGET_BAR_TOPLEFT_X;
            targetHealthBarXMax = TARGET_BAR_TOPRIGHT_X;
            targetHealthBarY = TARGET_BAR_TOPLEFT_Y;

            targetConX = TARGET_BAR_TOPLEFT_X - 5;
            targetConY = TARGET_BAR_TOPLEFT_Y;

            // Bottom of Y
            nameCoordX1 = HP_BAR_TOPLEFT_X;
            nameCoordY1 = HP_BAR_TOPLEFT_Y - 28;

            // Top Left of Y, T
            nameCoordX2 = HP_BAR_TOPLEFT_X - 3;
            nameCoordY2 = HP_BAR_TOPLEFT_Y - 35;

            // Bottom of T
            nameCoordX3 = HP_BAR_TOPLEFT_X - 1;
            nameCoordY3 = HP_BAR_TOPLEFT_Y - 28;
        }

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

        public static string GetNameFromBitmap(Bitmap bm)
        {
            Console.WriteLine(String.Format("Name coord 1 ({0},{1}) is {2}", nameCoordX1, nameCoordY1, bm.GetPixel(nameCoordX1, nameCoordY1)));
            Console.WriteLine(String.Format("Name coord 2 ({0},{1}) is {2}", nameCoordX2, nameCoordY2, bm.GetPixel(nameCoordX2, nameCoordY2)));
            Console.WriteLine(String.Format("Name coord 3 ({0},{1}) is {2}", nameCoordX3, nameCoordY3, bm.GetPixel(nameCoordX3, nameCoordY3)));
            if (bm.GetPixel(nameCoordX1, nameCoordY1) == NAME_WHITE && bm.GetPixel(nameCoordX2, nameCoordY2) == NAME_WHITE)
            {
                return "Yoyokazoo";
            }
            else if(bm.GetPixel(nameCoordX2, nameCoordY2) == NAME_WHITE && bm.GetPixel(nameCoordX3, nameCoordY3) == NAME_GREY)
            {
                return "Trakklo";
            }
            return "UNKNOWN";
        }
    }
}
