4Functional design
This chapter 4 outlines the functional design of the conveyor control system, focusing on operation, control mechanisms, and visual inspection integration. The system ensures precise speed and position control while incorporating imaging technology for material inspection. Environmental monitoring is also implemented to maintain stable conditions and ensure reliable performance. This chapter systematically explores each subsystem, its functions, and how they contribute to the efficiency and reliability of the conveyor control system.
4.1Functional analysis
4.1.1Conveyor control
The conveyor control system is designed for precise operation, integrating VFD, motor, gearbox, absolute encoder, and Arduino Mega. Communication between components is established using the Modbus protocol via MAX-485 transceivers.
A.Mechanical Drive Control System
The mechanical drive control system consists of key components: the Variable Frequency Drive (VFD), motor, gearbox, and absolute encoder. The VFD regulates the motor's speed and torque by adjusting the supply frequency and voltage. It receives control commands such as start, stop, and speed setpoints from the central controller while providing operational feedback for system monitoring and diagnostics.
The motor converts electrical energy from the VFD into mechanical motion, which is transmitted directly to the conveyor through the gearbox. The gearbox reduces the motor's speed while increasing torque to meet the conveyor's load requirements. To ensure precise control, an absolute encoder measures the conveyor shaft's position and sends this data to the central controller for feedback-based operation.


B.Central Controller and Data Communication
At the core of the system is the Arduino Mega, which serves as the central controller. It executes control algorithms and monitors the performance of the entire system. The Arduino Mega sends control commands to both the VFD and the encoder and receives feedback from both. For the VFD, it sends commands such as stop, forward or backward direction, and frequency settings (Hz) and receives the data output such as frequency output and voltage output in the motor. For the encoder, it sends absolute position data to calculate the actual speed. Ensuring the conveyor operates with stability and precision.
To enable reliable data communication between the Arduino Mega, VFD, and encoder, the system uses two MAX-485 transceivers. This transceiver supports the Modbus protocol over the RS-485 standard, ensuring stable and seamless data transmission among components. This setup guarantees efficient and synchronized system operation.

C.Connection diagram:

Figure 31: Connection diagram between Arduino with Encoder and VFD

4.1.2Vision machine on the conveyor
The camera and light source system is integrated into the conveyor to inspect the materials on the conveyor belt. The system includes RGB camera, Halogen lamp, and lighting setups is dark field lighting, all of which are crucial for achieving high-precision surface inspections of metal objects. The system is designed to provide detailed imagery for quality control, material inspection, and defect detection.
Here are the lights that will be used in the setup:
Floodlight 400 W [43]	

Figure 32: Image of Lamp’s shield
Dimension: 265 x 185 x 125 mm
Case Material: Die Cast Aluminium
Voltage: 220 - 240V AC, 50hz
Wattage: 400 W
IP rating: IP44	
Table 13: The specification of Floodlight
Halogen bar lamp [44]	

Figure 33: Image of Halogen lamp
Type: R7s
Height: 118 mm
Diameter: 8 mm
Voltage:  230 V
Power: 105 W, 120 W, 160 W, 200 W or more
Lifespan: 2,000 h
Lumen: 1900 - 4000 or more (depending on the power)
Color temperature: 3000K	
Table 14: The specification of Halogen bar lamp

4.1.3Sensor functions in operation:
It is necessary to check whether the conveyor belt affects the camera during operation, such as vibration from the motor, and if the lamp emits high temperature, it is necessary to reduce heat to ensure the safety of the camera and the life of the lamp by using a 24V/DC fan with a controller from the L298N motor driver.
Temperature control is particularly important in hyperspectral imaging because temperature fluctuations can affect the performance of the sensor. Heat waves or differences in air density can cause refraction, distorting the spectral data. A cooling system should be used to maintain stable environmental conditions in the experimental setup.
The high performance of the imaging spectrometer in practical applications can be affected by environmental factors, especially temperature changes, which pose a challenge to the stability of the instrument. Temperature fluctuations can cause spectral shifts, which directly affect the accuracy of the spectral measurement, and then affect the accuracy of the radiometric measurement [22].

Vibration sensor 801S [45]	






Figure 34: Image of 801S vibration sensor
This is a high sensitivity 801S vibration sensor module, which has two output signal pins. One digital pin (D0), when it detects some vibration up to a certain threshold, it can output High or Low level. One analog pin (A0), it can output the real-time voltage signal of the 801S vibration. Compared with other vibration or shock sensors, this 801S has the following features: Detect small vibrations; No direction limit; 60,000,000 times shock guarantee (special gold alloy plating); Low-cost circuit can adjust the sensitivity.
FEATURES:
the main chip: LM393, 801S.
work voltage: DC 5V.
with the signal output instructions.
with Analog and TTL level signal output signal output.
the output valid signal is high, the light goes out.
sensitivity adjustable (fine tuning).
vibration detection range, non-directional.
with mounting holes, firmware installation flexible and convenient.	
Table 15: The specification of 801S vibration sensor

Temperature sensor DS18B20 [46]	

Figure 35: Image of DS18B20 temperature sensor
The DS18B20 Digital Thermometer provides 9 to 12-bit (configurable) temperature readings which indicate the temperature of the device. Information is sent to/from the DS18B20 over a 1-Wire interface, so that only one wire (and ground) needs to be connected from a central microprocessor to a DS18B20. Power for reading, writing, and performing temperature conversions can be derived from the data line itself with no need for an external power source.
FEATURES:
Unique 1-Wire interface requires only one port pin for communication
Multidrop capability simplifies distributed temperature sensing applications
Requires no external components
Can be powered from data line. Power supply range is 3.0V to 5.5V
Zero standby power required
Measures temperatures from -55°C to +125°C. Fahrenheit equivalent is -67°F to +257°F ±0.5°C accuracy from -10°C to +85°C
Thermometer resolution is programmable from 9 to 12 bits
Converts 12-bit temperature to digital word in 750 ms (max.)
User-definable, non-volatile temperature alarm settings	
Table 16: The specification of DS18B20 Digital Thermometer

24V DC FAN [47]	

Figure 36: Image of 24V/DC fan
Type: RDH9025S
Nominal voltage: 24V/DC
Supply voltage: 12.0 - 27.6 V/DC
Current consumption (max.): 0.25 A
Power: 6.0 W
Speed: 3000 rpm
Noise development (max.):34 dB
Air flow: 103.2m³/h
Max. Pressure: 36.9 Pa
Lifespan: 30,000 h
Dimensions (l x w x h:) 92 x 92 x 25 mm
Type name: Axial fan	
Table 17: The specification of 24V DC fan


Motor driver L298N [48]	

Figure 37: Image of L298N motor driver
This L298N Motor Driver Module is a high-power motor driver module for driving DC and Stepper Motors. This module consists of an L298 motor driver IC and a 78M05 5V regulator. L298N Module can control up to 4 DC motors, or 2 DC motors with directional and speed control.
Brief Data:
• Input Voltage: 3.2V~40Vdc.
• Driver: L298N Dual H Bridge DC Motor Driver
• Power Supply: DC 5 V - 35 V
• Peak current: 2 Amp
• Operating current range: 0 ~ 36mA
• Maximum power consumption: 20W (when the temperature T = 75 ℃).
• Storage temperature: -25 ℃ ~ +130 ℃.	
Table 18: The specification of Motor driver L298N

24V/DC Power Supply [49]	

Figure 38: Image of 24V/DC power supply
Input Voltage (max): 264 V/AC
Output: 24 V/DC and 1.5 A
Power: 36 W
	
Table 19: The specification of 24V/DC Power Supply

4.2Summary
The conveyor control system is designed to ensure precise operation, integrating components such as VFD, motor, gearbox, absolute Encoder, and Arduino Mega, communicating via Modbus protocol using MAX-485 converter. VFD regulates the speed and torque of the motor through the frequency and voltage supply, and receives control commands from Arduino (start, stop, frequency adjustment) and sends feedback on the operating status. The encoder measures the position and speed of the conveyor shaft, providing data for feedback loop control. 
The system also integrates cameras and lighting (including halogen lamps and dark field lighting methods) to inspect the surface quality of materials. In addition, environmental factors such as temperature and motor’s vibration are strictly checked (by DS18B20 temperature sensor, 801S vibration sensor and 24V DC fan) to protect the device and maintain measurement accuracy, as temperature fluctuations can affect image data, especially hyperspectral images.


5Technical design
Chapter 5 provides a comprehensive engineering design overview of the major components of the conveyor system and their integration. It details the essential configuration of VFD, including initial motor parameter settings and RS-485 communication setup. This chapter covers the implementation of the encoder control, the speed calculation methods, and the cooling system design. Additionally, it also describes the operation of the User Interface, which incorporates both area scan and line scan camera functionality, and shows the hardware connections of the system, including detailed pinouts for the Arduino Mega and its interaction with various modules such as the MAX-485 transceiver, temperature sensor, and vibration sensor.
5.1Initial settings required for VFD
When the VFD is connected to the new motor or the motor is replaced, it is necessary to set the motor parameters such as rated power, rated current, rated voltage, etc. into the VFD settings so that during operation, the VFD can calculate algorithms such as Proportional–integral–derivative (PID) control, acceleration, deceleration or response time appropriately to control the motor smoothly. Otherwise, it will cause undervoltage or overload to the motor when setting low or high frequency. And it is possible to set the use of RS-485 port to transmit and receive signals from the computer.

Figure 39: Image of VFD
Refer to the User’s Manual [55] to adjust the parameters below with the four buttons: PRG/PRESET, FUNC/DATA, and the two up and down arrow buttons as shown in Figure 39. Here is how to use them: 
1.Press the PRG/PRESET button, and the function code group will appear, such as I.P (there are other groups like E, C, F, H, A, etc.). Using the up or down arrow buttons to search for the desired function code group.
2.Press the FUNC/DATA button. For example, if in the I.P group, it will display P 02 (Indicating access to the parameters of group P). Using the up or down arrow buttons to find the desired function code, like P 03 or P 04.
3.Press the FUNC/DATA button again, and it will display 0.20 (which is 0.2 KW). Using the up or down arrow buttons to change the data to the desired value.
4.After completing step 3, press the FUNC/DATA button again to return to Step 2 and search for other parameters in group P. If the change is successful, "SAUE" will appear.
5.After completing the parameter setting for that group, press the PRG/PRESET button to return to Step 1 and continue accessing other function code groups.

VFD Settings for the Motor of the project and RS-485 Port Configuration:
1.Motor Parameter Settings - Group P
P02: Rated power => Rated power: 0.25 KW, so P02 = 0.25.
P03: Rated current => Rated current: 1.14 A, so P03 = 1.14.

2.Control Parameter Settings - Group F
F03: Maximum frequency (set maximum speed) => Rated frequency: 50 Hz, so F03= 50.
F04: Base frequency (Set base frequency, combined with F05 for control law calculations like V/F) => Rated frequency: 50 Hz, so F 04= 50.
F05: Voltage at base frequency => Rated voltage: 230 V, so F05 = 230.
F06: Maximum output voltage => Rated voltage: 230 V, so F06 = 230.
F07: Acceleration time => F07 = 5 (s).
F08: Deceleration time => F08 = 5 (s).
Note: F07 and F08 can be adjusted based on requirements.

3.Speed Reference and Control Parameter Settings - Group H
H30: Select Run/Stop control reference and speed => H30 = 3 (Run & Frequency Command via RS 485).

4.Communication Settings (RS485 communication) - Group Y
Y01: Station Address => Y01 = 1. (In case there are multiple inverters, the address will be changed for each inverter for easy access to the desired one).
Y02: Communication error processing => Y02 = 0 (The inverter will stop and display an "Er08" alarm).
Y03: Error Confirmation/Auto-connection Delay => Y 03 = 200 (ms) = 0.2 (s).
Y04: Baud rate => Y04 = 2 (0: 2400 bps; 1: 4800 bps; 2: 9600 bps; 3: 19200 bps; 4: 38400 bps).
Y05: Data length => Y05 = 0 (0: 8 bits; 1: 7 bits).
Y06: Parity check => Y06 = 1 (0: None [2 stop bits]; 1: Even; 2: Odd; 3: None [1 stop bits]).
Y07: Stop bits => Y07 = 1 (0: 2 bits; 1: 1 bit).
Y10: Protocol => Y10 = 0 (Modbus-RTU).
Y99: Loader link function => Y99 = 0 (Folow H30 Data).

5.Enable Emergency Stop button - Group E
Function code E01 allows to assign commands to terminal [X1] in the VFD.
E01: Coast to a stop => E01 = 7.
Shorting the circuit between the BX-assigned terminal and terminal [CM] will immediately stop the inverter output so that the motor will coast to a stop without issuing any alarms.
Note: Jumper applied to the SINK is required to enable terminals [X1], [X2], [X3], [FWD], or [REV] using a relay contact.

Figure 40: Connection diagram of a Jumper to SINK (a) or SOURCE (b) in the VFD

5.2RJ-45 connector and RS-485 communication for VFD and Encoder
To connect an RS-485 communication, it is necessary to know which pins the VFD uses to send and transmit data. Here is a table of specifications for the RJ-45 connector as well as a description:
Pin No.	Signal name	Function	Remarks
1, 8	Voltage at the Common Collector (VCC)	Power source for the keypad	5V
2, 7	Ground (GND)	Reference voltage level 	Ground (0V)
3, 6	NC	No connection	
4	DX-	RS-485 communications data (-)	A terminating resistor of 112Ω is incorporated. Connection/ cut off is selected by a switch.
5	DX+	RS-485 communications data (+)	
Table 20: specifications for the RJ-45 connector of VFD

Figure 41: RJ-45 connector and RS-485 communication of VFD
Below are two images showing how to connect from VFD and Encoder via RS-485 communication to Arduino Mega:

Figure 42: VFD and Arduino connection diagram


Figure 43: Encoder and Arduino connection diagram
The image above illustrates the connection diagram between the Arduino Mega 2560, the RS-485 communication module, and an RJ45 network cable. The Arduino Mega uses the TX2 pin (pin 16) to transmit data and the RX2 pin (pin 17) of Serial2 to receive data. The UART signal from the Arduino is converted to RS-485 through the communication module, with the RS485+ and RS485- pins connected to the network cable for data transmission and reception. This setup is used to control the VFD inverter, which supports the Modbus RTU protocol.
The Absolute Encoder also supports the Modbus RTU protocol, so it is connected similarly to the VFD and will be connected to Serial1, using the TX1 pin (pin 18) to transmit data and the RX1 pin (pin 19) to receive data.
The remaining pins, such as DE and RE of the MAX-485, will be connected to the digital pins of the Arduino Mega. When switching to transmit mode, the DE and RE pins will be set to HIGH, and when switching to receive mode, they will be set to LOW. The VCC and GND pins will be connected to the 5V and GND pins of the Arduino, respectively.
Name	Function
DE (Driver Enable)	HIGH: Enable Transmit mode, allowing the module to send data to the RS-485 line.
LOW: Disconnect Transmit mode, do not send data.
RE (Receiver Enable)	LOW: Enable Receive mode, allowing the module to receive data from the RS-485 line.
HIGH: Turn off Receive mode, do not receive data.
Table 21: How the DE and RE of the MAX-485 transceiver work

5.3VFD and Encoder control method
5.3.1Control of VFD command
To control VFD via computer, need to know the following commands:

Figure 44: The commands of VFD: Operation command (Stop, Forward and Backward direction) and Frequency reference
1.Run Operation command: S06
Forward direction (FWD): S06 = 1.
Backward direction (REV): S06 = 2.
Stop: S06 = 0.
2.Frequency reference: S05
VFD receives the desired Frequency and will multiply by 0.01 Hz to get the frequency set for the Motor.


Figure 45: The commands of VFD: Output frequency and Output voltage for Monitoring
Use two more commands for Monitoring to see output frequency and output voltage:
1.Output frequency: M06 (multiply by 0.01)
2.Output voltage: M12 (multiply by 0.1)
In Arduino programming, the commands above must be converted into Hexadecimal (HEX) so that both the Arduino and the VFD can understand it. The conversion process is as follows:

Figure 46: Convert Operation command and Frequency reference from S group code to HEX code
For the S group, it is equivalent to 07:
Frequency Reference Address = S (07) + 05 = 0705 (hex).
Motor Control Address = S (07) + 06 = 0706 (hex).
For motor operation:
Forward direction (FWD): 0706 = 0001.
Backward direction (REV): 0706 = 0002.
Stop: 0706 = 0000.

Figure 47: Convert Operation command and Frequency reference from M group code to HEX code



For the M group, it is equivalent to 08:
Output Frequency Address = M (08) + 09 = 0809 (hex).
Output Voltage Address = M (08) + 12 = 080C (hex).
Note: Be sure to double-check the conversion to HEX to avoid confusion, as C in HEX equals 12.

5.3.2Control of Encoder command
Encoder control is divided into three stages [56]:
1.RS-485 Communication
The AMT21 starts transmitting the absolute position as soon as the Arduino starts transmitting data, eliminating the need for a command and response structure. Therefore, the Arduino will send 0x00 as the first byte, with valid data returned at the same time.
Get Position: 0x00
RS-485 data is transmitted by first turning on the RS-485 transceiver driver, then sending data via Serial1, then turning off the transceiver driver. The AMT21 will respond with the requested data.
It does not matter if the encoder is 12-bit or 14-bit, it will always respond with two full bytes, for a total of 16 bits. The top two bits are check bits that allow for data integrity verification. If the encoder is a 12-bit version, the bottom two bits will both be 0. For this data to be useful, it must be shifted right by 2 bits (or divided by 4).

2.Checksum Verification
When the RS-485 transmission is complete, the data needs to be validated using a checksum.
Using the equation from the data sheet, create a function to verify that the received value has a valid checksum. The checksum is stored in the top two bits of the received value. The check bits are parity on odd bits and even bits in the position response. Then keep checking for parity on all odd bits (bits 1, 3, 5, 7, 9, 11, 13) and parity on even bits (0, 2, 4, 6, 8, 10, 12, 14).
This function will return true if the checksum is valid.

3.Data Formatting
If the check bits are correct, then update the “currentPosition” variable, clearing the top two bits. Then, AND the “currentPosition” variable with 0x3FFF (0b00111111111111111111111111111) to ensure all lower 14 bits are retained.
Also, check the handle to see if the encoder resolution is 12-bit or 14-bit. If the resolution is 12-bit, then simply shift the current Position 2 bits to the right.
Finally, getting the absolute position from the encoder.


5.4Calculate velocity based on Frequency and Encoder Pulses 
There are two ways to calculate conveyor speed, one is using mechanical formular or the other is calculating from encoder.
A.The mechanical formula to calculate conveyor belt speed based on Frequency
The speed of the electric motor:

And the gear box with G ratio (30:1)



Conveyor speed:

Where:
Nmotor: 	Motor speed (revolutions per minute or rpm).
f motor:	Motor frequency (Hz).
Pmotor: 	Number of motor poles. 
Vconveyor: Conveyor belt speed (m/s).
π (pi):	Constant number (approximately 3.14159)
Droller: 	Diameter of the belt roller (m).
Nreduce: 	Speed of the belt roller (rpm).	

B.The formula from Encoder Pulses to calculate conveyor belt speed
Absolute position change:

Convert delta position to revolutions:


Calculate angular velocity (rad/s):

Conveyor speed (m/s):

Where:
ΔPosition: Absolute position change.
PPR: number of pulses per rotation (depending on bits number of encoder, such 14 bits or 12 bits).
Δt: time between measurements (s).
Pulley Diameter: diameter of the belt roller (m).

To explain the above formula:
1.Position change to revolutions.
2.Revolutions to angular velocity.
3.Angular to linear velocity using pulley radius.

C.Overflow correction for the velocity
When the encoder completes a full rotation and the position value resets to 0 (or its initial value in a cycle), it can cause errors in calculating the movement distance (ΔPosition) between measurements. This leads to an unexpected negative or overly large value, resulting in velocity calculation errors.
To address this issue, it is necessary to manage the transition from the maximum value back to the minimum value when the encoder completes a full rotation. This can be done using overflow correction. The solution works as follows: when the current value is smaller than the previous value, the encoder's cycle is added to the calculation to account for the overflow.
Check overflow compensation:
If ΔPosition < - (PPR / 2): This means the encoder has completed a full rotation, and the current value has wrapped around from the maximum value back to the minimum value. In this case, add the PPR to correct the overflow.
If ΔPosition > (PPR / 2): This means the encoder has completed a full rotation, and the current value has wrapped around from the minimum value back to the maximum value. In this case, subtract the PPR to correct the overflow.


D.Conclusion
Both formulas above give accurate results. In this project, the absolute encoder is used to calculate the real-time speed and compare it with the mechanical formula to avoid errors in the encoder's calculations. And avoid the situation where the encoder rotates one turn causing speed error.

5.5Cooling system and vibration measurement

Figure 48: Example of Cooling system
For the dark box, intake fans can be installed on one or both sides to push cool air inside, while exhaust fans can be installed on the top to push hot air generated by the lights out, similar to how fans are installed in a computer case. Theoretically, this setup should work; however, in practice, the actual installation will be determined during the testing phase.

Figure 49: Connection diagram of Cooling system
Connection diagram:
The connection diagram [Figure 49] uses a 24V power supply to power the system, including the L298N control circuit, two 24V DC fans, the DS18B20 temperature sensor with a wiring adapter, and the Arduino Mega. The 24V power supply provides power to the L298N circuit and the fans, with the positive (+) and negative (-) terminals of the power supply connected to the VCC and GND pins of the L298N circuit. The L298N controls the fans through PWM signals from the Arduino Mega, which are connected to the IN1 and IN2 pins (for Fan 1) and IN3 and IN4 pins (for Fan 2) on the L298N circuit. The 24V DC fans are connected to the OUT1, OUT2, and OUT3, OUT4 pins on the L298N circuit, where they receive the control current. The Arduino Mega sends PWM signals to the L298N to adjust the speed of the fans, helping to effectively control the cooling system. The DS18B20 temperature sensor measures the temperature, enabling the fans to operate at high or low power using depending on PID feedback control with the set temperature.
For detecting vibration from the motor [Figure 50], the 801S sensor is used. The VCC and GND pins are connected to the 5V and GND pins of the Arduino, respectively, while the Digital Output (DO) pin is connected to a digital pin to receive and process the signal. Combined with calculated velocity, a chart can be created in the UI to compare the vibration levels with the actual speed of the motor. 

Figure 50: Connection diagram of vibration measurement

5.6Connect the modules to the pins of the Arduino 
Below is the pin diagram of the Arduino Mega connected to the modules used in the project.
Pins of Arduino Mega	Pins of Modules	Note
Analog 8	RE, DE	MAX-485 transceiver (1) Encoder connection
RX1 19 (Serial 1)	DI	
TX1 18 (Serial 1)	RO	
Analog 7	RE, DE	MAX-485 transceiver (2): VFD connection
RX2 17 (Serial 2)	DI	
TX2 16 (Serial 2)	RO	
Digital 2	DAT	DS18B20 Temperature sensor
Digital 13	IN1	L298N Motor driver to control the fan
Digital 12	IN2	
Digital 11	IN3	
Digital 10	IN4	
Digital 7	ENA (PWM)	
Digital 6	ENB (PWM)	
Analog 9	D0	SW-420: Vibration sensor
Table 22: Pins used in Arduino Mega with modules

5.7Flow chart for system programming on UI
 
Figure 51: The flow chart for the operation of the UI
The conveyor control system is implemented through a user interface (UI). The process is divided into the following specific steps:
The process begins when the user launches the control interface. Here, the user sets up the connection with the Arduino Mega by selecting the communication port (COM Port) and the data transfer rate (Baud Rate). This is the basic setup step for establishing communication between the interface and the Arduino.
Once the setup is complete, the system checks the connection status. If the connection is successful, the user can proceed with the control steps. If not, the interface will prompt the user to check if the connection settings are correct, as the COM port or baud rate might be incorrect.
When the connection is successful, the user can control the conveyor system through the function buttons on the interface. Options include running the conveyor forward, running it backward, or stopping it. After selecting the direction, the user can set the conveyor's operating frequency by entering the desired value or adjusting the frequency slider, and control the fan of the cooling system, then sending the command to the Arduino. And there are 2 more functions to operate the camera: Area scan and Line scan.
The Arduino processes the commands from the interface and communicates with the VFD via RS-485 to control the conveyor and the encoder. At the same time, the Arduino receives feedback from the VFD and encoder, processes the data, and transmits operational parameters such as temperature, speed, output frequency, and output voltage, displaying them on the interface. These parameters are continuously updated so the user can monitor the conveyor’s status in real-time.
When the control process is complete, the user can choose to disconnect via the serial connection. The system will check the disconnect request, and if the user confirms, the connection will be terminated, and the process will end. If not, the system will return to the conveyor control step to continue operation.
The process is designed in a loop structure to ensure continuous and flexible system operation, while providing easy monitoring and adjustment through the user interface.

5.8Installation of vision setup
Based on the article [11], this system can be considered a larger version compared to the conveyor belt currently available in SMART research group. In the article, the belt size is 60 cm, while the belt in the lab is 30 cm. Therefore, a similar setup can be simulated using two lights instead of four, with the distance between the two lights (84 cm) and between the camera and the conveyor being the same or adjustable. 
Dark field lighting will be used with a 45-degree angle for the lights if the camera is positioned at a 90-degree downward angle. Additionally, experiments will be conducted with camera angles of 65 degrees and 45 degrees to assess and compare results from different angles. Then, the light angles will also be adjusted to minimize reflections from the metal surfaces.

Figure 52: Image of Practical setup on the article

5.9Area scan and Line scan function of Camera
Programming to start the camera In Visual Studio Code (.Net Framework C#), the publisher also has samples with all the camera functions and coded with many different coding languages such as C, C#, Python [57], and will be combined with OpenCV which is already integrated in Visual Studio Code.

Figure 53: OpenCV for C# programming in Visual Studio Code

Figure 54: C# programming function for Zed camera in Visual Studio Code
In this project, the object detection function will be the Area scan function, because it can identify objects within the camera's field of view.
As for the Line scan function, it is provided by the developer, Mr. Hojat, simulating the line scan by collecting frames from the ZED's right camera, extracting the middle column of each frame and arranging these columns horizontally to form a continuous 2D image.
Example:
1.Previous Line scan image	= [ |A|B| ]
2.Middle column from Frame	= [ |C| ]
3.New Line scan image 	= [ |A|B|C| ]
Note: the two functions above are just to prove that the camera can be operated by the UI, it does not have the ability to classify metals.


5.10 Change the Encoder connection method
In the case that the encoder's output, when connected to the Arduino Mega via the MAX-485 transceiver, does not function as expected, the connection will be switched to a direct interface with the computer using the cable shown in Figure 55. 
TRU COMPONENTS TC-KW-190 Serial converter USB, RS-485, RS-422 [50]
Transmission rate: 3 megabaud
Interfaces: RS485, RS422
USB class: FTDI driver
Max. output voltage: RS485 +12 V - 7 V
USB speed: full speed
USB connector: USB-A
I/O voltage: 5 V
Virtual COM connection: yes
FTDI internal IC: FT232RL	
Figure 55: Image of Serial converter USB, RS-485, RS-422

Table 23: The specification of Serial converter USB, RS-485, RS-422
When using this cable, it will be necessary to adjust both the hardware connections and the corresponding programming in the UI to ensure proper communication and functionality.

Figure 56: Connection diagram of Encoder to computer using of Serial converter USB to RS-485
Connection diagram:
This diagram shows the encoder is powered by 5V and grounded by the Arduino Mega, but it does not manage data communication. The encoder transmits data via RS-485 (RS485+ and RS485-) directly to an RS-485 to USB converter (A and B), which connects to a computer for data processing and communication.

5.11 Conclusion
Chapter 5 outlines the entire engineering implementation of the conveyor system, from basic component setup to advanced control methods.
The detailed configuration and control of the VFD and encoder ensure precise motor control and speed monitoring. The system uses two different methods to calculate speed - one based on a mechanical formula and the other using the position of the Encoder - to verify accuracy and provide backup calculations if one method fails. The design of the cooling system with temperature-controlled fans and vibration monitoring adds necessary safety features to the system.
The user interface flow diagram presents a user-friendly control interface that effectively integrates all the system functions. The vision setup, although primarily focused on demonstrating the camera functionality, will be developed for future metal sorting capabilities. Alternative solutions, such as an optional USB-RS485 converter for encoder connectivity, demonstrate the system’s adaptability to a variety of deployment requirements.
Overall, this technical design creates a flexible framework that meets the core project requirements while allowing for future enhancements and modifications.

6Realisation
1.Metal Classification Using Machine Vision: 
After conducting research on metal classification with cameras, a list of required components, along with their advantages and disadvantages, was compiled based on relevant articles. However, some aspects remained unclear, such as the reasoning behind the choice of certain camera angles and their effectiveness compared to other angles. The choice of illumination methods also needs further clarification to determine their suitability for this project. As a result, clear conclusions regarding the setup and installation cannot be made yet. Therefore, it is essential to discuss with the clients the possibility of experimenting with different setups to determine the suitable design for the project.
2.Arduino Programming and Encoder Integration: 
Programming was completed to control the VFD and encoder via Arduino, adding functionality for temperature measurement, vibration monitoring, and a cooling system. The encoder was used to calculate conveyor belt speed and support the camera's line scan function. However, there were issues with the encoder's performance, which led to the decision to connect the encoder directly to a computer to use the line scan functionality.
3.C# Application Development: 
A C# application was developed to serve as the user interface for controlling the Arduino. This application allows users to interact with the conveyor belt, controlling its speed, direction, and other features such as output data monitoring, fan control, and operating the camera with both area scan and line scan functions. However, several small issues arose during the integration process, including challenges with connecting the Arduino to the UI, using the encoder, and starting the area scan and line scan functions.
The experiments and issues mentioned above will be detailed in Section 7, along with the solutions implemented to address them.
7Testing and Verifications
This chapter 7 covers comprehensive testing and verification procedures for the major components of the conveyor system. After establishing the VFD parameters, motor connections, and communication interfaces, this chapter examines several key aspects: Arduino-UI connection, Arduino-Encoder integration, user interface functionality, lighting, and camera setup, testing both area scan and line scan camera functionality, and verification of the cooling system. Throughout these tests, various challenges and solutions are documented, with a particular focus on communication latency, encoder accuracy at different speeds, and camera synchronization issues. This chapter provides details on the system limitations and the solutions implemented to optimize performance.
7.1Connection between Arduino Mega and the UI 
The system requires accurate Serial connection settings between the Arduino and the user interface (UI). The baud rate and COM port must match; otherwise, the UI will show a "the port is not open" error and block access to other functions. Users can confirm the correct settings using the Arduino IDE, which shows the configured baud rate and the active COM port [Figure 57] or use Device Manager to find out or change the COM port  [Figure 58].

Figure 57: Check the COM port and baud rate of Arduino


Figure 58: Check COM port in Device Manager on computer
NOTE: Only one connection in the serial port, for example, if the serial monitor in Arduino IDE is open, the UI will not be used, the serial monitor must be closed to continue using the UI.
After setting up the correct COM port and baud rate in the UI to connect to the Arduino Mega, an error occurs regarding the output data, either being incorrect or incomplete. However, this error only appears at the beginning and disappears after a few seconds. Upon checking, it was found that when the connection to the Arduino Mega starts, the output data-related commands are not stable and need a few seconds to stabilize.
Therefore, the solution is to add a delay of about 5 seconds in the output display section of the UI to allow the Arduino enough time to stabilize the output data. If the COM port is connected again, this delay will be reactivated.
Finally, the current system has limitations for advanced features. While the Encoder-Arduino setup works well for calculating conveyor speed, it has trouble integrating with the line scan camera function. The main issue is the separate connections: the camera connects to the computer, while the encoder connects to the Arduino. This split setup causes delays in data transmission and processing. 
Additionally, because of the method of data transfer between the Arduino and the UI. The Arduino sends data through loop, which the UI then reads and processes. which adds more latency to the system. These combined delays prevent the precise timing needed for capturing images line by line at specific distances.

7.2Connection between Arduino Mega and Encoder 

Figure 59: Actual connection in Arduino Mega
After successfully connecting the Arduino Mega with the VFD and Encoder in Figure 59, both devices operate well together. However, while the VFD functions smoothly, the Encoder occasionally causes errors that affect the overall system performance.
Firstly, the encoder works fine at first, but sometimes encounters issues, such as failing to retrieve enough data from the encoder [Figure 60]. As a result, the system is unable to calculate the absolute position and speed, causing the encoder to stop working correctly. This issue can persist indefinitely, and although the connection has been checked and appears fine (as the system normally operates most of the time), the problem occasionally resolves temporarily by pressing the Reset button on the Arduino Mega, after which the system resumes normal operation.

Figure 60: Speed error from Encoder during use

To address this, a feature needs to be implemented to reset the MAX-485 connected to the encoder. This can be achieved by closing the Serial 1 port on the Arduino Mega when an error signal is received from the encoder. After waiting for 3 seconds, the port will be re-opened, allowing the system to resume normal operation.
Secondly, if the frequency of the VFD increases, the motor speed will increase, which in turn leads to the encoder generating pulses faster. If the number of encoder pulses becomes too high and the Arduino is unable to process and transmit commands quickly enough, it may result in pulse loss. This pulse loss causes incorrect position calculations.

Figure 61: Compare speeds from two different calculations when there is an error  
For example, at frequency 20 Hz, the speed can be calculated as 0.09 m/s, but as the frequency increases to 25 Hz or higher, the calculation speed is reduced, as shown in Figure 61, according to the mechanical formula, a frequency of 25 Hz will give a result of 0.15 m/s, but if calculated by encoder, it will only be 0.09 m/s. It is believed that this is due to the Arduino's inability to process the position data quickly enough, and the delay in processing the loop() function may contribute to this issue.

Figure 62: Position error when using MAX-485
This has a significant impact on the use of the line scan function, as high accuracy is required to calculate the distance for each line of the camera. Because Max 485 can only receive and transmit when its RE and DE are activated, meaning it can only receive and transmit once and repeatedly, and this Encoder must send a Command to it to transmit that Position. So as shown in the picture above, the Arduino receives positions at the same time [Figure 62].

7.3Testing the UI
The conveyor control system interface [Figure 63] has been successfully implemented with comprehensive functionality. The user interface efficiently manages serial communication, conveyor direction control, camera operation (both area scan and line scan modes), fan control, and variable frequency drive adjustment through an intuitive slider interface. Real-time monitoring of critical parameters including temperature, dual velocity measurements (from encoder and mechanical), output frequency, and voltage provides the operator with the necessary system feedback. The modular design of the interface, clear status indicators, and simple controls allow for efficient system operation and monitoring. Through testing, the system has demonstrated reliable performance in controlling conveyor operations. 
However, there are still a few notable issues such as the position will have a “Valid checksum” error and the Chart is still not clearly displayed, the Motor needs to be started for everything to work properly.
As for the output data, it is displayed correctly, but there may be a slight delay when the system is changing, such as when the frequency (Hz) of the VFD is adjusted. It takes a few seconds for the VFD parameters to appear. The reason for this is explained in section 7.1.


Figure 63: Image of User interface on operation


7.4Testing the Area scan and Line scan functions of the camera 
This is just a demo version to demonstrate the ability to operate the camera from the UI, so there is no metal classification function yet.
The Area scan function, which is the camera's object detection feature, can detect moving objects as well as their positions. However, after several tests, it became clear that this function needs further development because it cannot detect the objects moving on the conveyor belt.

Figure 64: Image of Area Scan function
The Line scan function can be considered operational, but due to delays in transmitting and processing data from the Arduino Mega, as outlined in sections 7.1 and 7.2, a basic approach was used. This approach involves adjusting the conveyor belt speed to match the line scan camera's capture speed. After several tests, it was found that a speed of approximately 9 cm/s allows the line scan images to be successfully stitched into a 2D picture.

Figure 65: Line scan image at Speed: 12 cm/s

Figure 66: Line scan image at Speed: 9 cm/s


Figure 67: Line scan image at Speed: 6 cm/s

Figure 68: Line scan image at Speed: 3 cm/s

7.5Adjustments to perform the sub-project “Line Scan”
Although the Arduino Mega is said to be capable of data transfer rates of up to 2.5 Mbps, the physical connection like the Max-485 is still limited, making the use of the Encoder not as expected. Therefore, this connection needs to be changed using the cable in the section 5.10 and connected in Figure 69.

Figure 69: Image of Serial converter USB to RS-485 connection
In C# UI programming, an additional serial port is created to communicate with full parameters, including a baud rate of 2M, data bits, stop bits, and parity. The methods for sending commands, receiving data, and checking data, as described in section 5.3 B (Encoder), will be reused. After confirming the position, which can range from 1 to 16.384 pulses (since the encoder is 14-bit), it is ensured that the encoder is functioning as expected.
As outlined in section 3.7, the line scan camera will be synchronized with the encoder to calculate the travel distance equal to the pixel size of the camera to trigger the camera to perform continuous line scan.
First, to calculate the travel distance per pulse, it is necessary to know how many pulses the encoder generates per revolution and the circumference of the belt.
1.The encoder has 14 bits, meaning it generates 214 = 16.384 pulses per revolution.
2.The circumference of the belt is π × Droller  = π × 5.9 cm ≈ 18.54 cm.
Then, Calculate the distance per pulse:

So, each pulse corresponds to approximately 0.00113 cm or 11.3 µm.
Secondly, since this is the Area scan camera with a simulated line scan, the pixel size of the line scan image is unknown, so it has been evaluated continuously to determine the desired pixel size.
After several experiments, it was determined that the pixel size could be approximately 22.6 µm, along with a limit on the maximum number of line scans in a 2D image. This limit was necessary because the continuous combination of line scans would result in pixel compression, causing the objects in the image to shrink. Initially, the experiment will be conducted with a maximum column count of approximately 700 columns.
Here are the Line scan images at four different speeds with the number of columns is 700:

Figure 70: Line scan image at Speed: 3 cm/s (with 700 columns)


Figure 71: Line scan image at Speed: 6 cm/s (with 700 columns)


Figure 72: Line scan image at Speed: 9 cm/s (with 700 columns)


Figure 73: Line scan image at Speed: 12 cm/s (with 700 columns)

If the line scan images are not consistent across different conveyor speeds, the cause could be: 
1.As the conveyor belt moves faster, the object moves a larger distance between scans, so the pixel columns are further apart. This means that a smaller physical distance on the object is assigned to a smaller number of pixels, causing the image to be horizontally "compressed".
2.As the conveyor slows down, the object moves less between scans, so the pixel columns are closer together. This causes a larger physical distance on the object to be assigned to more pixels, causing the image to be "stretched" horizontally.
Suppose each pixel column of the camera corresponds to 1 mm on the conveyor belt.
If the conveyor belt runs normally, each scan will capture exactly 1 mm and combine them to create an image with the correct ratio.
If the conveyor belt runs twice as fast, the object moves 2 mm in the time between two scans, then the image will be scaled down horizontally.
If the conveyor belt is slowed down by half, the object only moves 0.5 mm between two scans, then the image will be stretched.
Several attempts have been made to solve this problem by adjusting the number of encoder pulses, the pixel settings of the line scan image, and the size of the 2D image, but none have been successful. However, it is possible to adjust the maximum number of columns for the line scan image.
Therefore, it is necessary to determine the appropriate number of columns to match the size of the object. At a speed of 3.089 cm/s (5 Hz), the exact 2D image size corresponds to 1,950 columns, while at a speed of 9.268 cm/s (15 Hz), the approximate size is 660 columns. 
Therefore, calculating, a formula is needed for the appropriate number of columns for different speeds:


And “k” is the adjustment factor to adjust the correct number of columns for each different speed:


Substituting Sreference: 1950 (reference column count at vreference: 3.089 cm/s) and Scurrent: 660 (desired column count at vcurrent: 9.268 cm/s) into the formulas above:


And substitute the above values into the formula to determine the number of columns, ensuring the ability to calculate the desired column count:


Or




Here are the Line scan images at four different speeds with new column calculation:

Figure 74: Line scan image at Speed: 12 cm/s (using the formula)


Figure 75: Line scan image at Speed: 9 cm/s (using the formula)


Figure 76: Line scan image at Speed: 6 cm/s (using the formula)


Figure 77: Line scan image at Speed: 3 cm/s (using the formula)
