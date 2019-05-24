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
using System.IO;
using System.Text.RegularExpressions;

using WindowsInput;
using InputManager;

namespace EQ_helper
{
    public partial class Form1 : Form
    {
        System.Timers.Timer CurrentStateTimer;

        public bool updateStatus(String status)
        {
            status = EQScreen.currentCharacterName + ": " + status;
            labelStatus5.Text = labelStatus4.Text;
            labelStatus4.Text = labelStatus3.Text;
            labelStatus3.Text = labelStatus2.Text;
            labelStatus2.Text = labelStatus.Text;

            labelStatus.Text = status;
            Console.WriteLine(status);
            return true;
        }

        EQPlayer player;

        public Form1()
        {
            InitializeComponent();
            EQScreen.Initialize();
            player = new EQPlayer(updateStatus);

            CurrentStateTimer = new System.Timers.Timer(300);
            CurrentStateTimer.Elapsed += UpdateCurrentState;
            CurrentStateTimer.AutoReset = true;
            CurrentStateTimer.Enabled = true;
        }

        private void UpdateCurrentState(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (EQState.mostRecentState == null) { return; }

            //labelHealthPercentFilled.Text = EQState.mostRecentState.health.ToString(); ;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            player.KickOffCoreLoop();
        }

        private void buttonFightSingle_Click(object sender, EventArgs e)
        {
            player.KickOffSingleFight();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            //SlackHelper.SendSlackMessageAsync("Test");

            //player.KickOffSpamZero();
            //DamageShieldBotTask();

            //Mouse.Move(703, 634);
            //Mouse.ButtonDown(Mouse.MouseKeys.Left); Thread.Sleep(50);
            //Mouse.ButtonUp(Mouse.MouseKeys.Left); Thread.Sleep(50);

            //ScreenCapture.CaptureWindowToFile(EQScreen.GetEQWindowHandle(), "Test1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //EQScreen.SetNextCharacter();
            //ScreenCapture.CaptureWindowToFile(EQScreen.GetEQWindowHandle(), "Test2.bmp", System.Drawing.Imaging.ImageFormat.Bmp);          

            //Console.WriteLine("BMTest width = " + bmTest.Width + " " + bmTest.Height);
            //ScreenCapture.CaptureWindowToFile(Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault().MainWindowHandle, "F14Test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //ScreenCapture.CaptureWindowToFile(EQScreen.GetEQWindowHandle(), "Test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            //EQState currentEQState = EQState.GetCurrentEQState();
        }

        private void labelStatus5_Click(object sender, EventArgs e)
        {
            
        }
    }
}
