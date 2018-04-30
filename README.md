# Introduction
This repository is part of the happimeter project. Within this repository you will find the smartphone app that is used to to display data from the happimeter server. Additionally, it is used in order to pair with a Android Smartwatch (NOT ANDROID WEAR). This repository also contains the code of the Android Smartwatch application.

# Affiliation
This Project was developed as part of the Happimeter project at MIT Center for Collective Intelligence.

# Getting Started Android Watch

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

# Build and Test
In order to build and run the project you need to have Visual Studio for Mac (Or visual Studio on Windows) with the xamarin package installed.

# Project Structure
The whole solution consists of 5 Projects at the moment. There are three different projects one for each platform we are supporting:
- <b>Happimeter.iOS:</b> Contains the native code for the iOS platform
- <b>Happimeter.Droid:</b> Contains the native code for Android platforms
- <b>Happimeter.Watch.Droid:</b> Contains the native code for the android watch

Additionally the solution contains two shared code libries. This libraries allow to share code between the different native platforms.
- <b>Happimeter:</b> This project contains code that is shared only between the Happimeter.iOS and the Happimeter.Droid platforms. It contains most of the UI of the smartphone app. The UI is build via Xamarin forms with xaml and code behind files. The projects follow the MVVM (Model-View-ViewModel) Software design pattern. 
- <b>Happimeter.Core:</b> This projects contains code that is shared between all three native platfrom projects. It contains most of the database related classes, since they are almost identical for all three apps. In this project also the Bluetooth Messages are defined that are used to exchange data between Watch and Phone.

# Bluetooth FAQ
The communication between watch and phone is handled via Bluetooth LE. The Watch runs a so-called Gatt-Server, which can handle write operations to the watch and read operation from the watch. The watch can also push data from the watch to the phone with so-called Notifications.  It is worth taking a look at the basics of Bluetooth LE in order to understand the fundamentals of how the technology works (Gatt-Server, Services and characteristics should be unterstood).
Since Bluetooth LE itself is stateless and has a strongly limited bandwith per read/write/notification operation (between 20 and 550 bytes per operation), we needed to implement a mechanism that allows us to send larger data packets. The main logic for this can be found in the `ReadHostContext.cs` and `WriteReceiverContext.cs` classes. 
