using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VideoEnhancement.ViewModels
{
    public class VideoPlayerViewModel
    {
        private MediaPlayer _OriginalVideoMediaPlayer { get; set; }

        public MediaPlayer OriginalVideoMediaPlayer
        {
            get => _OriginalVideoMediaPlayer;
            private set
            {
                _OriginalVideoMediaPlayer = value;
                OnPropertyChanged(nameof(OriginalVideoMediaPlayer));
            }
        }

        private MediaPlayer _EnhancedVideoMediaPlayer { get; set; }
        public MediaPlayer EnhancedVideoMediaPlayer
        {
            get => _EnhancedVideoMediaPlayer;
            private set
            {
                _EnhancedVideoMediaPlayer = value;
                OnPropertyChanged(nameof(EnhancedVideoMediaPlayer));
            }
        }

        public LibVLC _LibVLC { get; set; }
        public ICommand OpenVideoFileCommand { get; set; }

        private string _playPauseButtonText = "Play";
        private int _volume;
        public string PlayPauseButtonText
        {
            get => _playPauseButtonText;
            set
            {
                _playPauseButtonText = value;
                OnPropertyChanged(nameof(PlayPauseButtonText));
            }
        }

        public int Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OriginalVideoMediaPlayer.Volume = _volume;
                OnPropertyChanged(nameof(Volume));
            }
        }



        private long _videoLength;
        public long VideoLength
        {
            get => _videoLength;
            set
            {
                _videoLength = value;
                OnPropertyChanged(nameof(VideoLength));
            }
        }

        private long _currentTime;
        public long CurrentTime
        {
            get { return _currentTime; }
            set
            {
                if (_currentTime != value)
                {

                    _currentTime = value;
                    OriginalVideoMediaPlayer.Time = _currentTime;
                    EnhancedVideoMediaPlayer.Time = _currentTime;

                    OnPropertyChanged(nameof(CurrentTime));
                }
            }
        }

        public ICommand PlayPauseCommand { get; private set; }

        public VideoPlayerViewModel()
        {
            _LibVLC = new LibVLC();
            OriginalVideoMediaPlayer = new MediaPlayer(_LibVLC);
            EnhancedVideoMediaPlayer = new MediaPlayer(_LibVLC);

            PlayPauseCommand = new RelayCommand(PlayPauseVideo);
            Volume = 10;
            OpenVideoFileCommand = new RelayCommand(OpenVideo);

            

            OriginalVideoMediaPlayer.TimeChanged += OnTimeChanged;
            // EnhancedVideoMediaPlayer.TimeChanged += OnTimeChanged;
        }

        private void OnTimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            EnhancedVideoMediaPlayer.Time = OriginalVideoMediaPlayer.Time;
            CurrentTime = e.Time;
        }
        private void OnOriginalVideoTimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            EnhancedVideoMediaPlayer.Time = OriginalVideoMediaPlayer.Time;
            CurrentTime = e.Time;
        }

        public void OpenVideo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Video files (*.mp4;*.avi;*.mkv;*.mov)|*.mp4;*.avi;*.mkv;*.mov"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                OriginalVideoMediaPlayer.Stop();
                OriginalVideoMediaPlayer.Media?.Dispose();
                EnhancedVideoMediaPlayer.Stop();
                EnhancedVideoMediaPlayer.Media?.Dispose();
                PlayVideos(openFileDialog.FileName, openFileDialog.FileName);
            }
        }

        public void PlayVideos(string filePathBefore, string filePathAfter)
        {
            var mediaBefore = new Media(_LibVLC, new Uri(filePathBefore));
            
            OriginalVideoMediaPlayer.Play(mediaBefore);
            mediaBefore.Parse(MediaParseOptions.ParseLocal);
            VideoLength = mediaBefore.Duration;

            mediaBefore.ParsedChanged += (sender, args) =>
            {
                if (args.ParsedStatus == MediaParsedStatus.Done)
                {
                    VideoLength = mediaBefore.Duration;
                    OnPropertyChanged(nameof(VideoLength));
                }
            };

            // placeholder. TODO: implement enhancement method as I did in python
            var mediaAfter = new Media(_LibVLC, new Uri(filePathAfter));
            EnhancedVideoMediaPlayer.Play(mediaAfter);

            PlayPauseButtonText = "Pause";
            OnPropertyChanged(nameof(PlayPauseButtonText));
        }

        private void PlayPauseVideo()
        {
            if (OriginalVideoMediaPlayer.IsPlaying)
            {
                OriginalVideoMediaPlayer.Pause();
                EnhancedVideoMediaPlayer.Pause();
                PlayPauseButtonText = "Play";
                SyncVideos();
            }
            else
            {
                OriginalVideoMediaPlayer.Play();
                EnhancedVideoMediaPlayer.Play();
                PlayPauseButtonText = "Pause";
                SyncVideos();
            }
            OnPropertyChanged(nameof(PlayPauseButtonText));
        }


        // here, all functions are placeholders
        public void ProcessAndSynchronizeFrames()
        {
            
        }

        private void SyncVideos()
        {
            OriginalVideoMediaPlayer.TimeChanged += (sender, args) =>
            {
                long currentTime = OriginalVideoMediaPlayer.Time;
                EnhancedVideoMediaPlayer.Time = currentTime;
            };
        }
        
        // TODO: todo for future to play and process video in real time
        public void PlayRealTimeCameraFeed()
        {
            var media = new Media(_LibVLC, ":dshow://");
            OriginalVideoMediaPlayer.Play(media);

        }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;

            public RelayCommand(Action execute)
            {
                _execute = execute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                _execute();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
