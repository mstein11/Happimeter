using System;
using System.IO;
using Android.App;
using Android.Media.Audiofx;
//using Plugin.Permissions;
//using Plugin.Permissions.Abstractions;
using System.Threading.Tasks;
using Android.Runtime;



namespace Happimeter.Watch.Droid.openSMILE
{
    public static class Smile
    {


        public static void Setup(Activity context)
        {

            // JNI onload
            SmileJNI.Load();


            // must copy all config file to cache directory, s.t. openSMILE can access them
            string cacheDir = context.CacheDir.AbsolutePath;
            Directory.CreateDirectory(cacheDir);


            string configFileName = "openSMILE.conf";
            string configFileNewPath = Path.Combine(cacheDir, configFileName);

            CopyAssetTo(context, configFileName, configFileNewPath);

            // call SMILEExtract
            SmileJNI.SMILExtractJNI(configFileNewPath, 1);


        }


        static void CopyAssetTo(Activity context, string assetName, string newPath)
        {

            var readStream = context.Assets.Open(assetName);
            FileStream writeStream = new FileStream(newPath, FileMode.OpenOrCreate, FileAccess.Write);

            
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
