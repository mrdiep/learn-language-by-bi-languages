using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using VoiceSubtitle.Model;
using System.Windows.Input;
using System.IO;
using System.Windows;
using System;
using System.Reflection;

namespace VoiceSubtitle.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private AppDataContext appDataContext;

        public ICommand SaveEditCurrent { get; }
        public ICommand CancelEditSource { get; }
        public ICommand DeleteSource { get; }

        public MainViewModel(AppDataContext appDataContext, DispatchService dispatchService)
        {
            this.appDataContext = appDataContext;
            CancelEditSource = new ActionCommand((x) => EditCurrent = null);
            SaveEditCurrent = new ActionCommand((x) =>
            {
                SourcePath source = x as SourcePath;
                if (source == null)
                    return;

                try
                {
                    File.Delete(source.Path);
                    SourcePaths.Remove(source);
                    AddNewSource(ref source);
                    MessageBox.Show("Save Success", App.AppTitle);
                }
                catch (Exception)
                {
                    MessageBox.Show("Failing to save", App.AppTitle);
                }
            });
            DeleteSource = new ActionCommand((x) =>
            {
                SourcePath source = x as SourcePath;
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
                        MessageBox.Show("Error on delete project", App.AppTitle);
                    }
                }
            });

            SourcePaths = new ObservableCollection<SourcePath>();

            var captions = appDataContext.LoadCaptions();
            dispatchService.Invoke(() => { captions.ForEach(x => SourcePaths.Add(x)); });

        
        }

        public ObservableCollection<SourcePath> SourcePaths { get; }

        private SourcePath editCurrent;

        public SourcePath EditCurrent
        {
            get
            {
                return editCurrent;
            }
            set
            {
                Set(ref editCurrent, value);
            }
        }

        public void AddNewSource(ref SourcePath source)
        {
            source.Save();
            SourcePaths.Add(source);
        }
    }
}