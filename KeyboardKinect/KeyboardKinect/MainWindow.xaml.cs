using Microsoft.Kinect;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System;

namespace KeyboardKinect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor _sensor = null;
        private ColorFrameReader _colorReader = null;
        private DepthFrameReader _depthReader = null;
        private List<ushort[]> calibrationFrameData = null;
        private List<Rectangle> keys;
        private bool calibrating = false;

        int frameWidth;
        int frameHeight;

        /// <summary>
        /// The main window of the app.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            frameWidth = 512;
            frameHeight = 424;

            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _depthReader = _sensor.DepthFrameSource.OpenReader();
                _depthReader.FrameArrived += DepthReader_FrameArrived;

                _sensor.Open();
            }
        }

        private void DepthReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            canvas.Children.Clear();

            using (DepthFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (calibrating)
                    {
                        
                        calibrating = false;
                    }
                    //camera.Source = frame.ToBitmap();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (_depthReader != null)
            {
                _depthReader.Dispose();
                _depthReader = null;
            }

            if (_colorReader != null)
            {
                _colorReader.Dispose();
                _colorReader = null;
            }

            if (_sensor != null)
            {
                _sensor.Close();
                _sensor = null;
            }
        }

        private void addKeyButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private bool hitDetected(ushort[] calib, ushort[] frame)
        {
            for(var i = 0; i<calib.Length; i++)
            {
                var difference = calib[i] - frame[i];
                if (difference < 10 && difference > 5)
                    return true;
            }
            return false;
        }
    }
}
