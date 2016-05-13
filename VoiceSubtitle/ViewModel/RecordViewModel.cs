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
        public ICommand RecordPressedCommand { get; }
        public ICommand ListenAgain { get; }

        private DispatcherTimer timer;
        private WasapiCapture capture;
        private string fileName;

        public RecordViewModel()
        {
            MessengerInstance.Register<bool>(this, "OnAppShutdownToken", OnAppShutdown);
            RecordPressedCommand = new ActionCommand(() =>
            {
                if (!IsRecord && IsShowPanel)
                {
                    IsShowPanel = false;
                    return;
                }

                IsShowPanel = true;

                if (IsRecord)
                {
                    IsRecord = false;
                    capture.StopRecording();

                    return;
                }

                Record();
            });

            ListenAgain = new ActionCommand(() =>
            {
                WaveFileReader waveFileReader = null;
                WaveOutEvent waveOut = null;
                try
                {
                    waveFileReader = new WaveFileReader(fileName);
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

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => BlinkRed = !BlinkRed;
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
                var CaptureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
                var SelectedDevice = CaptureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);
                capture = new WasapiCapture(SelectedDevice);

                capture.ShareMode = AudioClientShareMode.Shared;
                capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);

                IsRecord = true;
                fileName = FolderManager.FolderRecordPath + $@"\{Guid.NewGuid().ToString("N").ToLower()}.wmv";

                writer = new WaveFileWriter(fileName, capture.WaveFormat);

                capture.StartRecording();
                capture.RecordingStopped += OnRecordingStopped;
                capture.DataAvailable += CaptureOnDataAvailable;
            }
            catch (Exception e)
            {
                IsRecord = false;
            }
        }

        private WaveFileWriter writer;

        private void CaptureOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            writer.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            IsRecord = false;
            writer?.Dispose();
            writer = null;
            capture?.Dispose();
            capture = null;
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

        private bool isRecord;

        public bool IsRecord
        {
            get
            {
                return isRecord;
            }
            set
            {
                Set(ref isRecord, value);

                if (value)
                    timer.Start();
                else
                    timer.Stop();
            }
        }

        private bool blinkRed;

        public bool BlinkRed
        {
            get
            {
                return blinkRed;
            }
            set
            {
                Set(ref blinkRed, value);
            }
        }
    }
}