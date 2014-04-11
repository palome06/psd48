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
    /// Interaction logic for JoyStick.xaml
    /// </summary>
    public partial class JoyStick : UserControl
    {
        public Base.LibGroup Tuple
        {
            set
            {
                CEE = new AoCEE(this, value)
                {
                    //Skill1 = "技能甲",
                    //Skill1Valid = true,
                    //Skill2 = "技能乙",
                    //Skill2Valid = true,
                    //Skill3 = "技能丙",
                    //Skill3Valid = false,
                    //ExtSkill1 = "辅助技能甲",
                    //ExtSkill1Valid = true,
                    //ExtSkill2 = ""
                    Skill1 = null,
                    Skill2 = null,
                    Skill3 = null,
                    Skill4 = null,
                    ExtSkill1 = null,
                    ExtSkill2 = null
                };
                this.DataContext = CEE;
            }
        }

        public AoCEE CEE { get; set; }

        public JoyStick()
        {
            InitializeComponent();
        }

        public string DecideMessage { get; set; }

        private void DecideButtonClick(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("input?" + DecideMessage);
            if (input != null)
                input(DecideMessage ?? "");
        }
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("0");
        }
        private void PetButtonClick(object sender, RoutedEventArgs e)
        {
        }

        public event Util.InputMessageHandler input;

        private void CZ01ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("CZ01");
        }
        private void CZ02ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("CZ02");
        }
        private void CZ03ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("CZ03");
        }
        private void CZ05ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("CZ05");
        }

        private void Skill1ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.Skill1.Code);
        }
        private void Skill2ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.Skill2.Code);
        }
        private void Skill3ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.Skill3.Code);
        }
        private void Skill4ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.Skill4.Code);
        }
        private void ExtSkill1ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.ExtSkill1.Code + "(" + CEE.ExtHolder1 + ")");
        }

        private void SkOptChecked(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("@#1");
        }

        private void SkOptUnchecked(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("@#2");
        }

        private void TpOptChecked(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("@#3");
        }

        private void TpOptUnchecked(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("@#4");
        }
        // Force An input of Cancel(0) in case of fatal struck.
        private void SmoothForceClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("0");
        }
    }
}
