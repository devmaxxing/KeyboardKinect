using LightBuzz.Vitruvius;
using Microsoft.Kinect;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        private ushort[] calibrationFrameData = null;
        private Rect keyboardArea = Rect.Empty;
        private bool calibrating = false;

        /// <summary>
        /// The main window of the app.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _depthReader = _sensor.DepthFrameSource.OpenReader();
                _depthReader.FrameArrived += DepthReader_FrameArrived;

                _colorReader = _sensor.ColorFrameSource.OpenReader();
                _colorReader.FrameArrived += ColorReader_FrameArrived;

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
                        frame.CopyFrameDataToArray(calibrationFrameData);
                        calibrating = false;
                    }
                    //camera.Source = frame.ToBitmap();
                }
            }
        }

        private void ColorReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap();
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

        private void setDepthButton_click(object sender, RoutedEventArgs e)
        {
            calibrating = true;
        }
    }
}
