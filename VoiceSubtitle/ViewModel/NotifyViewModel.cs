﻿using GalaSoft.MvvmLight;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace VoiceSubtitle.ViewModel
{
    public class NotifyViewModel : ViewModelBase
    {
        private CancellationTokenSource tokenSource;
        private string text;

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                Set(ref text, value);
                if (tokenSource != null)
                    tokenSource.Cancel();

                tokenSource = new CancellationTokenSource();

                CancellationToken ct = tokenSource.Token;
                Task.Factory.StartNew(async ()=> {
                    try
                    {
                        await Task.Delay(4000, ct);
                        text = "";
                        RaisePropertyChanged("Text");
                    }
                    catch
                    {

                    }
                    finally
                    {
                        tokenSource = null;
                    }
                });
            }
        }

        public void MessageBox(string text)
        {
            var window = new MessageWindow();
            window.Topmost = true;
            window.Text = text;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowDialog();
        }
    }
}