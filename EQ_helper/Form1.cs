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

            CurrentStateTimer = new System.Timers.Timer(300);
            CurrentStateTimer.Elapsed += UpdateCurrentState;
            CurrentStateTimer.AutoReset = true;
            CurrentStateTimer.Enabled = true;
        }

        private void UpdateCurrentState(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (EQState.mostRecentState == null) { return; }

            labelHealthPercentFilled.Text = EQState.mostRecentState.health.ToString(); ;
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
            /*
            EQScreen.SetComputer(false);

            // find window handles
            Process[] ps = Process.GetProcessesByName("eqgame");
            int processNum = 0;
            foreach(Process p in ps)
            {
                Bitmap userBm = ScreenCapture.CaptureWindowBM(p.MainWindowHandle);
                string name = EQScreen.GetNameFromBitmap(userBm);
                Console.WriteLine("Process " + p.MainWindowTitle + " name = " + name);
                ScreenCapture.CaptureWindowToFile(p.MainWindowHandle, "Test" + processNum + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                processNum++;
            }
            */

            //StartBuyHouseLoop();

            //testCullRoguePoints();

            //SlackHelper.SendSlackMessageAsync("Test");

            //player.KickOffSpamZero();
            //DamageShieldBotTask();

            //Mouse.Move(703, 634);
            //Mouse.ButtonDown(Mouse.MouseKeys.Left); Thread.Sleep(50);
            //Mouse.ButtonUp(Mouse.MouseKeys.Left); Thread.Sleep(50);

            //Bitmap bmTest = EQScreen.GetEQBitmap();

            //Console.WriteLine("BMTest width = " + bmTest.Width + " " + bmTest.Height);
            //ScreenCapture.CaptureWindowToFile(Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault().MainWindowHandle, "F14Test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            ScreenCapture.CaptureWindowToFile(EQScreen.GetEQWindowHandle(), "Test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            //EQState currentEQState = EQState.GetCurrentEQState();

        }

        public static async Task<bool> StartBuyHouseLoop()
        {
            await Task.Delay(3000);

            //for(int i = 1; i < 5; i++)
            while(true)
            {
                await BuyHouse();
            }

            return true;
        }

        static Random rng = new Random();
        public static async Task<bool> BuyHouse()
        {
            Mouse.Move(1167, 628); await Task.Delay(50);
            Mouse.ButtonDown(Mouse.MouseKeys.Right); await Task.Delay(50);
            Mouse.ButtonUp(Mouse.MouseKeys.Right); await Task.Delay(50);

            await Task.Delay(500 + rng.Next(100));

            Mouse.Move(1199, 816); await Task.Delay(50);
            Mouse.ButtonDown(Mouse.MouseKeys.Left); await Task.Delay(50);
            Mouse.ButtonUp(Mouse.MouseKeys.Left); await Task.Delay(50);

            await Task.Delay(500 + rng.Next(100));

            Mouse.Move(1166, 808); await Task.Delay(50);
            Mouse.ButtonDown(Mouse.MouseKeys.Left); await Task.Delay(50);
            Mouse.ButtonUp(Mouse.MouseKeys.Left); await Task.Delay(50);

            await Task.Delay(500 + rng.Next(100));

            Mouse.Move(1227, 768); await Task.Delay(50);
            Mouse.ButtonDown(Mouse.MouseKeys.Left); await Task.Delay(50);
            Mouse.ButtonUp(Mouse.MouseKeys.Left); await Task.Delay(50);

            await Task.Delay(1100 + rng.Next(200));

            return true;
        }

        private List<Point> cullRoguePoints(List<Point> unculledPoints, float cullThreshold)
        {
            List<Point> culledPoints = new List<Point>();

            float xTotal = 0.0f, yTotal = 0.0f, xAvg = 0.0f, yAvg = 0.0f;
            foreach(Point p in unculledPoints)
            {
                xTotal += p.X;
                yTotal += p.Y;
            }
            xAvg = xTotal / unculledPoints.Count;
            yAvg = yTotal / unculledPoints.Count;

            foreach (Point p in unculledPoints)
            {
                float xDist = Math.Abs(p.X - xAvg);
                float yDist = Math.Abs(p.Y - yAvg);

                double totalDist = Math.Sqrt((xDist * xDist) + (yDist * yDist));

                Console.WriteLine("Dist between Point and avg = " + totalDist);

                if (totalDist < cullThreshold) { culledPoints.Add(p); }
            }

            return culledPoints;
        }

        private void testCullRoguePoints()
        {
            List<Point> testPoints = new List<Point>();
            Point goodPoint1 = new Point(50, 50);
            Point goodPoint2 = new Point(52, 51);
            Point goodPoint3 = new Point(49, 55);
            Point goodPoint4 = new Point(47, 52);

            Point badPoint1 = new Point(100, 50);

            testPoints.Add(goodPoint1);
            testPoints.Add(goodPoint2);
            testPoints.Add(goodPoint3);
            testPoints.Add(goodPoint4);
            testPoints.Add(badPoint1);

            List<Point> culledTestPoints = cullRoguePoints(testPoints, 100);
            Console.WriteLine(String.Format("Original points: {0} Culled Points: {1}", testPoints.Count, culledTestPoints.Count));
        }

        private void labelStatus5_Click(object sender, EventArgs e)
        {
            
        }
    }
}
