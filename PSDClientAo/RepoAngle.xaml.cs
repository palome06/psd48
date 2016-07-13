using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace PSD.ClientAo
{
    /// <summary>
    /// Interaction logic for RepoAngle.xaml
    /// </summary>
    public partial class RepoAngle : UserControl
    {
        public event Util.InputMessageHandler input;

        public RepoAngle()
        {
            InitializeComponent();
            //currentTB = null;
            migiInputBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    string text = migiInputBox.Text;
                    if (text.StartsWith("\\\\"))
                    {
                        string cmd = text.Substring("\\\\".Length).ToUpper().Trim();
                        if (input != null)
                            input(cmd);
                    }
                    else
                    {
                        if (input != null)
                            input("@@" + migiInputBox.Text);
                    }
                    migiInputBox.Text = "";
                }
            };
            migiTextBlock.Document = new FlowDocument();
            migiChatBlock.Document = new FlowDocument();
        }

        public void IncrText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Run r1 = new Run(text);
                Paragraph pr = new Paragraph(new Run(text)
                {
                    Foreground = new SolidColorBrush(Colors.Black),
                    FontFamily = new System.Windows.Media.FontFamily("SimSun"),
                    FontSize = 14
                }) { Margin = new Thickness(0.0) };
                migiTextBlock.Document.Blocks.Add(pr);
                migiTextBlock.ScrollToEnd();
                svText.ScrollToEnd();
            }
        }

        public void DisplayChat(string nick, string hero, string text)
        {
            Paragraph pr = new Paragraph() { Margin = new Thickness(0.0) };
            if (hero != null)
            {
                pr.Inlines.Add(new Run(hero + "(" + nick + "): ")
                {
                    Foreground = new SolidColorBrush(Colors.Aqua),
                    FontSize = 14
                });
            }
            else
            {
                pr.Inlines.Add(new Run(nick + ": ")
                {
                    Foreground = new SolidColorBrush(Colors.Aqua),
                    FontSize = 14
                });
            }
            pr.Inlines.Add(new Run(text)
            {
                Foreground = new SolidColorBrush(Colors.Yellow),
                FontFamily = new System.Windows.Media.FontFamily("SimSun"),
                FontSize = 14
            });
            migiChatBlock.Document.Blocks.Add(pr);
            migiChatBlock.ScrollToEnd();
            svChat.ScrollToEnd();
        }
    }
}
