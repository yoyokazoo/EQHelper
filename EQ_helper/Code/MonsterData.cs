using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace EQ_helper
{
    public enum MonsterCon
    {
        NONE = 0,
        GREY = 1,
        GREEN = 2,
        LIGHT_BLUE = 3,
        DARK_BLUE = 4,
        WHITE = 5,
        YELLOW = 6,
        RED = 7
    }

    public struct MonsterInfo
    {
        public Color color { get; set; }
        public MonsterCon con { get; set; }
        public String name { get; set; }

       public MonsterInfo(Color clr, MonsterCon c, String n)
        {
            color = clr;
            con = c;
            name = n;
        }
    }

    static class MonsterData
    {
        public static MonsterInfo NONE_MONSTER_INFO = new MonsterInfo(      Color.FromArgb(22, 23, 33),        MonsterCon.NONE,        "None");
        public static MonsterInfo GREY_MONSTER_INFO = new MonsterInfo(      Color.FromArgb(111, 112, 111),  MonsterCon.GREY,        "Grey");
        public static MonsterInfo LIGHT_BLUE_MONSTER_INFO = new MonsterInfo(Color.FromArgb(0, 210, 209),    MonsterCon.LIGHT_BLUE,  "Light Blue");
        public static MonsterInfo GREEN_MONSTER_INFO = new MonsterInfo(     Color.FromArgb(0, 112, 0),      MonsterCon.GREEN,       "Green");
        public static MonsterInfo DARK_BLUE_MONSTER_INFO = new MonsterInfo( Color.FromArgb(0, 56, 222),     MonsterCon.DARK_BLUE,   "Dark Blue");
        public static MonsterInfo WHITE_MONSTER_INFO = new MonsterInfo(     Color.FromArgb(209, 210, 209),  MonsterCon.WHITE,       "White");
        public static MonsterInfo YELLOW_MONSTER_INFO = new MonsterInfo(    Color.FromArgb(209, 210, 0),        MonsterCon.YELLOW,      "Yellow");
        public static MonsterInfo RED_MONSTER_INFO = new MonsterInfo(       Color.FromArgb(209, 0, 0),      MonsterCon.RED,         "Red");

        public static MonsterInfo getInfoFromColor(Color colorToCheck)
        {
            if (Color.Equals(GREY_MONSTER_INFO.color, colorToCheck)){ return GREY_MONSTER_INFO; }
            if (Color.Equals(LIGHT_BLUE_MONSTER_INFO.color, colorToCheck)) { return LIGHT_BLUE_MONSTER_INFO; }
            if (Color.Equals(GREEN_MONSTER_INFO.color, colorToCheck)) { return GREEN_MONSTER_INFO; }
            if (Color.Equals(DARK_BLUE_MONSTER_INFO.color, colorToCheck)) { return DARK_BLUE_MONSTER_INFO; }
            if (Color.Equals(WHITE_MONSTER_INFO.color, colorToCheck)) { return WHITE_MONSTER_INFO; }
            if (Color.Equals(YELLOW_MONSTER_INFO.color, colorToCheck)) { return YELLOW_MONSTER_INFO; }
            if (Color.Equals(RED_MONSTER_INFO.color, colorToCheck)) { return RED_MONSTER_INFO; }

            return NONE_MONSTER_INFO;
        }
        

    }
}
