using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace LightMusic.Controls {
    public sealed class CustomMediaTransportControls : MediaTransportControls {
        public event EventHandler<EventArgs> OpenFile;
        public event EventHandler<EventArgs> StopPlay;
        public event EventHandler<EventArgs> PlayPause;

        public CustomMediaTransportControls() {
            this.DefaultStyleKey = typeof(CustomMediaTransportControls);
        }

        protected override void OnApplyTemplate() {
            Button openButton = GetTemplateChild("OpenFileButton") as Button;
            openButton.Click += OpenButton_Click;
            Button stopButton = GetTemplateChild("StopButton") as Button;
            stopButton.Click += stopButton_Click;
            Button PlayPauseButton = GetTemplateChild("PlayPauseButton") as Button;
            PlayPauseButton.Click += PlayPauseButton_Click;

            base.OnApplyTemplate();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e) {
            OpenFile?.Invoke(this, EventArgs.Empty);
        }

        private void stopButton_Click(object sender, RoutedEventArgs e) {
            StopPlay?.Invoke(this, EventArgs.Empty);
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e) {
            PlayPause?.Invoke(this, EventArgs.Empty);
        }
    }
}
