using GalaSoft.MvvmLight;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using VoiceSubtitle.Model;

namespace VoiceSubtitle.ViewModel
{
    public class RecordViewModel : ViewModelBase
    {
        private NotifyViewModel _notifyViewModel;
        private PlayerViewModel _playerViewModel;
        private DispatcherTimer _timer;
        private WasapiCapture _capture;
        private string _fileName;
        private WaveFileWriter _writer;

        public ICommand RecordPressedCommand { get; }
        public ICommand ListenAgain { get; }


        public RecordViewModel(NotifyViewModel notifyViewModel, PlayerViewModel playerViewModel)
        {
            this._notifyViewModel = notifyViewModel;
            this._playerViewModel = playerViewModel;

            MessengerInstance.Register<bool>(this, "OnAppShutdownToken", OnAppShutdown);
            RecordPressedCommand = new ActionCommand(() =>
            {
                try
                {
                    if (!playerViewModel.IsShowViewer)
                        return;

                    if (!IsRecord && IsShowPanel)
                    {
                        IsShowPanel = false;
                        return;
                    }

                    IsShowPanel = true;

                    if (IsRecord)
                    {
                        IsRecord = false;
                        _capture.StopRecording();

                        return;
                    }

                    Record();
                }
                catch
                {
                }
            });

            ListenAgain = new ActionCommand(() =>
            {
                if (!playerViewModel.IsShowViewer)
                    return;

                WaveFileReader waveFileReader = null;
                WaveOutEvent waveOut = null;
                try
                {
                    waveFileReader = new WaveFileReader(_fileName);
                    waveOut = new WaveOutEvent();

                    waveOut.Init(waveFileReader);
                    waveOut.Play();
                    waveOut.PlaybackStopped += (s, e) =>
                    {
                        waveFileReader.Dispose();
                        waveFileReader.Close();
                    };
                }
                catch
                {
                    waveFileReader?.Dispose();
                    waveFileReader?.Close();
                    waveOut?.Dispose();
                }
            });

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += (s, e) => BlinkRed = !BlinkRed;
        }

        private void OnAppShutdown(bool obj)
        {
            var files = Directory.GetFiles(FolderManager.FolderRecordPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        private void Record()
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
                var selectedDevice = captureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);
                _capture = new WasapiCapture(selectedDevice);

                _capture.ShareMode = AudioClientShareMode.Shared;
                _capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);

                IsRecord = true;
                _fileName = FolderManager.FolderRecordPath + $@"\{Guid.NewGuid().ToString("N").ToLower()}.wmv";

                _writer = new WaveFileWriter(_fileName, _capture.WaveFormat);

                _capture.StartRecording();
                _capture.RecordingStopped += OnRecordingStopped;
                _capture.DataAvailable += CaptureOnDataAvailable;
            }
            catch (Exception e)
            {
                IsRecord = false;
                _writer?.Dispose();
                _writer = null;
                _capture?.Dispose();
                _capture = null;

                _notifyViewModel.ShowMessageBox("Can not record. Please check your cable.");
            }
        }

        private void CaptureOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            _writer.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            IsRecord = false;
            _writer?.Dispose();
            _writer = null;
            _capture?.Dispose();
            _capture = null;
        }

        private bool _isShowPanel;

        public bool IsShowPanel
        {
            get
            {
                return _isShowPanel;
            }
            set
            {
                Set(ref _isShowPanel, value);
            }
        }

        private bool _isRecord;

        public bool IsRecord
        {
            get
            {
                return _isRecord;
            }
            set
            {
                Set(ref _isRecord, value);

                if (value)
                    _timer.Start();
                else
                {
                    _timer.Stop();
                    BlinkRed = true;
                }
            }
        }

        private bool _blinkRed;

        public bool BlinkRed
        {
            get
            {
                return _blinkRed;
            }
            set
            {
                Set(ref _blinkRed, value);
            }
        }
    }
}