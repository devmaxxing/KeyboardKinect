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

    public partial class MainWindow : Window
    {
        private List<Key> keys;

        private bool calibrating;
        private bool detecting;

        int frameWidth;
        int frameHeight;

        private ushort[] calib;

        private KinectSensor _sensor;
        private DepthFrameReader _depthReader;
        
        public MainWindow()
        {
            InitializeComponent();

            keys = new List<Key>();

            calibrating = false;
            detecting = false;

            frameWidth = 512;
            frameHeight = 424;

            calib = new ushort[frameWidth * frameHeight];

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
            using (DepthFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (calibrating)
                    {
                        frame.CopyFrameDataToArray(calib);
                        calibrating = false;
                    }
                    if (detecting)
                    {

                    }else
                    {
                        camera.Source = ToBitmap(frame);
                    }                    
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

            if (_sensor != null)
            {
                _sensor.Close();
                _sensor = null;
            }
        }

        private void addKeyButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private List<int> getPixelIndices(Rectangle r)
        {
            List<int> keyPixels = new List<int>();
            for(var x = 0; x<frameWidth; x++)
            {
                for(var y = 0; y<frameWidth; y++)
                {
                    HitTestResult result = VisualTreeHelper.HitTest(canvas, new Point(x, y));
                    if (result != null)
                    {
                        keyPixels.Add(y * frameWidth + x);
                    }
                }                
            }
            return keyPixels;
        }

        private ImageSource ToBitmap(DepthFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;

            ushort minDepth = frame.DepthMinReliableDistance;
            ushort maxDepth = frame.DepthMaxReliableDistance;

            ushort[] depthData = new ushort[width * height];
            byte[] pixelData = new byte[width * height * (PixelFormats.Bgr32.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(depthData);

            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < depthData.Length; ++depthIndex)
            {
                ushort depth = depthData[depthIndex];
                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                pixelData[colorIndex++] = intensity; // Blue
                pixelData[colorIndex++] = intensity; // Green
                pixelData[colorIndex++] = intensity; // Red

                ++colorIndex;
            }

            int stride = width * PixelFormats.Bgr32.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixelData, stride);
        }

        private bool hitDetected(ushort[] keyPixels, ushort[] frame)
        {
            foreach(int i in keyPixels)
            {
                var difference = calib[i] - frame[i];
                if (difference < 20 && difference > 10)
                    return true;
            }
            return false;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            detecting = !detecting;
            if (detecting)
            {
                camera.Source = null;
            }
        }
    }
}
