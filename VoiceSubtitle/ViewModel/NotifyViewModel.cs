using GalaSoft.MvvmLight;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace VoiceSubtitle.ViewModel
{
    public class NotifyViewModel : ViewModelBase
    {
        private CancellationTokenSource _tokenSource;
        private string _text;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                Set(ref _text, value);
                if (_tokenSource != null)
                    _tokenSource.Cancel();

                _tokenSource = new CancellationTokenSource();

                var ct = _tokenSource.Token;
                Task.Factory.StartNew(async ()=> {
                    try
                    {
                        await Task.Delay(4000, ct);
                        _text = "";
                        RaisePropertyChanged("Text");
                    }
                    catch
                    {

                    }
                    finally
                    {
                        _tokenSource = null;
                    }
                });
            }
        }

        public void ShowMessageBox(string text)
        {
            var window = new MessageWindow();
            window.Topmost = true;
            window.Text = text;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowDialog();
        }
    }
}