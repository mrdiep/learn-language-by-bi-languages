using System.IO;
using System.Windows;
using System.Windows.Controls;

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
            Button button = sender as Button;
            string type = button.Tag as string;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string file = files[0];
                string ext = Path.GetExtension(file);

                if (ext == ".mp4" || ext == ".mkv")
                {
                    VideoPath = file;
                    if(string.IsNullOrEmpty(VideoName))
                    {
                        VideoName = Path.GetFileName(file);
                    }
                }
                else if (type == "englishsub" && ext == ".srt")
                {
                    SubEngPath = file;
                }
                else if (type == "othersub" && ext == ".srt")
                {
                    SubOtherPath = file;
                }
            }
        }
    }
}