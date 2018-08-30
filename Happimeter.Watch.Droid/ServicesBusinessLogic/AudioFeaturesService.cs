using System.Diagnostics;
using Happimeter.Watch.Droid.openSMILE;
using System.Json;
using Happimeter.Core.Helper;
using Happimeter.Core.Services;
using Java.Lang;
using System.Collections.Generic;
using Happimeter.Core.Database;
using System;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Happimeter.Watch.Droid.Workers;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
    public class AudioFeaturesService : IAudioFeaturesService
    {
        
        private const string IsActiveKey = "FEATURE_EXTRACTION_ACTIVE";
        private bool IsInitialized = false;

        //private bool IsPaired
        //{
        //    get { return !!ServiceLocator.Instance.Get<IDatabaseContext>().GetCurrentBluetoothPairing();  } 
        //}
        public bool IsActive
        {
            get
            {

                return ServiceLocator.Instance.Get<IConfigService>().GetConfigValueByKey(IsActiveKey) == "yes";
            }
            set
            {
                //if (!IsPaired) {
                //    Debug.WriteLine("Cannot start Audio Feature extraction since")
                //}
                try
                {
                    if (IsActive && !value)
                    {
                        Stop();
                    }
                    else if (!IsActive && value)
                    {
                        // start
                        Start();
                    }
                }
                finally
                {
                    ServiceLocator.Instance.Get<IConfigService>().AddOrUpdateConfigEntry(IsActiveKey, value ? "yes" : "no");
                }


            }
        }

        public void Toggle()
        {
            IsActive = !IsActive;

        }


        public void OnApplicationStartup() {
            if (IsActive) {
                Start();
            }
        }

        private void Start() {
            if (!IsInitialized)
            {
                Initialize();
                IsInitialized = true;
            }
            Smile.Run();
        }

        private void Stop() {
            Smile.Stop();            
        }



        [JsonObject]
        private class SmileMessage
        {
            [JsonObject]
            public class floatData {
                [JsonProperty("0")]
                public float Vad { get; set; }
            }

            [JsonProperty("msgname", Required = Required.Always)]
            public string Msgname { get; set; }
            [JsonProperty("msgtype", Required = Required.Always)]
            public string Msgtype { get; set;  }
            [JsonProperty("smileTime")]
            public float SmileTime { get; set; }
            [JsonProperty("floatData")]
            public floatData FloatData { get; set; }
        }

        private class FeatureListener : SmileJNI.Listener
        {
            public void onSmileMessageReceived(string text)
            {
                var json = text.Replace("(null)", "null");

                SmileMessage msg = Newtonsoft.Json.JsonConvert.DeserializeObject<SmileMessage>(json);

                var dbRecord = new AudioFeatures();
                dbRecord.Vad = msg.FloatData.Vad;
                dbRecord.Timestamp = DateTime.UtcNow;

                /**
                 * TODO: Push record to database
                 */


                // BluetoothWorker.GetInstance().SendNotification(UuidHelper.DataExchangeNotifyCharacteristicUuid, new DataExchangeInitMessage());

                                     
            }
        }

        private void Initialize()
        {

            var Fl = new FeatureListener();
            Smile.RegisterListener(Fl);
            Debug.WriteLine("Registered audio feature listener");
        }


    }
}