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

        private SourcePath current;

        public SourcePath Current
        {
            get { return current; }
            set { Set(ref current, value); }
        }

        private ICommand chooseSource;

        public ICommand ChooseSource
        {
            get
            {
                return chooseSource ?? (chooseSource = new ActionCommand(
                    (source) =>
                    {
                        SourcePath current = source as SourcePath;
                        Current = current;
                    }));
            }
        }
    }
}