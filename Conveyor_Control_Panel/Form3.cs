using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using OpenCvSharp;  // Library of OpenCVSharp
using sl;           // Library of Zed camera


namespace Conveyor_Control_Panel
{
    public partial class Form3 : Form
    {
        private Camera zedCamera;
        private RuntimeParameters runtimeParams;
        private bool isRunning = true;

        private long previousPosition;
        private long currentPosition = 0;


        private float pixelSize = 22.6f;         // Size Camera's pixel (µm)
        private float stepSize = 11.3f;         // Distance of a Encoder pulse (µm)

        private const float baseSpeed = 3.089f;  // Based speed (cm/s)
        private const int baseColumns = 1950;    // Column limit is equivalent to resolution of monitor
        private int currentMaxColumns;           // Dynamic column limit based on speed
        private float K = 0.00251f;               // Adjustment factor
        private bool needsReset = false;         // Flag to mark to reset image
        private float currentSpeed;              // Refer to Form1 to access currentSpeed

        public Form3(Camera camera, RuntimeParameters runtimeParams)
        {
            InitializeComponent();
            this.zedCamera = camera;
            this.runtimeParams = runtimeParams;
            currentMaxColumns = baseColumns;
        }

        // Methods to handle position and speed updates
        public void UpdatePosition(long newPosition)
        {
            previousPosition = currentPosition;
            currentPosition = newPosition;
        }

        public void UpdateSpeed(float newSpeed)
        {
            float oldMaxColumns = currentMaxColumns;
            currentSpeed = newSpeed;

            // Apply the formula to calculate the columns for line scan images
            currentMaxColumns = (int)(baseColumns * (baseSpeed / currentSpeed) * (1 + K * (currentSpeed - baseSpeed)));
            Console.WriteLine($"Mc: {currentMaxColumns}");
                
            // Ensure have a reasonable minimum number of columns
            currentMaxColumns = Math.Max(currentMaxColumns, 100);

            // Nếu số cột thay đổi đáng kể (ví dụ: thay đổi > 1%), đánh dấu cần reset ảnh
            if (Math.Abs(oldMaxColumns - currentMaxColumns) > 0.01 * oldMaxColumns)
            {
                needsReset = true;
            }
        }

        public void StartDisplay2()
        {
            string winName = "Line Scan";
            Cv2.NamedWindow(winName, WindowFlags.Normal);

            // Initialize variables for capturing columns
            OpenCvSharp.Mat lineScanImage = new OpenCvSharp.Mat(); // To store the constructed 2D line scan image

            while (isRunning)
            {
                // Grab the next frame
                ERROR_CODE err = zedCamera.Grab(ref runtimeParams);

                if (err == ERROR_CODE.SUCCESS)
                {
                    // Kiểm tra nếu cần reset ảnh
                    if (needsReset)
                    {
                        if (lineScanImage != null && !lineScanImage.Empty())
                        {
                            lineScanImage.Dispose();
                            lineScanImage = new OpenCvSharp.Mat();
                        }
                        needsReset = false;
                    }

                    // Tính khoảng cách đã di chuyển từ encoder (µm)
                    float distanceMoved = Math.Abs((float)(currentPosition - previousPosition)) * stepSize;
                    
                    // If both are equal, create Line Image and assemble
                    if (distanceMoved >= pixelSize)
                    {
                        // Retrieve ZED image
                        sl.Mat rightImage = new sl.Mat();
                        rightImage.Create(new Resolution((uint)zedCamera.ImageWidth, (uint)zedCamera.ImageHeight), MAT_TYPE.MAT_8U_C4);
                        zedCamera.RetrieveImage(rightImage, VIEW.RIGHT);

                        //Get data from zedimage
                        IntPtr dataPtr = rightImage.GetPtr();
                        int width = rightImage.GetWidth();
                        int height = rightImage.GetHeight();

                        // Convert sl.Mat to OpenCvSharp.Mat
                        OpenCvSharp.Mat currentFrame = OpenCvSharp.Mat.FromPixelData(height, width, OpenCvSharp.MatType.CV_8UC4, dataPtr);

                        // Extract the middle column and clone immediately to a new Mat
                        var middleColumn = new OpenCvSharp.Mat();

                        currentFrame.Col(width / 2).CopyTo(middleColumn); // Copy data instead of referencing

                        // Check the grafted column number
                        int currentCols = lineScanImage.Empty() ? 0 : lineScanImage.Cols;

                        // Giải phóng tài nguyên không cần thiết
                        currentFrame.Dispose();

                        rightImage.Free(sl.MEM.CPU);

                        // Put the columns together
                        if (lineScanImage.Empty())
                        {
                            lineScanImage = middleColumn.Clone();
                        }
                        else if (lineScanImage.Cols < currentMaxColumns)
                        {
                            //Create a new empty Mat to contain the image
                            OpenCvSharp.Mat newLineScanImage = new OpenCvSharp.Mat();

                            // Merge middleColumn into lineScanImage horizontally using HConcat.
                            // lineScanImage: Large image containing multiple previously merged columns.
                            // middleColumn: New image column to add.
                            // newLineScanImage: Result image after merging
                            Cv2.HConcat(lineScanImage, middleColumn, newLineScanImage);

                            // Free previous lineScanImage before update
                            // If not, it will increase until the memory overflows
                            lineScanImage.Dispose();
                            lineScanImage = newLineScanImage;
                        }
                        else
                        {
                            // Can put more functions as saved images here

                            // Free the process memory when using camera
                            // If not, it will increase until the memory overflows
                            lineScanImage.Dispose();

                            // Reset the image
                            lineScanImage = middleColumn.Clone();
                        }
                        middleColumn.Dispose();
                        
                        // Cập nhật lại encoder để chuẩn cho lần chụp tiếp theo
                        previousPosition = currentPosition;
                    }

                    // Only attempt to display if lineScanImage is not null and has data
                    if (lineScanImage != null && !lineScanImage.Empty())
                    {
                        // Display the constructed line scan image
                        Cv2.ImShow(winName, lineScanImage);
                    }

                    // The code below is used to capure image with the other camera
                    // Check if the capture interval has passed (10 seconds)
                    /*DateTime startTime = DateTime.Now;
                    if ((DateTime.Now - startTime).TotalMilliseconds > CaptureInterval)
                    {
                        // Retrieve the left camera image for reference
                        sl.Mat leftImage = new sl.Mat();
                        leftImage.Create(new Resolution((uint)zedCamera.ImageWidth, (uint)zedCamera.ImageHeight), MAT_TYPE.MAT_8U_C4);
                        zedCamera.RetrieveImage(leftImage, VIEW.LEFT);

                        // Get data from zedimage
                        IntPtr dataPtr1 = leftImage.GetPtr();
                        int width1 = leftImage.GetWidth();
                        int height1 = leftImage.GetHeight();

                        OpenCvSharp.Mat leftImageCv = OpenCvSharp.Mat.FromPixelData(height1, width1, OpenCvSharp.MatType.CV_8UC4, dataPtr1);

                        // Combine line scan image and left image
                        OpenCvSharp.Mat combinedImage = new OpenCvSharp.Mat();
                        Cv2.HConcat(new OpenCvSharp.Mat[] { lineScanImage, leftImageCv }, combinedImage);

                        // Check and draw rectangles (if any)
                        if (!(selectionRect.width == 0))
                        {
                            Cv2.Rectangle(leftImageCv, new OpenCvSharp.Rect(selectionRect.x, selectionRect.y, selectionRect.width, selectionRect.height), new OpenCvSharp.Scalar(220, 180, 20), 2);
                       }

                        // Display the combined image
                        Cv2.ImShow("Combined Image", combinedImage);

                        // Reset the timer
                        startTime = DateTime.Now;
                    }*/

                    // Exit the loop if 'q' is pressed
                    int key = Cv2.WaitKey(1);
                    if (key == 'q' || Cv2.GetWindowProperty(winName, WindowPropertyFlags.Visible) < 1)
                    {
                        isRunning = false; // Exit the loop
                        Thread.Sleep(10);
                    }
                }
            }

            // Release resources
            zedCamera.DisableObjectDetection();
            zedCamera.Close();
            lineScanImage?.Dispose();
            Cv2.DestroyAllWindows();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isRunning = false;
            base.OnFormClosing(e);
        }
    }
}
