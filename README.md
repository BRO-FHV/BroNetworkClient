CSharpLedController
===================

C# Application for controlling the LEDs of the BeagleBone via UDP packets




To communicate with the beaglebone we use a simple Protocoll:

To set the username, the application sends "UserName=" + userName via UDP to the BB;

To enable a LED, the application sends "EnableLED=" + ledNumber (1-4) via UDP to the BB;

To disable a LED, the application sends "DisableLED=" + ledNumber (1-4) vio UDP to the BB;

To inform the clients that a LED has changed, the BB sends "SetLED=" + ledNumber + ":" + "true" | "false" to the application

Every other sent or received Text are treatened as chatmessage.
