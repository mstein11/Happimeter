using System;
using System.IO;
using Android.App;
using Android.Media.Audiofx;
using System.Threading.Tasks;
using Android.Runtime;
using System.Collections.Generic;
using Microsoft.AppCenter;
using Android.Net.Wifi.Aware;

namespace Happimeter.Watch.Droid.openSMILE
{
    public static class Smile
    {

        public static bool IsRunning = false;
        private static bool IsInitialized = false;
        private static string MainConfigFilePath;

        public static void Initialize()
        {
            if (!IsInitialized)
            {
                _initialize();
                IsInitialized = true;
            }
        }

        public static void Run()
        {

            if (IsRunning)
            {
                return;
            }

            if (!IsInitialized)
            {
                _initialize();
                IsInitialized = true;
            }

            Task.Run(() =>
            {
                SmileJNI.SMILExtractJNI(MainConfigFilePath, 1);
            });

            IsRunning = true;

        }

        public static void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            Task.Run(() =>
            {
                SmileJNI.SMILEndJNI();
            });

            IsRunning = false;
        }


        private static void _initialize()
        {

            // JNI onload
            SmileJNI.Load();


            // must copy all config file to cache directory, s.t. openSMILE can access them
            string cacheDir = Application.Context.CacheDir.AbsolutePath;
            Directory.CreateDirectory(cacheDir);

            var mainConfigFileName = "openSMILE.conf";
            MainConfigFilePath = Path.Combine(cacheDir, mainConfigFileName);
            CopyAssetTo(mainConfigFileName, MainConfigFilePath);

            var additionalConfigFileNames = new List<string> {
                "BufferModeRb.conf.inc",
                "messages.conf.inc",
                "features.conf.inc",
                "lstmvad_rplp18d_12.net",
                "rplp18d_norm.dat"
            };

            foreach (string configFileName in additionalConfigFileNames)
            {
                string configFileNewPath = Path.Combine(cacheDir, configFileName);
                CopyAssetTo(configFileName, configFileNewPath);
            }

        }

        /**
         * process the messages from openSMILE (redirect to app activity etc.)
         */

        public static void RegisterListener(SmileJNI.Listener listener)
        {
            SmileJNI.RegisterListener(listener);
        }



        private static void CopyAssetTo(string assetName, string newPath)
        {

            var readStream = Application.Context.Assets.Open(assetName);
            FileStream writeStream = new FileStream(newPath, FileMode.Create, FileAccess.Write);


            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = readStream.Read(buffer, 0, Length);
            // write the required bytes
            while (bytesRead > 0)
            {
                writeStream.Write(buffer, 0, bytesRead);
                bytesRead = readStream.Read(buffer, 0, Length);
            }
            readStream.Close();
            writeStream.Close();

        }

    }
}
