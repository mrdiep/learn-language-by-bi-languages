using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using VoiceSubtitle.Model;
using System.Windows.Input;
using System.Threading.Tasks;

namespace VoiceSubtitle.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(AppDataContext appDataContext, DispatchService dispatchService)
        {
            SourcePaths = new ObservableCollection<SourcePath>();
            Task.Factory.StartNew(async () =>
            {
                var captions = await appDataContext.LoadCaptions();
                dispatchService.Invoke(() => { captions.ForEach(x => SourcePaths.Add(x)); });
            }
            );
        }

        public ObservableCollection<SourcePath> SourcePaths { get; set; }

    }
}