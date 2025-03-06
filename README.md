This repository contains the source code for my graduation project: Improving Test Setup for Automation of Metal Recycling, completed during my internship at the Smart Mechatronics and Robotics (SMART) Research Group, Saxion University, from September 2024 to February 2025.

Project Overview:
The project focuses on improving a conveyor test setup to support automated metal classification. 
This is part of a larger research collaboration between SMART Research Group and Riwald Recycling, aiming to develop automated solutions for sorting different types of metals using machine vision systems.  
![image](https://github.com/user-attachments/assets/25157c17-ed8b-4bc4-9ec9-4634f96f9d82)  

Key features of the project include:

1. Developing software to control and monitor a conveyor belt using Arduino Mega, FRENIC-Mini Variable Frequency Drive (VFD) and AMT212B-V Absolute encoder .
2. Creating a user interface (UI) using C# (.NET framework) to allow operators to control the system and view real-time data.
3. Simulate line scan functionality based on area scan functionality of the camera.

Technologies Used:

1. Programming Languages: C#, C++, Arduino.
2. Hardware: Arduino Mega, Absolute Encoder, Fuji Frenic Mini VFD, MAX-485 transceivers, L298N motor driver, S801 vibration sensor, DS18B20 temperature sensor, ZED Camera (temporary replacement for Hyperspectral Camera), Halogen Lights.
3. Software Tools: Visual Studio 2022 (.Net framework), Arduino IDE.
4. Communication Protocol: RS-485 (Modbus).

Introduce about the codes:
1. 
2. 
3. 
4. 

Full codes:

1. C++ (Arduino): https://github.com/B-Q-Vinh/Conveyor-belt-control-and-Convert-Area-Scan-To-Line-Scan-Of-Camera/blob/master/Conveyor_Control_Arduino.ino
2. C# (User interface): https://github.com/B-Q-Vinh/Conveyor-belt-control-and-Convert-Area-Scan-To-Line-Scan-Of-Camera/blob/master/Conveyor_Control_Panel/Form1.cs
3. C# (Area scan functionality): https://github.com/B-Q-Vinh/Conveyor-belt-control-and-Convert-Area-Scan-To-Line-Scan-Of-Camera/blob/master/Conveyor_Control_Panel/Form2.cs
4. C# (Line scan functionality): https://github.com/B-Q-Vinh/Conveyor-belt-control-and-Convert-Area-Scan-To-Line-Scan-Of-Camera/blob/master/Conveyor_Control_Panel/Form3.cs

Note: Since I'm still new to coding in C++ and C#, the code has not yet been optimized or made clear.
