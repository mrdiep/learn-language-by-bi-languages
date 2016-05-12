using GalaSoft.MvvmLight;
using VoiceSubtitle.Model;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VoiceSubtitle.ViewModel
{
    public class FavoriteViewModel : ViewModelBase
    {
        public ObservableCollection<FavoriteModel> Items { get; }
        public ICommand AddFavorite { get; }
        public ICommand RemoveCommand { get; }
        public ICommand SwitchToProject { get; }

        private NotifyViewModel notifyViewModel;
        private DispatchService dispatchService;

        public FavoriteViewModel(NotifyViewModel notifyViewModel, DispatchService dispatchService)
        {
            this.notifyViewModel = notifyViewModel;
            this.dispatchService = dispatchService;
            Items = new ObservableCollection<FavoriteModel>();
            MessengerInstance.Register<bool>(this, "CloseAllFlyoutToken", (x) => IsShowPanel = false);
            Task.Factory.StartNew(FetchData);
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

                var itemExists = Items.Where(t => t.Text == model.Text).FirstOrDefault();
                if (itemExists != null)
                    return;

                Items.Add(model);
                IsShowPanel = true;
                Selected = model;
                Add(model);
                notifyViewModel.Text = "Add Success";
            });

            SwitchToProject = new ActionCommand(x =>
            {
                if (MessageBox.Show("Do you want to switch source?", App.AppTitle, MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;

                var model = x as FavoriteModel;
                var currentSource = ServiceLocator.Current.GetInstance<MainViewModel>().SourcePaths.Where(c => c.Path == model.Source).FirstOrDefault();
                if (currentSource == null)
                {
                    notifyViewModel.ShowMessageBox("Source do not exists");
                    return;
                }

                ServiceLocator.Current.GetInstance<PlayerViewModel>().SwitchSource.Execute(currentSource);
            });

            RemoveCommand = new ActionCommand(x =>
            {
                var model = x as FavoriteModel;
                Items.Remove(model);
                Remove(model);
                notifyViewModel.Text = "Remove Success";
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
            var items = new List<FavoriteModel>();
            string connectionString = $@"Data Source={Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\favorites.db;Version=3;";
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
                            var pro = new FavoriteModel()
                            {
                                Text = reader["word"] as string,
                                Film = reader["film"] as string,
                                Source = reader["source"] as string,
                            };

                            items.Add(pro);
                        }
                    }
                }
            }

            dispatchService.Invoke(() => items.ForEach(x => Items.Add(x)));
        }

        private void Add(FavoriteModel model)
        {
            string connectionString = $@"Data Source={Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\favorites.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO `favorites`(`word`,`film`,`source`) VALUES(@word,@film,@source)";
                    connection.Open();

                    command.Parameters.AddWithValue("@word", model.Text);
                    command.Parameters.AddWithValue("@film", model.Film);
                    command.Parameters.AddWithValue("@source", model.Source);

                    var e = command.ExecuteNonQuery();
                }
            }
        }

        private void Remove(FavoriteModel model)
        {
            string connectionString = $@"Data Source={Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\favorites.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM `favorites` WHERE `word`=@word";
                    connection.Open();

                    command.Parameters.AddWithValue("@word", model.Text);
                    var e = command.ExecuteNonQuery();
                }
            }
        }
    }
}