using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace EQ_helper
{
    public class EQState
    {
        public float health;
        public float mana;

        public float petHealth;

        public float targetHealth;
        public MonsterInfo targetInfo;

        public static EQState mostRecentState = null;

        public enum CharacterState
        {
            SITTING,
            STANDING,
            COMBAT,
            POISONED,
            UNKNOWN
        }
        public CharacterState characterState;

        public double minutesSinceMidnight;

        public static EQState GetMostRecentEQState()
        {
            return mostRecentState;
        }

            public static EQState GetCurrentEQState()
        {
            EQState currentEQState = new EQState();

            Bitmap bm = EQScreen.GetEQBitmap();
            currentEQState.mana = GetManaPercentFilled(bm);
            currentEQState.health = GetHealthPercentFilled(bm);
            currentEQState.targetHealth = GetTargetHealthPercentFilled(bm);
            currentEQState.petHealth = GetPetHealthPercentFilled(bm);
            currentEQState.targetInfo = GetTargetMonster(bm);
            currentEQState.characterState = GetCharacterState(bm);
            currentEQState.minutesSinceMidnight = GetMinutesSinceMidnight();
            bm.Dispose();

            mostRecentState = currentEQState;

            return currentEQState;
        }

        public static float GetManaPercentFilled(Bitmap bm)
        {
            return EQScreen.GetBarPercentFilled(bm, EQScreen.manaBarXMin, EQScreen.manaBarXMax, EQScreen.manaBarY, EQScreen.IsManaColor);
        }
        public static float GetHealthPercentFilled(Bitmap bm)
        {
            return EQScreen.GetBarPercentFilled(bm, EQScreen.healthBarXMin, EQScreen.healthBarXMax, EQScreen.healthBarY, EQScreen.IsHealthColor);
        }
        public static float GetTargetHealthPercentFilled(Bitmap bm)
        {
            return EQScreen.GetBarPercentFilled(bm, EQScreen.targetHealthBarXMin, EQScreen.targetHealthBarXMax, EQScreen.targetHealthBarY, EQScreen.IsHealthColor);
        }
        public static float GetPetHealthPercentFilled(Bitmap bm)
        {
            return EQScreen.GetBarPercentFilled(bm, EQScreen.petHealthBarXMin, EQScreen.petHealthBarXMax, EQScreen.petHealthBarY, EQScreen.IsPetHealthColor);
        }
        public static MonsterInfo GetTargetMonster(Bitmap bm)
        {
            Color conColor = bm.GetPixel(EQScreen.targetConX, EQScreen.targetConY);
            return MonsterData.getInfoFromColor(conColor);
        }
        public static CharacterState GetCharacterState(Bitmap bm)
        {
            Color characterPixel = bm.GetPixel(EQScreen.characterStateX, EQScreen.characterStateY);//bm.GetPixel(1659, 902); // clean me up

            if (Color.Equals(characterPixel, EQScreen.SITTING_CHARACTER_COLOR)) { return CharacterState.SITTING; }
            if (Color.Equals(characterPixel, EQScreen.STANDING_CHARACTER_COLOR)) { return CharacterState.STANDING; }
            if (Color.Equals(characterPixel, EQScreen.COMBAT_CHARACTER_COLOR)) { return CharacterState.COMBAT; }
            if (Color.Equals(characterPixel, EQScreen.POISONED_CHARACTER_COLOR)) { return CharacterState.POISONED; }

            return CharacterState.UNKNOWN;
        }
        public static double GetMinutesSinceMidnight()
        {
            // Monday, 2/4/2019 at 11:13:24 is exactly 1pm in game
            // In-game hours are 3 minutes long, so a day is 72 minutes
            // so let's start time at midnight
            // "02/04/2019 10:34:22" is the time above -39 minutes
            // Night time is from 9pm-7am

            DateTime startTime = DateTime.Parse("02/04/2019 10:34:22");
            DateTime now = DateTime.Now;

            TimeSpan timeSinceMidnight = now - startTime;
            Console.WriteLine(String.Format("timeSinceMidnight: {0}", timeSinceMidnight));
            Console.WriteLine(String.Format("timeSinceMidnight.TotalMinutes: {0}", timeSinceMidnight.TotalMinutes));
            double remainderMinutes = timeSinceMidnight.TotalMinutes % 72;
            Console.WriteLine(String.Format("remainderMinutes: {0}", remainderMinutes));

            return remainderMinutes;
        }
    }
}
