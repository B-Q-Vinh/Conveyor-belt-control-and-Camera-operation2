Conveyor Control Panel  
C# Windows Forms application for controlling and monitoring industrial conveyor systems with integrated camera functionality for quality control.  
![image](https://github.com/user-attachments/assets/7aa00576-99f3-426b-9d24-7650c5619b6c)

Features  
A. Motor Control & Monitoring (communicate with Arduino)
1. Directional Control: Forward, backward, and stop functionality for conveyor motor  
2. Speed Control: Precise frequency adjustment (0.00 - 50.00 Hz) via slider or direct input  
3. Real-time monitor: Output frequency (Hz), Output voltage (V) Conveyor speed (cm/s), Temperature in the box (°C), Vibration levels (in a chart).

B. Visual System
1. Area Scan: Full field-of-view object detection using ZED camera
2. Line Scan: Line scanning at a specific position
3. Position Tracking: Integrated encoder for precise position monitoring

C. Temperature Adjustment (communicate with Arduino)
Automated fan control with three modes:  
1. Auto: Temperature-based fan activation
2. Full: Maximum cooling capacity
3. Stop: Manually disable fans

D. Serial Communication: 
1. Arduino control interface (motor control, sensor data)
2. Absolute encoder (position tracking)
3. Configurable Connections: Selectable COM ports and baud rates

E. Safety Features
1. Error Detection & Recovery: Automatic detection and recovery from communication errors
2. System Protection: Temperature monitoring and automated cooling
3. Fail-safe Operation: Motor must be stopped before disconnecting or closing

F. Other Specifications

1. Mechanical formula and Formula using Encoder:  
Roller diameter: 5.9 cm, Motor poles: 2, Gear ratio: 30.    
Speed calculation: π × D × (120×f/P) / (G × 60)  
Arduino Mega will update Frequency to UI, then use that frequency to calculate speed.

Another way to calculate speed is to rely on the Encoder to give the actual speed:
```
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
```  

3. Encoder Specifications:
Resolution: 14-bit (16384 PPR), Update interval: 10ms, Communication: 2Mbps.  
Product link: https://www.sameskydevices.com/product/motion-and-control/rotary-encoders/absolute/modular/amt212b-v  
Manual user (for C++): https://github.com/same-sky/AMT21_RS485_Sample_Code_Mega  
Example (for C#):
```
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
```  

5. Camera System:  
Install required NuGet packages: ZED Camera integration with OpenCVSharp and Plotly.  
![image](https://github.com/user-attachments/assets/fcdcfb4e-6433-4893-aedf-b2a452f27c6c)
![image](https://github.com/user-attachments/assets/2a3ef6f1-0e41-43f9-b532-f7bcc853838b)
![image](https://github.com/user-attachments/assets/aac17710-4a6e-4a3e-b1f3-62db7e4c8ec1)

Example to operate the camera:
```
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
```





