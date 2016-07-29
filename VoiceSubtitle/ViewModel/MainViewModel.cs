using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using VoiceSubtitle.Model;
using System.Windows.Input;
using System.IO;
using System.Windows;
using System;

namespace VoiceSubtitle.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private AppDataContext _appDataContext;
        private NotifyViewModel _notifyViewModel;
        public ICommand SaveEditCurrent { get; }
        public ICommand CancelEditSource { get; }
        public ICommand DeleteSource { get; }

        public ICommand EditSource { get; }

        public MainViewModel(AppDataContext appDataContext, DispatchService dispatchService, NotifyViewModel notifyViewModel)
        {
            this._appDataContext = appDataContext;
            this._notifyViewModel = notifyViewModel;
            MessengerInstance.Register<bool>(this, "CloseAllFlyoutToken", x => IsShowProjectPanel = false);
            CancelEditSource = new ActionCommand(x => EditCurrent = null);
            SaveEditCurrent = new ActionCommand(x =>
            {
                var source = x as SourcePath;
                if (source == null)
                    return;

                try
                {
                    File.Delete(source.Path);
                    SourcePaths.Remove(source);
                    AddNewSource(ref source);
                    notifyViewModel.ShowMessageBox("Save Success");
                }
                catch (Exception)
                {
                    notifyViewModel.ShowMessageBox("Failing to save");
                }
            });

            EditSource = new ActionCommand(x =>
            {
                var source = x as SourcePath;
                if (source == null)
                    return;

                EditCurrent = source;
            });
            DeleteSource = new ActionCommand(x =>
            {
                var source = x as SourcePath;
                if (source == null)
                    return;

                var result = MessageBox.Show("Do you want to delete this project?", "Delete", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    SourcePaths.Remove(source);
                    try
                    {
                        File.Delete(source.Path);
                    }
                    catch (Exception ex)
                    {
                        notifyViewModel.ShowMessageBox("Error on delete project");
                    }
                }
            });

            SourcePaths = new ObservableCollection<SourcePath>();

            var captions = appDataContext.LoadCaptions();
            dispatchService.Invoke(() => { captions.ForEach(x => SourcePaths.Add(x)); });
        }

        public ObservableCollection<SourcePath> SourcePaths { get; }

        private bool _isShowProjectPanel;

        public bool IsShowProjectPanel
        {
            get
            {
                return _isShowProjectPanel;
            }
            set
            {
                Set(ref _isShowProjectPanel, value);
            }
        }

        private SourcePath _editCurrent;

        public SourcePath EditCurrent
        {
            get
            {
                return _editCurrent;
            }
            set
            {
                Set(ref _editCurrent, value);
            }
        }

        public void AddNewSource(ref SourcePath source)
        {
            source.Save();
            SourcePaths.Add(source);
        }
    }
}