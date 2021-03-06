using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace VoiceSubtitle.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<FavoriteViewModel>();
            SimpleIoc.Default.Register<SettingViewModel>();
            SimpleIoc.Default.Register<VideoViewModel>();
            SimpleIoc.Default.Register<DispatchService>();
            SimpleIoc.Default.Register<PlayerViewModel>();
            SimpleIoc.Default.Register<AppDataContext>();
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<NotifyViewModel>();
            SimpleIoc.Default.Register<RecordViewModel>();
            SimpleIoc.Default.Register<SubtitleDownloaderViewModel>();
            SimpleIoc.Default.Register<CambridgeDictionaryViewModel>();
        }

        public RecordViewModel RecordViewModel => ServiceLocator.Current.GetInstance<RecordViewModel>();
        public CambridgeDictionaryViewModel CambridgeDictionaryViewModel => ServiceLocator.Current.GetInstance<CambridgeDictionaryViewModel>();
        public FavoriteViewModel FavoriteViewModel => ServiceLocator.Current.GetInstance<FavoriteViewModel>();
        public NotifyViewModel NotifyViewModel => ServiceLocator.Current.GetInstance<NotifyViewModel>();
        public SubtitleDownloaderViewModel SubtitleDownloaderViewModel => ServiceLocator.Current.GetInstance<SubtitleDownloaderViewModel>();
        public SettingViewModel SettingViewModel => ServiceLocator.Current.GetInstance<SettingViewModel>();
        public VideoViewModel VideoViewModel => ServiceLocator.Current.GetInstance<VideoViewModel>();
        public PlayerViewModel PlayerViewModel => ServiceLocator.Current.GetInstance<PlayerViewModel>();
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public static void Cleanup()
        {
        }
    }
}