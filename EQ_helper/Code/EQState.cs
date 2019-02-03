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

        public static EQState GetCurrentEQState()
        {
            EQState currentEQState = new EQState();

            Bitmap bm = EQScreen.GetEQBitmap();
            currentEQState.mana = GetManaPercentFilled(bm);
            currentEQState.health = GetHealthPercentFilled(bm);
            currentEQState.targetHealth = GetTargetHealthPercentFilled(bm);
            currentEQState.petHealth = GetPetHealthPercentFilled(bm);
            currentEQState.targetInfo = GetTargetMonster(bm);
            bm.Dispose();

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
    }
}
