using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace EQ_helper
{
    public class EQPlayer
    {
        private static long BURNOUT_TIME_MILLIS = (long)(14.5 * 60 * 1000);
        private long lastBurnoutCastTime = 0;

        private static long SKIN_LIKE_ROCK_MILLIS = (long)(25 * 60 * 1000);
        private long lastSkinLikeRockCastTime = 0;

        private static long ARCH_SHIELDING_TIME_MILLIS = (long)(1 * 60 * 60 * 1000);
        private long lastArchShieldingCastTime = 0;

        private static long DMG_SHIELD_TIME_MILLIS = (long)(4 * 60 * 1000);
        private long lastDmgShieldCastTime = 0;

        private static long FARMING_LIMIT_TIME_MILLIS = (long)(8.5 * 60 * 60 * 1000);
        private long lastFarmingLimitTime = 0;

        public enum PlayerState {
            WAITING_TO_FOCUS,
            FOCUSED_ON_EQ_WINDOW,
            // SUMMONING_PET,
            // EQUIPPING_PET,
            // BUFFING_PET,
            // BUFFING_SELF,
            CHECK_COMBAT_STATUS,
            CHECK_PREPAREDNESS,

            GTFO,

            CASTING_BURNOUT_ON_PET,
            CASTING_ARCH_SHIELDING_ON_SELF,

            FINDING_SUITABLE_TARGET,

            PULL_WITH_NUKE,
            PULL_WITH_PET_OUT,
            PULL_WITH_PET_BACK,


            CASTING_DMG_SHIELD_ON_PET,
            WAITING_FOR_MANA,

            PREPARED_FOR_BATTLE,

            PREPARED_FOR_BATTLE_NIGHT,
            PREPARED_FOR_BATTLE_DAY,

            LEVEL_UP_SKILL,

            CHECKING_FOR_PYZJN,

            PYZJN_FOUND,
            NO_PYZJN_FOUND,

            TARGET_FOUND,
            WAITING_FOR_PET_TO_KILL,
            PULLING_PET_BACK,

            KILLING_TARGET_ASAP,

            ATTEMPT_TO_LOOT,
            HIDE_CORPSES,

            MURDERING_EVERYTHING,

            CHECK_FARMING_TIME_LIMIT,

            EXITING_CORE_GAMEPLAY_LOOP,
        }

        Func<String, bool> updateStatus;
        PlayerState desiredBehaviorState = PlayerState.MURDERING_EVERYTHING;

        System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer();

        public EQPlayer(Func<String, bool> updateStatusFunc)
        {
            this.updateStatus = updateStatusFunc;
            soundPlayer.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Sounds\\airhorn.wav";
        }

        public void KickOffCoreLoop()
        {
            //soundPlayer.Play();
            //updateStatus("Inside main loop");

            ListenForTells();
            //RogueLoopTask();
            //DruidLoopTask();
            //ClericLoopTask();
            //PyzjnLoopTask();
            //DmgShieldLoopTask();
            CoreGameplayLoopTask();



            //MultiAccountLoopTask();
        }

        async Task<PlayerState> ChangeStateBasedOnTaskResult(Task<bool> task, PlayerState successState, PlayerState failureState)
        {
            bool taskResult = await task;
            return taskResult ? successState : failureState;
        }

        PlayerState ChangeStateBasedOnBool(bool boolToCheck, PlayerState successState, PlayerState failureState)
        {
            return boolToCheck ? successState : failureState;
        }

        public static bool CurrentTimeInsideDuration(long startTime, long duration)
        {
            return (DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime) < duration;
        }

        async Task<bool> MultiAccountLoopTask()
        {
            updateStatus("Kicking off dual account loop task");

            foreach(String character in EQScreen.GetAllPlayerNames())
            {
                Console.WriteLine("Found character " + character);
            }
            EQScreen.SetNextCharacter();

            DmgShieldLoopTask();
            //DruidLoopTask();
            RogueLoopTask();

            return true;
        }

        static void ListenForTells()
        {
            foreach (String playerName in EQScreen.GetAllPlayerNames())
            {
                if(playerName == "Unknown") { continue; }

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    String path = @"C:\Users\Peter\Desktop\eq\everquest_rof2\Logs\eqlog_" + playerName + "_EQ Reborn.txt";
                // https://hooks.slack.com/services/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 // croc-bot incoming

                //[Sun Feb 10 18:16:02 2019] Ghaleon tells you, 'neil is gettin close to my place'
                Regex tellRx = new Regex(@"\[.*\] ([^\s]*) tells you, \'(.*)\'", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding.Default))
                    {
                        while (sr.ReadLine() != null) { }

                        while (true)
                        {
                            string line = sr.ReadLine();
                            if (line != null)
                            {
                                Match tellMatch = tellRx.Match(line);

                                if (tellMatch.Success)
                                {
                                    string name = tellMatch.Groups[1].Value;
                                    string message = tellMatch.Groups[2].Value;

                                    if (message.Contains("Master."))
                                    {

                                    }
                                    else
                                    {
                                        Console.WriteLine("You were sent a tell from " + name + " " + "'" + message + "'");
                                        var slackMessage = name + " sent you a tell: " + message;
                                        SlackHelper.SendSlackMessageAsync(slackMessage);
                                    }
                                }

                            }
                            Thread.Sleep(50);
                        }
                    }
                }).Start();
            }
        }

        async Task<bool> DmgShieldLoopTask()
        {
            updateStatus("Kicking off core gameplay loop");

            lastFarmingLimitTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            EQState currentEQState = EQState.GetCurrentEQState();
            PlayerState currentPlayerState = PlayerState.WAITING_TO_FOCUS;
            while (currentPlayerState != PlayerState.EXITING_CORE_GAMEPLAY_LOOP)
            {
                if(EQScreen.currentCharacterName != "Yoyokazoo")
                {
                    await Task.Delay(100);
                    continue;
                }
                await EQTask.FocusOnEQWindowTask();
                currentEQState = EQState.GetCurrentEQState();

                // always update EQState here?
                switch (currentPlayerState)
                {
                    case PlayerState.WAITING_TO_FOCUS:
                        updateStatus("Focusing on EQ Window");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FocusOnEQWindowTask(),
                            PlayerState.FOCUSED_ON_EQ_WINDOW,
                            PlayerState.EXITING_CORE_GAMEPLAY_LOOP);
                        break;
                    case PlayerState.FOCUSED_ON_EQ_WINDOW:
                        updateStatus("Focused on EQ Window");
                        await EQTask.HideCorpsesTask();
                        currentPlayerState = PlayerState.CHECK_FARMING_TIME_LIMIT;
                        break;
                    case PlayerState.CHECK_FARMING_TIME_LIMIT:
                        updateStatus("Checking farming time limit");
                        currentPlayerState = PlayerState.WAITING_FOR_MANA;

                        if (!CurrentTimeInsideDuration(lastFarmingLimitTime, FARMING_LIMIT_TIME_MILLIS))
                        {
                            currentPlayerState = PlayerState.EXITING_CORE_GAMEPLAY_LOOP;
                        }
                        break;
                    case PlayerState.WAITING_FOR_MANA:
                        updateStatus("Resting for mana");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.RestUntilFullManaTask(),
                            PlayerState.CHECKING_FOR_PYZJN,
                            PlayerState.WAITING_FOR_MANA);
                        break;
                    case PlayerState.CHECKING_FOR_PYZJN:
                        updateStatus("Checking for Pyzjn");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FindSpecificTarget(),
                            PlayerState.KILLING_TARGET_ASAP,
                            PlayerState.FINDING_SUITABLE_TARGET);
                        break;
                    case PlayerState.KILLING_TARGET_ASAP:
                        updateStatus("Killing target ASAP");
                        SlackHelper.SendSlackMessageAsync("Killing Lockjaw, check for loot");
                        await EQTask.PetAttackTask();
                        await EQTask.NukeTask();
                        await EQTask.EnterCombatTask();
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.NukeUntilDeadTask(),
                            PlayerState.CHECK_FARMING_TIME_LIMIT,
                            PlayerState.CHECK_FARMING_TIME_LIMIT);
                        break;
                    case PlayerState.FINDING_SUITABLE_TARGET:
                        updateStatus("Finding Suitable Target");
						bool foundTargetResult = await EQTask.FindAnyTargetWithMacroTask();
                        currentPlayerState = ChangeStateBasedOnBool(foundTargetResult,
                            PlayerState.CASTING_DMG_SHIELD_ON_PET,
                            PlayerState.CHECK_FARMING_TIME_LIMIT);
                        break;
                    case PlayerState.CASTING_DMG_SHIELD_ON_PET:
                        updateStatus("Casting Damage Shield on target");
                        if (!CurrentTimeInsideDuration(lastDmgShieldCastTime, DMG_SHIELD_TIME_MILLIS))
                        {
                            lastDmgShieldCastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            await EQTask.DamageShieldBotTask();
                        }
                        await Task.Delay(5000);
                        await EQTask.DeselectTargetTask();
                        currentPlayerState = PlayerState.CHECK_FARMING_TIME_LIMIT;
                        break;
                }

                EQScreen.SetNextCharacter();
            }

            updateStatus("Exited Core Gameplay, attempting to camp");
            await EQTask.CampTask();
            EQScreen.SetNextCharacter();
            return true;
        }

        async Task<bool> RogueLoopTask()
        {
            int creaturesPulled = 0;
            int throwingDaggerStack = 0;

            lastFarmingLimitTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            updateStatus("Kicking off rogue loop");
            EQState currentEQState = EQState.GetCurrentEQState();
            PlayerState currentPlayerState = PlayerState.WAITING_TO_FOCUS;

            while (currentPlayerState != PlayerState.EXITING_CORE_GAMEPLAY_LOOP)
            {
                if (EQScreen.currentCharacterName != "Trakklo")
                {
                    await Task.Delay(100);
                    continue;
                }
                await EQTask.FocusOnEQWindowTask();
                currentEQState = EQState.GetCurrentEQState();

                switch (currentPlayerState)
                {
                    case PlayerState.WAITING_TO_FOCUS:
                        updateStatus("Focusing on EQ Window");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FocusOnEQWindowTask(),
                            PlayerState.FOCUSED_ON_EQ_WINDOW,
                            PlayerState.EXITING_CORE_GAMEPLAY_LOOP);
                        break;
                    case PlayerState.FOCUSED_ON_EQ_WINDOW:
                        updateStatus("Focused on EQ Window");
                        await EQTask.HideCorpsesTask();
                        currentPlayerState = PlayerState.CHECK_COMBAT_STATUS;
                        break;
                    case PlayerState.CHECK_FARMING_TIME_LIMIT:
                        updateStatus("Checking farming time limit");
                        currentPlayerState = PlayerState.CHECK_COMBAT_STATUS;
                        if (!CurrentTimeInsideDuration(lastFarmingLimitTime, FARMING_LIMIT_TIME_MILLIS))
                        {
                            currentPlayerState = PlayerState.EXITING_CORE_GAMEPLAY_LOOP;
                        }

                        creaturesPulled++;
                        if(creaturesPulled % 30 == 0)
                        {
                            SlackHelper.SendSlackMessageAsync("Out of daggers, go refill");
                            currentPlayerState = PlayerState.EXITING_CORE_GAMEPLAY_LOOP;
                        }

                        break;
                    case PlayerState.CHECK_COMBAT_STATUS:
                        updateStatus("Checking combat Status");
                        currentPlayerState = ChangeStateBasedOnBool(currentEQState.characterState == EQState.CharacterState.COMBAT,
                            PlayerState.PREPARED_FOR_BATTLE,
                            PlayerState.WAITING_FOR_MANA);
                        break;
                    case PlayerState.PREPARED_FOR_BATTLE:
                        updateStatus("Killing target ASAP");
                        await EQTask.ScoochForwardTask();
                        await EQTask.PullWithThrowingWeaponTask();
                        await EQTask.EnterCombatTask();
                        await EQTask.EnterCombatTask();
                        await EQTask.EnterCombatTask();
                        currentPlayerState = PlayerState.KILLING_TARGET_ASAP;
                        break;
                    case PlayerState.KILLING_TARGET_ASAP:
                        updateStatus("Killing target ASAP");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.LevelSkillUntilDeadTask(),
                            PlayerState.ATTEMPT_TO_LOOT,
                            PlayerState.KILLING_TARGET_ASAP);
                        break;
                    case PlayerState.WAITING_FOR_MANA:
                        updateStatus("Resting until fuly healed");
                        await EQTask.RestUntilFullyHealedTask();
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.RestUntilFullyHealedTask(),
                            PlayerState.FINDING_SUITABLE_TARGET,
                            PlayerState.WAITING_FOR_MANA);
                        break;
                        if(currentEQState.characterState == EQState.CharacterState.COMBAT) {
                            SlackHelper.SendSlackMessageAsync("Attacked while resting, something's probably wrong.");
                            currentPlayerState = PlayerState.PREPARED_FOR_BATTLE;
                        }
                    case PlayerState.FINDING_SUITABLE_TARGET:
                        updateStatus("Finding Suitable Target");
                        //bool foundTargetResult = await EQTask.FindNearestTargetTask(false);
                        bool foundTargetResult = await EQTask.FindAnyTargetWithMacroTask();
                        currentPlayerState = ChangeStateBasedOnBool(foundTargetResult,
                            PlayerState.PREPARED_FOR_BATTLE,
                            PlayerState.CHECK_COMBAT_STATUS);
                        break;
                    case PlayerState.ATTEMPT_TO_LOOT:
                        updateStatus("Attempting to loot");
                        //await EQTask.ScoochForwardTask();
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.LootTask(false),
                            PlayerState.HIDE_CORPSES,
                            PlayerState.HIDE_CORPSES);
                        break;
                    case PlayerState.HIDE_CORPSES:
                        updateStatus("Hiding Corpses");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.HideCorpsesTask(),
                            PlayerState.CHECK_FARMING_TIME_LIMIT,
                            PlayerState.CHECK_FARMING_TIME_LIMIT);
                        break;
                }

                EQScreen.SetNextCharacter();
            }

            updateStatus("Exited Core Gameplay, attempting to camp");
            await EQTask.CampTask();
            EQScreen.SetNextCharacter();
            return true;
        }

        async Task<bool> DruidLoopTask()
        {
            updateStatus("Kicking off druid loop");
            EQState currentEQState = EQState.GetCurrentEQState();
            PlayerState currentPlayerState = PlayerState.WAITING_TO_FOCUS;

            while (currentPlayerState != PlayerState.EXITING_CORE_GAMEPLAY_LOOP)
            {
                if (EQScreen.currentCharacterName != "Durdle")
                {
                    await Task.Delay(100);
                    continue;
                }
                await EQTask.FocusOnEQWindowTask();
                currentEQState = EQState.GetCurrentEQState();
                
                // always update EQState here?
                switch (currentPlayerState)
                {
                    case PlayerState.WAITING_TO_FOCUS:
                        updateStatus("Focusing on EQ Window");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FocusOnEQWindowTask(),
                            PlayerState.FOCUSED_ON_EQ_WINDOW,
                            PlayerState.EXITING_CORE_GAMEPLAY_LOOP);
                        break;
                    case PlayerState.FOCUSED_ON_EQ_WINDOW:
                        updateStatus("Focused on EQ Window");
                        await EQTask.HideCorpsesTask();
                        currentPlayerState = PlayerState.CHECK_COMBAT_STATUS;
                        break;
                    case PlayerState.CHECK_FARMING_TIME_LIMIT:
                        updateStatus("Checking farming time limit");
                        if (!CurrentTimeInsideDuration(lastFarmingLimitTime, FARMING_LIMIT_TIME_MILLIS))
                        {
                            lastFarmingLimitTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        }
                        currentPlayerState = PlayerState.CHECK_COMBAT_STATUS;
                        break;
                    case PlayerState.CHECK_COMBAT_STATUS:
                        updateStatus("Checking combat status");
                        currentPlayerState = ChangeStateBasedOnBool(currentEQState.characterState == EQState.CharacterState.COMBAT,
                            PlayerState.PULL_WITH_NUKE,
                            PlayerState.WAITING_FOR_MANA);
                        break;
                    case PlayerState.PULL_WITH_NUKE:
                        updateStatus("Pulling with Nuke");
                        await EQTask.PullWithSpellTask();
                        await EQTask.EnterCombatTask();

                        currentPlayerState = PlayerState.KILLING_TARGET_ASAP;
                        break;
                    case PlayerState.KILLING_TARGET_ASAP:
                        updateStatus("Killing target ASAP");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.NukeTask(),
                            PlayerState.ATTEMPT_TO_LOOT,
                            PlayerState.KILLING_TARGET_ASAP);
                        break;
                    case PlayerState.WAITING_FOR_MANA:
                        updateStatus("Resting for mana");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.RestUntilFullManaTask(),
                            PlayerState.CASTING_BURNOUT_ON_PET,
                            PlayerState.WAITING_FOR_MANA);
                        break;
                    case PlayerState.CASTING_BURNOUT_ON_PET:
                        updateStatus("Casting skin like rock");
                        if (!CurrentTimeInsideDuration(lastSkinLikeRockCastTime, SKIN_LIKE_ROCK_MILLIS))
                        {
                            lastSkinLikeRockCastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            await EQTask.ApplyPetBuffTask();
                            await EQTask.RestTask();
                        }
                        currentPlayerState = PlayerState.FINDING_SUITABLE_TARGET;
                        break;
                    case PlayerState.FINDING_SUITABLE_TARGET:
                        updateStatus("Finding Suitable Target");
                        //bool foundTargetResult = await EQTask.FindNearestTargetTask(true);
                        bool foundTargetResult = await EQTask.FindAnyTargetWithMacroTask();
                        currentPlayerState = ChangeStateBasedOnBool(foundTargetResult,
                            PlayerState.PULL_WITH_NUKE,
                            PlayerState.CHECK_COMBAT_STATUS);
                        break;
                    case PlayerState.ATTEMPT_TO_LOOT:
                        updateStatus("Attempting to loot");
                        await EQTask.ScoochForwardTask();
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.LootTask(true),
                            PlayerState.HIDE_CORPSES,
                            PlayerState.HIDE_CORPSES);
                        break;
                    case PlayerState.HIDE_CORPSES:
                        updateStatus("Hiding Corpses");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.HideCorpsesTask(),
                            PlayerState.CHECK_FARMING_TIME_LIMIT,
                            PlayerState.CHECK_FARMING_TIME_LIMIT);
                        break;
                }

                EQScreen.SetNextCharacter();
            }

            updateStatus("Exited Core Gameplay, attempting to camp");
            await EQTask.CampTask();
            EQScreen.SetNextCharacter();
            return true;
        }

        async Task<bool> ClericLoopTask()
        {
            updateStatus("Kicking off cleric loop");
            EQState currentEQState = EQState.GetCurrentEQState();
            PlayerState currentPlayerState = PlayerState.WAITING_TO_FOCUS;

            while (currentPlayerState != PlayerState.EXITING_CORE_GAMEPLAY_LOOP)
            {
                // always update EQState here?
                switch (currentPlayerState)
                {
                    case PlayerState.WAITING_TO_FOCUS:
                        updateStatus("Focusing on EQ Window");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FocusOnEQWindowTask(),
                            PlayerState.FOCUSED_ON_EQ_WINDOW,
                            PlayerState.EXITING_CORE_GAMEPLAY_LOOP);
                        break;
                    case PlayerState.FOCUSED_ON_EQ_WINDOW:
                        updateStatus("Focused on EQ Window");
                        currentPlayerState = PlayerState.CHECK_COMBAT_STATUS;
                        break;
                    case PlayerState.CHECK_COMBAT_STATUS:
                        updateStatus("Focused on EQ Window");
                        currentPlayerState = ChangeStateBasedOnBool(currentEQState.characterState == EQState.CharacterState.COMBAT,
                            PlayerState.KILLING_TARGET_ASAP,
                            PlayerState.WAITING_FOR_MANA);
                        break;
                    case PlayerState.KILLING_TARGET_ASAP:
                        updateStatus("Killing target ASAP");
                        await EQTask.NukeTask();
                        await EQTask.EnterCombatTask();
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.NukeUntilDeadTask(),
                            PlayerState.ATTEMPT_TO_LOOT,
                            PlayerState.ATTEMPT_TO_LOOT);
                        break;
                    case PlayerState.WAITING_FOR_MANA:
                        updateStatus("Resting for mana");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.RestUntilFullManaTask(),
                            PlayerState.FINDING_SUITABLE_TARGET,
                            PlayerState.WAITING_FOR_MANA);
                        break;
                    case PlayerState.FINDING_SUITABLE_TARGET:
                        updateStatus("Finding Suitable Target");
                        bool foundTargetResult = await EQTask.FindNearestTargetTask();
                        //bool foundTargetResult = await EQTask.FindAnyTargetWithMacroTask();
                        currentPlayerState = ChangeStateBasedOnBool(foundTargetResult,
                            PlayerState.KILLING_TARGET_ASAP,
                            PlayerState.CHECK_COMBAT_STATUS);
                        break;
                    case PlayerState.ATTEMPT_TO_LOOT:
                        updateStatus("Attempting to loot");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.LootTask(true),
                            PlayerState.HIDE_CORPSES,
                            PlayerState.HIDE_CORPSES);
                        break;
                    case PlayerState.HIDE_CORPSES:
                        updateStatus("Hiding Corpses");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.HideCorpsesTask(),
                            PlayerState.CHECK_COMBAT_STATUS,
                            PlayerState.CHECK_COMBAT_STATUS);
                        break;
                }
            }

            updateStatus("Exited Core Gameplay ");
            return true;
        }

        async Task<bool> PyzjnLoopTask()
        {
            updateStatus("Kicking off cleric loop");
            EQState currentEQState = EQState.GetCurrentEQState();
            PlayerState currentPlayerState = PlayerState.WAITING_TO_FOCUS;

            while (currentPlayerState != PlayerState.EXITING_CORE_GAMEPLAY_LOOP)
            {
                // always update EQState here?
                switch (currentPlayerState)
                {
                    case PlayerState.WAITING_TO_FOCUS:
                        updateStatus("Focusing on EQ Window");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FocusOnEQWindowTask(),
                            PlayerState.FOCUSED_ON_EQ_WINDOW,
                            PlayerState.EXITING_CORE_GAMEPLAY_LOOP);
                        break;
                    case PlayerState.FOCUSED_ON_EQ_WINDOW:
                        updateStatus("Focused on EQ Window");
                        currentPlayerState = PlayerState.CHECK_COMBAT_STATUS;
                        break;
                    case PlayerState.CHECK_COMBAT_STATUS:
                        updateStatus("Focused on EQ Window");
                        currentPlayerState = ChangeStateBasedOnBool(currentEQState.characterState == EQState.CharacterState.COMBAT,
                            PlayerState.KILLING_TARGET_ASAP,
                            PlayerState.CASTING_BURNOUT_ON_PET);
                        break;
                    case PlayerState.KILLING_TARGET_ASAP:
                        updateStatus("Killing target ASAP");
                        await EQTask.PetAttackTask();
                        await EQTask.EnterCombatTask();
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.NukeUntilDeadTask(),
                            PlayerState.ATTEMPT_TO_LOOT,
                            PlayerState.ATTEMPT_TO_LOOT);
                        break;
                    case PlayerState.CASTING_BURNOUT_ON_PET:
                        updateStatus("Casting burnout on pet and setting timer");
                        if (!CurrentTimeInsideDuration(lastBurnoutCastTime, BURNOUT_TIME_MILLIS))
                        {
                            lastBurnoutCastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            await EQTask.ApplyPetBuffTask();
                            await EQTask.RestTask();
                        }
                        currentPlayerState = PlayerState.PREPARED_FOR_BATTLE;
                        break;
                    case PlayerState.PREPARED_FOR_BATTLE:
                        updateStatus("Prepared for battle, checking day/night");
                        currentPlayerState = ChangeStateBasedOnBool(currentEQState.minutesSinceMidnight < 21 || currentEQState.minutesSinceMidnight > 57,
                            PlayerState.PREPARED_FOR_BATTLE_DAY,
                            PlayerState.PREPARED_FOR_BATTLE_DAY); // because I target with macro now, just kill all the skellies
                        break;
                    case PlayerState.PREPARED_FOR_BATTLE_DAY:
                        updateStatus("DAY looking for Pyzjn");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FindSpecificTarget(),
                            PlayerState.PYZJN_FOUND,
                            PlayerState.NO_PYZJN_FOUND);
                        break;
                    case PlayerState.PYZJN_FOUND:
                        updateStatus("PYZJN_FOUND");
                        int timesToAlert = 100;
                        while (timesToAlert > 0)
                        {
                            //soundPlayer.Play();
                            if (timesToAlert % 50 == 0)
                            {
                                SlackHelper.SendSlackMessageAsync("Pyzjn Found!");
                            }
                            timesToAlert--;
                            await Task.Delay(2000);
                        }
                        break;
                    case PlayerState.NO_PYZJN_FOUND:
                        updateStatus("No pyzjn found, finding target");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FindAnyTargetWithMacroTask(),
                            PlayerState.TARGET_FOUND,
                            PlayerState.CHECK_COMBAT_STATUS);
                        break;
                    case PlayerState.TARGET_FOUND:
                        updateStatus("Target found, sending pet");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.PetAttackTask(),
                            PlayerState.WAITING_FOR_PET_TO_KILL,
                            PlayerState.CHECK_COMBAT_STATUS);
                        break;
                    case PlayerState.WAITING_FOR_PET_TO_KILL:
                        updateStatus("Waiting for pet to kill");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.WaitUntilDeadTask(),
                            PlayerState.ATTEMPT_TO_LOOT,
                            PlayerState.ATTEMPT_TO_LOOT);
                        break;
                    case PlayerState.ATTEMPT_TO_LOOT:
                        updateStatus("Attempting to loot");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.LootTask(true),
                            PlayerState.HIDE_CORPSES,
                            PlayerState.HIDE_CORPSES);
                        break;
                    case PlayerState.HIDE_CORPSES:
                        updateStatus("Hiding Corpses");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.HideCorpsesTask(),
                            PlayerState.CHECK_COMBAT_STATUS,
                            PlayerState.CHECK_COMBAT_STATUS);
                        break;
                }
            }

            updateStatus("Exited Core Gameplay ");
            return true;
        }

        async Task<bool> CoreGameplayLoopTask()
        {
            lastFarmingLimitTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            updateStatus("Kicking off core gameplay loop");
            EQState currentEQState = EQState.GetCurrentEQState();
            PlayerState currentPlayerState = PlayerState.WAITING_TO_FOCUS;
            EQScreen.currentCharacterName = "Yoyokazoo";

            while (currentPlayerState != PlayerState.EXITING_CORE_GAMEPLAY_LOOP)
            {
                // always update EQState here?
                switch (currentPlayerState)
                {
                    case PlayerState.WAITING_TO_FOCUS:
                        updateStatus("Focusing on EQ Window");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FocusOnEQWindowTask(), 
                            PlayerState.FOCUSED_ON_EQ_WINDOW,
                            PlayerState.EXITING_CORE_GAMEPLAY_LOOP);
                        break;
                    case PlayerState.FOCUSED_ON_EQ_WINDOW:
                        updateStatus("Focused on EQ Window");
                        currentPlayerState = PlayerState.CHECK_COMBAT_STATUS;
                        break;
                    // TODO: // SUMMONING_PET,
                    // EQUIPPING_PET,
                    case PlayerState.CHECK_COMBAT_STATUS:
                        updateStatus("Focused on EQ Window");
                        currentPlayerState = ChangeStateBasedOnBool(currentEQState.characterState == EQState.CharacterState.COMBAT,
                            PlayerState.KILLING_TARGET_ASAP,
                            PlayerState.CHECK_FARMING_TIME_LIMIT);
                        break;
                    case PlayerState.KILLING_TARGET_ASAP:
                        updateStatus("Killing target ASAP");
                        //await EQTask.PetAttackTask();
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.NukeUntilDeadTask(),
                            PlayerState.ATTEMPT_TO_LOOT,
                            PlayerState.ATTEMPT_TO_LOOT);
                        break;
                    // TODO: // SUMMONING_PET,
                    // EQUIPPING_PET,
                    case PlayerState.CHECK_FARMING_TIME_LIMIT:
                        updateStatus("Checking farming time limit");
                        currentPlayerState = PlayerState.CHECK_PREPAREDNESS;

                        if (!CurrentTimeInsideDuration(lastFarmingLimitTime, FARMING_LIMIT_TIME_MILLIS))
                        {
                            currentPlayerState = PlayerState.EXITING_CORE_GAMEPLAY_LOOP;
                        }
                        break;
                    case PlayerState.CHECK_PREPAREDNESS:
                        updateStatus("Checking preparedness? nothing atm");

                        await EQTask.DeselectTargetTask();
                        currentPlayerState = ChangeStateBasedOnBool(currentEQState.petHealth > 0.05,
                            PlayerState.CASTING_BURNOUT_ON_PET,
                            PlayerState.CASTING_BURNOUT_ON_PET);
                        break;

                        currentPlayerState = PlayerState.CASTING_BURNOUT_ON_PET;
                        // set timer
                        /*
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.ApplyPetBuffTask(),
                            PlayerState.TARGET_FOUND,
                            PlayerState.PREPARED_FOR_BATTLE)
                            )
                            */
                        break;
                    case PlayerState.GTFO:
                        updateStatus("PET DEAD GTFO");

                        await EQTask.GTFOTask();

                        currentPlayerState =  PlayerState.EXITING_CORE_GAMEPLAY_LOOP;
                        // set timer
                        /*
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.ApplyPetBuffTask(),
                            PlayerState.TARGET_FOUND,
                            PlayerState.PREPARED_FOR_BATTLE)
                            )
                            */
                        break;
                    case PlayerState.CASTING_BURNOUT_ON_PET:
                        updateStatus("Casting burnout on pet and setting timer2");
                        if(!CurrentTimeInsideDuration(lastBurnoutCastTime, BURNOUT_TIME_MILLIS))
                        {
                            lastBurnoutCastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            await EQTask.ApplyPetBuffTask();
                        }
                        currentPlayerState = PlayerState.CASTING_ARCH_SHIELDING_ON_SELF;
                        break;
                    case PlayerState.CASTING_ARCH_SHIELDING_ON_SELF:
                        updateStatus("Casting arch shielding on self and setting timer");
                        if (!CurrentTimeInsideDuration(lastArchShieldingCastTime, ARCH_SHIELDING_TIME_MILLIS))
                        {
                            lastArchShieldingCastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            await EQTask.ApplySelfBuffTask();
                        }
                        currentPlayerState = PlayerState.WAITING_FOR_MANA;
                        break;
                    case PlayerState.WAITING_FOR_MANA:
                        updateStatus("Resting for mana");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.RestUntilFullManaTask(),
                            PlayerState.FINDING_SUITABLE_TARGET,
                            PlayerState.WAITING_FOR_MANA);
                        // set timer
                        /*
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.ApplyPetBuffTask(),
                            PlayerState.TARGET_FOUND,
                            PlayerState.PREPARED_FOR_BATTLE)
                            )
                            */
                        break;
                    case PlayerState.PREPARED_FOR_BATTLE_NIGHT:
                        updateStatus("NIGHT looking for Pyzjn");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FindSpecificTarget(),
                            PlayerState.PYZJN_FOUND,
                            PlayerState.FINDING_SUITABLE_TARGET);
                        break;
                    case PlayerState.FINDING_SUITABLE_TARGET:
                        updateStatus("Finding Suitable Target");
                        bool foundTargetResult = await EQTask.FindNearestTargetTask(false, MonsterCon.GREY);
                        //bool foundTargetResult = await EQTask.FindAnyTargetWithMacroTask();
                        currentPlayerState = ChangeStateBasedOnBool(foundTargetResult,
                            PlayerState.CASTING_DMG_SHIELD_ON_PET,
                            PlayerState.CHECK_COMBAT_STATUS);
                        // set timer
                        /*
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.ApplyPetBuffTask(),
                            PlayerState.TARGET_FOUND,
                            PlayerState.PREPARED_FOR_BATTLE)
                            )
                            */
                        break;
                    case PlayerState.CASTING_DMG_SHIELD_ON_PET:
                        updateStatus("Casting Damage Shield on Pet");
                        if(!CurrentTimeInsideDuration(lastDmgShieldCastTime, DMG_SHIELD_TIME_MILLIS))
                        {
                            lastDmgShieldCastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            await EQTask.ApplyDamageShieldTask(true);
                        }
                        await Task.Delay(500);
                        await EQTask.FindNearestTargetTask(true);
                        currentPlayerState = PlayerState.PULL_WITH_NUKE;
                        // set timer
                        /*
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.ApplyPetBuffTask(),
                            PlayerState.TARGET_FOUND,
                            PlayerState.PREPARED_FOR_BATTLE)
                            )
                            */
                        break;
                    case PlayerState.PULL_WITH_NUKE:
                        updateStatus("NUKE PULLING");
                        await EQTask.NukeTask(); await Task.Delay(500);
                        await EQTask.EnterCombatTask();
                        await EQTask.NukeTask(); await Task.Delay(500);
                        currentPlayerState = PlayerState.KILLING_TARGET_ASAP;
                        break;
                    case PlayerState.PULL_WITH_PET_OUT:
                        updateStatus("PULLING OUT WITH PET");
                        await EQTask.PetAttackTask();
                        currentPlayerState = PlayerState.KILLING_TARGET_ASAP;
                        break;
                    case PlayerState.PULL_WITH_PET_BACK:
                        updateStatus("PULLING BACK WITH PET");
                        await EQTask.PetBackTask();
                        currentPlayerState = PlayerState.KILLING_TARGET_ASAP;
                        break;
                    case PlayerState.PREPARED_FOR_BATTLE:
                        updateStatus("Prepared for battle, checking day/night");
                        currentPlayerState = ChangeStateBasedOnBool(currentEQState.minutesSinceMidnight < 21 || currentEQState.minutesSinceMidnight > 57,
                            PlayerState.PREPARED_FOR_BATTLE_NIGHT,
                            PlayerState.PREPARED_FOR_BATTLE_DAY);
                        break;
                    
                    case PlayerState.PREPARED_FOR_BATTLE_DAY:
                        updateStatus("DAY looking for Pyzjn");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FindSpecificTarget(),
                            PlayerState.PYZJN_FOUND,
                            PlayerState.LEVEL_UP_SKILL);
                        break;
                    case PlayerState.LEVEL_UP_SKILL:
                        updateStatus("Leveling up skill");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.LevelUpSkillTask(),
                            PlayerState.PREPARED_FOR_BATTLE,
                            PlayerState.PREPARED_FOR_BATTLE);
                        break;
                    case PlayerState.PYZJN_FOUND:
                        updateStatus("PYZJN_FOUND");
                        int timesToAlert = 100;
                        while (timesToAlert > 0)
                        {
                            //soundPlayer.Play();
                            if(timesToAlert % 50 == 0)
                            {
                                SlackHelper.SendSlackMessageAsync("Pyzjn found!");
                            }
                            timesToAlert--;
                            await Task.Delay(2000);
                        }
                        currentPlayerState = PlayerState.EXITING_CORE_GAMEPLAY_LOOP;
                        await EQTask.CampTask();
                        break;
                    case PlayerState.NO_PYZJN_FOUND:
                        updateStatus("No pyzjn found, finding target");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FindAnyTargetWithMacroTask(),
                            PlayerState.TARGET_FOUND,
                            PlayerState.PREPARED_FOR_BATTLE);
                        break;
                    case PlayerState.TARGET_FOUND:
                        updateStatus("Target found, sending pet");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.PetAttackTask(),
                            PlayerState.WAITING_FOR_PET_TO_KILL,
                            PlayerState.PREPARED_FOR_BATTLE);
                        break;
                    case PlayerState.WAITING_FOR_PET_TO_KILL:
                        updateStatus("Waiting for pet to kill");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.WaitUntilDeadTask(),
                            PlayerState.PREPARED_FOR_BATTLE,
                            PlayerState.PREPARED_FOR_BATTLE);
                        break;
                    case PlayerState.ATTEMPT_TO_LOOT:
                        updateStatus("Attempting to loot");
                        Random r = new Random();
                        if (r.Next() % 2 == 0)
                        {
                            await EQTask.ScoochForwardTask();
                        }
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.LootTask(false),
                            PlayerState.HIDE_CORPSES,
                            PlayerState.HIDE_CORPSES);
                        break;
                    case PlayerState.HIDE_CORPSES:
                        updateStatus("Hiding Corpses");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.HideCorpsesTask(),
                            PlayerState.CHECK_COMBAT_STATUS,
                            PlayerState.CHECK_COMBAT_STATUS);
                        break;
                }
            }

            updateStatus("Exited Core Gameplay ");
            await EQTask.CampTask();
            return true;
        }

        public void KickOffSingleFight()
        {
            Task.Run(() => EQTask.CoreGameplayLoopTask());
        }

        public void KickOffSpamZero()
        {
            Task.Run(() => EQTask.CoreGameplayLoopTask());
        }
    }
}
