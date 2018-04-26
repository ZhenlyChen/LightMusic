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

namespace LightMusic.Views
{
    public sealed partial class MediaPlayerPage : Page, INotifyPropertyChanged
    {
        // TODO WTS: Set your default video and image URIs
        // For more on the MediaPlayer and adjusting controls and behavior see https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/media-playback
        private const string DefaultSource = "ms-appx:///Assets/music.mp3";

        // The poster image is displayed until the video is started
        // private const string DefaultPoster = "ms-appx:///Assets/music.svg";

        // The DisplayRequest is used to stop the screen dimming while watching for extended periods
        private DisplayRequest _displayRequest = new DisplayRequest();
        private bool _isRequestActive = false;
        private bool displayDisk = true;
        public bool DisplayDisk {
            get { return displayDisk; }
            set { Set(ref displayDisk, value); }
        }
        

        public MediaPlayerPage()
        {
            InitializeComponent();

            // mpe.PosterSource = new BitmapImage(new Uri(DefaultPoster));
            // mpe.Source = MediaSource.CreateFromUri(new Uri(DefaultSource));

            // 处理旋转盘片的显示和隐藏
            mpe.MediaOpened += async (Media, o) => {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => {
                    EllStoryboard.Begin();
                    mpe.TransportControls.IsStopEnabled = true;
                    if (mpe.NaturalVideoHeight > 0) {
                        DisplayDisk = false;
                    } else {
                        DisplayDisk = true;
                    }
                });
            };

            mpe.MediaEnded += async (m, o) => {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => {
                    EllStoryboard.Stop();
                    EllStoryboard.Begin();
                    EllStoryboard.Pause();
                });
            };


        }
        
        private void CustomMTC_PlayPause(object sender, EventArgs e) {
            if (mpe.CurrentState == MediaElementState.Playing) {
                EllStoryboard.Pause();
            } else if (mpe.CurrentState == MediaElementState.Paused || mpe.CurrentState == MediaElementState.Stopped) {
                EllStoryboard.Resume();
            }
        }


        private void CustomMTC_StopPlay(object sender, EventArgs e) {
            EllStoryboard.Stop();
            EllStoryboard.Begin();
            EllStoryboard.Pause();
        }


        private async void CustomMTC_OpenFile(object sender, EventArgs e) {
            var picker = new FileOpenPicker {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".wmv");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".rmvb");
            picker.FileTypeFilter.Add(".mpeg");
            picker.FileTypeFilter.Add(".wma");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null) {
                mpe.AutoPlay = true;
                //get the stream from the storage file
                var mediaStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                //set the source to the video stream 
                mpe.SetSource(mediaStream, file.ContentType);
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //mpe.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //mpe.MediaPlayer.Pause();
            //mpe.MediaPlayer.PlaybackSession.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            if (sender is MediaPlaybackSession playbackSession && playbackSession.NaturalVideoHeight != 0)
            {
                if (playbackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    if (!_isRequestActive)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            _displayRequest.RequestActive();
                            _isRequestActive = true;
                        });
                    }
                }
                else
                {
                    if (_isRequestActive)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            _displayRequest.RequestRelease();
                            _isRequestActive = false;
                        });
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
