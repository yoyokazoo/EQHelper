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
        public bool updateStatus(String status)
        {
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
            player = new EQPlayer(updateStatus);
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
            //var webhookUrl = new Uri("https://hooks.slack.com/services/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");
            //var slackClient = new SlackClient(webhookUrl);
            //var message = "TESTING BEEP BEEP";
            //slackClient.SendMessageAsync(message);

            //player.KickOffSpamZero();
            //DamageShieldBotTask();

            Mouse.Move(703, 634);
            Mouse.ButtonDown(Mouse.MouseKeys.Left); Thread.Sleep(50);
            Mouse.ButtonUp(Mouse.MouseKeys.Left); Thread.Sleep(50);

            Bitmap bmTest = EQScreen.GetEQBitmap();

            Console.WriteLine("BMTest width = " + bmTest.Width + " " + bmTest.Height);
            ScreenCapture.CaptureWindowToFile(EQScreen.GetEQWindowHandle(), "Test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            EQState currentEQState = EQState.GetCurrentEQState();
            
        }

        private void labelStatus5_Click(object sender, EventArgs e)
        {

        }
    }
}
