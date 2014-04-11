using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PSD.ClientAo.Card
{
    /// <summary>
    /// Interaction logic for Hitori.xaml
    /// </summary>
    public partial class Nikojin : UserControl
    {
        public Nikojin(Image face, ushort ut)
        {
            this.Face = new Image() { Source = face.Source };
            this.UT = ut;
            InitializeComponent();
            cardCheckBox.Content = Face;
        }

        public Image Face { private set; get; }

        public ushort UT { private set; get; }

        internal void SetActive(bool active)
        {
            if (active)
                cardCheckBox.Template = Resources["touchableCard"] as ControlTemplate;
            else
                cardCheckBox.Template = Resources["soundCard"] as ControlTemplate;
            cardCheckBox.ApplyTemplate();
        }

        internal void SetAsLock(bool locked)
        {
            Rectangle rec = cardCheckBox.Template.FindName("scSelMask", cardCheckBox) as Rectangle;
            rec.Opacity = locked ? 0.3 : 0;
        }
    }
}
