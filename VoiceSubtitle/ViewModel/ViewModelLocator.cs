using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace VoiceSubtitle.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<SettingViewModel>();
            SimpleIoc.Default.Register<VideoViewModel>();
            SimpleIoc.Default.Register<DispatchService>();
            SimpleIoc.Default.Register<PlayerViewModel>();
            SimpleIoc.Default.Register<AppDataContext>();
            SimpleIoc.Default.Register<MainViewModel>();

            SimpleIoc.Default.Register<CambridgeDictionaryViewModel>();
        }
        public SettingViewModel SettingViewModel => ServiceLocator.Current.GetInstance<SettingViewModel>();
        public VideoViewModel VideoViewModel => ServiceLocator.Current.GetInstance<VideoViewModel>();
        public PlayerViewModel PlayerViewModel => ServiceLocator.Current.GetInstance<PlayerViewModel>();
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public static void Cleanup()
        {
        }
    }
}