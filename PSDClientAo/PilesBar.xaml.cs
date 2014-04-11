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

namespace PSD.ClientAo
{
    /// <summary>
    /// Interaction logic for PilesBar.xaml
    /// </summary>
    public partial class PilesBar : UserControl
    {
        public Base.LibGroup Tuple
        {
            set
            {
                Field = new AoField(this, value)
                    {
                        //TuxCount = 64,
                        //TuxDises = 12,
                        //MonCount = 18,
                        //MonDises = 9,
                        //EveCount = 18,
                        //EveDises = 1,
                        //ScoreAka = 9,
                        //ScoreAo = 12,

                        //PoolAka = 17,
                        //PoolAo = 6,
                        //Monster1 = 17,
                        //Monster2 = 13,
                        //Eve1 = 0
                    };
                mainGrid.DataContext = Field;
            }
        }

        public AoField Field { private set; get; }

        public PilesBar()
        {
            InitializeComponent();
            //Field = new AoField(this, Tuple)
            //{
            //    TuxCount = 64,
            //    TuxDises = 12,
            //    MonCount = 18,
            //    MonDises = 9,
            //    EveCount = 18,
            //    EveDises = 1,
            //    ScoreAka = 9,
            //    ScoreAo = 12,

            //    PoolAka = 17,
            //    PoolAo = 6,
            //    Monster1 = 17,
            //    Monster2 = 13,
            //    Eve1 = 0
            //};
            //mainGrid.DataContext = Field;
        }
    }
}
