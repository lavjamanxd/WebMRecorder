﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Frapper;
using WebMRecorder.Properties;
using Size = System.Drawing.Size;

namespace WebMRecorder
{
    /// <summary>
    /// Interaction logic for RecordWindow.xaml
    /// </summary>
    public partial class RecordWindow : Window, IDisposable
    {
        private DispatcherTimer _timer;
        private Rect _selectedArea;
        private TranslateTransform _controlGridRenderTransform;


        public List<Bitmap> CaptureBitmaps { get; set; }

        public int Fps { get; set; }

        public RecordWindow()
        {
            InitializeComponent();

            Height = SystemParameters.PrimaryScreenHeight;
            Width = SystemParameters.PrimaryScreenWidth;
            Left = 0;
            Top = 0;

            Deactivated += OnDeactivated;
        }

        private void OnDeactivated(object sender, EventArgs eventArgs)
        {
            Topmost = true;
        }

        public void SetupRecordingWindow(Rect selectedArea)
        {
            _selectedArea = selectedArea;
            RecordingAreaIndicator.SetValue(LeftProperty, selectedArea.X-2);
            RecordingAreaIndicator.SetValue(TopProperty, selectedArea.Y-2);
            RecordingAreaIndicator.Width = selectedArea.Width+4;
            RecordingAreaIndicator.Height = selectedArea.Height+4;

            StartButton.Click += StartButtonOnClick;
            StopButton.Click += StopButtonOnClick;
            ReplayButton.Click+=ReplayButtonOnClick;
            _controlGridRenderTransform = new TranslateTransform();

            ControlGrid.RenderTransform = _controlGridRenderTransform;

            _controlGridRenderTransform.X = selectedArea.X-2 + selectedArea.Width+4;
            _controlGridRenderTransform.Y = selectedArea.Y-2 + selectedArea.Height+2 - ControlGrid.ActualHeight;

            Fps = 25;

            _timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 1000 / Fps)
            };

            _timer.Tick += CaptureOnTick;
            CaptureBitmaps = new List<Bitmap>();
        }

        private void ReplayButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Hide();
            var replayWindow = new ReplayWindow();
            replayWindow.SetupPlayer(CaptureBitmaps, _selectedArea, Fps);
            replayWindow.Show();
        }

        private int _frames = 0;
        private void CaptureOnTick(object sender, EventArgs eventArgs)
        {
            CaptureBitmaps.Add(CaptureImage(_selectedArea));
            _frames++;
            CapturedFramesTextBlock.Text = _frames.ToString();
        }

        private void Convert()
        {
            var path = Directory.GetCurrentDirectory() + "\\ffmpeg.exe";
            var ffmpeg = new FFMPEG(path);

            ffmpeg.RunCommand(
                $"-framerate {Fps} -i img%03d.png -vf fps={Fps} -c:v libvpx -b:v 1M output.webm");

            foreach (var file in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.png"))
            {
                File.Delete(file);
            }
            
            Application.Current.Dispatcher.Invoke(() => ConvertingStackPanel.Visibility = Visibility.Collapsed,
                DispatcherPriority.Normal);
        }

        private Bitmap CaptureImage(Rect rectangle)
        {
            var target = new Bitmap((int)rectangle.Width, (int)rectangle.Height);

            using (var g = Graphics.FromImage(target))
            {
                g.CopyFromScreen((int)rectangle.X, (int)rectangle.Y, 0, 0, new Size((int)rectangle.Width, (int)rectangle.Height));
            }
            return target;
        }

        private async void StopButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ConvertingStackPanel.Visibility = Visibility.Visible;
            ReplayButton.IsEnabled = true;

            _timer.Stop();

            foreach (var bitmap in CaptureBitmaps)
            {
                var filename = "img" + $"{CaptureBitmaps.IndexOf(bitmap):000}" + ".png";
                bitmap.Save(filename);
            }


            await Task.Run(()=> Convert());

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private void StartButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var oldfile = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.webm").FirstOrDefault();
            if (oldfile != null)
            {
                File.Delete(oldfile);
            }

            _timer.Start();
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
        }

        public void Dispose()
        {
            Deactivated -= OnDeactivated;
        }
    }
}
