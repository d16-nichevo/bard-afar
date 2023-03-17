using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BardAfar
{
    // Iteration modes. In other words, how to move from one song to the next.
    internal enum IterationMode
    {
        Next,
        Random
    }

    // Loop mode. What do to after a song is finished.
    internal enum LoopMode
    {
        Stop,
        RepeatTrack,
        RepeatDirectory
    }

    /// <summary>
    /// The view model for this application.
    /// Used for binding to the XAML.
    /// </summary>
    internal class ViewModel : INotifyPropertyChanged
    {
        //////////////////////////////////
        //  PROPERTIES
        //////////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;


        // - - - - - - - - - - - - - - - - - - - - 
        // Server Setup Parameters
        // - - - - - - - - - - - - - - - - - - - - 

        private string _audioFilesDirectory = Settings.Default.ServerInfoAudioDirectory;
        public string AudioFilesDirectory
        {
            get => _audioFilesDirectory;
            set { _audioFilesDirectory = value; OnPropertyChanged(); }
        }

        private string _hostOrIpAddress = Settings.Default.ServerInfoHostOrIpAddress;
        public string HostOrIpAddress
        {
            get => _hostOrIpAddress;
            set { _hostOrIpAddress = value; OnPropertyChanged(); }
        }
        private ushort _portHttpListener = Settings.Default.ServerInfoPortHttpListener;
        public ushort PortHttpListener
        {
            get => _portHttpListener;
            set { _portHttpListener = value; OnPropertyChanged(); }
        }

        private ushort _portWebSocket = Settings.Default.ServerInfoPortWebSocket;
        public ushort PortWebSocket
        {
            get => _portWebSocket;
            set { _portWebSocket = value; OnPropertyChanged(); }
        }

        private uint _trackPaddingSeconds = Settings.Default.TrackPaddingSeconds;
        public uint TrackPaddingSeconds
        {
            get => _trackPaddingSeconds;
            set { _trackPaddingSeconds = value; OnPropertyChanged(); }
        }

        // - - - - - - - - - - - - - - - - - - - - 
        // Log/Directory Control Visibility
        // - - - - - - - - - - - - - - - - - - - - 

        private bool _logVisible = false;
        public bool DirViewVisible
        {
            get => !_logVisible;
            set { _logVisible = !value; OnPropertyChanged(nameof(DirViewVisible)); OnPropertyChanged(nameof(LogVisible)); }
        }
        public bool LogVisible
        {
            get => _logVisible;
            set { _logVisible = value; OnPropertyChanged(nameof(DirViewVisible)); OnPropertyChanged(nameof(LogVisible)); }
        }

        // - - - - - - - - - - - - - - - - - - - - 
        // Play Mode
        // - - - - - - - - - - - - - - - - - - - - 

        private bool _playing = false;
        public bool Playing
        {
            get => _playing;
            set { _playing = value; OnPropertyChanged(); OnPropertyChanged(nameof(Stopped)); }
        }
        public bool Stopped
        {
            get => !_playing;
            set { _playing = !value; OnPropertyChanged(); OnPropertyChanged(nameof(Playing)); }
        }

        // - - - - - - - - - - - - - - - - - - - - 
        // Iteration Mode
        // - - - - - - - - - - - - - - - - - - - - 

        private IterationMode _iterationMode = (IterationMode)Settings.Default.IterationMode;
        public IterationMode IterationMode
        {
            get => _iterationMode;
            set { _iterationMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(IterateNext)); OnPropertyChanged(nameof(IterateRandom)); }
        }
        public bool IterateNext { get { return IterationMode == IterationMode.Next; } }
        public bool IterateRandom { get { return IterationMode == IterationMode.Random; } }

        // - - - - - - - - - - - - - - - - - - - - 
        // Loop Mode
        // - - - - - - - - - - - - - - - - - - - - 

        private LoopMode _loopMode = (LoopMode)Settings.Default.LoopMode;
        public LoopMode LoopMode
        {
            get => _loopMode;
            set
            {
                _loopMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoopRepeatDirectory));
                OnPropertyChanged(nameof(LoopRepeatTrack));
                OnPropertyChanged(nameof(LoopStop));
            }
        }
        public bool LoopRepeatDirectory { get { return LoopMode == LoopMode.RepeatDirectory; } }
        public bool LoopRepeatTrack { get { return LoopMode == LoopMode.RepeatTrack; } }
        public bool LoopStop { get { return LoopMode == LoopMode.Stop; } }

        // - - - - - - - - - - - - - - - - - - - - 
        // Media Element Volume
        // - - - - - - - - - - - - - - - - - - - - 

        private double _volume = Settings.Default.Volume;
        public double Volume
        {
            get => _volume;
            set { _volume = Math.Clamp(value, 0.0, 1.0); OnPropertyChanged(); }
        }

        // - - - - - - - - - - - - - - - - - - - - 
        // Track Timer
        // - - - - - - - - - - - - - - - - - - - - 

        private DateTime _trackEndTime = DateTime.MinValue;
        public TimeSpan TrackRemaining
        {
            get
            {
                var span = _trackEndTime - DateTime.Now;
                // Don't show negative times:
                span = span >= TimeSpan.Zero ? span : TimeSpan.Zero;
                return span;
            }
        }

        public DateTime TrackEndTime
        {
            set { _trackEndTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(TrackRemaining)); }
        }

        public void RefreshTrackTime()
        {
            OnPropertyChanged(nameof(TrackRemaining));
        }

        // - - - - - - - - - - - - - - - - - - - - 
        // Log Contents
        // - - - - - - - - - - - - - - - - - - - - 

        private ConcurrentDictionary<DateTime, string> _log = new ConcurrentDictionary<DateTime, string>();
        private string _logFormat = Settings.Default.LogFormat; // We fetch this to store locally as it can be a tiny bit slow to access.
        public string Log
        {
            get
            {
                string log = string.Empty;
                _log.OrderBy(x => x.Key).ToList().ForEach(x =>
                {
                    log += String.Format(_logFormat, x.Key, x.Value) + Environment.NewLine;
                });
                return log;
            }
        }

        public void AppendToLog(string entry)
        {
            bool added = false;
            while (!added)
            {
                added = _log.TryAdd(DateTime.Now, entry);
                if (!added)
                {
                    // This is to stop collisions. It's probably not
                    // the best way to do this, as I suspect it blocks
                    // some important thread, but it should be called
                    // very infrequently so it'll probably be alright.
                    Thread.Sleep(10);
                }
            }

            // Limit the log's size. Remove oldest entries first.
            while (_log.Count > Settings.Default.LogLimitLines)
            {
                _log.TryRemove(_log.OrderBy(x => x.Key).First());
            }

            OnPropertyChanged(nameof(Log));
        }

        //////////////////////////////////
        //  METHODS
        //////////////////////////////////

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
