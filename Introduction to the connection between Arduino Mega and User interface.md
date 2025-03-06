The introduction to the main functionality of the Arduino code, focusing on the Command part for the VFD and Interfacing with the User Interface (UI) , also briefly mentioning other functions.

A. VFD Control via RS-485
RS-485 User’s manual (for Frenic mini): https://americas.fujielectric.com/files/RS-485_Users_Manual_24A7-E-0082.pdf
Arduino Mega acts as the master controller, sending control commands to VFD (Variable Frequency Drive) via RS-485 communication (using ModbusMaster library). The main commands include:

1.Run Operation command: S06
Forward direction (FWD): S06 = 1.
Backward direction (REV): S06 = 2.
Stop: S06 = 0.

2.Frequency reference: S05
VFD receives the desired Frequency and will multiply by 0.01 Hz to get the frequency set for the Motor.

Use two more commands for Monitoring to see output frequency and output voltage:
1.Output frequency: M06 (multiply by 0.01)
2.Output voltage: M12 (multiply by 0.1)

In Arduino programming, the commands above must be converted into Hexadecimal (HEX) so that both the Arduino and the VFD can understand it. The conversion process is as follows:

For the S group, it is equivalent to 07:
1. Frequency Reference Address = S (07) + 05 = 0705 (hex).
2. Motor Control Address = S (07) + 06 = 0706 (hex).

For motor operation:
1. Forward direction (FWD): 0706 = 0001.
2. Backward direction (REV): 0706 = 0002.
3. Stop: 0706 = 0000.

For the M group, it is equivalent to 08:
1. Output Frequency Address = M (08) + 09 = 0809 (hex).
2. Output Voltage Address = M (08) + 12 = 080C (hex).
Note: Be sure to double-check the conversion to HEX to avoid confusion, as C in HEX equals 12

Example:
// Define Modbus register addresses for VFD
const int frequencyRegister = 0x0705;         // Register for setting VFD frequency
const int operationCommandRegister = 0x0706;  // Register for starting/stopping VFD
const int outputFrequencyRegister = 0x0809;   // Register for reading output frequency
const int outputVoltageRegister = 0x080C;     // Register for reading output voltage

// VFD control with commands
void processSerialCommand(String input) {
  if (input == String("Stop")) {
    node.writeSingleRegister(operationCommandRegister, 0x0000); // Stop
    //Serial.println("VFD Stopped.");
  } else if (input == String("Forward")) {
    node.writeSingleRegister(operationCommandRegister, 0x0001); // Forward
    //Serial.println("VFD Forward.");
  } else if (input == String("Backward")) {
    node.writeSingleRegister(operationCommandRegister, 0x0002); // Reverse
    //Serial.println("VFD Reverse.");
  } else {
    float frequency = input.toFloat(); // Frequency setting
    if (frequency >= 0.00 && frequency <= 50.00) {
      int frequencyValue = frequency * 100;
      node.writeSingleRegister(frequencyRegister, frequencyValue);
      //Serial.print("Frequency Set to: ");
      //Serial.print(frequency);
      //Serial.println(" Hz");
    } else {
      //Serial.println("Invalid input. Enter Stop, Forward, Backward, or frequency (0.00 to 50.00).");
    }
  }
}

// Read VFD frequency and voltage
void readVFDStatus() {
  uint8_t result = node.readHoldingRegisters(outputFrequencyRegister, 1);
  if (result == node.ku8MBSuccess) {
    outputFrequency = node.getResponseBuffer(0) * 0.01;
  }

  result = node.readHoldingRegisters(outputVoltageRegister, 1);
  if (result == node.ku8MBSuccess) {
    outputVoltage = node.getResponseBuffer(0) * 0.1;
  }
}

// RS-485 transmission control functions
void preTransmission() {
  digitalWrite(RS485_VFD_RE_DE, HIGH);
}

// RS-485 receive control functions
void postTransmission() {
  digitalWrite(RS485_VFD_RE_DE, LOW);
}

B. Send Data To User Interface (UI)
Arduino periodically sends a series of data to the computer so that the UI will separate each element to update the display parameters:

Example: F: 15.00, V: 220.0, T: 32.5, V: 300, F: Run

Explanation:
F: Actual Frequency from VFD
V: Actual Voltage from VFD
T: Temperature from DS18B20 sensor
V: Vibration from 801S sensor
F: Fan status (Run/Stop)

C. Process Overview
1. UI sends command (Stop, Forward, Backward or Frequency).
2. Arduino receives command via Serial.
3. Arduino sends Modbus command to VFD.
4. Arduino reads back VFD status (Frequency, Voltage).
5. Arduino reads more from sensor (Temperature, Vibration).
6. Arduino sends all data to UI via Serial to update the interface.

D. Other functions
Such as reading sensors, especially the DS18B20 temperature sensor, which requires two libraries: OneWire.h and DallasTemperature.h to read the temperature.
Once the temperature is measured, a PID feedback control is used to control the L298N motor driver (connected to a fan) via PWM.
