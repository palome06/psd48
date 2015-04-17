using PSD.ClientAo.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PSD.ClientAo
{
    /// <summary>
    /// Interaction logic for Orchis40.xaml
    /// </summary>
    public partial class Orchis40 : UserControl
    {
        #region Members and Constants
        public Orchis40()
        {
            InitializeComponent();
            onBoards = new List<Ruban>();
            rblives = new Dictionary<Ruban, UCounter>();
            gcThread = new Thread(delegate() { RubanGC(); });
            gcThread.Start();
        }

        public Base.LibGroup Tuple
        {
            set
            {
                Orch = new AoOrchis(this, value);
                DataContext = Orch;
            }
        }

        public AoOrchis Orch { private set; get; }

        public void Close()
        {
            if (gcThread != null && gcThread.IsAlive)
                gcThread.Abort();
        }

        private Thread gcThread;

        public static readonly TimeSpan XURA = TimeSpan.FromSeconds(0.13);
        public static readonly TimeSpan SURA = TimeSpan.FromSeconds(0.3);
        public static readonly TimeSpan DURA = TimeSpan.FromSeconds(0.5);
        public static readonly TimeSpan LSURA = TimeSpan.FromSeconds(1.05);
        public static readonly TimeSpan LDURA = TimeSpan.FromSeconds(4);
        public const double CENTER_X = 290, CENTER_Y = 112; // OLD_CENTER_Y = 162;
        public const double CENTER_X_BIAS = 145;

        public const double ORCHIS_WIDTH = 450;
        public const int ORCHIS_CAP = 9;
        private static readonly List<Ruban> EMPTY_LIST = new List<Ruban>();

        private List<Ruban> onBoards;
        private IDictionary<Ruban, UCounter> rblives;

        #endregion Members and Constants
        #region FlyingGet

        private void FlyingGet(List<Card.Ruban> rubans,
            double x1, double y1, double x2, double y2, bool fade)
        {
            //<Canvas x:Name="cardLock" Canvas.Left="80" Canvas.Top="90"
            // Width="450" Height="125" Background="LightCoral"/>
            Canvas flyingBody = new Canvas() { Width = 450, Height = 125 };
            //flyingBody.Background = new SolidColorBrush(Colors.LightCoral);
            Canvas.SetLeft(flyingBody, x1);
            Canvas.SetTop(flyingBody, y1);
            flyingBody.Visibility = Visibility.Visible;
            // Clear Tux
            int sz = rubans.Count;
            for (int i = 0; i < sz; ++i)
            {
                rubans[i].Index = i;
                Canvas.SetLeft(rubans[i], i * 30);
                flyingBody.Children.Add(rubans[i]);
            }
            lock (aniCanvas.Children)
            {
                aniCanvas.Children.Add(flyingBody);
            }
            DoubleAnimation aniAppr = new DoubleAnimation() { From = 0, To = 1, Duration = SURA };
            Storyboard.SetTarget(aniAppr, flyingBody);
            Storyboard.SetTargetProperty(aniAppr, new PropertyPath(UIElement.OpacityProperty));
            aniAppr.BeginTime = TimeSpan.FromSeconds(0);

            DoubleAnimation aniX = new DoubleAnimation() { From = x1, To = x2, Duration = DURA };
            Storyboard.SetTarget(aniX, flyingBody);
            Storyboard.SetTargetProperty(aniX, new PropertyPath(Canvas.LeftProperty));
            aniX.BeginTime = SURA;

            DoubleAnimation aniY = new DoubleAnimation() { From = y1, To = y2, Duration = DURA };
            Storyboard.SetTarget(aniY, flyingBody);
            Storyboard.SetTargetProperty(aniY, new PropertyPath(Canvas.TopProperty));
            aniY.BeginTime = SURA;

            Storyboard sb = new Storyboard();
            sb.Children.Add(aniAppr);
            sb.Children.Add(aniX);
            sb.Children.Add(aniY);

            if (fade)
            {
                DoubleAnimation aniFade = new DoubleAnimation() { From = 1, To = 0, Duration = SURA };
                Storyboard.SetTarget(aniFade, flyingBody);
                Storyboard.SetTargetProperty(aniFade, new PropertyPath(UIElement.OpacityProperty));
                aniFade.BeginTime = SURA + DURA;
                sb.Children.Add(aniFade);
            }

            sb.Begin();
            new Thread(delegate()
            {
                if (fade)
                    Thread.Sleep(SURA + DURA + SURA);
                else
                    Thread.Sleep(SURA + DURA);
                aniCanvas.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock (aniCanvas.Children)
                    {
                        aniCanvas.Children.Remove(flyingBody);
                    }
                    sb.Remove();
                }));
            }).Start();
        }

        private void FlyingShow(List<Card.Ruban> rubans, double x, double y, bool isLong)
        {
            Canvas flyingBody = new Canvas() { Width = 450, Height = 125 };
            Canvas.SetLeft(flyingBody, x);
            Canvas.SetTop(flyingBody, y);
            flyingBody.Visibility = Visibility.Visible;
            // Clear Tux
            int sz = rubans.Count;
            for (int i = 0; i < sz; ++i)
            {
                rubans[i].Index = i;
                Canvas.SetLeft(rubans[i], i * 30);
                flyingBody.Children.Add(rubans[i]);
            }
            lock (aniCanvas.Children)
            {
                aniCanvas.Children.Add(flyingBody);
            }
            TimeSpan holdDura = isLong ? LDURA : DURA;
            DoubleAnimation aniAppr = new DoubleAnimation() { From = 0, To = 1, Duration = SURA };
            Storyboard.SetTarget(aniAppr, flyingBody);
            Storyboard.SetTargetProperty(aniAppr, new PropertyPath(UIElement.OpacityProperty));
            aniAppr.BeginTime = TimeSpan.FromSeconds(0);

            DoubleAnimation aniFade = new DoubleAnimation() { From = 1, To = 0, Duration = SURA };
            Storyboard.SetTarget(aniFade, flyingBody);
            Storyboard.SetTargetProperty(aniFade, new PropertyPath(UIElement.OpacityProperty));
            aniFade.BeginTime = SURA + holdDura;

            Storyboard sb = new Storyboard();
            sb.Children.Add(aniAppr);
            sb.Children.Add(aniFade);

            sb.Begin();
            new Thread(delegate()
            {
                Thread.Sleep(SURA + holdDura + SURA);
                aniCanvas.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock (aniCanvas.Children)
                    {
                        aniCanvas.Children.Remove(flyingBody);
                    }
                    sb.Remove();
                }));
            }).Start();
        }

        private void FlyingUp(ContentControl uc, double x1, double y1, double x2, double y2)
        {
            //<Canvas x:Name="cardLock" Canvas.Left="80" Canvas.Top="90"
            // Width="450" Height="125" Background="LightCoral"/>
            Canvas flyingBody = new Canvas() { Width = 450, Height = 125 };
            //flyingBody.Background = new SolidColorBrush(Colors.LightCoral);
            Canvas.SetLeft(flyingBody, x1);
            Canvas.SetTop(flyingBody, y1);
            flyingBody.Visibility = Visibility.Visible;
            Canvas.SetLeft(uc, 0);
            flyingBody.Children.Add(uc);
            lock (aniCanvas.Children)
            {
                aniCanvas.Children.Add(flyingBody);
            }
            DoubleAnimation aniAppr = new DoubleAnimation() { From = 0, To = 1, Duration = LSURA };
            Storyboard.SetTarget(aniAppr, flyingBody);
            Storyboard.SetTargetProperty(aniAppr, new PropertyPath(UIElement.OpacityProperty));
            aniAppr.BeginTime = TimeSpan.FromSeconds(0);

            DoubleAnimation aniX = new DoubleAnimation() { From = x1, To = x2, Duration = DURA };
            Storyboard.SetTarget(aniX, flyingBody);
            Storyboard.SetTargetProperty(aniX, new PropertyPath(Canvas.LeftProperty));
            aniX.BeginTime = LSURA;

            DoubleAnimation aniY = new DoubleAnimation() { From = y1, To = y2, Duration = DURA };
            Storyboard.SetTarget(aniY, flyingBody);
            Storyboard.SetTargetProperty(aniY, new PropertyPath(Canvas.TopProperty));
            aniY.BeginTime = LSURA;

            Storyboard sb = new Storyboard();
            sb.Children.Add(aniAppr);
            sb.Children.Add(aniX);
            sb.Children.Add(aniY);

            DoubleAnimation aniFade = new DoubleAnimation() { From = 1, To = 0, Duration = SURA };
            Storyboard.SetTarget(aniFade, flyingBody);
            Storyboard.SetTargetProperty(aniFade, new PropertyPath(UIElement.OpacityProperty));
            aniFade.BeginTime = LSURA + DURA;
            sb.Children.Add(aniFade);

            sb.Begin();
            new Thread(delegate()
            {
                Thread.Sleep(LSURA + DURA + SURA);
                aniCanvas.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock (aniCanvas.Children)
                    {
                        aniCanvas.Children.Remove(flyingBody);
                    }
                    sb.Remove();
                }));
            }).Start();
        }

        private void ParseCord(ushort s, out double x, out double y)
        {
            switch (s)
            {
                //case 0: x = CENTER_X; y = CENTER_Y; break;
                case 1: x = 0; y = 5; break;
                case 2: x = 0; y = 170; break;
                case 3: x = 280; y = -10; break;
                case 4: x = 240; y = 390; break;
                case 5: x = 570; y = 5; break;
                case 6: x = 650; y = 170; break;
                default: x = CENTER_X; y = CENTER_Y; break;
            }
        }
        private void ParseCordSelf(ushort s, out double x, out double y)
        {
            // 15,57
            if (s == 4) { x = -15; y = 333; }
            else
            {
                ParseCord(s, out x, out y);
                x += 48; y -= 41;
            }
        }
        public void ShowFlyingGet(List<Card.Ruban> rubans, ushort sfrom, ushort sto, bool isLong)
        {
            if (sto == 0) // Discard, put card into dices (Orchis)
            {
                double xb, yb;
                ParseCord(sfrom, out xb, out yb);
                InsertCard(rubans, xb, yb);
            }
            else if (sfrom == sto) // Show Self, 3->3, Equip
            {
                double x, y;
                ParseCordSelf(sfrom, out x, out y);
                FlyingShow(rubans, x, y, isLong);
            }
            else // Normal, Obtain cards or card transfer
            {
                double x1, x2, y1, y2;
                ParseCord(sfrom, out x1, out y1);
                ParseCord(sto, out x2, out y2);
                FlyingGet(rubans, x1, y1, x2, y2, true);
            }
        }

        #endregion FlyingGet
        #region OrchisBoard

        private void InsertCard(List<Ruban> comers, double xb, double yb)
        {
            if (comers.Count > ORCHIS_CAP)
            {
                comers.RemoveRange(0, comers.Count - ORCHIS_CAP);
                OrchisAni(onBoards.ToList(), EMPTY_LIST, comers.ToList(), xb, yb);
            }
            else if (onBoards.Count + comers.Count > ORCHIS_CAP)
            {
                int diff = onBoards.Count + comers.Count - ORCHIS_CAP;
                List<Ruban> take = onBoards.GetRange(0, diff).ToList();
                List<Ruban> leave = onBoards.GetRange(diff, onBoards.Count - diff).ToList();
                OrchisAni(take, leave, comers.ToList(), xb, yb);
            }
            else
                OrchisAni(EMPTY_LIST, onBoards.ToList(), comers.ToList(), xb, yb);
        }

        //public void OrchisAni(List<Ruban> removes, List<Ruban> resets, List<Ruban> comers, double xb, double yb)
        //{
        //    Storyboard sb = new Storyboard();
        //    // [0:XURA] -> fade elements in Orchis
        //    foreach (Ruban ruban in removes)
        //    {
        //        DoubleAnimation aniAr = new DoubleAnimation() { From = 1, To = 0, Duration = XURA };
        //        Storyboard.SetTarget(aniAr, ruban);
        //        Storyboard.SetTargetProperty(aniAr, new PropertyPath(UIElement.OpacityProperty));
        //        aniAr.BeginTime = TimeSpan.FromSeconds(0);
        //        sb.Children.Add(aniAr);
        //    }
        //    foreach (Ruban ruban in removes)
        //        onBoards.Remove(ruban);
        //    // [XURA:XURA+SURA] -> move remainders to new position
        //    for (int i = 0; i < resets.Count; ++i)
        //    {
        //        Ruban ruban = resets[i];
        //        double dx = GetRubanX(i, resets.Count + comers.Count);
        //        DoubleAnimation aniRs = new DoubleAnimation() { From = Canvas.GetLeft(ruban), To = dx, Duration = SURA };
        //        Storyboard.SetTarget(aniRs, ruban);
        //        Storyboard.SetTargetProperty(aniRs, new PropertyPath(Canvas.LeftProperty));
        //        aniRs.BeginTime = XURA;
        //        sb.Children.Add(aniRs);
        //    }
        //    sb.Begin();
        //}

        public void OrchisAni(List<Ruban> removes, List<Ruban> resets, List<Ruban> comers, double xb, double yb)
        {
            if (comers.Count > 0)
            {
                lock (centreCanvas.Children)
                {
                    foreach (Ruban ruban in comers)
                    {
                        ruban.Opacity = 0;
                        centreCanvas.Children.Add(ruban);
                        onBoards.Add(ruban);
                        rblives.Add(ruban, new UCounter(0));
                    }
                }
            }
            Storyboard sb = new Storyboard();
            // [0:XURA] -> fade elements in Orchis
            foreach (Ruban ruban in removes)
            {
                DoubleAnimation aniAr = new DoubleAnimation() { From = 1, To = 0, Duration = XURA };
                Storyboard.SetTarget(aniAr, ruban);
                Storyboard.SetTargetProperty(aniAr, new PropertyPath(UIElement.OpacityProperty));
                aniAr.BeginTime = TimeSpan.FromSeconds(0);
                sb.Children.Add(aniAr);
            }
            foreach (Ruban ruban in removes)
                onBoards.Remove(ruban);
            // [XURA:XURA+SURA] -> move remainders to new position
            for (int i = 0; i < resets.Count; ++i)
            {
                Ruban ruban = resets[i];
                double dx = GetRubanX(i, resets.Count + comers.Count);
                DoubleAnimation aniRs = new DoubleAnimation() { From = Canvas.GetLeft(ruban), To = dx, Duration = SURA };
                Storyboard.SetTarget(aniRs, ruban);
                Storyboard.SetTargetProperty(aniRs, new PropertyPath(Canvas.LeftProperty));
                aniRs.BeginTime = XURA;
                sb.Children.Add(aniRs);
            }
            // Comers : Animation of Flying, Immediately
            if (comers.Count > 0)
            {
                List<Ruban> copies = new List<Ruban>();
                foreach (Ruban ruban in comers)
                    copies.Add(ruban.Clone());
                FlyingGet(copies, xb, yb, CENTER_X_BIAS +
                    GetRubanX(resets.Count, resets.Count + comers.Count), CENTER_Y, false);
            }
            // [XURA+SURA:XURA+SURA+SURA] -> Highlight new elements
            for (int i = 0; i < comers.Count; ++i)
            {
                Ruban ruban = comers[i];
                double dx = GetRubanX(i + resets.Count, resets.Count + comers.Count);
                Canvas.SetLeft(ruban, dx);
                // Register ruban item into the Canvas Children
                //ruban.Opacity = 0;
                //centreCanvas.Children.Add(ruban);
                DoubleAnimation ani = new DoubleAnimation() { From = 0, To = 1, Duration = SURA };
                Storyboard.SetTarget(ani, ruban);
                Storyboard.SetTargetProperty(ani, new PropertyPath(UIElement.OpacityProperty));
                ani.BeginTime = SURA + DURA;
                sb.Children.Add(ani);
            }
            //if (removes.Count > 0)
            //{
            //    new Thread(delegate()
            //    {
            //        Thread.Sleep(XURA);
            //        centreCanvas.Dispatcher.BeginInvoke((Action)(() =>
            //        {
            //            foreach (Ruban ruban in removes)
            //            {
            //                //MessageBox.Show("In Removing Ruban " + ruban.UT + ".");
            //                lock (centreCanvas.Children)
            //                {
            //                    centreCanvas.Children.Remove(ruban);
            //                }
            //            }
            //            sb.Remove();
            //        }));
            //    }).Start();
            //}
            sb.Begin();
            if (removes.Count > 0)
            {
                new Thread(delegate()
                {
                    Thread.Sleep(SURA + XURA);
                    centreCanvas.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lock (centreCanvas.Children)
                        {
                            foreach (Ruban ruban in removes)
                                centreCanvas.Children.Remove(ruban);
                        }
                        //sb.Remove();
                    }));
                }).Start();
            }
        }
        private double GetRubanX(int idx, int cap)
        {
            if (cap * PersonalBag.HITORI_SIZE < ORCHIS_WIDTH)
            {
                double gap = (ORCHIS_WIDTH - cap * PersonalBag.HITORI_SIZE) / 2;
                return gap + idx * PersonalBag.HITORI_SIZE;
            }
            else
            {
                double each = (ORCHIS_WIDTH - PersonalBag.HITORI_SIZE) / (cap - 1);
                return idx * each;
            }
        }

        public void RubanGC()
        {
            while (true)
            {
                Thread.Sleep(5000);
                lock (rblives)
                {
                    List<Ruban> toRemove = new List<Ruban>();
                    foreach (var pair in rblives)
                    {
                        if (rblives[pair.Key].Value >= 2)
                            toRemove.Add(pair.Key);
                        else
                            rblives[pair.Key].Incr();
                    }
                    foreach (Ruban ruban in toRemove)
                        rblives.Remove(ruban);
                    if (toRemove.Count > 0)
                    {
                        List<Ruban> remains = onBoards.Except(toRemove).ToList();
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            OrchisAni(toRemove, remains, EMPTY_LIST, 0, 0);
                        }));
                    }
                }
            }
        }

        #endregion OrchisBoard
        #region FlashOn
        //130,118
        public void FlashEve1(Suban suban, ushort from)
        {
            double x, y;
            ParseCord(from, out x, out y);
            FlyingUp(suban, x, y, -41, -110);
        }
        public void FlashMon1(Ruban ruban, ushort from)
        {
            double x, y;
            ParseCord(Orch.AD.Player2Position(from), out x, out y);
            FlyingUp(ruban, x, y, 24, -118);
        }
        public void FlashMon2(Ruban ruban, ushort from)
        {
            double x, y;
            ParseCord(Orch.AD.Player2Position(from), out x, out y);
            FlyingUp(ruban, x, y, 73, -118);
        }

        #endregion FlashOn

        #region Trails
        public static readonly TimeSpan TRIAL_TIME = TimeSpan.FromSeconds(0.36);
        public static readonly TimeSpan TRIAL_KEEP = TimeSpan.FromSeconds(1.2);

        //public const double CENTER_X = 290, CENTER_Y = 112; // OLD_CENTER_Y = 162;
        //public const double CENTER_X_BIAS = 145;

        //public const double ORCHIS_WIDTH = 450;
        //public const int ORCHIS_CAP = 9;
        public void NextTrail(char fromType, ushort from, char toType, ushort to)
        {
            if (fromType == toType && from == to)
                return; // self assignment, do nothing here

            Line line = new Line() { StrokeThickness = 2.5, Stroke = this.Resources["lineStroke"] as RadialGradientBrush };
            double x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            if (fromType == 'T')
                ParseCord(Orch.AD.Player2Position(from), out x1, out y1);
            else if (fromType == 'M')
            {
                if (from == 1) { x1 = 24; y1 = -118; }
                else if (from == 2) { x1 = 73; y1 = -118; }
            }
            else if (fromType == 'P') // TODO: deal with pool
            {
                if (from == 1) { } // AKA
                else if (from == 2) { } // AO
            }
            if (toType == 'T')
                ParseCord(Orch.AD.Player2Position(to), out x2, out y2);
            else if (toType == 'M')
            {
                if (to == 1) { x2 = 24; y2 = -118; }
                else if (to == 2) { x2 = 73; y2 = -118; }
            }
            else if (toType == 'P') // TODO: deal with pool
            {
                if (to == 1) { } // AKA
                else if (to == 2) { } // AO
            }

            line.X1 = line.X2 = x1;
            line.Y1 = line.Y2 = y1;

            Storyboard sb = new Storyboard();
            DoubleAnimation aniG;
            aniG = new DoubleAnimation() { From = x1, To = x2, Duration = TRIAL_TIME };
            Storyboard.SetTarget(aniG, line);
            Storyboard.SetTargetProperty(aniG, new PropertyPath(Line.X2Property));
            sb.Children.Add(aniG);

            aniG = new DoubleAnimation() { From = y1, To = y2, Duration = TRIAL_TIME };
            Storyboard.SetTarget(aniG, line);
            Storyboard.SetTargetProperty(aniG, new PropertyPath(Line.Y2Property));
            sb.Children.Add(aniG);

            aniG = new DoubleAnimation() { From = x1, To = x2, Duration = TRIAL_TIME };
            aniG.BeginTime = TRIAL_TIME + TRIAL_KEEP;
            Storyboard.SetTarget(aniG, line);
            Storyboard.SetTargetProperty(aniG, new PropertyPath(Line.X1Property));
            sb.Children.Add(aniG);

            aniG = new DoubleAnimation() { From = y1, To = y2, Duration = TRIAL_TIME };
            aniG.BeginTime = TRIAL_TIME + TRIAL_KEEP;
            Storyboard.SetTarget(aniG, line);
            Storyboard.SetTargetProperty(aniG, new PropertyPath(Line.Y1Property));
            sb.Children.Add(aniG);

            //var oaukf = new ObjectAnimationUsingKeyFrames();
            //oaukf.KeyFrames.Add(new DiscreteObjectKeyFrame(
            //    Visibility.Collapsed, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.5))));
            //Storyboard.SetTarget(oaukf, line);
            //Storyboard.SetTargetProperty(oaukf, new PropertyPath(Line.VisibilityProperty));
            //sb.Children.Add(oaukf);

            line.Visibility = Visibility.Visible;
            aniCanvas.Children.Add(line);
            sb.Begin();

            new Thread(delegate()
            {
                Thread.Sleep(TRIAL_TIME + TRIAL_KEEP + TRIAL_TIME);
                aniCanvas.Dispatcher.BeginInvoke((Action)(() =>
                {
                    aniCanvas.Children.Remove(line);
                }));
            }).Start();
        }
        #endregion Trails
    }
}
