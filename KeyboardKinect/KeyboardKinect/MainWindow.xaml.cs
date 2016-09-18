using LightBuzz.Vitruvius;
using Microsoft.Kinect;
using Emgu.CV;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Emgu.CV.Structure;
using System;
using Emgu.Util;
using Emgu.CV.CvEnum;
using HandGestureRecognition.SkinDetector;

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

        IColorSkinDetector skinDetector;

        Image<Bgr, Byte> currentFrame;
        Image<Bgr, Byte> currentFrameCopy;

        AdaptiveSkinDetector detector;

        int frameWidth;
        int frameHeight;

        Hsv hsv_min;
        Hsv hsv_max;
        Ycc YCrCb_min;
        Ycc YCrCb_max;

        Seq<System.Drawing.Point> hull;
        Seq<System.Drawing.Point> filteredHull;

        System.Drawing.Rectangle handRect;
        MCvBox2D box;
        Emgu.CV.Structure.Ellipse ellip;

        /// <summary>
        /// The main window of the app.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            frameWidth = 512;
            frameHeight = 424;
            detector = new AdaptiveSkinDetector(1, AdaptiveSkinDetector.MorphingMethod.NONE);
            hsv_min = new Hsv(0, 45, 0);
            hsv_max = new Hsv(20, 255, 255);
            YCrCb_min = new Ycc(0, 131, 80);
            YCrCb_max = new Ycc(255, 185, 135);
            box = new MCvBox2D();
            ellip = new Emgu.CV.Structure.Ellipse();

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
                    var bitmap = frame.ToBitmap();
                    camera.Source = bitmap;
                    currentFrame = new Image<Bgr,Byte>(BitmapFromWriteableBitmap(bitmap));
                    currentFrameCopy = currentFrame.Copy();
          
                    skinDetector = new YCrCbSkinDetector();

                    Image<Gray, Byte> skin = skinDetector.DetectSkin(currentFrameCopy, YCrCb_min, YCrCb_max);
                    ExtractContourAndHull(skin);
                }
            }
        }

        private void ExtractContourAndHull(Image<Gray, byte> skin)
        {
            using (MemStorage storage = new MemStorage())
            {

                Contour<System.Drawing.Point> contours = skin.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage);
                Contour<System.Drawing.Point> biggestContour = null;

                Double Result1 = 0;
                Double Result2 = 0;
                while (contours != null)
                {
                    Result1 = contours.Area;
                    if (Result1 > Result2)
                    {
                        Result2 = Result1;
                        biggestContour = contours;
                    }
                    contours = contours.HNext;
                }

                if (biggestContour != null)
                {
                    Contour<System.Drawing.Point> currentContour = biggestContour.ApproxPoly(biggestContour.Perimeter * 0.0025, storage);
                    biggestContour = currentContour;


                    hull = biggestContour.GetConvexHull(ORIENTATION.CV_CLOCKWISE);
                    box = biggestContour.GetMinAreaRect();
                    System.Drawing.PointF[] points = box.GetVertices();

                    System.Drawing.Point[] ps = new System.Drawing.Point[points.Length];
                    for (int i = 0; i < points.Length; i++)
                        ps[i] = new System.Drawing.Point((int)points[i].X, (int)points[i].Y);

                    filteredHull = new Seq<System.Drawing.Point>(storage);
                    for (int i = 0; i < hull.Total; i++)
                    {
                        if (Math.Sqrt(Math.Pow(hull[i].X - hull[i + 1].X, 2) + Math.Pow(hull[i].Y - hull[i + 1].Y, 2)) > box.size.Width / 10)
                        {
                            filteredHull.Push(hull[i]);
                        }
                    }
                }
            }
        }

        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
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

        private void addKeyButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
