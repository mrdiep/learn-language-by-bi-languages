﻿using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using VoiceSubtitle.Helper;
using VoiceSubtitle.ViewModel;
using System.Linq;


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
            var listBox = sender as ListBox;
            
            if (listBox.SelectedItem != null)
            {
                listBox.UpdateLayout();
                listBox.ScrollToCenterOfView(listBox.SelectedItem);
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            listView.SelectedItem = null;
        }

        private void PrimarySearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                ServiceLocator.Current.GetInstance<PlayerViewModel>().SearchPrimaryCaption((sender as TextBox).Text);
        }

        private void PrimaryCaptionDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file = files[0];
                var ext = Path.GetExtension(file);

                if (ext == ".srt")
                {
                    ServiceLocator.Current.GetInstance<PlayerViewModel>().UpdatePrivateCaption(file);
                }
            }
        }

        private void TranslatedCaptionDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file = files[0];
                var ext = Path.GetExtension(file);

                if (ext == ".srt")
                {
                    ServiceLocator.Current.GetInstance<PlayerViewModel>().UpdateTranslatedCaption(file);
                }
            }
        }

        private void VideoPathDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {                
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file = files[0];
                var ext = Path.GetExtension(file);

                if (VideoViewModel.VideoExtenstionSupported.Contains(ext.ToLower()))
                {
                    ServiceLocator.Current.GetInstance<PlayerViewModel>().UpdateVideoPath(file);
                }
            }
        }
    }
}