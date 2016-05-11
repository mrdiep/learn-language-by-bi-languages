using MahApps.Metro.Controls;
using System.Windows;

namespace VoiceSubtitle
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : MetroWindow
    {
        public MessageWindow()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MessageWindow), new PropertyMetadata(string.Empty));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}