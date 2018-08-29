using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Android.Runtime;
using Java.Interop;
using Java.Lang;

namespace Happimeter.Watch.Droid.openSMILE
{

    [Register("edu.mit.Happimeter_Watch_Droid.SmileJNI")]
    public class SmileJNI : Java.Lang.Object {


        public static void Load() {
            
            JavaSystem.LoadLibrary("smile_jni");
      
        }

        [DllImport("smile_jni")]
        private static extern void Java_com_audeering_opensmile_androidtemplate_SmileJNI_SMILExtractJNI(
            IntPtr env,
            IntPtr jniClass,
            IntPtr configfile,
            IntPtr updateProfile
        );

        [DllImport("smile_jni")]
        private static extern void Java_com_audeering_opensmile_androidtemplate_SmileJNI_SMILEndJNI(
            IntPtr env,
            IntPtr jniClass,
            IntPtr nll
        );


        public static void SMILExtractJNI(
            string configfile,
            int updateProfile
        ) {

            IntPtr env = JNIEnv.Handle;
            IntPtr jniClass = JNIEnv.FindClass(typeof(SmileJNI));


            try
            {
                using (var java_configfile = new Java.Lang.String(configfile)) 
                using(var java_updateProfile = new Java.Lang.Integer(updateProfile))
                {
                    Java_com_audeering_opensmile_androidtemplate_SmileJNI_SMILExtractJNI(
                        env,
                        jniClass,
                        java_configfile.Handle,
                        java_updateProfile.Handle
                    );
                }
            } finally {
  
                JNIEnv.DeleteGlobalRef(jniClass);

            }
        }


        public static void SMILEndJNI()
        {

            IntPtr env = JNIEnv.Handle;
            IntPtr jniClass = JNIEnv.FindClass(typeof(SmileJNI));


            try
            {

                using (var java_nptr = new Java.Lang.Long(0))
                {
                    Java_com_audeering_opensmile_androidtemplate_SmileJNI_SMILEndJNI(
                        env,
                        jniClass,
                        java_nptr.Handle
                    );
                }

            }
            finally
            {

                JNIEnv.DeleteGlobalRef(jniClass);

            }
        }



        public interface Listener
        {
            void onSmileMessageReceived(string text);
        }
        private static Listener listener_;
        public static void RegisterListener(Listener listener)
        {
            SmileJNI.listener_ = listener;
        }

        /**
         * this is the first method called by openSMILE binary. it redirects the call to the Android
         * app activity.
         * @param text JSON encoded string
         */
        [Export]
        public static void ReceiveMessage(Java.Lang.String text)
        {
            Debug.WriteLine("Incoming message");
            if (listener_ != null)
                listener_.onSmileMessageReceived((string)text);
        }

    }
}

