using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PSD.ClientAo.Card
{
    public class MoveThumb : Thumb
    {
        public MoveThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
            this.PreviewMouseLeftButtonUp += MoveThumb_PreviewMouseLeftButtonUp;
            XUnit = 90; YUnit = 130;
            XTolerance = 2;
            YTolerance = 2;
        }

        public int XUnit { set; get; }
        public int YUnit { set; get; }
        public int XTolerance { set; get; }
        public int YTolerance { set; get; }

        private void MoveThumb_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (inDrag)
            {
                Ruban ruban = this.DataContext as Ruban;
                Canvas rubanship = VisualTreeHelper.GetParent(ruban) as Canvas;
                var top = e.GetPosition(rubanship) - e.GetPosition(ruban);
                double ax = top.X + XUnit / 3, ay = top.Y + YUnit / 3;

                // basu round operation
                inDrag = false;
                ruban.OnMove((int)(ax / XUnit), (int)(ay / YUnit));
                ruban.Opacity = 1;
            }
        }

        private bool inDrag = false;

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Control ruban = this.DataContext as Control;
            Canvas rubanship = VisualTreeHelper.GetParent(ruban) as Canvas;

            if (ruban != null)
            {
                double left = Canvas.GetLeft(ruban);
                double top = Canvas.GetTop(ruban);

                double minLeft = double.IsNaN(left) ? 0 : left;
                double minTop = double.IsNaN(top) ? 0 : top;

                double deltaHorizontal = System.Math.Max(-minLeft, e.HorizontalChange);
                double deltaVertical = System.Math.Max(-minTop, e.VerticalChange);

                if (rubanship != null && !double.IsNaN(rubanship.Width) && !double.IsNaN(rubanship.Height))
                {
                    deltaHorizontal = System.Math.Min(rubanship.Width - left - ruban.Width, deltaHorizontal);
                    deltaVertical = System.Math.Min(rubanship.Height - top - ruban.Height, deltaVertical);
                }

                if (deltaHorizontal >= XTolerance || deltaHorizontal <= -XTolerance)
                {
                    Canvas.SetLeft(ruban, left + deltaHorizontal);
                    inDrag = true;
                }
                if (deltaVertical >= YTolerance || deltaVertical <= -YTolerance)
                {
                    Canvas.SetTop(ruban, top + deltaVertical);
                    inDrag = true;
                }
                if (inDrag)
                {
                    Canvas.SetZIndex(ruban, 50);
                    ruban.Opacity = 0.8;
                }
            }
        }
    }
}
