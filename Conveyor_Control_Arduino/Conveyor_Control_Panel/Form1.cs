using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using System.Collections.Concurrent;
using sl; // Library of Zed camera
using OpenCvSharp; // Library of OpenCVSharp


namespace Conveyor_Control_Panel
{
    public partial class Form1 : Form
    {
        // ARDUINO OUTPUT VARIABLES
        // These variables handle data communication with Arduino
        private float previousValue = 0.00f; // Stores last valid frequency value
        string serialDataIn;                 // Raw serial data from Arduino 
        private bool isFirstRead = true;     // Flag for initial reading delay

        // SYSTEM OUTPUT VARIABLES
        // Stores real-time sensor data
        private float outputFrequency = 0f;
        private float outputVoltage = 0f;
        private float temperature = 0f;
        private float vibration = 0f;

        // ZED CAMERA VARIABLES
        // Handles Area scan and Line scan functionality
        private Camera zedCamera; // Main camera object
        private ObjectDetectionRuntimeParameters objRuntimeParams; // Camera parameters

        // LINE CHART VARIABLES
        // Used for data visualization
        private Random random = new Random();
        private List<(double Vibration, double Speed)> dataList = new List<(double, double)>();

        // ENCODER VARIABLES
        // Manages position tracking and timing
        private const int UpdateInterval = 1000; // Every microSeconds for sending and receiving data of Encoder
        private bool isUpdatingPosition = false;
        

        // THREAD-SAFE POSITION TRACKING
        // Ensures reliable position data across threads for line scan function
        private ConcurrentQueue<long> positionUpdateQueue = new ConcurrentQueue<long>();
        private volatile bool isPositionUpdateRunning = false;

        // EVENT HANDLING FOR LINE SCANNING
        // Enables communication between forms for line scanning
        public delegate void PositionUpdateHandler(long newPosition);
        public event PositionUpdateHandler PositionUpdateEvent;

        public delegate void SpeedUpdateHandler(float newSpeed);
        public event SpeedUpdateHandler SpeedUpdateEvent;
        private float currentSpeed;

        // MECHANICAL CONSTANTS
        // Physical parameters for speed calculation
        private const float D_roller = 5.9f; // Roller diameter (cm)
        private const float P_motor = 2f;   //  Motor poles
        private const float G_ratio = 30f;  // Gear ratio
        private float MechanicalSpeed = 0f; // Calculated speed

        // ENCODER CALCULATION CONSTANTS
        // Parameters for precise speed measurement
        private const float PI = (float)Math.PI;
        private const int PPR = 16384;              // Pulses Per Revolution (14-bit)
        private const int UPDATE_INTERVAL_MS = 10;  // Update interval (ms)
        private long previousTimeMs;                // Timing variable
        private ushort previousPosition;            // Previous encoder position
        private Queue<float> speedSamples = new Queue<float>(); // Speed averaging
        private const int SAMPLE_COUNT = 10; // Number of samples for averaging

        // Fan status variable
        private bool fanStatus = false;

        // ERROR HANDLING VARIABLES
        // Manages communication errors and recovery
        private int consecutiveErrors = 0;
        private const int ERROR_WINDOW_SIZE = 10;
        private const int MAX_RETRY = 3;
        private const double ERROR_THRESHOLD = 0.75;
        private Queue<bool> errorHistory = new Queue<bool>();
        private bool isResetting = false;

        //Model to stop the motor for using disconnect or out the ui
        private bool StopStatus = true;

        // Form constructor - Initializes the application
        public Form1()
        {
            InitializeComponent();
            InitializeChart(); // Setup data visualization

            // Initialize timing variables for speed calculation
            previousTimeMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            previousPosition = 0;
        }

        // Form load event handler - Sets initial UI state
        private void Form1_Load(object sender, EventArgs e)
        {
            button_Connect.Enabled = true;
            button_Disconnect.Enabled = false;
            comboBox_BaudRate.Text = "115200";
        }

        // Handles connection button click event.
        // Establishes serial communication with Arduino and encoder.
        private void Button_Connect_Click(object sender, EventArgs e)
        {
            try
            {
                // Open the serial port for communication
                serialPort1.PortName = ComboBox_ComPort.Text;
                serialPort1.BaudRate = Convert.ToInt32(comboBox_BaudRate.Text);
                serialPort1.DataBits = 8;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Parity = Parity.None;
                serialPort1.Open();

                // Open the serial port for communication
                serialPort2.PortName = ComboBox_ComPort2.Text;
                serialPort2.BaudRate = 2000000;
                serialPort2.DataBits = 8;
                serialPort2.StopBits = StopBits.One;
                serialPort2.Parity = Parity.None;
                serialPort2.Open();

                // Update UI state
                button_Connect.Enabled = false;
                button_Disconnect.Enabled = true;
                isFirstRead = true;
                progressBar1.Value = 100;
                speedSamples.Clear();

                // Start continuous position update for the line scan function of camera
                isUpdatingPosition = true;
                Thread positionUpdateThread = new Thread(UpdatePositionContinuously);
                positionUpdateThread.IsBackground = true;
                positionUpdateThread.Start();
            }
            catch(Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void Button_Disconnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (StopStatus == true)
                {
                    if (serialPort1.IsOpen)
                    {
                        DialogResult result = MessageBox.Show(
                            "Do you want to close the Serial ports?",
                            "Confirm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        if (result == DialogResult.Yes)
                        {
                            // Reset speed calculation variables
                            previousTimeMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            previousPosition = 0;

                            // Stop all background processes
                            isUpdatingPosition = false;      // Stop position update thread
                            isPositionUpdateRunning = false; // Stop position broadcasting
                            isFirstRead = true;              // Reset first read flag

                            // Clear any remaining position updates in the queue
                            while (positionUpdateQueue.TryDequeue(out _)) { }
                            speedSamples.Clear();

                            // Close serial ports
                            serialPort1.Close();
                            serialPort2.Close();

                            // Update UI
                            button_Connect.Enabled = true;
                            button_Disconnect.Enabled = false;
                            progressBar1.Value = 0;

                            // Wait briefly to ensure threads have stopped
                            Thread.Sleep(100);

                            MessageBox.Show("Serial Ports are closed", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Motor must be stopped", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // For choosing the COM's serial ports
        private void ComboBox_ComPort_DropDown(object sender, EventArgs e)
        {
            string[] portlists = SerialPort.GetPortNames();
            ComboBox_ComPort.Items.Clear();
            ComboBox_ComPort.Items.AddRange(portlists);
        }

        private void ComboBox_ComPort2_DropDown(object sender, EventArgs e)
        {
            string[] portlists = SerialPort.GetPortNames();
            ComboBox_ComPort2.Items.Clear();
            ComboBox_ComPort2.Items.AddRange(portlists);
        }

        // Form Closing Handlers
        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            if (StopStatus == true) // Check if the Stop button is on
            {
                if (serialPort1.IsOpen) // Check if serialport is connected
                {
                    DialogResult result = MessageBox.Show(
                        "Do you want to close the control panel?",
                        "Confirm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        // Reset speed calculation variables
                        previousTimeMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        previousPosition = 0;

                        // Stop all background processes
                        isUpdatingPosition = false;      // Stop position update thread
                        isPositionUpdateRunning = false; // Stop position broadcasting
                        isFirstRead = true;              // Reset first read flag

                        // Clear any remaining position updates in the queue
                        while (positionUpdateQueue.TryDequeue(out _)) { }
                        speedSamples.Clear();

                        // Close serial ports
                        serialPort1.Close();
                        serialPort2.Close();

                        // Wait briefly to ensure threads have stopped
                        Thread.Sleep(100);
                    }
                    else
                    {
                        e.Cancel = true; // Cancel the close event
                    }

                    if (zedCamera != null)
                    {
                        zedCamera.DisableObjectDetection();
                        zedCamera.Close();
                        Cv2.DestroyAllWindows();
                    }
                }
            }
            else
            {
                MessageBox.Show("Motor must be stopped", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true; // Ensure the form closes
            }
        }

        // Handles serial data reception from Arduino
        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            serialDataIn = serialPort1.ReadLine();
            this.BeginInvoke(new EventHandler(ProcessData));
        }

        // Processes received serial data.
        // Parses and updates system parameters from Arduino data.
        private async void ProcessData(object sender, EventArgs e)
        {
            // Initial delay for system stabilization
            if (isFirstRead)
            {
                await Task.Delay(5000);
                isFirstRead = false;   
            }

            // Check and remove excess characters such as spaces and characters
            serialDataIn = serialDataIn.Trim();

            // Check the data containing a comma and at least 5 parts
            if (serialDataIn.Contains(","))
            {
                string[] dataParts = serialDataIn.Split(',');
                if (dataParts.Length == 5)
                {
                    try
                    {
                        // Extract individual data components
                        string frequencyData = dataParts[0].Split(':')[1].Trim();
                        string voltageData = dataParts[1].Split(':')[1].Trim();
                        string temperatureData = dataParts[2].Split(':')[1].Trim();
                        string vibrationData = dataParts[3].Split(':')[1].Trim();
                        string fanData = dataParts[4].Split(':')[1].Trim();

                        // Parse and store values
                        outputFrequency = float.Parse(frequencyData, System.Globalization.CultureInfo.InvariantCulture);
                        outputVoltage = float.Parse(voltageData, System.Globalization.CultureInfo.InvariantCulture);
                        temperature = float.Parse(temperatureData, System.Globalization.CultureInfo.InvariantCulture);
                        vibration = float.Parse(vibrationData, System.Globalization.CultureInfo.InvariantCulture);

                        // Update UI displays
                        Label_FrequencyOutput.Text = $"{outputFrequency} Hz";
                        Label_VoltageOutput.Text = $"{outputVoltage} V";
                        Label_Temperature.Text = $"{temperature} °C";

                        // Update fan status
                        bool newFanStatus = string.Equals(fanData?.Trim(), "Run", StringComparison.OrdinalIgnoreCase);
                        if (fanStatus != newFanStatus)
                        {
                            fanStatus = newFanStatus;
                            UpdateFanStatusDisplay();
                        }

                        // Calculate and update conveyor speed
                        CalculateConveyorSpeed(outputFrequency);
                    }
                    catch (Exception ex)
                    {
                        // Processing exceptions if there is an error in the data analysis process
                        MessageBox.Show("Error in data analysis: " + ex.Message);
                    }
                }
                else
                {
                    // If the data does not have enough 5 parts
                    MessageBox.Show("Incomplete data, Please check the format again");
                }
            }
            else
            {
                // If the data does not contain commas, error notification
                MessageBox.Show("Received data is not in correct format");
            }
        }

        // Calculates conveyor speed based on motor frequency
        // Uses mechanical formula: Speed = π * D * (120*f/P) / (G * 60)
        // Where: D = roller diameter, f = frequency, P = poles, G = gear ratio
        public float CalculateConveyorSpeed(float outputFrequency)
        {
            MechanicalSpeed = (PI * D_roller * ((120 * outputFrequency / P_motor) / G_ratio)) / 60;
            
            Label_Velocity_2.Text = $"{MechanicalSpeed:F3} cm/s";

            // Update chart on UI thread
            this.Invoke((MethodInvoker)delegate
            {
                UpdateChartData(MechanicalSpeed, vibration);
                
            });
            UpdateSpeed(MechanicalSpeed);
            return MechanicalSpeed;
        }

        // Stops the motor, Forward/Backward control and updates UI accordingly
        private void Button_Stop_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("Stop");
                    Button_Stop.BackColor = System.Drawing.Color.Red;
                    Button_Forward.BackColor = System.Drawing.Color.Gray;
                    Button_BackWard.BackColor = System.Drawing.Color.Gray;
                    StopStatus = true;
                    Label_StopNote.Visible = true;
                    MessageBox.Show("Motor has stopped");
                }
                else
                {
                    MessageBox.Show("Serial ports are not open");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending command: " + ex.Message);
            }
        }

        private void Button_BackWard_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("Backward");
                    Button_Stop.BackColor = System.Drawing.Color.White;
                    Button_BackWard.BackColor = System.Drawing.Color.Green;
                    Button_Forward.BackColor = System.Drawing.Color.Gray;
                    StopStatus = false;
                    Label_StopNote.Visible = false;
                    MessageBox.Show("Motor has been set in backward direction");
                }
                else
                {
                    MessageBox.Show("Serial ports are not open");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending command: " + ex.Message);
            }
        }

        private void Button_Forward_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("Forward");
                    Button_Stop.BackColor = System.Drawing.Color.White;
                    Button_Forward.BackColor = System.Drawing.Color.Green;
                    Button_BackWard.BackColor = System.Drawing.Color.Gray;
                    StopStatus = false;
                    Label_StopNote.Visible = false;
                    MessageBox.Show("Motor has been set in forward direction");
                }
                else
                {
                    MessageBox.Show("Serial ports are not open");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending command: " + ex.Message);
            }
        }

        // Frequency control for VFD
        private void TrackBar_Frequency_Scroll(object sender, EventArgs e)
        {
            // Convert the tradingbar value to float (divided 100)
            float floatValue = TrackBar_Frequency.Value / 100f;
            // Display values ​​in label with dots as decimal
            Label_Frequency.Text = $"Value: {floatValue.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} Hz";
            previousValue = floatValue; // Update valid value
            Calculatedvelocity(previousValue); // Update value into Desired velocity
        }

        private void TextBox_Frequency_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow to enter the number, dot (.) And Backspace key
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true; // eliminate invalid characters
            }

            // only allow one dot
            if (e.KeyChar == '.' && TextBox_Frequency.Text.Contains('.'))
            {
                e.Handled = true; // do not allow additional dots
            }
        }

        private void TextBox_Frequency_TextChanged(object sender, EventArgs e)
        {
            // Get the current value from Label
            string labelText = Label_Frequency.Text.Replace("Value: ", "").Replace("Hz", "").Trim();
            float.TryParse(labelText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float currentLabelValue);

            // Check and process the import value into the textbox
            if (float.TryParse(TextBox_Frequency.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float inputValue))
            {
                if (inputValue >= 0.00f && inputValue <= 50.00f)
                {
                    // Convert to trackbar value
                    TrackBar_Frequency.Value = (int)(inputValue * 100);
                    Label_Frequency.Text = $"Value: {inputValue.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} Hz";
                    previousValue = inputValue; // Update valid value
                    Calculatedvelocity(previousValue); // Update value into Desired velocity
                }
                else
                {
                    // If outside the range, set the default value
                    MessageBox.Show("Please enter a value from 0.00 to 50.00.", "Input error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    TextBox_Frequency.Text = currentLabelValue.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    TextBox_Frequency.SelectionStart = TextBox_Frequency.Text.Length; // Set the end of the string
                }
            }
            else if (!string.IsNullOrEmpty(TextBox_Frequency.Text))
            {
                // if not converted, error message
                MessageBox.Show("Please enter a valid number.", "Input error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                TextBox_Frequency.Text = currentLabelValue.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                TextBox_Frequency.SelectionStart = TextBox_Frequency.Text.Length; // Set the end of the string
            }
        }

        private void Button_Frequency_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if serialport is not connected
                if (serialPort1 == null || !serialPort1.IsOpen)
                {
                    MessageBox.Show("Serial ports are not open", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the Stop button is on
                if (Button_Stop.Enabled && Button_Stop.BackColor == Color.Red)
                {
                    MessageBox.Show("Stop is enable. Please disable it before sending data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Format previousValue to have two decimal places with invariant culture
                string formattedValue = previousValue.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

                // Display the formatted value in the message box
                MessageBox.Show($"Submitted Value: {formattedValue}", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Send the formatted value through SerialPort
                serialPort1.Write(formattedValue);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        // Calculates conveyor speed based on motor frequency (Placed in Desired velocity)
        public float Calculatedvelocity(float outputFrequency)
        {
            float Calculatedspeed = (PI * D_roller * ((120 * outputFrequency / P_motor) / G_ratio)) / 60;

            Label_Desiredspeed.Text = $"{Calculatedspeed:F3} cm/s";

            return Calculatedspeed;
        }

        // Buttons to operate the camera with two function: Area scan and Line scan
        private void Button_AreaScan_Click(object sender, EventArgs e)
        {
            // Set Initialization parameters
            InitParameters initParams = new InitParameters() // initial Initparams object
            {
                resolution = RESOLUTION.HD2K,
                coordinateUnits = UNIT.METER,
                coordinateSystem = COORDINATE_SYSTEM.RIGHT_HANDED_Y_UP,
                depthMode = DEPTH_MODE.PERFORMANCE,
                cameraFPS = 15
            };

            // Open the camera
            zedCamera = new Camera(0);
            ERROR_CODE err = zedCamera.Open(ref initParams);
            if (err != ERROR_CODE.SUCCESS)
            {
                Console.WriteLine("Failed to open camera, Error Code: " + err);
                MessageBox.Show("Error opening camera");
                return;
            }

            // Enable positional tracking
            PositionalTrackingParameters trackingParams = new PositionalTrackingParameters();
            err = zedCamera.EnablePositionalTracking(ref trackingParams);
            if (err != ERROR_CODE.SUCCESS)
            {
                Console.WriteLine("ERROR in Enable Tracking. Exiting...");
                MessageBox.Show("Error tracking camera");
                return;
            }

            // Enable Object Detection
            ObjectDetectionParameters object_detection_parameters = new ObjectDetectionParameters()
            {
                detectionModel = sl.OBJECT_DETECTION_MODEL.MULTI_CLASS_BOX_FAST,
                enableObjectTracking = true,
                maxRange = -1,
                batchParameters = new BatchParameters(),
                enableSegmentation = false,
            };

            err = zedCamera.EnableObjectDetection(ref object_detection_parameters);
            if (err != ERROR_CODE.SUCCESS)
            {
                CameraInformation info = zedCamera.GetCameraInformation();
                MessageBox.Show("Error enabling object detection");
                return;
            }

            // Create Runtime parameters
            RuntimeParameters runtimeParameters = new RuntimeParameters();

            // Set detection confidence threshold
            objRuntimeParams.detectionConfidenceThreshold = 50;

            // Open Form2 to show the camera feed
            Form2 display = new Form2(zedCamera, runtimeParameters, objRuntimeParams);

            display.StartDisplay();
        }

        private void Button_LineScan_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen) //Check if serialport is not connected
                {
                    if (MechanicalSpeed > 0f) //Check if motor is not running
                    {
                        // Set Initialization parameters
                        InitParameters initParams1 = new InitParameters()
                        {
                            resolution = RESOLUTION.HD720,
                            cameraFPS = 60
                        };

                        // Open the camera
                        zedCamera = new Camera(0);
                        ERROR_CODE err = zedCamera.Open(ref initParams1);
                        if (err != ERROR_CODE.SUCCESS)
                        {
                            Console.WriteLine("Failed to open camera, Error Code: " + err);
                            MessageBox.Show("Error opening camera.");
                            return;
                        }

                        // Create Runtime parameters
                        RuntimeParameters runtimeParameters1 = new RuntimeParameters();

                        // Open Form3 to show the camera feed
                        Form3 display2 = new Form3(zedCamera, runtimeParameters1);

                        // Position updates
                        PositionUpdateEvent += display2.UpdatePosition;

                        SpeedUpdateEvent += display2.UpdateSpeed;

                        // Start a dedicated thread for broadcasting position updates
                        Thread positionBroadcastThread = new Thread(BroadcastPositionUpdates)
                        {
                            IsBackground = true,
                            Priority = ThreadPriority.Highest
                        };
                        positionBroadcastThread.Start();

                        display2.StartDisplay2();
                    }
                    else
                    {
                        MessageBox.Show("Motor must be running");
                    }
                }
                else
                {
                    MessageBox.Show("Encoder must be connected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending command: " + ex.Message);
            }
        }

        // Chart Visualization Method for Vibration and Speed
        private void InitializeChart()
        {
            chart1.Series.Clear();

            Series series = new Series(" ");
            series.ChartType = SeriesChartType.Point;
            series.MarkerStyle = MarkerStyle.Circle;
            series.MarkerSize = 8;
            series.Color = Color.Blue;
            chart1.Series.Add(series);

            ChartArea chartArea = chart1.ChartAreas[0];

            // Configure axis labels to show 2 decimal places for X-axis (Speed)
            chartArea.AxisX.LabelStyle.Format = "F2";  // Format to 2 decimal places

            chartArea.AxisX.Title = "Speed (cm/s)";
            chartArea.AxisY.Title = "Vibration";

            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = 32;
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY.Maximum = 1024;

            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
        }

        private void UpdateChartData(float SpeedChart, float VibrationChart)
        {
            // Add point to main series
            chart1.Series[" "].Points.AddXY(SpeedChart, VibrationChart);

        }

        // Fan Control Method
        private void Button_StopFan_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen) //Check if serialport is not connected
                {
                    serialPort1.Write("FanStop");
                    Button_StopFan.BackColor = System.Drawing.Color.Red;
                    Button_AutoFan.BackColor = System.Drawing.Color.Gray;
                    Button_FullFan.BackColor = System.Drawing.Color.Gray;
                    MessageBox.Show("Fans have been closed");
                }
                else
                {
                    MessageBox.Show("Serial ports are not open");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending command: " + ex.Message);
            }
        }

        private void Button_AutoFan_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen) //Check if serialport is not connected
                {
                    serialPort1.Write("FanAuto");
                    Button_StopFan.BackColor = System.Drawing.Color.Gray;
                    Button_AutoFan.BackColor = System.Drawing.Color.Green;
                    Button_FullFan.BackColor = System.Drawing.Color.Gray;
                    MessageBox.Show("Fans switch to Automatic mode");
                }
                else
                {
                    MessageBox.Show("Serial ports are not open");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending command: " + ex.Message);
            }
        }

        private void Button_FullFan_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen) //Check if serialport is not connected
                {
                    serialPort1.Write("FanFull");
                    Button_StopFan.BackColor = System.Drawing.Color.Gray;
                    Button_AutoFan.BackColor = System.Drawing.Color.Gray;
                    Button_FullFan.BackColor = System.Drawing.Color.Green;
                    MessageBox.Show("Fans switch to On mode");
                }
                else
                {
                    MessageBox.Show("Serial ports are not open");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending command: " + ex.Message);
            }
        }

        // Add method to update fan status display
        private void UpdateFanStatusDisplay()
        {
            Label_FanStatus.Text = fanStatus ? "ON" : "OFF";
            Label_FanStatus.BackColor = fanStatus ? Color.Green : Color.Red;
        }

        // Methods to update Position from encoder continuously by sending and receiving data
        // Along with checking sum, calculating the speed and reset the serial port if the error
        private void UpdatePositionContinuously()
        {
            var stopwatch = Stopwatch.StartNew();

            while (isUpdatingPosition)
            {
                if (isResetting)
                {
                    Thread.Sleep(1000); // Pause during reset
                    continue;
                }

                long elapsedTicks = stopwatch.ElapsedTicks;

                // Convert UpdateIntervalMicroseconds to ticks
                long intervalTicks = UpdateInterval * (Stopwatch.Frequency / 1_000_000);

                if (elapsedTicks >= intervalTicks) // Send and receive data in every 100 ms
                {
                    stopwatch.Restart();
                    RequestAndProcessPosition();
                }
            }
        }

        // Activate the sending, receiving and processing data from encoder
        private void RequestAndProcessPosition()
        {
            int retries = 0;
            bool success = false;
            ushort currentPosition = 0;
            float speedEncoder = 0;

            while (retries < MAX_RETRY && !success)
            {
                try
                {
                    serialPort2.Write(new byte[] { 0x54 }, 0, 1); //Send the command to read Position
                    WaitMicroseconds(100);                  //IO operations take time
                    byte[] response = new byte[2];          //Response from encoder should be exactly 2 bytes
                    serialPort2.Read(response, 0, 2);       //Low byte comes first
                    ushort bytePosition = (ushort)(response[0] | (response[1] << 8)); //High byte next, OR it into our 16 bit holder but get the high bit into the proper placeholder

                    if (VerifyChecksum(bytePosition)) //Checksum the position
                    {
                        currentPosition = (ushort)(bytePosition & 0x3FFF);  //Position can be used
                        speedEncoder = CalculateSpeed(currentPosition);     //Calculate the speed
                        positionUpdateQueue.Enqueue(currentPosition);       //Update positions to form 3
                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    getposition.Invoke(new Action(() => getposition.Text = "Read Error: " + ex.Message));
                }
                retries++;
            }

            if (success) //Check the error when processing position
            {
                errorHistory.Enqueue(false);
                if (errorHistory.Count > ERROR_WINDOW_SIZE) errorHistory.Dequeue();
                consecutiveErrors = 0;

                getposition.Invoke(new Action(() =>
                {
                    getposition.Text = currentPosition.ToString();
                    if (speedEncoder > 0) Label_Velocity.Text = $"{speedEncoder:F3} cm/s";
                }));
            }
            else
            {
                errorHistory.Enqueue(true);
                if (errorHistory.Count > ERROR_WINDOW_SIZE) errorHistory.Dequeue();
                consecutiveErrors++;
                HandleErrorRate();
            }
        }

        private bool VerifyChecksum(ushort message)
        {
            //Checksum is invert of XOR of bits, so start with 0b11, so things end up inverted
            ushort checksum = 0x3;
            for (int i = 0; i < 14; i += 2)
            {
                checksum ^= (ushort)((message >> i) & 0x3);
            }
            return checksum == (message >> 14);
        }

        // Error Handling Method while using Encoder
        private void HandleErrorRate()
        {
            double errorRate = (double)errorHistory.Count(e => e) / errorHistory.Count; //Calculate the error rate
            getposition.Invoke(new Action(() =>
            {
                getposition.Text = "Invalid checksum";
            }));

            //If the number of consecutive errors is too large or the error rate exceeds the threshold 
            if (consecutiveErrors >= MAX_RETRY * 2 || errorRate >= ERROR_THRESHOLD)
            {
                ResetSerialPort2();
            }
        }

        // The function stops waiting exactly according to microSecond
        private void WaitMicroseconds(int microseconds)
        {
            var stopwatch = Stopwatch.StartNew();
            long ticks = microseconds * (Stopwatch.Frequency / 1_000_000); // The number of ticks corresponds to microonseconds

            while (stopwatch.ElapsedTicks < ticks)
            {
                Thread.Sleep(1); // Allow CPUs to handle other tasks without stopping
            }
        }

        //Calculate the velocity from the potition of the encoder
        private float CalculateSpeed(ushort currentPosition)
        {
            long currentTimeMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // Check if enough time has elapsed
            if (currentTimeMs - previousTimeMs >= UPDATE_INTERVAL_MS)
            {
                // Calculate time difference in seconds
                float deltaTime = (currentTimeMs - previousTimeMs) / 1000.0f;

                // Calculate absolute position change
                long deltaPosition = Math.Abs(currentPosition - previousPosition);

                // Handle overflow when encoder completes one revolution
                if (deltaPosition < -(PPR / 2))
                {
                    deltaPosition += PPR;
                }
                else if (deltaPosition > (PPR / 2))
                {
                    deltaPosition -= PPR;
                }

                // Convert position delta to revolutions
                float revolutions = (float)deltaPosition / PPR;

                // Calculate angular velocity (rad/s)
                float angularVelocity = revolutions * 2 * PI / deltaTime;

                // Calculate linear speed (cm/s)
                float linearSpeed = Math.Abs(angularVelocity * D_roller / 2);

                // Store speed sample
                if (speedSamples.Count >= SAMPLE_COUNT)
                {
                    speedSamples.Dequeue(); // Remove oldest sample
                }
                speedSamples.Enqueue(linearSpeed);

                // Update previous values
                previousPosition = currentPosition;
                previousTimeMs = currentTimeMs;

                // Return average speed
                return speedSamples.Average();
            }

            return speedSamples.Count > 0 ? speedSamples.Average() : 0;
        }

        // Method to reset the serial port
        private void ResetSerialPort2()
        {
            try
            {
                // Store the current COM port
                string currentPort = serialPort2.PortName;
                isResetting = true;

                if (serialPort2.IsOpen)
                {
                    serialPort2.DiscardInBuffer(); // delete old data before reading
                    serialPort2.Close();
                    Thread.Sleep(200); // Wait before reopening
                }

                // Configure port before opening
                serialPort2.PortName = currentPort; // Maintain the selected COM port
                serialPort2.BaudRate = 2000000;
                serialPort2.DataBits = 8;
                serialPort2.StopBits = StopBits.One;
                serialPort2.Parity = Parity.None;
                serialPort2.Open();

                errorHistory.Clear();
                consecutiveErrors = 0;
                isResetting = false;

                // Update UI to show reset was completed
                getposition.Invoke(new Action(() =>
                {
                    getposition.Text = "Reset completed";
                }));
            }
            catch (Exception ex)
            {
                getposition.Invoke(new Action(() =>
                {
                    getposition.Text = "Error: " + ex.Message;
                }));
            }
        }

        // Method to broadcast position updates for line scan in form 3
        private void BroadcastPositionUpdates()
        {
            isPositionUpdateRunning = true;
            while (isPositionUpdateRunning)
            {
                // Process all queued positions
                while (positionUpdateQueue.TryDequeue(out long position))
                {
                    // Broadcast to all subscribed forms
                    PositionUpdateEvent?.Invoke(position);
                }

                // Prevent tight spinning
                Thread.Sleep(1);
            }
        }

        //The button to activate Encoder's serial ports by hand
        private void Button_ResetEncoder_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    ResetSerialPort2();
                }
                else
                {
                    MessageBox.Show("Serial ports are not open");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending command: " + ex.Message);
            }
        }

        // Method to speed updates for line scan in form 3
        public void UpdateSpeed(float newSpeed)
        {
            currentSpeed = newSpeed;
            SpeedUpdateEvent?.Invoke(newSpeed); // Kích hoạt sự kiện
        }
    }
}
