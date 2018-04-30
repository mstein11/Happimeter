# Introduction
This repository is part of the happimeter project. Within this repository you will find the smartphone app that is used to to display data from the happimeter server. Additionally, it is used in order to pair with a Android Smartwatch (NOT ANDROID WEAR). This repository also contains the code of the Android Smartwatch application.

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

# Affiliation
This Project was developed as part of the Happimeter project at MIT Center for Collective Intelligence.

# Build and Test
In order to build and run the project you need to have Visual Studio for Mac (Or visual Studio on Windows) with the xamarin package installed.

# Project Structure
The whole solution consists of 5 Projects at the moment. There are three different projects one for each platform we are supporting:
- Happimeter.iOS: Contains the native code for the iOS platform
- Happimeter.Droid: Contains the native code for Android platforms
- Happimeter.Watch.Droid: Contains the native code for the android watch

Additionally the solution contains two shared code libries. This libraries allow to share code between the different native platforms.
- Happimeter: This project contains code that is shared only between the Happimeter.iOS and the Happimeter.Droid platforms. It contains most of the UI of the smartphone app. The UI is build via Xamarin forms with xaml and code behind files. The projects follow the MVVM (Model-View-ViewModel) Software design pattern. 
- Happimeter.Core: This projects contains code that is shared between all three native platfrom projects. It contains most of the database related classes, since they are almost identical for all three apps. In this project also the Bluetooth Messages are defined that are used to exchange data between Watch and Phone.
# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://www.visualstudio.com/en-us/docs/git/create-a-readme). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
