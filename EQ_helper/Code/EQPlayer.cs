using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQ_helper
{
    public class EQPlayer
    {
        private static long BURNOUT_TIME_MILLIS = (long)(14.5 * 60 * 1000); // - 30 secs
        private long lastBurnoutCastTime = 0;

        private static long DMG_SHIELD_TIME_MILLIS = (long)(14.5 * 60 * 1000); // - 30 secs
        private long lastDmgShieldCastTime = 0;

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

            PYZJN_FOUND,
            NO_PYZJN_FOUND,

            TARGET_FOUND,
            WAITING_FOR_PET_TO_KILL,
            PULLING_PET_BACK,

            KILLING_TARGET_ASAP,

            ATTEMPT_TO_LOOT,
            HIDE_CORPSES,

            MURDERING_EVERYTHING,

            EXITING_CORE_GAMEPLAY_LOOP,
        }

        Func<String, bool> updateStatus;
        PlayerState currentPlayerState;
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
            updateStatus("Inside main loop");
            currentPlayerState = PlayerState.WAITING_TO_FOCUS;

            CoreGameplayLoopTask();
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

        async Task<bool> CoreGameplayLoopTask()
        {
            updateStatus("Kicking off core gameplay loop");
            EQState currentEQState = EQState.GetCurrentEQState();

            while(currentPlayerState != PlayerState.EXITING_CORE_GAMEPLAY_LOOP)
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
                            PlayerState.CHECK_PREPAREDNESS);
                        break;
                    case PlayerState.KILLING_TARGET_ASAP:
                        updateStatus("Killing target ASAP");
                        await EQTask.PetAttackTask();
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.NukeUntilDeadTask(),
                            PlayerState.ATTEMPT_TO_LOOT,
                            PlayerState.ATTEMPT_TO_LOOT);
                        break;
                    // TODO: // SUMMONING_PET,
                    // EQUIPPING_PET,
                    case PlayerState.CHECK_PREPAREDNESS:
                        updateStatus("Checking preparedness? nothing atm");

                        await EQTask.DeselectTargetTask();
                        currentPlayerState = ChangeStateBasedOnBool(currentEQState.petHealth > 0.05,
                            PlayerState.CASTING_BURNOUT_ON_PET,
                            PlayerState.GTFO);
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
                        currentPlayerState = PlayerState.WAITING_FOR_MANA;
                        break;
                    case PlayerState.WAITING_FOR_MANA:
                        updateStatus("Resting for mana");
                        await EQTask.RestUntilFullManaTask();
                        currentPlayerState = PlayerState.FINDING_SUITABLE_TARGET;
                        // set timer
                        /*
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.ApplyPetBuffTask(),
                            PlayerState.TARGET_FOUND,
                            PlayerState.PREPARED_FOR_BATTLE)
                            )
                            */
                        break;
                    case PlayerState.FINDING_SUITABLE_TARGET:
                        updateStatus("Finding Suitable Target");
                        bool foundTargetResult = await EQTask.FindNearestTargetTask();
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
                            await EQTask.DamageShieldBotTask();
                        }
                        await Task.Delay(500);
                        await EQTask.FindNearestTargetTask();
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
                        currentPlayerState = PlayerState.PULL_WITH_PET_OUT;
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
                    case PlayerState.PREPARED_FOR_BATTLE_NIGHT:
                        updateStatus("NIGHT looking for Pyzjn");
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.FindSpecificTarget(),
                            PlayerState.PYZJN_FOUND,
                            PlayerState.NO_PYZJN_FOUND);
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
                            soundPlayer.Play();
                            if(timesToAlert % 50 == 0)
                            {
                                var webhookUrl = new Uri("https://hooks.slack.com/services/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");
                                var slackClient = new SlackClient(webhookUrl);
                                var message = "PYZJN_FOUND";
                                slackClient.SendMessageAsync(message);
                            }
                            timesToAlert--;
                            await Task.Delay(2000);
                        }
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
                        currentPlayerState = await ChangeStateBasedOnTaskResult(EQTask.LootCorpseTask(),
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