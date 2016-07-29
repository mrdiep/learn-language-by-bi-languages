using System.IO;
using System.Windows;
using System.Windows.Controls;
using VoiceSubtitle.ViewModel;
using System.Linq;

namespace VoiceSubtitle
{
    public partial class EditCaptionInfo : UserControl
    {
        public EditCaptionInfo()
        {
            InitializeComponent();
        }

        public string VideoName
        {
            get { return (string)GetValue(VideoNameProperty); }
            set { SetValue(VideoNameProperty, value); }
        }

        public static readonly DependencyProperty VideoNameProperty =
            DependencyProperty.Register("VideoName", typeof(string), typeof(EditCaptionInfo), new PropertyMetadata(string.Empty));

        public string VideoPath
        {
            get { return (string)GetValue(VideoPathProperty); }
            set { SetValue(VideoPathProperty, value); }
        }

        public static readonly DependencyProperty VideoPathProperty =
            DependencyProperty.Register("VideoPath", typeof(string), typeof(EditCaptionInfo), new PropertyMetadata(string.Empty));

        public string SubEngPath
        {
            get { return (string)GetValue(SubEngPathProperty); }
            set { SetValue(SubEngPathProperty, value); }
        }

        public static readonly DependencyProperty SubEngPathProperty =
            DependencyProperty.Register("SubEngPath", typeof(string), typeof(EditCaptionInfo), new PropertyMetadata(string.Empty));

        public string SubOtherPath
        {
            get { return (string)GetValue(SubOtherPathProperty); }
            set { SetValue(SubOtherPathProperty, value); }
        }

        public static readonly DependencyProperty SubOtherPathProperty =
            DependencyProperty.Register("SubOtherPath", typeof(string), typeof(EditCaptionInfo), new PropertyMetadata(string.Empty));

        private void TextBox_Drop(object sender, DragEventArgs e)
        {
            var button = sender as Button;
            var name = button.Name as string;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file = files[0];
                var ext = Path.GetExtension(file);

                if (VideoViewModel.VideoExtenstionSupported.Contains(ext.ToLower()))
                {
                    VideoPath = file;
                    if(string.IsNullOrEmpty(VideoName))
                    {
                        VideoName = Path.GetFileNameWithoutExtension(file);
                    }
                }
                else if (name == "englishsub" && ext.ToLower() == ".srt")
                {
                    SubEngPath = file;
                }
                else if (name == "othersub" && ext.ToLower() == ".srt")
                {
                    SubOtherPath = file;
                }
            }
        }
    }
}