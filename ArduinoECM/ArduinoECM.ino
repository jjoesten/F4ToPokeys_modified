/*
 Name:		ArduinoECM.ino
 Created:	12/22/2023 2:55:45 AM
 Author:	joshu

 Columns are anode +
 Rows are cathode -
*/

#include <CmdMessenger.h>
#include <Button2.h>

constexpr auto CLOCK_PIN = 3;
constexpr auto LATCH_PIN = 4;
constexpr auto DATA_PIN = 2;

constexpr auto OFF_PIN = 0;
constexpr auto OPR_PIN = 10;

constexpr auto MODE1_PIN = 7;
constexpr auto MODE3_PIN = 8;

constexpr auto RESET_PIN = 5;
constexpr auto BIT_PIN = 6;

constexpr auto DIM_PIN = A0;
constexpr auto PWM_PIN = 11;

constexpr auto BUTTON_1_PIN = A5;
constexpr auto BUTTON_2_PIN = A4;
constexpr auto BUTTON_3_PIN = A3;
constexpr auto BUTTON_4_PIN = A2;
constexpr auto BUTTON_5_PIN = A1;
constexpr auto BUTTON_ALT_PIN = 13;
constexpr auto BUTTON_FRM_PIN = 12;
constexpr auto BUTTON_SPL_PIN = 9;

CmdMessenger cmdMessenger = CmdMessenger(Serial);

byte lightMatrix[6] = { B00000000, B00000000, B00000000, B00000000, B00000000, B00000000 };

Button2 oprButton;
Button2 offButton;
Button2 mode1Button;
Button2 mode3Button;
Button2 bitButton;
Button2 resetButton;
Button2 button1;
Button2 button2;
Button2 button3;
Button2 button4;
Button2 button5;
Button2 buttonAlt;
Button2 buttonFrm;
Button2 buttonSpl;

// List of recognized CmdMessenger callbacks
enum {
	kHandshakeRequest,
	kHandshakeResponse,
	kSetLed,
	kAction,
	kStatus,
};

void attachCommandCallbacks() {
	cmdMessenger.attach(onUnknownCallback);
	cmdMessenger.attach(kHandshakeRequest, onHandshake);
	cmdMessenger.attach(kSetLed, onSetLed);
}

void onUnknownCallback() {
	cmdMessenger.sendCmd(kStatus, "Unknown command");
}

void onHandshake() {
	cmdMessenger.sendCmdStart(kHandshakeResponse);
	cmdMessenger.sendCmdArg("ArduinoECM");
	cmdMessenger.sendCmdArg(getSerialNumber());
	cmdMessenger.sendCmdEnd();
}

void onSetLed() {
  unsigned int column = cmdMessenger.readInt16Arg();
  unsigned int row = cmdMessenger.readInt16Arg();
  unsigned int value = cmdMessenger.readBoolArg();

  bitWrite(lightMatrix[column], row, value);

  cmdMessenger.sendCmdStart(kStatus);
  cmdMessenger.sendCmdArg("LED_Set");
  cmdMessenger.sendCmdEnd();
}

void setupPins()
{
	pinMode(LATCH_PIN, OUTPUT);
	pinMode(CLOCK_PIN, OUTPUT);
	pinMode(DATA_PIN, OUTPUT);

	//pinMode(OFF_PIN, INPUT_PULLUP);
	//pinMode(OPR_PIN, INPUT_PULLUP);
	//pinMode(MODE1_PIN, INPUT_PULLUP);
	//pinMode(MODE3_PIN, INPUT_PULLUP);
	//pinMode(BIT_PIN, INPUT_PULLUP);
	//pinMode(RESET_PIN, INPUT_PULLUP);
	//pinMode(BUTTON_1_PIN, INPUT_PULLUP);
	//pinMode(BUTTON_2_PIN, INPUT_PULLUP);
	//pinMode(BUTTON_3_PIN, INPUT_PULLUP);
	//pinMode(BUTTON_4_PIN, INPUT_PULLUP);
	//pinMode(BUTTON_5_PIN, INPUT_PULLUP);
	//pinMode(BUTTON_ALT_PIN, INPUT_PULLUP);
	//pinMode(BUTTON_FRM_PIN, INPUT_PULLUP);
	//pinMode(BUTTON_SPL_PIN, INPUT_PULLUP);
}

void setupButtons()
{
	oprButton.begin(OPR_PIN);
	offButton.begin(OFF_PIN);
	mode1Button.begin(MODE1_PIN);
	mode3Button.begin(MODE3_PIN);
	bitButton.begin(BIT_PIN);
	resetButton.begin(RESET_PIN);
	button1.begin(BUTTON_1_PIN);
	button2.begin(BUTTON_2_PIN);
	button3.begin(BUTTON_3_PIN);
	button4.begin(BUTTON_4_PIN);
	button5.begin(BUTTON_5_PIN);
	buttonAlt.begin(BUTTON_ALT_PIN);
	buttonFrm.begin(BUTTON_FRM_PIN);
	buttonSpl.begin(BUTTON_SPL_PIN);

	oprButton.setPressedHandler(onPressedHandler);
	oprButton.setReleasedHandler(onReleasedHandler);
	offButton.setPressedHandler(onPressedHandler);
	offButton.setReleasedHandler(onReleasedHandler);
	mode1Button.setPressedHandler(onPressedHandler);
	mode1Button.setReleasedHandler(onReleasedHandler);
	mode3Button.setPressedHandler(onPressedHandler);
	mode3Button.setReleasedHandler(onReleasedHandler);
	bitButton.setPressedHandler(onPressedHandler);
	resetButton.setPressedHandler(onPressedHandler);
	button1.setPressedHandler(onPressedHandler);
	button2.setPressedHandler(onPressedHandler);
	button3.setPressedHandler(onPressedHandler);
	button4.setPressedHandler(onPressedHandler);
	button5.setPressedHandler(onPressedHandler);
	buttonAlt.setPressedHandler(onPressedHandler);
	buttonFrm.setPressedHandler(onPressedHandler);
	buttonSpl.setPressedHandler(onPressedHandler);
}

// the setup function runs once when you press reset or power the board
void setup() {

	setupPins();

	setupButtons();

	Serial.begin(57600);

	cmdMessenger.printLfCr();

	attachCommandCallbacks();

	cmdMessenger.sendCmd(kStatus, "Started");
}

// the loop function runs over and over again until power down or reset
void loop() {
	cmdMessenger.feedinSerialData();

	// Get and set dimming value
	int pwmValue = map(analogRead(DIM_PIN), 0, 1023, 230, 0);
	analogWrite(PWM_PIN, pwmValue);

	writeToRegisters();

	// Buttons
	oprButton.loop();
	offButton.loop();
	mode1Button.loop();
	mode3Button.loop();
	bitButton.loop();
	resetButton.loop();
	button1.loop();
	button2.loop();
	button3.loop();
	button4.loop();
	button5.loop();
	buttonAlt.loop();
	buttonFrm.loop();
	buttonSpl.loop();
}

String getSerialNumber() {
	return "ArduinoECM0001";
}

void writeToRegisters() {
  byte col = B00000001;
  for (int i = 0; i < 6; i++) {
    digitalWrite(LATCH_PIN, LOW);
    shiftOut(DATA_PIN, CLOCK_PIN, MSBFIRST, col);
    shiftOut(DATA_PIN, CLOCK_PIN, MSBFIRST, lightMatrix[i]);
    digitalWrite(LATCH_PIN, HIGH);
    col = col << 1;
    delay(2);
  }
}

void allLightsOff() {
  for (int i = 0; i < 6; i++) {
    lightMatrix[i] = B00000000;
  }
}

void onPressedHandler(Button2& btn) {

}

void onReleasedHandler(Button2& btn) {
	if (btn == oprButton || btn == offButton) {
		// Stby pressed
	}
	else if (btn == mode1Button || btn == mode3Button) {
		// Mode 2 pressed
	}
}
