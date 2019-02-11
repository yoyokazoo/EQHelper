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

            /*
            Bitmap bmTest = EQScreen.GetEQBitmap();

            Console.WriteLine("BMTest width = " + bmTest.Width + " " + bmTest.Height);
            ScreenCapture.CaptureWindowToFile(EQScreen.GetEQWindowHandle(), "Test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            EQState currentEQState = EQState.GetCurrentEQState();
            */

            String path = @"C:\Users\Peter\Desktop\eq\everquest_rof2\Logs\eqlog_Yoyokazoo_EQ Reborn.txt";
            // https://hooks.slack.com/services/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 // croc-bot incoming
            // 
            /*
            using (FileStream eqLogStream = File.OpenRead(path))
            {

            }
            */

            //[Sun Feb 10 18:16:02 2019] Ghaleon tells you, 'neil is gettin close to my place'
            Regex tellRx = new Regex(@"\[.*\] ([^\s]*) tells you, \'(.*)\'", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.Default))
            {
                while (sr.ReadLine() != null) { }

                while (true)
                {
                    string line = sr.ReadLine();
                    if(line != null)
                    {
                        Match tellMatch = tellRx.Match(line);

                        if(tellMatch.Success)
                        {
                            string name = tellMatch.Groups[1].Value;
                            string message = tellMatch.Groups[2].Value;

                            if(message.Contains("Master."))
                            {

                            }
                            else
                            {
                                Console.WriteLine("You were sent a tell from " + name + " " + "'" + message + "'");

                                var webhookUrl = new Uri("https://hooks.slack.com/services/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");
                                var slackClient = new SlackClient(webhookUrl);
                                var slackMessage = name + " sent you a tell: " + message;
                                slackClient.SendMessageAsync(slackMessage);
                            }
                        }
                        
                    }
                }
            }
        }

        private void labelStatus5_Click(object sender, EventArgs e)
        {

        }
    }
}
