using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;  // Library of OpenCVSharp
using sl;           // Library of Zed camera
using ScottPlot;    // Library of 3D Drawing
using System.Drawing.Drawing2D;
using ScottPlot.WinForms;

namespace Conveyor_Control_Panel
{
    public partial class Form2 : Form
    {
        private Camera zedCamera;
        private RuntimeParameters runtimeParams;
        private ObjectDetectionRuntimeParameters objRuntimeParams;
        private bool isRunning = true; // Marking the state is running
        private Form plotForm = null;

        // Declare variables
        sl.Mat zedImage = null;
        //OpenCvSharp.Mat cvImage = null;
        OpenCvSharp.Mat cvImageCopy = null;
        OpenCvSharp.Mat lastGrayImage = null;

        public Form2(Camera camera, RuntimeParameters runtimeParams, ObjectDetectionRuntimeParameters objRuntimeParams)
        {
            InitializeComponent();
            this.zedCamera = camera;
            this.runtimeParams = runtimeParams;
            this.objRuntimeParams = objRuntimeParams;
        }

        private void ShowPeakDetectionPlot(OpenCvSharp.Mat grayImage)
        {
            // Create a copy to work with
            OpenCvSharp.Mat workingCopy = grayImage.Clone();

            // Apply heavy Gaussian smoothing
            OpenCvSharp.Mat blurred = new OpenCvSharp.Mat();
            var blurKernel = new OpenCvSharp.Size(51, 51);
            Cv2.GaussianBlur(workingCopy, blurred, blurKernel, 0);

            // Find the first peak (global maximum) in the blurred image
            OpenCvSharp.Point peak1Index;
            double maxVal, minVal;
            OpenCvSharp.Point minLoc;
            Cv2.MinMaxLoc(blurred, out minVal, out maxVal, out minLoc, out peak1Index);

            // Find the second peak
            OpenCvSharp.Mat blurredCopy = blurred.Clone();
            int maskRadius = 5;
            int y1 = Math.Max(0, peak1Index.Y - maskRadius);
            int y2 = Math.Min(blurredCopy.Height, peak1Index.Y + maskRadius + 1);
            int x1 = Math.Max(0, peak1Index.X - maskRadius);
            int x2 = Math.Min(blurredCopy.Width, peak1Index.X + maskRadius + 1);

            // Create rectangle for mask area
            OpenCvSharp.Rect maskArea = new OpenCvSharp.Rect(x1, y1, x2 - x1, y2 - y1);
            OpenCvSharp.Mat roi = new OpenCvSharp.Mat(blurredCopy, maskArea);
            roi.SetTo(new Scalar(-1));

            // Find second peak
            OpenCvSharp.Point peak2Index;
            Cv2.MinMaxLoc(blurredCopy, out minVal, out maxVal, out minLoc, out peak2Index);

            // Compute PSR for both peaks
            double psr1 = ComputePSR(blurred, peak1Index);
            double psr2 = ComputePSR(blurred, peak2Index);
            double avgPsr = (psr1 + psr2) / 2.0;
            string psrText = $"Avg PSR: {avgPsr:F2}";

            // Create a colored version of the image for display
            OpenCvSharp.Mat coloredImage = new OpenCvSharp.Mat();
            Cv2.CvtColor(workingCopy, coloredImage, ColorConversionCodes.GRAY2BGR);

            // Draw red circles around the peaks and annotate
            int circleRadius = 10;
            Cv2.Circle(coloredImage, peak1Index, circleRadius, new Scalar(0, 0, 255), 2);
            Cv2.Circle(coloredImage, peak2Index, circleRadius, new Scalar(0, 0, 255), 2);
            Cv2.PutText(coloredImage, psrText, new OpenCvSharp.Point(10, 30), HersheyFonts.HersheySimplex, 1, new Scalar(0, 0, 255), 2);

            // Create or reuse the plot form
            if (plotForm == null || plotForm.IsDisposed)
            {
                plotForm = new Form
                {
                    Text = "Peak Detection Results",
                    Size = new System.Drawing.Size(1500, 800),
                    StartPosition = FormStartPosition.CenterScreen
                };

                // Make the form non-modal
                plotForm.FormBorderStyle = FormBorderStyle.Sizable;
                plotForm.ShowInTaskbar = true;
            }
            else
            {
                plotForm.Controls.Clear();
            }

            // Create top panel for the image
            Panel imagePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = plotForm.Height / 2
            };
            plotForm.Controls.Add(imagePanel);

            // Add image to panel
            Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(coloredImage);
            PictureBox pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = bmp
            };
            imagePanel.Controls.Add(pictureBox);

            // Create bottom panel for the 3D surface plot
            Panel plotPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            plotForm.Controls.Add(plotPanel);

            // Create ScottPlot control
            var plotControl = new ScottPlot.WinForms.FormsPlot()
            {
                Dock = DockStyle.Fill
            };
            plotPanel.Controls.Add(plotControl);

            // Convert blurred Mat to double array for heatmap
            double[,] intensityData = new double[blurred.Height, blurred.Width];
            for (int y = 0; y < blurred.Height; y++)
            {
                for (int x = 0; x < blurred.Width; x++)
                {
                    intensityData[y, x] = blurred.At<byte>(y, x);
                }
            }

            // Add heatmap to plot
            var heatmap = plotControl.Plot.Add.Heatmap(intensityData);
            plotControl.Plot.Add.ColorBar(heatmap);
            plotControl.Plot.Title("Intensity Heatmap (Gaussian Blurred)");
            plotControl.Plot.XLabel("X Coordinate");
            plotControl.Plot.YLabel("Y Coordinate");

            // Mark peaks with scatter points
            var scatter = plotControl.Plot.Add.Scatter(
                new double[] { peak1Index.X, peak2Index.X },
                new double[] { peak1Index.Y, peak2Index.Y }
            );
            scatter.Color = ScottPlot.Colors.Red;
            scatter.LegendText = "Peaks";
            scatter.MarkerSize = 10;
            scatter.MarkerShape = ScottPlot.MarkerShape.FilledCircle;
                
            // Add PSR text annotation
            plotControl.Plot.Add.Text(psrText, 10, 10);

            // Configure axis limits and invert Y axis (image coordinates)
            plotControl.Plot.Axes.SetLimits(0, blurred.Width, blurred.Height, 0);

            // Enable anti-aliasing (different in ScottPlot 5.x)
            plotControl.Plot.Axes.AntiAlias(true);

            // Add legend
            plotControl.Plot.Legend.IsVisible = true;
            plotControl.Plot.Legend.Alignment = ScottPlot.Alignment.UpperRight;

            // Refresh the plot
            plotControl.Refresh();

            // Show the form if it's not already visible
            if (!plotForm.Visible)
            {
                plotForm.Show(this);
            }
            else
            {
                plotForm.BringToFront();
            }

            // Clean up
            workingCopy.Dispose();
            blurred.Dispose();
            blurredCopy.Dispose();
            coloredImage.Dispose();
        }

        static double ComputePSR(OpenCvSharp.Mat gray, OpenCvSharp.Point peak, int windowSize = 11, int centerSize = 3)
        {
            int y = peak.Y;
            int x = peak.X;
            int halfWindow = windowSize / 2;
            int halfCenter = centerSize / 2;
            int h = gray.Height;
            int w = gray.Width;

            // Compute window boundaries
            int y1 = Math.Max(0, y - halfWindow);
            int y2 = Math.Min(h, y + halfWindow + 1);
            int x1 = Math.Max(0, x - halfWindow);
            int x2 = Math.Min(w, x + halfWindow + 1);

            OpenCvSharp.Mat window = new OpenCvSharp.Mat(gray, new OpenCvSharp.Rect(x1, y1, x2 - x1, y2 - y1));
            OpenCvSharp.Mat windowFloat = new OpenCvSharp.Mat();
            window.ConvertTo(windowFloat, MatType.CV_32F);

            // Define the center region boundaries relative to the window
            int centerY1 = Math.Max(0, (y - halfCenter) - y1);
            int centerY2 = Math.Min(window.Height, (y + halfCenter + 1) - y1);
            int centerX1 = Math.Max(0, (x - halfCenter) - x1);
            int centerX2 = Math.Min(window.Width, (x + halfCenter + 1) - x1);

            // Create mask for sidelobe
            OpenCvSharp.Mat mask = OpenCvSharp.Mat.Ones(window.Size(), MatType.CV_8U);
            OpenCvSharp.Rect centerRect = new OpenCvSharp.Rect(centerX1, centerY1, centerX2 - centerX1, centerY2 - centerY1);

            if (centerRect.X >= 0 && centerRect.Y >= 0 &&
                centerRect.X + centerRect.Width <= mask.Width &&
                centerRect.Y + centerRect.Height <= mask.Height)
            {
                OpenCvSharp.Mat centerRoi = new OpenCvSharp.Mat(mask, centerRect);
                centerRoi.SetTo(new Scalar(0));
            }

            // Apply mask and get sidelobe values
            OpenCvSharp.Mat sidelobe = new OpenCvSharp.Mat();
            Cv2.BitwiseAnd(windowFloat, windowFloat, sidelobe, mask);

            // Calculate statistics only on non-zero elements
            Scalar mean, stddev;
            Cv2.MeanStdDev(sidelobe, out mean, out stddev, mask);

            if (stddev[0] == 0)
                return 0.0;

            float peakValue = gray.At<byte>(y, x);
            double psr = (peakValue - mean[0]) / stddev[0];

            // Clean up
            window.Dispose();
            windowFloat.Dispose();
            mask.Dispose();
            sidelobe.Dispose();

            return psr;
        }

        public void StartDisplay()
        {
            string winName = "Area Scanning";
            Cv2.NamedWindow(winName, WindowFlags.Normal);

            while (isRunning)
            {
                // Grab the next frame
                ERROR_CODE err = zedCamera.Grab(ref runtimeParams);

                if (err == ERROR_CODE.SUCCESS)
                {
                    // Retrieve ZED image
                    zedImage = new sl.Mat();
                    zedImage.Create(new Resolution((uint)zedCamera.ImageWidth, (uint)zedCamera.ImageHeight), MAT_TYPE.MAT_8U_C4);
                    zedCamera.RetrieveImage(zedImage, VIEW.LEFT);

                    // Get data from zedimage
                    IntPtr dataPtr = zedImage.GetPtr();
                    int width = zedImage.GetWidth();
                    int height = zedImage.GetHeight();

                    // Create a byte array of the appropriate size for the image data
                    byte[] imageData = new byte[height * width * 4]; // 4 bytes per pixel (for CV_8UC4)

                    // Copy data from IntPtr to byte array
                    Marshal.Copy(dataPtr, imageData, 0, imageData.Length);

                    // Make sure cvImageCopy is initialized
                    if (cvImageCopy == null)
                    {
                        cvImageCopy = new OpenCvSharp.Mat(height, width, MatType.CV_8UC4);
                    }

                    // Update data into cvImageCopy
                    Marshal.Copy(imageData, 0, cvImageCopy.Data, imageData.Length);

                    // Store the grayscale image for peak detection
                    if (lastGrayImage == null)
                    {
                        lastGrayImage = new OpenCvSharp.Mat();
                    }
                    Cv2.CvtColor(cvImageCopy, lastGrayImage, ColorConversionCodes.BGRA2GRAY);

                    // Retrieve object detection data
                    Objects objects = new Objects();
                    ERROR_CODE detectionErr = zedCamera.RetrieveObjects(ref objects, ref objRuntimeParams);

                    if (detectionErr == ERROR_CODE.SUCCESS)
                    {
                        // Display texts
                        string Text1 = $"Press C to Show brightness peak detection";
                        string Text2 = $"Press Q to exit"; 
                        Cv2.PutText(cvImageCopy, Text1, new OpenCvSharp.Point(10, 30), HersheyFonts.HersheySimplex, 1.0, Scalar.Green, 2);
                        Cv2.PutText(cvImageCopy, Text2, new OpenCvSharp.Point(10, 70), HersheyFonts.HersheySimplex, 1.0, Scalar.Green, 2);

                        // Display object count
                        string objectCountText = $"Detected Objects: {objects.numObject}";
                        Cv2.PutText(cvImageCopy, objectCountText, new OpenCvSharp.Point(10, 110), HersheyFonts.HersheySimplex, 1.0, Scalar.Green, 2);

                        // Display positions of detected objects
                        for (int i = 0; i < objects.numObject; i++)
                        {
                            var objectData = objects.objectData[i]; // Retrieve object data
                            var position = objectData.position;    // Replace this with actual position
                            string positionText = $"Object {i + 1}: Position = ({position.X:F2}, {position.Y:F2}, {position.Z:F2})";
                            Cv2.PutText(cvImageCopy, positionText, new OpenCvSharp.Point(10, 150 + i * 40), HersheyFonts.HersheySimplex, 0.8, Scalar.White, 2);
                        }
                    }

                    // Show the image
                    Cv2.ImShow(winName, cvImageCopy);

                    // Free the process memory when using camera
                    zedImage.Free(sl.MEM.CPU);

                    // Wait for a short interval and check for key presses
                    int key = Cv2.WaitKey(1);
                    if (key == 'q' || Cv2.GetWindowProperty(winName, WindowPropertyFlags.Visible) < 1)
                    {
                        isRunning = false; // Exit the loop
                    }
                    else if (key == 'c')
                    {
                        // Show peak detection plot when 'C' is pressed
                        if (lastGrayImage != null)
                        {
                            ShowPeakDetectionPlot(lastGrayImage);
                        }
                    }

                    Thread.Sleep(10);
                }
            }

            // Release the camera and close the OpenCV window
            if (cvImageCopy != null) cvImageCopy.Dispose();
            if (lastGrayImage != null) lastGrayImage.Dispose();
            if (plotForm != null && !plotForm.IsDisposed) plotForm.Close();

            zedCamera.DisableObjectDetection();
            zedCamera.DisablePositionalTracking("");
            zedCamera.Close();
            Cv2.DestroyAllWindows();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isRunning = false;
            base.OnFormClosing(e);
        }
    }
}