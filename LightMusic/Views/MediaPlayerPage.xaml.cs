using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls.Primitives;
using System.Diagnostics;

namespace LightMusic.Views {
    public sealed partial class MediaPlayerPage : Page, INotifyPropertyChanged {
        private bool displayDisk = true;
        private DispatcherTimer dispatcherTimer;

        public bool DisplayDisk {
            get { return displayDisk; }
            set { Set(ref displayDisk, value); }
        }


        public MediaPlayerPage() {
            InitializeComponent();
            // 处理旋转盘片的显示和隐藏
            mpe.MediaOpened += async (Media, o) => {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => {
                    mpe.TransportControls.IsStopEnabled = true;
                    if (mpe.NaturalVideoHeight > 0) {
                        DisplayDisk = false;
                    } else {
                        DisplayDisk = true;
                    }

                    MediaElement media = Media as MediaElement;
                    String allTime = media.NaturalDuration.TimeSpan.ToString();
                    TimeRemainingElement.Text = allTime.Substring(0, allTime.IndexOf('.'));
                    ProgressSlider.Maximum = media.NaturalDuration.TimeSpan.TotalSeconds;

                    ProgressSlider.IsEnabled = true;
                    dispatcherTimer = new DispatcherTimer();
                    dispatcherTimer.Tick += dispatcherTimer_Tick;
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                    dispatcherTimer.Start();
                    PlayPauseSymbol.Symbol = Symbol.Pause;
                });
            };


            mpe.MediaEnded += async (m, o) => {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => {
                    EllStoryboard.Stop();
                    EllStoryboard.Begin();
                    EllStoryboard.Pause();
                    dispatcherTimer.Stop();
                    PlayPauseSymbol.Symbol = Symbol.Play;
                    TimeElapsedElement.Text = "00:00:00";
                    ProgressSlider.Value = 0;
                });
            };

            ProgressSlider.ValueChanged += MediaSlider_ValueChanged;


        }

        private void MediaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
            
            mpe.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        private void dispatcherTimer_Tick(object sender, object e) {
            string time = mpe.Position.ToString();
            TimeElapsedElement.Text = time.Contains(".") ? time.Substring(0, time.IndexOf('.')) : time;
            ProgressSlider.Value = mpe.Position.TotalSeconds;
        }

        private void CustomMTC_PlayPause(object sender, EventArgs e) {
            if (mpe.CurrentState == MediaElementState.Playing) {
                EllStoryboard.Pause();
                dispatcherTimer.Stop();
            } else if (mpe.CurrentState == MediaElementState.Paused || mpe.CurrentState == MediaElementState.Stopped) {
                EllStoryboard.Resume();
                dispatcherTimer.Start();
            }
        }
        
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null) {
            if (Equals(storage, value)) {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e) {
            var picker = new FileOpenPicker {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            string[] type = { ".mp3", ".mp4", ".avi", ".wmv", ".rmvb", ".mpeg", ".wma" };
            foreach (string s in type) {
                picker.FileTypeFilter.Add(s);
            }
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null) {
                mpe.AutoPlay = true;
                //get the stream from the storage file
                var mediaStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                //set the source to the video stream 
                mpe.SetSource(mediaStream, file.ContentType);
                EllStoryboard.Begin();
                // MediaSource.CreateFromStorageFile(file);
                JsonObject res = await Services.MusicService.SearchSong(file.Name);
                string songId = Services.MusicService.GetSongIdFromSearch(res);
                JsonObject songData = await Services.MusicService.GetSongDetail(songId);
                string url = Services.MusicService.GetAlbumImageFromSong(songData);
                string AlbumTitle = Services.MusicService.GetAlbumTitleFromSong(songData);
                DiskImage.ImageSource = new BitmapImage(new Uri(url));


                TitleText.Text = file.Name;
                AlbumText.Text = AlbumTitle;
                EllStoryboard.Resume();
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
            mpe.Volume = e.NewValue / 100;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e) {
            if (mpe.CurrentState == MediaElementState.Playing) {
                EllStoryboard.Stop();
                EllStoryboard.Begin();
                EllStoryboard.Pause();

                TimeElapsedElement.Text = "00:00:00";
                ProgressSlider.Value = 0;

                dispatcherTimer.Stop();

                mpe.Stop();
                PlayPauseSymbol.Symbol = Symbol.Play;
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e) {
            if (mpe.CurrentState == MediaElementState.Playing) {
                // 停止播放
                PlayPauseSymbol.Symbol = Symbol.Play;
                mpe.Pause();
                EllStoryboard.Pause();
                dispatcherTimer.Stop();
            } else if (mpe.CurrentState == MediaElementState.Paused || mpe.CurrentState == MediaElementState.Stopped) {
                // 继续播放
                PlayPauseSymbol.Symbol = Symbol.Pause;
                mpe.Play();
                EllStoryboard.Resume();
                dispatcherTimer.Start();
            }

        }

        private void FullWindowButton_Click(object sender, RoutedEventArgs e) {
            ApplicationView view = ApplicationView.GetForCurrentView();

            bool isInFullScreenMode = view.IsFullScreenMode;

            if (isInFullScreenMode) {
                view.ExitFullScreenMode();
            } else {
                view.TryEnterFullScreenMode();
            }
        }
    }
}
