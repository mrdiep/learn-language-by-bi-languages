using System.Windows.Controls;
using VoiceSubtitle.Helper;

namespace VoiceSubtitle
{
    public partial class CaptionViewer : UserControl
    {
        public CaptionViewer()
        {
            InitializeComponent();
        }

        private void ScrollToSelected(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox.SelectedItem != null)
            {
                listBox.UpdateLayout();
                listBox.ScrollToCenterOfView(listBox.SelectedItem);
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            listView.SelectedItem = null;
        }
    }
}