using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQ_helper
{
    public class EQPlayer
    {
        public enum PlayerState {
            WAITING_TO_FOCUS,
            FOCUSED_ON_EQ_WINDOW,
            // SUMMONING_PET,
            // EQUIPPING_PET,
            // BUFFING_PET,
            // BUFFING_SELF,
            PREPARED_FOR_BATTLE,

            PREPARED_FOR_BATTLE_NIGHT,
            PREPARED_FOR_BATTLE_DAY,

            LEVEL_UP_SKILL,

            PYZJN_FOUND,
            NO_PYZJN_FOUND,

            TARGET_FOUND,
            WAITING_FOR_PET_TO_KILL,
            PULLING_PET_BACK,

            ATTEMPT_TO_LOOT,


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

        async Task<bool> CoreGameplayLoopTask()
        {
            updateStatus("Kicking off core gameplay loop");

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
                        currentPlayerState = PlayerState.PREPARED_FOR_BATTLE;
                        break;
                    // TODO: // SUMMONING_PET,
                    // EQUIPPING_PET,
                    // BUFFING_PET,
                    // BUFFING_SELF,
                    case PlayerState.PREPARED_FOR_BATTLE:
                        updateStatus("Prepared for battle, checking day/night");
                        EQState currentEQState = EQState.GetCurrentEQState();
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
                            PlayerState.PREPARED_FOR_BATTLE,
                            PlayerState.PREPARED_FOR_BATTLE);
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
