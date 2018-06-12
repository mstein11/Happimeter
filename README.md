# Introduction
This repository is part of the happimeter project. 
This repository contains the happimeter smartphone app and the happimeter android watch app. 

The happimeter is capable of gathering the data from different sensors of a smartwatch and send them to the happimeter server.
The following sensor data can be collected:

| Sensor | Description|
|---|---|
| Accelerometer | Measures the physical activity of a user on three axis. |
| Heartrate sensor | Measures the heartrate of a user. |
| Step Counter | Measures how many steps a user takes over the course of a day. |
| Microphone | Measures the noise level around an user. No audio data are recorded, only a measure between 0 and 1 representing the noise level is saved. In the future, we want to distinguish between human voice and background noise to better capture instances of communication. |
| Bluetooth | Measures the proximity of a user to other users or points of interest. |

Running the happimeter smartwatch app drains the battery of such a device. The happimeter is capable to run in two distinct modes:
1. <b>Battery-safer mode:</b> While running in battery safer mode, measuremnts are taken from the senors in five minute intervals. The battery usually lasts between 8 and 12 hours.
2. <b>Continous mode:</b> The watch continously takes sensor measurements. It aggregates and stores the taken measures in one minute intervals. The battery lasts around 5 hours.



# Technical Information

## Getting Started Android Watch

1. Get the edu.mit.android_watch.apk file
2. Install Adb on your computer
3. Activate USB-Debugging on your watch
   - On Home Screen Swipe to the right
   - Navigate to Settings
   - Navigate to About Watch
   - Navigate to Developer Options
   - Activate USB Debugging
4. Connect the watch to your laptop
5. If this is the first time you connect the Watch, you might need to add the computer to the list of Trusted Devices (A Dialog will pop-up, you have to accept)
6. On the terminal run: `adb install <Path to Apk>` (If the app is already installed on the watch, you have to add the option -r)
7. Start the app

## Build and Test
In order to build and run the project you need to have Visual Studio for Mac (Or visual Studio on Windows) with the xamarin package installed.

## Project Structure
The whole solution consists of 5 Projects at the moment. There are three different projects one for each platform we are supporting:
- <b>Happimeter.iOS:</b> Contains the native code for the iOS platform
- <b>Happimeter.Droid:</b> Contains the native code for Android platforms
- <b>Happimeter.Watch.Droid:</b> Contains the native code for the android watch

Additionally the solution contains two shared code libries. This libraries allow to share code between the different native platforms.
- <b>Happimeter:</b> This project contains code that is shared only between the Happimeter.iOS and the Happimeter.Droid platforms. It contains most of the UI of the smartphone app. The UI is build via Xamarin forms with xaml and code behind files. The projects follow the MVVM (Model-View-ViewModel) Software design pattern. 
- <b>Happimeter.Core:</b> This projects contains code that is shared between all three native platfrom projects. It contains most of the database related classes, since they are almost identical for all three apps. In this project also the Bluetooth Messages are defined that are used to exchange data between Watch and Phone.

## Bluetooth FAQ
The communication between watch and phone is handled via Bluetooth LE. The Watch runs a so-called Gatt-Server, which can handle write operations to the watch and read operation from the watch. The watch can also push data from the watch to the phone with so-called Notifications.  
<br>
### Gatt Server and related logic
It is worth taking a look at the basics of Bluetooth LE in order to understand the fundamentals of how the technology works (Gatt-Server, Services and characteristics should be unterstood).
The watch initializes a GattServer in `BluetoothWorker.cs` class. The GattServer can have two services depending on the mode the watch is currently running in:
- <b>HappimeterAuthService (HappimeterAuthService.cs)</b>: This service is avaiable on the GattServer before the watch is paired to the phone. By Writing to the Characteristics of this Service, you can pair the watch to a phone. Only after the watch is paired, it will start to collect data.
- <b>HappimeterService (HappimeterService.cs)</b>: This service is avaiable on the GattServer after the watch was paired to the phone. It contains  the following characteristics:
  - <b>HappimeterMeasurementModeCharacteristic.cs</b>: used to change the measurement mode the watch is running in (BatterySaferMode: one measurement per 10 minutes; Continuous Mode: One measurement every minute).
  - <b>HappimeterDataChacteristic.cs</b>: used to send the survey responses and measurements from the watch to the phone
  - <b>HappimeterGenericQuestionCharacteristic.cs</b>: used to push the additional questions to the watch, after they were downloaded in the settings tab of the smart phone app

### Automatic Data Exchange between Watch and Phone
In order to make phone and watch exchange data automatically, even if the smartphone app is not running in the background, we make the watch emit and iBeacon bluetooth signal. If the smartphone (especially on iOS) detects the ibeacon signal, it will start up the happimeter app and in turn, the happimeter app initializes the data exchange between watch and phone. It can take up to 15 minutes for the phone to detect the iBeacon signal. In order to account for this limitation we make the watch advertise as ibeacon for 20 minutes and then stop the advertising for 20 minutes before we start to advertise again. The classes that contain the code for this mechanism are: `BeaconWorker.cs` (watch) and `BeaconWakeupSerivce.cs` (one implementation for iOS and Android each)

### Bandwith and Large Read and Write Operations
Since Bluetooth LE itself is stateless and has a strongly limited bandwith per read/write/notification operation (between 20 and 550 bytes per operation), we needed to implement a mechanism that allows us to send larger data packets. The main logic for this can be found in the `ReadHostContext.cs` and `WriteReceiverContext.cs` classes. 
<br>
In order to send larger data packages we splitt them into serveral Read/Write Operations. In order to initiate a data exchange, the first message that is send contains a header of 20 bytes. The header contains the message name and the message size in bytes. 


## Measurements
### Collection of Measurements on the watch
The watch is capable of collecting data through two different methods: <b>Continuous Mode</b> and <b>Battery Safer Mode</b>. In Continous Mode the watch lasts up to 5 hours and is collecting measurements in one minute intervals (Average, std.dev. etc of all measure during one minute) with the sensor collecting data continously. The Battery safer mode is currently configured to collect a measuremnt every five minute, however during those five minute, the sensors are collecting data only for 60 seconds, the other four minutes the watch can enter a hibernate mode.

### MicrophoneWorker, MeasurementWorker and BluetoothScannerWorker
The logic for calculating, aggregating and storing the measures taken from the different sensors of the watch can be found in the classes MeasurementWorker.cs, MicrophoneWorker.cs and BluetoothScannerWorker.cs. 

# Affiliation
This Project was developed as part of the Happimeter project at MIT Center for Collective Intelligence.