using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

namespace WebcamToggleApp
{
    public partial class Form1 : Form
    {
        private VideoCapture? _capture;
        private System.Windows.Forms.Timer _timer;
        private PictureBox cameraBox;

        public Form1()
        {
            InitializeComponent();
            InitializeUI(); // Initialize the UI components here

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 30; // Capture frame every 30ms (~33fps)
            _timer.Tick += new EventHandler(Timer_Tick);
        }

        // Initialize form elements like buttons and picture box
        private void InitializeUI()
        {
            // Ensure the form size is large enough to display the buttons and the camera box
            this.Size = new System.Drawing.Size(700, 600);
            this.Text = "Webcam Toggle App";  // Set form title

            // Start Camera Button
            Button startButton = new Button
            {
                Text = "Start Camera",
                Location = new System.Drawing.Point(20, 10), // Positioned at the top left
                Size = new System.Drawing.Size(100, 30) // Button size
            };
            startButton.Click += StartButton_Click;

            // Stop Camera Button
            Button stopButton = new Button
            {
                Text = "Stop Camera",
                Location = new System.Drawing.Point(140, 10), // Positioned next to the start button
                Size = new System.Drawing.Size(100, 30) // Button size
            };
            stopButton.Click += StopButton_Click;

            // PictureBox to display the camera feed
            cameraBox = new PictureBox
            {
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(640, 480), // Standard webcam resolution
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.Black // Set background color to ensure visibility
            };

            // Add the buttons and camera box to the form's Controls collection
            this.Controls.Add(startButton);
            this.Controls.Add(stopButton);
            this.Controls.Add(cameraBox);

            // Make sure the controls are properly refreshed
            this.PerformLayout();
        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            if (_capture == null)
            {
                _capture = new VideoCapture(0); // Use the first webcam
            }

            _timer.Start(); // Start capturing frames
        }

        private void StopButton_Click(object? sender, EventArgs e)
        {
            _timer.Stop(); // Stop capturing frames

            if (_capture != null)
            {
                _capture.Dispose(); // Release the camera
                _capture = null;
            }

            cameraBox.Image = null; // Clear the camera feed
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_capture != null && _capture.IsOpened)
            {
                Mat frame = _capture.QueryFrame();

                if (frame != null)
                {
                    Image<Bgr, Byte> imageFrame = frame.ToImage<Bgr, Byte>();
                    Bitmap bitmap = ImageToBitmap(imageFrame);

                    cameraBox.Image = bitmap; // Display the bitmap in the PictureBox
                }
            }
        }

        private Bitmap ImageToBitmap(Image<Bgr, Byte> image)
        {
            Bitmap bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            // Get the pointer to the image data and manually copy it to the bitmap
            IntPtr ptr = image.MIplImage.ImageData;
            int bytes = image.MIplImage.ImageSize;
            byte[] imageData = new byte[bytes];

            // Copy the image data into the byte array
            Marshal.Copy(ptr, imageData, 0, bytes);

            // Copy the byte array into the bitmap
            Marshal.Copy(imageData, 0, bmpData.Scan0, bytes);

            bitmap.UnlockBits(bmpData);
            return bitmap;
        }
    }
}
