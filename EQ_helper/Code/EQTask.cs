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

        const Keys DAMAGE_SHIELD_KEY = Keys.D1;
        const Keys FIND_NORMAL_MOB_KEY = Keys.D2;
        const Keys PET_ATTACK_KEY = Keys.D3;
        const Keys PET_BACK_KEY = Keys.D4;
        const Keys NUKE_KEY = Keys.D5;
        const Keys REST_KEY = Keys.D6;//Keys.Control|Keys.S; // This should work??
        const Keys LOOT_KEY = Keys.D7;
        const Keys BUFF_PET_KEY = Keys.D8;
        const Keys HIDE_CORPSES_KEY = Keys.D9;
        const Keys FIND_SPECIAL_MOB_KEY = Keys.D0;
        const Keys HEAL_PET_KEY = Keys.OemMinus;
        const Keys GATE_KEY = Keys.Oemplus;

        const Keys TARGET_SELF_KEY = Keys.F1;
        const Keys TARGET_NEAREST_MOB_KEY = Keys.F8;
        const Keys RESELECT_PREVIOUS_TARGET_KEY = Keys.F9;
        const Keys TARGET_NEAREST_CORPSE_KEY = Keys.F10;
        const Keys CHANGE_CAMERA_ANGLE_KEY = Keys.F11;

        const Keys ENTER_COMBAT_KEY = Keys.Q;
        const Keys DESELECT_TARGETS_KEY = Keys.Escape;

        public static async Task<bool> FocusOnEQWindowTask()
        {
            IntPtr h = EQScreen.GetEQWindowHandle();
            if (h == IntPtr.Zero) { return false; }

            SetForegroundWindow(h);

            await Task.Delay(750);
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

            await LootTask();

            return true;
        }

        public static async Task<bool> LootTask(bool lootAll = false)
        {
            Keyboard.KeyPress(TARGET_NEAREST_CORPSE_KEY); await Task.Delay(500);
            Keyboard.KeyPress(LOOT_KEY); await Task.Delay(1500);

            if (lootAll)
            {
                Mouse.Move(703, 634);
                Mouse.ButtonDown(Mouse.MouseKeys.Left); await Task.Delay(50);
                Mouse.ButtonUp(Mouse.MouseKeys.Left); await Task.Delay(50);
                await Task.Delay(2500);
            }

            Keyboard.KeyPress(DESELECT_TARGETS_KEY); await Task.Delay(500);

            return true;
        }

        public static async Task<bool> NukeUntilDeadTask()
        {
            EQState currentState = EQState.GetCurrentEQState();
            int nukeAttempts = 1;
            while (currentState.targetHealth > 0.00 && nukeAttempts < 50)
            {
                nukeAttempts++;
                await NukeTask();
                currentState = EQState.GetCurrentEQState();
            }

            return true;
        }

        public static async Task<bool> LevelSkillUntilDeadTask()
        {
            EQState currentState = EQState.GetCurrentEQState();
            int levelSkillAttempts = 1;
            while (currentState.targetHealth > 0.00 && levelSkillAttempts < 50)
            {
                levelSkillAttempts++;
                await LevelUpSkillTask();
                currentState = EQState.GetCurrentEQState();
            }

            return true;
        }

        public static async Task<bool> EnterCombatTask()
        {
            Keyboard.KeyPress(ENTER_COMBAT_KEY); await Task.Delay(200);
            return true;
        }

        public static async Task<bool> DeselectTargetTask()
        {
            Keyboard.KeyPress(DESELECT_TARGETS_KEY); await Task.Delay(100);
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
            Keyboard.KeyPress(NUKE_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(NUKE_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(NUKE_KEY); await Task.Delay(1000);

            EQState currentState = EQState.GetCurrentEQState();
            return (currentState.targetHealth <= 0.00);
        }

        public static async Task<bool> ApplyPetBuffTask()
        {
            // Target self
            Keyboard.KeyPress(TARGET_SELF_KEY); await Task.Delay(1000);

            // Put Buff On Pet
            Keyboard.KeyPress(BUFF_PET_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(BUFF_PET_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(BUFF_PET_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(BUFF_PET_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(BUFF_PET_KEY); await Task.Delay(1000);

            // Wait for Buff to Finish
            await Task.Delay(8000);

            // deselect pet
            Keyboard.KeyPress(DESELECT_TARGETS_KEY); await Task.Delay(200);

            return true;
        }

        public static async Task<bool> ApplySelfBuffTask()
        {
            // Target self
            Keyboard.KeyPress(TARGET_SELF_KEY); await Task.Delay(1000);

            // Put Buff On Self
            Keyboard.KeyPress(HEAL_PET_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(HEAL_PET_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(HEAL_PET_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(HEAL_PET_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(HEAL_PET_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(HEAL_PET_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(HEAL_PET_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(HEAL_PET_KEY); await Task.Delay(1000);

            // Wait for Buff to Finish
            await Task.Delay(11000);

            // deselect pet
            Keyboard.KeyPress(DESELECT_TARGETS_KEY); await Task.Delay(200);

            return true;
        }

        public static async Task<bool> ApplyDamageShieldTask(bool targetSelf = false)
        {
            // Target self
            if (targetSelf) {
                Keyboard.KeyPress(DESELECT_TARGETS_KEY); await Task.Delay(100);
                Keyboard.KeyPress(TARGET_SELF_KEY); await Task.Delay(1000);
            }

            // Put Shield On Pet
            Keyboard.KeyPress(DAMAGE_SHIELD_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(DAMAGE_SHIELD_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(DAMAGE_SHIELD_KEY); await Task.Delay(1000);

            // Wait for Buff to Finish
            await Task.Delay(8000);

            return true;
        }

        public static async Task<bool> HideCorpsesTask()
        {
            // Hide Corpses
            Keyboard.KeyPress(HIDE_CORPSES_KEY); await Task.Delay(2000);

            Keyboard.KeyPress(DESELECT_TARGETS_KEY); await Task.Delay(500);
            return false;
        }

        public static async Task<bool> RetargetFoundTargetTask()
        {
            Keyboard.KeyPress(RESELECT_PREVIOUS_TARGET_KEY); await Task.Delay(500);
            return true;
        }

        public static async Task<bool> FindNearestTargetTask(bool cycleCamera = false, MonsterCon minCon = MonsterCon.GREY, MonsterCon maxCon = MonsterCon.RED)
        {
            Keyboard.KeyPress(DESELECT_TARGETS_KEY); await Task.Delay(200);
            int findTargetAttempts = 1;
            int maxFindAttempts = 100;
            while (findTargetAttempts <= 100)
            {
                // Find Target
                Keyboard.KeyPress(TARGET_NEAREST_MOB_KEY); await Task.Delay(500);
                EQState currentState = EQState.GetCurrentEQState();
                bool conInCorrectRange = ((int)currentState.targetInfo.con >= (int)minCon && (int)currentState.targetInfo.con <= (int)maxCon);
                Console.WriteLine(String.Format("min {0} max {1} target {2} in range? {3}", (int)minCon, (int)maxCon, (int)currentState.targetInfo.con, conInCorrectRange));
                if ((int)currentState.targetInfo.con >= (int)minCon && (int)currentState.targetInfo.con <= (int)maxCon) { return true; }
                if (currentState.targetInfo.con != MonsterCon.NONE) { Keyboard.KeyPress(DESELECT_TARGETS_KEY); await Task.Delay(200); }
                //if (currentState.targetInfo.con != MonsterCon.NONE) { return true; }
                findTargetAttempts++;

                if(findTargetAttempts % 10 == 0)
                {
                    // change camera view
                    if(cycleCamera)
                    {
                        Keyboard.KeyPress(CHANGE_CAMERA_ANGLE_KEY); await Task.Delay(500);
                    }
                }
            }

            return false;
        }

        public static async Task<bool> FindSpecificTarget()
        {
            await HideCorpsesTask();

            Keyboard.KeyPress(DESELECT_TARGETS_KEY); await Task.Delay(200);
            EQState currentState = EQState.GetCurrentEQState();
            if (currentState.targetInfo.con != MonsterCon.NONE) { return false; }

            Keyboard.KeyPress(DESELECT_TARGETS_KEY); await Task.Delay(200);
            Keyboard.KeyPress(FIND_SPECIAL_MOB_KEY); await Task.Delay(500);
            currentState = EQState.GetCurrentEQState();
            if (currentState.targetInfo.con != MonsterCon.NONE && currentState.characterState != EQState.CharacterState.COMBAT) { return true; }

            return false;
        }

        // Macro should be close to
        // /target a_
        public static async Task<bool> FindAnyTargetWithMacroTask()
        {
            int findTargetAttempts = 1;
            while (findTargetAttempts < 5)
            {
                // Find Target
                Keyboard.KeyPress(FIND_NORMAL_MOB_KEY); await Task.Delay(1500);
                EQState currentState = EQState.GetCurrentEQState();
                if (currentState.targetInfo.con != MonsterCon.NONE) { return true; }
                findTargetAttempts++;
            }

            return false;
        }

        public static async Task<bool> PetAttackTask()
        {
            // Find Target
            Keyboard.KeyPress(PET_ATTACK_KEY); await Task.Delay(500);

            // wait until they hit target
            EQState currentState = EQState.GetCurrentEQState();
            int attackTargetAttempts = 1;
            while (currentState.targetHealth > 0.98 && attackTargetAttempts <= 10)
            {
                attackTargetAttempts++;
                if(attackTargetAttempts % 3 == 0) { Keyboard.KeyPress(PET_ATTACK_KEY); }
                await Task.Delay(1000);
                currentState = EQState.GetCurrentEQState();
            }

            return true;
        }

        public static async Task<bool> PetBackTask()
        {
            // pull back
            Keyboard.KeyPress(PET_BACK_KEY); await Task.Delay(500);
            Keyboard.KeyPress(PET_BACK_KEY); await Task.Delay(500);
            Keyboard.KeyPress(PET_BACK_KEY); await Task.Delay(500);
            Keyboard.KeyPress(PET_BACK_KEY); await Task.Delay(500);

            return true;
        }

        public static async Task<bool> GTFOTask()
        {
            // pull back
            for (int i = 0; i < 100; i++)
            {
                Keyboard.KeyPress(GATE_KEY); await Task.Delay(500);
            }

            return true;
        }

        public static async Task<bool> PetPullTask()
        {
            // Find Target
            Keyboard.KeyPress(PET_ATTACK_KEY); await Task.Delay(500);
            Keyboard.KeyPress(PET_ATTACK_KEY); await Task.Delay(500);
            Keyboard.KeyPress(PET_ATTACK_KEY); await Task.Delay(500);

            // wait until they hit target
            EQState currentState = EQState.GetCurrentEQState();
            while (currentState.targetHealth > 0.98)
            {
                await Task.Delay(300);
                currentState = EQState.GetCurrentEQState();
            }

            // pull back
            Keyboard.KeyPress(PET_BACK_KEY); await Task.Delay(500);
            Keyboard.KeyPress(PET_BACK_KEY); await Task.Delay(500);
            Keyboard.KeyPress(PET_BACK_KEY); await Task.Delay(1000);
            Keyboard.KeyPress(PET_BACK_KEY); await Task.Delay(1000);

            // wait till they get back
            await Task.Delay(500);

            return true;
        }

        public static async Task<bool> RestUntilFullManaTask()
        {
            float manaThreshold = 0.98f;
            float hpThreshold = 0.98f;

            EQState currentState = EQState.GetCurrentEQState();
            if (currentState.mana >= manaThreshold && currentState.health >= hpThreshold) { return true; }

            // rest
            if(currentState.characterState != EQState.CharacterState.SITTING)
            {
                Keyboard.KeyPress(REST_KEY); await Task.Delay(1000);
            }
            
            if (!(currentState.characterState == EQState.CharacterState.SITTING || currentState.characterState == EQState.CharacterState.POISONED) && currentState.targetHealth > 0.02)
            {
                Console.WriteLine("CS:" + currentState.characterState); return false;
            }

            return false;
        }

        public static async Task<bool> RestUntilFullyHealedTask()
        {
            float hpThreshold = 0.98f;

            EQState currentState = EQState.GetCurrentEQState();
            if (currentState.health >= hpThreshold) { return true; }

            // rest
            Keyboard.KeyPress(REST_KEY); await Task.Delay(1000);
            while (currentState.health < hpThreshold)
            {
                await Task.Delay(2000);
                currentState = EQState.GetCurrentEQState();
                if (!(currentState.characterState == EQState.CharacterState.SITTING || currentState.characterState == EQState.CharacterState.POISONED) &&
                    currentState.targetHealth > 0.02)
                {
                    Console.WriteLine("CS:" + currentState.characterState); return false;
                }
            }

            return true;
        }

        public static async Task<bool> RestTask()
        {
            // rest
            Keyboard.KeyPress(REST_KEY); await Task.Delay(1000);
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
                    Keyboard.KeyPress(FIND_SPECIAL_MOB_KEY); await Task.Delay(500);
                    Keyboard.KeyPress(FIND_SPECIAL_MOB_KEY); await Task.Delay(500);
                    Keyboard.KeyPress(FIND_SPECIAL_MOB_KEY); await Task.Delay(8000);
                    currentState = EQState.GetCurrentEQState();
                }

                await RestUntilFullManaTask();
            }

            return true;
        }

        public static async Task<bool> LevelUpSkillTask()
        {
            Keyboard.KeyPress(FIND_SPECIAL_MOB_KEY); await Task.Delay(3500);

            return true;
        }

        public static async Task<bool> DamageShieldBotTask()
        {
            Keyboard.KeyPress(DAMAGE_SHIELD_KEY); await Task.Delay(500);
            Keyboard.KeyPress(DAMAGE_SHIELD_KEY); await Task.Delay(500);
            Keyboard.KeyPress(DAMAGE_SHIELD_KEY); await Task.Delay(5000);

            return true;
        }

        public static async Task<bool> CampTask()
        {
            Keyboard.KeyPress(GATE_KEY); await Task.Delay(1000);


            return true;
        }

        public static async Task<bool> PullWithSpellTask()
        {
            Keyboard.KeyPress(Keys.OemMinus); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.OemMinus); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.OemMinus); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.OemMinus); await Task.Delay(1000);
            Keyboard.KeyPress(Keys.OemMinus); await Task.Delay(1000);

            await Task.Delay(5000);


            return true;
        }

        public static async Task<bool> PullWithThrowingWeaponTask()
        {
            Keyboard.KeyPress(Keys.OemMinus); await Task.Delay(1000);

            await Task.Delay(2000);
            return true;
        }

        public static async Task<bool> ScoochForwardTask()
        {
            Keyboard.KeyPress(Keys.W, 65); await Task.Delay(500);

            return true;
        }
    }
}
