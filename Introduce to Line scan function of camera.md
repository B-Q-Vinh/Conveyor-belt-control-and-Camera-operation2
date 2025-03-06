This application is developed to and display images from a ZED camera using the line scan method.  
1. Creating 2D line scan images from the middle columns of consecutive frames.
2. Dynamically adjusting column count based on conveyor speed.
3. Synchronizing image acquisition with encoder signals.

Features  
A. ZED Camera Control  
Initialize and configure the ZED camera.  
Collect and process images in real-time.  

B. Encoder Synchronization  
Track conveyor position through encoder.  
Calculate movement distance to ensure image accuracy.  
Maintain consistent pixel/distance ratio.  

C. Dynamic Speed Adjustment  
Calculate maximum columns based on conveyor speed.  
Adaptive formula to maintain image quality at different speeds.  
Automatically refresh images when speed changes significantly.  

D. Line Scan Image Processing   
Extract the center column from each frame, simulating the line scan by collecting frames from the ZED's right camera, extracting the middle column of each frame and arranging these columns horizontally to form a continuous 2D image.  
Example:  
1. Previous Line scan image	= [ |A|B| ]
2. Middle column from Frame	= [ |C| ]
3. New Line scan image 	= [ |A|B|C| ]  
Create 2D images by concatenating pixel columns.  
Automatically adjust image size based on speed.  

E. How It Works
1. Camera continuously captures frames
2. Software monitors encoder signals to determine movement distance
3. When distance exceeds pixel size, a column from the middle of the frame is extracted
4. The column is appended to the line scan image
5. When the maximum number of columns is reached (calculated based on speed), the image is refreshed
6. Image is continuously displayed in an OpenCV window
Example:
When the 2D image is being composited by line scan images  
![image](https://github.com/user-attachments/assets/80e9c4ae-cdbc-47cc-bdb9-5ab6f52253ec)
After the image is completed  
![image](https://github.com/user-attachments/assets/0880bf9b-3590-4289-839c-7df8058866bb)
  
F. Formula to calculate column of line scan images  
currentMaxColumns = baseColumns * (baseSpeed / currentSpeed) * (1 + K * (currentSpeed - baseSpeed))  
Where:  
1. baseColumns is the reference column count (1950)
2. baseSpeed is the reference speed (3.089 cm/s)
3. K is an adjustment factor (0.00251)
4. currentSpeed is the current conveyor speed
