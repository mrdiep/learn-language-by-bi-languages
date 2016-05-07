using System.Windows.Controls;

namespace VoiceSubtitle
{
    public partial class VideoViewer : UserControl
    {
        public static MediaElement MediaPlayer { get; set; }

        public VideoViewer()
        {
            InitializeComponent();
            MediaPlayer = mediaPlayer;
        }
    }
}