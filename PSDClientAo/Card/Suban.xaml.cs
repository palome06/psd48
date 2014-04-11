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
    /// Interaction logic for Ruban.xaml
    /// </summary>
    public partial class Suban : ContentControl
    {
        public Image Face { private set; get; }

        public ushort UT { private set; get; }

        public Suban(Image face, ushort ut)
        {
            this.Face = new Image() { Source = face.Source };
            this.UT = ut;
            InitializeComponent();
            cardBody.Content = Face;
        }
        public Suban Clone() { return new Suban(Face, UT); }
    }
}
