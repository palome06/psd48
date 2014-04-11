using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PSD.ClientAo.OI
{
    public class MovePanelThumb : Thumb
    {
        public MovePanelThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Control tv = this.DataContext as Control;
            Canvas tvship = VisualTreeHelper.GetParent(tv) as Canvas;

            if (tv != null)
            {
                double left = Canvas.GetLeft(tv);
                double top = Canvas.GetTop(tv);

                double minLeft = double.IsNaN(left) ? 0 : left;
                double minTop = double.IsNaN(top) ? 0 : top;

                double deltaHorizontal = System.Math.Max(-minLeft, e.HorizontalChange);
                double deltaVertical = System.Math.Max(-minTop, e.VerticalChange);

                if (tvship != null && !double.IsNaN(tvship.Width) && !double.IsNaN(tvship.Height))
                {
                    deltaHorizontal = System.Math.Min(tvship.Width - left - tv.Width, deltaHorizontal);
                    deltaVertical = System.Math.Min(tvship.Height - top - tv.Height, deltaVertical);
                }
                Canvas.SetLeft(tv, left + deltaHorizontal);
                Canvas.SetTop(tv, top + deltaVertical);
            }
        }
    }
}

