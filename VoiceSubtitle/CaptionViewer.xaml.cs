using Microsoft.Practices.ServiceLocation;
using System.Windows.Controls;
using VoiceSubtitle.Helper;
using VoiceSubtitle.ViewModel;

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


        private void PrimarySearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key==System.Windows.Input.Key.Enter)
                ServiceLocator.Current.GetInstance<PlayerViewModel>().SearchPrimaryCaption((sender as TextBox).Text);
        }
    }
}