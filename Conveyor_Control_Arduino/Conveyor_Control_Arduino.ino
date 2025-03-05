#include <OneWire.h>           // Library for OneWire communication
#include <DallasTemperature.h> // Library for DS18B20 temperature sensor
#include <ModbusMaster.h>      // Library for Modbus RTU communication
#include <PID_v1.h>            // Library for PID control

// This code includes control of VFD, Encoder, fans, sensors and speed calculations.
// However, the control of the encoder and the speed calculations has been transferred to connect to the computer.
// So the relevant functions have been hidden.

// Define baud rates for Serial communication
#define BAUDRATE 115200       // Serial monitor baud rate
#define VFD_BAUDRATE 9600     // Baud rate for RS-485 communication with VFD
//#define ENCODER_BAUDRATE 2000000  // RS-485 speed for encoder

// RS-485 commands for encoder
/*#define RS485_POS 0x00         // Encoder position reading command
#define RESOLUTION 14            // Encoder resolution (14-bit)
#define PPR 16384                // Pulses per revolution (2^14 = 16384)
#define PULLEY_DIAMETER 0.059    // Pulley diameter (meters)
#define ENCODER_ADDRESS 0x54     // Default encoder address

// Define RS-485 pins for encoder
//#define RS485_ENCODER_RE_DE A8   // RE/DE of Encoder*/

// Define RS-485 pins for VFD
#define RS485_VFD_RE_DE A7      // RE/DE Control pin for RS-485 transmission and receive

// Define sensor and motor control pins
#define S801S_PIN A9          // 801S Vibration sensor pin
#define IN1 13                // Motor control pin 1
#define IN2 12                // Motor control pin 2
#define IN3 11                // Motor control pin 3
#define IN4 10                // Motor control pin 4
#define ENA 7                 // PWM speed control for motor A
#define ENB 6                 // PWM speed control for motor B
#define DS18B20_PIN 2         // DS18B20 temperature sensor pin

// Define Modbus register addresses for VFD
const int frequencyRegister = 0x0705;         // Register for setting VFD frequency
const int operationCommandRegister = 0x0706;  // Register for starting/stopping VFD
const int outputFrequencyRegister = 0x0809;   // Register for reading output frequency
const int outputVoltageRegister = 0x080C;     // Register for reading output voltage

// Create Modbus object
ModbusMaster node;

// Define variable to store data
/*unsigned long previousPosition = 0; // The previous position of the encoder
unsigned long currentPosition = 0;    // The current location of encoder
unsigned long previousTime = 0;         // previous measurement time
float conveyorSpeed = 0.0;           // Conveyor speed*/
float outputFrequency = 0.0;          // output frequency of VFD
float outputVoltage = 0.0;            // output voltage of VFD

/*unsigned long previousPosition = 0; // The previous position of the encoder
unsigned long currentPosition = 0;    // The current pocation of encoder
unsigned long previousTime = 0;       // previous measurement time of position
float conveyorSpeed = 0.0;            // Conveyor speed from encoder*/
float outputFrequency = 0.0;          // output frequency of VFD
float outputVoltage = 0.0;            // output voltage of VFD

// Define parameters for velocity
/*const float P_motor = 2;      // Polar number of the motor
const float G_ratio = 30;       // Gearbox ratio
const float D_roller = 0.059;   // Roller diameter (m)
float conveyorSpeedFromFormula; // Mechanical conveyor speed*/

// Create OneWire and DallasTemperature objects for temperature sensor
OneWire oneWire(DS18B20_PIN);
DallasTemperature sensors(&oneWire);

// Global variable to store current command mode
String currentMode = "FanAuto";  // Default mode
boolean isFanRunning = false;    // Fan status

// PID control variables
double setPoint = 30.0; // Desired temperature
double input, output;
double Kp = 10, Ki = 0.1, Kd = 1; // PID tuning parameters

// Create PID object
PID tempPID(&input, &output, &setPoint, Kp, Ki, Kd, DIRECT);

void setup() {
  // Install communication
  Serial.begin(BAUDRATE); // Initialize Serial Monitor
  //Serial1.begin(ENCODER_BAUDRATE, SERIAL_8N1); // Initialize Serial1 for RS-485 communication
  Serial2.begin(VFD_BAUDRATE, SERIAL_8E1);       // Initialize Serial2 for RS-485 communication

  // Initialize RS-485 for encoder
  //pinMode(RS485_ENCODER_RE_DE, OUTPUT);
  //digitalWrite(RS485_ENCODER_RE_DE, LOW);

  // Initialize RS-485 for VFD
  pinMode(RS485_VFD_RE_DE, OUTPUT);
  digitalWrite(RS485_VFD_RE_DE, LOW);

  // Initialize vibration sensor and driver motor
  pinMode(S801S_PIN, INPUT);
  pinMode(ENA, OUTPUT);
  pinMode(IN1, OUTPUT);
  pinMode(IN2, OUTPUT);
  pinMode(ENB, OUTPUT);
  pinMode(IN3, OUTPUT);
  pinMode(IN4, OUTPUT);

  sensors.begin(); // Initialize temperature sensor

  // Configure Modbus communication
  node.begin(1, Serial2); // Set Modbus slave ID 1
  node.preTransmission(preTransmission);
  node.postTransmission(postTransmission);

  // Configure PID
  tempPID.SetMode(AUTOMATIC);       // Enable PID control
  tempPID.SetOutputLimits(0, 242);  // 0 to 242 (95% of 255) for PWM output
  tempPID.SetSampleTime(1000);      // Update every 1 second
}

void loop() {
  // Read the Encoder position every 400ms
  /*unsigned long currentTime = millis();
  if (currentTime - previousTime >= 400) {
    currentPosition = getEncoderPosition();
    conveyorSpeed = calculateSpeed(currentPosition, previousPosition, previousTime, currentTime);
    previousPosition = currentPosition;
    previousTime = currentTime;
  }*/

  // Read and process serial commands
  if (Serial.available()) {
    String input = Serial.readStringUntil('\n'); // Enter commands
    if (input.startsWith("Fan")) {
      currentMode = input;  // Store the mode
      processFanCommand(input); // Input for fan control
    } else {
      processSerialCommand(input); // Input for VFD control
    }
  }

  executeFanMode(); // Execute the current mode of fan
  readVFDStatus(); // Read VFD data
  float temperature = readTemperature(); // Read temperature sensor
  long vibrationValue = readVibration(); // Read vibration sensor
  //calculateConveyorSpeed(); // Mechanical calculation of conveyor speed

  // Display output information to the ui
  Serial.print("F: ");
  Serial.print(outputFrequency);
  Serial.print(", ");

  Serial.print("V: ");
  Serial.print(outputVoltage);
  Serial.print(", ");

  Serial.print("T: ");
  Serial.print(temperature);
  Serial.print(", ");

  Serial.print("V: ");
  Serial.print(vibrationValue);
  Serial.print(", ");

  Serial.print("F: ");
  if (isFanRunning) {
    Serial.println("Run");
  } else {
    Serial.println("Stop");
  }

  delay(10);
}

// Read temperature sensor
float readTemperature() {
  sensors.requestTemperatures();
  return sensors.getTempCByIndex(0);
}

// Read vibration sensor
long readVibration() {
  int analogValue = analogRead(S801S_PIN);
  return analogValue;
}

// Execute fan mode
void executeFanMode() {
  if (currentMode == "FanFull" && !isFanRunning) {
    runFansFullSpeed();
    isFanRunning = true;
  }
  else if (currentMode == "FanStop" && isFanRunning) {
    stopFans();
    isFanRunning = false;
  }
  else if (currentMode == "FanAuto") {
    input = readTemperature(); // Get current temperature
    tempPID.Compute(); // Compute PID output

    // Apply PID output to fans
    if (output > 0) {
      runFansWithPID(output);
      isFanRunning = true;
    } else {
      stopFans();
      isFanRunning = false;
    }
  }
}

// Processing commands for fan Control
void processFanCommand(String input) {
  currentMode = input;  // Store the mode
  
  if (input == "FanStop") {
    stopFans();
    isFanRunning = false;
  }
  else if (input == "FanFull") {
    runFansFullSpeed();
    isFanRunning = true;
  }
  // FanAuto will be handled in executeFanMode()
}

// Stop fans
void stopFans() {
  // Stop both motors
  digitalWrite(IN1, LOW);
  digitalWrite(IN2, LOW);
  digitalWrite(IN3, LOW);
  digitalWrite(IN4, LOW);

  // Turn off the PWM signal
  analogWrite(ENA, 0);
  analogWrite(ENB, 0);
}

void runFansFullSpeed() {
  // Start in the right direction
  digitalWrite(IN1, HIGH);
  digitalWrite(IN2, LOW);
  digitalWrite(IN3, HIGH);
  digitalWrite(IN4, LOW);

  // Maximum speed
  analogWrite(ENA, 242); // Tốc độ 95% cho động cơ A
  analogWrite(ENB, 242); // Tốc độ 95% cho động cơ B
}

// Control fan speed with PID
void runFansWithPID(int pidOutput) {
  // Start in the right direction
  digitalWrite(IN1, HIGH);
  digitalWrite(IN2, LOW);
  digitalWrite(IN3, HIGH);
  digitalWrite(IN4, LOW);

  // Apply PID-calculated speed to both fans
  analogWrite(ENA, pidOutput);
  analogWrite(ENB, pidOutput);
}

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

/*
// The control of the encoder and the speed calculations have been hidden
// Mechanical formula for speed
void calculateConveyorSpeed() {
  conveyorSpeedFromFormula = (PI * D_roller * ((120 * outputFrequency / P_motor) / G_ratio)) / 60;
}*/


// Control RS-485 for encoder
/*void setStateRS485(uint8_t re_de_pin, bool state) {
  digitalWrite(re_de_pin, state);
}*/

// Read the Encoder position and check your checksum
/*unsigned long getEncoderPosition() {
  setStateRS485(RS485_ENCODER_RE_DE, HIGH); // Transmission mode
  delayMicroseconds(100); // wait for feedback
  Serial1.write(ENCODER_ADDRESS + RS485_POS); // Send command to read the position
  setStateRS485(RS485_ENCODER_RE_DE, LOW); // Switch to the receiving mode
  delayMicroseconds(100); // wait for feedback 

  if (Serial1.available() >= 2) {
    uint8_t lowByte = Serial1.read();
    uint8_t highByte = Serial1.read();
    unsigned int rawData = (highByte << 8) | lowByte;

    // Check the checksum
    if (verifyChecksumRS485(rawData)) {
      rawData &= 0x3FFF; // Remove Checksum10
      if (RESOLUTION == 12) rawData = rawData >> 2; // If the Encoder is 12-bit, go down 2 bits

      return rawData;
    } else {
      //Serial.println("Checksum error: Invalid encoder data.");
      resetRS485Connection(); // Reset RS-485 if there is an error Checksum
      return 0; // Returns 0 if checksum is wrong
    }
  } else {
    //Serial.println("Error: No response from encoder.");
    resetRS485Connection(); Reset RS-485 if there is an error Checksum
    return 0; // Returns 0 if checksum is wrong
  }
}

// Check the checksum
bool verifyChecksumRS485(uint16_t message) {
  uint16_t checksum = 0x3; // Checksum is the reversal of XOR bits, starting with 0b11
  for (int i = 0; i < 14; i += 2) {
    checksum ^= (message >> i) & 0x3;
  }
  return checksum == (message >> 14);
}

// Calculate the speed of the conveyor
float calculateSpeed(unsigned long currentPosition, unsigned long previousPosition, unsigned long previousTimeMs, unsigned long currentTimeMs) 
{
  float deltaTime = (currentTimeMs - previousTimeMs) / 1000.0;
  if (deltaTime == 0) return 0; // Prevent division by zero
  long deltaPosition = abs(currentPosition - previousPosition); // Calculate the absolute position change

  // Treatment of overflowing when the encoder turns all around
  if (deltaPosition < -(PPR / 2)) { // Chuyển từ giá trị cao nhất về thấp nhất
    deltaPosition += PPR;
  } else if (deltaPosition > (PPR / 2)) { // Chuyển từ giá trị thấp nhất về cao nhất
    deltaPosition -= PPR;
  }

  // Convert Delta position into number of rotation
  float revolutions = (float)deltaPosition / PPR;

  // Calculation of angular speed (rad/s)
  float angularVelocity = revolutions * 2 * PI / deltaTime;

  // Calculate the linear speed (m/s)
  float linearSpeed = angularVelocity * PULLEY_DIAMETER / 2;

  // Returns the speed
  return abs(linearSpeed);
}*/


// Encoder's Reset serial port function
/*void resetRS485Connection() {
  digitalWrite(RS485_ENCODER_RE_DE, HIGH); // Turn off the receiver mode, do not receive data
  Serial1.end();    // Turn off the Serial1 communication of encoder
  delay(3000);      // Wait
  Serial1.begin(ENCODER_BAUDRATE, SERIAL_8N1); // Open the serial port
}*/