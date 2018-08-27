using System;
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

        //[DllImport("smile_jni.so", EntryPoint = "SMILEndJNI")]
        //public static extern void SMILEndJNI();



        /**
         * process the messages from openSMILE (redirect to app activity etc.)
         */
        public interface Listener
        {
            void onSmileMessageReceived(string text);
        }
        private static Listener listener_;
        public static void registerListener(Listener listener)
        {
            listener_ = listener;
        }

        /**
         * this is the first method called by openSMILE binary. it redirects the call to the Android
         * app activity.
         * @param text JSON encoded string
         */
        [Export]
        public static void ReceiveMessage(Java.Lang.String text)
        {
            if (listener_ != null)
                listener_.onSmileMessageReceived("test");
        }

    }
}

