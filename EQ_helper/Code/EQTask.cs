using InputManager;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

using WindowsInput;
using InputManager;

namespace EQ_helper
{
    // probably should be stateless.  Perform single actions and let player update on 100ms increments? tasks should be super small
    // and player should handle the more complicated state tracking to allow for interrupts?
    public static class EQTask
    {
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        public static async Task<bool> FocusOnEQWindowTask()
        {
            IntPtr h = EQScreen.GetEQWindowHandle();
            if (h == IntPtr.Zero) { return false; }

            SetForegroundWindow(h);

            await Task.Delay(500);
            return true;
        }

        // TODO: revamp this
        public static bool CurrentTimeInsideDuration(long startTime, long duration)
        {
            return (DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime) < duration;
        }

        private static long FARMING_TIME_MILLIS = (long)(60 * 60 * 1000);
        private static long PETBUFF_TIME_MILLIS = (long)(14.5 * 60 * 1000);
        public static async Task<bool> CoreGameplayLoopTask()
        {
            await FocusOnEQWindowTask();

            long farmingStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long lastBuffCastTime = 0;

            while (CurrentTimeInsideDuration(farmingStartTime, FARMING_TIME_MILLIS))
            {
                await RestUntilFullManaTask();

                if (!CurrentTimeInsideDuration(lastBuffCastTime, PETBUFF_TIME_MILLIS))
                {
                    await ApplyPetBuffTask();
                    lastBuffCastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }

                await RestUntilFullManaTask();
                await FindMobShieldThenSendPetAndKillTask();
            }

            return true;
        }

        public static async Task<bool> DoSingleFightTask()
        {
            await FocusOnEQWindowTask();
            //await FindAndKillMobTask();
            await FindMobShieldThenSendPetAndKillTask();
            await RestUntilFullManaTask();

            return true;
        }

        public static async Task<bool> FindAndKillMobTask()
        {
            await ApplyDamageShieldTask();

            bool findTargetResult = await FindAnyTargetWithMacroTask();
            if(!findTargetResult)
            {
                return false;
            }

            await PetPullTask();
            await NukeUntilDeadTask();

            return true;
        }

        public static async Task<bool> FindMobShieldThenSendPetAndKillTask()
        {
            await HideCorpsesTask();

            bool findTargetResult = await FindNearestTargetTask();
            if(!findTargetResult){ return false; }

            await ApplyDamageShieldTask();
            await RetargetFoundTargetTask();

            await PetAttackTask();
            await NukeUntilDeadTask();

            await LootCorpseTask();

            return true;
        }

        public static async Task<bool> LootCorpseTask()
        {
            Keyboard.KeyPress(Keys.F10); await Task.Delay(500);
            Keyboard.KeyPress(Keys.D7); await Task.Delay(500);
            Keyboard.KeyPress(Keys.Escape); await Task.Delay(500);

            return true;
        }

        public static async Task<bool> NukeUntilDeadTask()
        {
            EQState currentState = EQState.GetCurrentEQState();
            int nukeAttempts = 1;
            while (currentState.targetHealth > 0.00 && nukeAttempts < 15)
            {
                nukeAttempts++;
                await NukeTask();
                currentState = EQState.GetCurrentEQState();
            }

            return true;
        }

        public static async Task<bool> EnterCombatTask()
        {
            Keyboard.KeyPress(Keys.Q); await Task.Delay(100);
            return true;
        }

        public static async Task<bool> DeselectTargetTask()
        {
            Keyboard.KeyPress(Keys.Escape); await Task.Delay(100);
            return true;
        }

        public static async Task<bool> WaitUntilDeadTask()
        {
            EQState currentState = EQState.GetCurrentEQState();
            int attempts = 0;
            while (currentState.targetHealth > 0.00 && currentState.targetInfo.con != MonsterCon.NONE && attempts < 150)
            {
                attempts++;
                await Task.Delay(100);
                currentState = EQState.GetCurrentEQState();
            }
            await Task.Delay(50);

            return true;
        }

        public static async Task<bool> NukeTask()
        {
            Keyboard.KeyPress(Keys.D5); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.D5); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.D5); await Task.Delay(1000);
            await Task.Delay(5000);

            return true;
        }

        public static async Task<bool> ApplyPetBuffTask()
        {
            // Put Buff On Pet
            Keyboard.KeyPress(Keys.D8); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.D8); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.D8); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.D8); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.D8); await Task.Delay(1000);

            // Wait for Buff to Finish
            await Task.Delay(8000);

            return true;
        }

        public static async Task<bool> ApplyDamageShieldTask()
        {
            // Put Shield On Pet
            Keyboard.KeyPress(Keys.D1); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.D1); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.D1); await Task.Delay(1000);

            // Wait for Buff to Finish
            await Task.Delay(8000);

            return true;
        }

        public static async Task<bool> HideCorpsesTask()
        {
            // Hide Corpses
            Keyboard.KeyPress(Keys.D9); await Task.Delay(2000);

            Keyboard.KeyPress(Keys.Escape); await Task.Delay(500);
            return false;
        }

        public static async Task<bool> RetargetFoundTargetTask()
        {
            Keyboard.KeyPress(Keys.F9); await Task.Delay(500);
            return true;
        }

        public static async Task<bool> FindNearestTargetTask()
        {
             Keyboard.KeyPress(Keys.Escape); await Task.Delay(200);
            int findTargetAttempts = 1;
            int maxFindAttempts = 100;
            while (findTargetAttempts <= 100)
            {
                // Find Target
                Keyboard.KeyPress(Keys.F8); await Task.Delay(500);
                EQState currentState = EQState.GetCurrentEQState();
                if (currentState.targetInfo.con != MonsterCon.NONE) { return true; }
                findTargetAttempts++;

                if(findTargetAttempts % 10 == 0)
                {
                    // change camera view
                    Keyboard.KeyPress(Keys.F11); await Task.Delay(500);
                }
            }

            return false;
        }

        public static async Task<bool> FindSpecificTarget()
        {
            await HideCorpsesTask();

            Keyboard.KeyPress(Keys.Escape); await Task.Delay(200);
            EQState currentState = EQState.GetCurrentEQState();
            if (currentState.targetInfo.con != MonsterCon.NONE) { return false; }

            Keyboard.KeyPress(Keys.Escape); await Task.Delay(200);
            Keyboard.KeyPress(Keys.D0); await Task.Delay(500);
            currentState = EQState.GetCurrentEQState();
            if (currentState.targetInfo.con != MonsterCon.NONE && currentState.characterState != EQState.CharacterState.COMBAT) { return true; }

            return false;
        }

        // Macro should be close to
        // /target a_
        public static async Task<bool> FindAnyTargetWithMacroTask()
        {
            await HideCorpsesTask();

            int findTargetAttempts = 1;
            while (findTargetAttempts < 20)
            {
                // Find Target
                Keyboard.KeyPress(Keys.D2); await Task.Delay(2000);
                EQState currentState = EQState.GetCurrentEQState();
                if (currentState.targetInfo.con != MonsterCon.NONE) { return true; }
                findTargetAttempts++;
            }

            return false;
        }

        public static async Task<bool> PetAttackTask()
        {
            // Find Target
            Keyboard.KeyPress(Keys.D3); await Task.Delay(500);

            // wait until they hit target
            EQState currentState = EQState.GetCurrentEQState();
            int attackTargetAttempts = 1;
            while (currentState.targetHealth > 0.98 && attackTargetAttempts <= 10)
            {
                attackTargetAttempts++;
                if(attackTargetAttempts % 3 == 0) { Keyboard.KeyPress(Keys.D3); }
                await Task.Delay(1000);
                currentState = EQState.GetCurrentEQState();
            }

            return true;
        }

        public static async Task<bool> PetBackTask()
        {
            // pull back
            Keyboard.KeyPress(Keys.D4); await Task.Delay(500);
            Keyboard.KeyPress(Keys.D4); await Task.Delay(500);
            Keyboard.KeyPress(Keys.D4); await Task.Delay(500);
            Keyboard.KeyPress(Keys.D4); await Task.Delay(500);

            return true;
        }

        public static async Task<bool> GTFOTask()
        {
            // pull back
            Keyboard.KeyPress(Keys.Add); await Task.Delay(500);
            Keyboard.KeyPress(Keys.Add); await Task.Delay(500);
            Keyboard.KeyPress(Keys.Add); await Task.Delay(500);
            Keyboard.KeyPress(Keys.Add); await Task.Delay(9000);
            Keyboard.KeyPress(Keys.Add); await Task.Delay(500);
            Keyboard.KeyPress(Keys.Add); await Task.Delay(500);
            Keyboard.KeyPress(Keys.Add); await Task.Delay(500);
            Keyboard.KeyPress(Keys.Add); await Task.Delay(9000);
            Keyboard.KeyPress(Keys.Add); await Task.Delay(500);
            Keyboard.KeyPress(Keys.Add); await Task.Delay(500);
            Keyboard.KeyPress(Keys.Add); await Task.Delay(500);
            Keyboard.KeyPress(Keys.Add); await Task.Delay(9000);

            return true;
        }

        public static async Task<bool> PetPullTask()
        {
            // Find Target
            Keyboard.KeyPress(Keys.D3); await Task.Delay(500);
            Keyboard.KeyPress(Keys.D3); await Task.Delay(500);
            Keyboard.KeyPress(Keys.D3); await Task.Delay(500);

            // wait until they hit target
            EQState currentState = EQState.GetCurrentEQState();
            while (currentState.targetHealth > 0.98)
            {
                await Task.Delay(2000);
                currentState = EQState.GetCurrentEQState();
            }

            // pull back
            Keyboard.KeyPress(Keys.D4); await Task.Delay(500);
            Keyboard.KeyPress(Keys.D4); await Task.Delay(500);
            Keyboard.KeyPress(Keys.D4); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.D4); await Task.Delay(1000);

            // wait till they get back
            await Task.Delay(1000);

            return true;
        }

        public static async Task<bool> RestUntilFullManaTask()
        {
            float manaThreshold = 0.98f;
            EQState currentState = EQState.GetCurrentEQState();
            if (currentState.mana >= manaThreshold) { return true; }

            // rest
            Keyboard.KeyPress(Keys.D6); await Task.Delay(1000);
            while (currentState.mana < manaThreshold) {
                await Task.Delay(1000);
                currentState = EQState.GetCurrentEQState();
            }

            return true;
        }

        public static async Task<bool> SpamZeroTask(int fullManaBarsToSpam)
        {
            await FocusOnEQWindowTask();

            int timesSpammed = 0;
            while (timesSpammed < fullManaBarsToSpam)
            {
                timesSpammed++;
                EQState currentState = EQState.GetCurrentEQState();
                while (currentState.mana > 0.10)
                {
                    Keyboard.KeyPress(Keys.D0); await Task.Delay(500);
                    Keyboard.KeyPress(Keys.D0); await Task.Delay(500);
                    Keyboard.KeyPress(Keys.D0); await Task.Delay(8000);
                    currentState = EQState.GetCurrentEQState();
                }

                await RestUntilFullManaTask();
            }

            return true;
        }

        public static async Task<bool> LevelUpSkillTask()
        {
            Keyboard.KeyPress(Keys.Oemplus); await Task.Delay(3500);

            return true;
        }

        public static async Task<bool> DamageShieldBotTask()
        {
            Keyboard.KeyPress(Keys.D1); await Task.Delay(500);
            Keyboard.KeyPress(Keys.D1); await Task.Delay(500);
            Keyboard.KeyPress(Keys.D1); await Task.Delay(5000);

            return true;
        }
    }
}
