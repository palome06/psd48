using System;
using System.Windows;
using System.Windows.Input;

namespace PSD.ClientAo.Auxs
{
    /// <summary>
    /// Interaction logic for MessageHouse.xaml
    /// </summary>
    public partial class MessageHouse : Window
    {
        /// <summary>
        /// 禁止在外部实例化
        /// </summary>
        private MessageHouse()
        {
            InitializeComponent();
        }

        public new string Title
        {
            get { return lblTitle.Text; }
            set { lblTitle.Text = value; }
        }

        public string Message
        {
            get { return lblMsg.Text; }
            set { lblMsg.Text = value; }
        }

        private int mSelection = 2;
        public int Selection
        {
            get { return mSelection; }
            set {
                mSelection = value;
                if (mSelection == 1)
                    yesBoarder.Visibility = Visibility.Collapsed;
                else if (mSelection > 1)
                    yesBoarder.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 静态方法 模拟MESSAGEBOX.Show方法
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="msg">消息</param>
        /// <returns></returns>
        [STAThread]
        public static bool? Show(string title, string msg)
        {
            return new MessageHouse()
            {
                Title = title,
                Message = msg,
                Selection = 1
            }.ShowDialog();
        }

        private void Yes_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void No_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}