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
        private InfraredFrameReader _infraredReader = null;
        private DepthFrameReader _depthReader = null;

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

                _infraredReader = _sensor.InfraredFrameSource.OpenReader();
                _infraredReader.FrameArrived += InfraredReader_FrameArrived;

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
                    camera.Source = frame.ToBitmap();
                }
            }
        }

        private void InfraredReader_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
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

            if (_infraredReader != null)
            {
                _infraredReader.Dispose();
                _infraredReader = null;
            }

            if (_sensor != null)
            {
                _sensor.Close();
                _sensor = null;
            }
        }
    }
}
