using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;

namespace WebMRecorder
{
    public partial class MainWindow : Window
    {
        public int Fps { get; set; }
        public List<Bitmap> CaptureBitmaps;

        private ScreenAreaSelectorWindow _screenSelectionWindow;
        private DispatcherTimer _timer;
        private Rect _selectedArea;

        public MainWindow()
        {
            InitializeComponent();
            StartSelectArea.Click += StartSelectAreaOnClick;
        }
        

        private void StartSelectAreaOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            _screenSelectionWindow = new ScreenAreaSelectorWindow();
            _screenSelectionWindow.Show();
            _screenSelectionWindow.Closed += ScreenSelectionWindowOnClosed;
            Hide();
        }

        private void ScreenSelectionWindowOnClosed(object sender, EventArgs eventArgs)
        {
            _screenSelectionWindow.Closed -= ScreenSelectionWindowOnClosed;
            _selectedArea = _screenSelectionWindow.GetSelectionRectangle();
            var recordWindow = new RecordWindow();
            int fps;
            int.TryParse(FpsTextBox.Text, out fps);
            recordWindow.SetupRecordingWindow(_selectedArea);
            recordWindow.Fps = fps;
            recordWindow.Show();
        }

    }
}
