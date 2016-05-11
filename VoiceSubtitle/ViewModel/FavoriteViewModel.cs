using GalaSoft.MvvmLight;
using System.Threading.Tasks;
using VoiceSubtitle.Model;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;
using System.Linq;
using System.Windows;

namespace VoiceSubtitle.ViewModel
{
    public class FavoriteViewModel : ViewModelBase
    {
        public ObservableCollection<FavoriteModel> Items { get; }
        public ICommand AddFavorite { get; }
        public ICommand RemoveCommand { get; }
        public ICommand SwitchToProject { get; }

        NotifyViewModel notifyViewModel;
        public FavoriteViewModel(NotifyViewModel notifyViewModel)
        {
            this.notifyViewModel = notifyViewModel;
            Items = new ObservableCollection<FavoriteModel>();
            MessengerInstance.Register<bool>(this, "CloseAllFlyoutToken", (x) => IsShowPanel = false);
            //Task.Factory.StartNew(FetchData);
            AddFavorite = new ActionCommand((x) =>
            {
                string text = x as string;
                var currentSource = ServiceLocator.Current.GetInstance<PlayerViewModel>().CurrentSource;
                var model = new FavoriteModel()
                {
                    Film = currentSource.VideoName,
                    Source = currentSource.Path,
                    Text = text
                };
                Items.Add(model);
                IsShowPanel = true;
                Selected = model;
            });

            SwitchToProject = new ActionCommand(x => {
                if (MessageBox.Show("Do you want to switch source?", App.AppTitle, MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;

                var model = x as FavoriteModel;
                var currentSource = ServiceLocator.Current.GetInstance<MainViewModel>().SourcePaths.Where(c => c.Path == model.Source).FirstOrDefault() ;
                if (currentSource == null)
                {
                    notifyViewModel.MessageBox("Source do not exists");
                    return;
                }
                                
                ServiceLocator.Current.GetInstance<PlayerViewModel>().SwitchSource.Execute(currentSource);

            });

            RemoveCommand = new ActionCommand(x => {
                var model = x as FavoriteModel;
                Items.Remove(model);
            });
        }

        private bool isShowPanel;

        public bool IsShowPanel
        {
            get
            {
                return isShowPanel;
            }
            set
            {
                Set(ref isShowPanel, value);
            }
        }
        private FavoriteModel selected;

        public FavoriteModel Selected
        {
            get
            {
                return selected;
            }
            set
            {
                Set(ref selected, value);
            }
        }
        private void FetchData()
        {
            string connectionString = $@"Data Source={Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Dict\dict.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM `favorites`";
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string word = reader["word"] as string;

                            var pro = new FavoriteModel()
                            {
                                Text = reader["word"] as string,
                                Film = reader["film"] as string,
                                Source = reader["source"] as string,
                            };

                            Items.Add(pro);
                        }
                    }
                }
            }
        }
    }
}