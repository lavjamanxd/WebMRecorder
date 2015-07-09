using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WebMRecorder
{
    /// <summary>
    /// Interaction logic for ReplayWindow.xaml
    /// </summary>
    public partial class ReplayWindow : Window
    {
        private List<Bitmap> _bitmaps;

        private DispatcherTimer _timer;

        public ReplayWindow()
        {
            InitializeComponent();
        }

        public void SetupPlayer(List<Bitmap> bitmaps, Rect recordSize, int fps)
        {
            _bitmaps = bitmaps;
            ReplayImage.Width = recordSize.Width;
            ReplayImage.Height = recordSize.Height;
            Height = recordSize.Height + 100;
            Width = recordSize.Width;

            _timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 1000/fps)
            };
            _timer.Tick+= TimerOnTick;
            PlayButton.Click+=PlayButtonOnClick;
            StopButton.Click+=StopButtonOnClick;
            converter= new BitmapToSourceConverter();
            ProgressSlider.Minimum = 0;
            ProgressSlider.Maximum = _bitmaps.Count-1;
            ProgressSlider.TickFrequency = 1;
            ProgressSlider.Value = 0;
            ProgressSlider.ValueChanged+=ProgressSliderOnValueChanged;
        }

        private void ProgressSliderOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            ReplayImage.Source = converter.Convert(_bitmaps.ElementAt(_pos), null, null, null) as BitmapImage;
            _pos = (int) ProgressSlider.Value;
        }

        private int _pos;

        private void StopButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            _pos = 0;
            _timer.Stop();
        }

        private void PlayButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            _pos = 0;
            _timer.Start();
        }

        private BitmapToSourceConverter converter;

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (_pos == _bitmaps.Count)
            {
                _timer.Stop();
                _pos = 0;
                return;
            }
            _pos++;

            ProgressSlider.Value = _pos;
        }

    }
}
