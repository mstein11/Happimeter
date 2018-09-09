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
using System.Collections.Concurrent;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
    public class AudioFeaturesService : IAudioFeaturesService
    {
        public ConcurrentBag<double> VadMeasures { get; set; } = new ConcurrentBag<double>();
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


        private bool CalledAppStartup = false;
        public void OnApplicationStartup()
        {
            if (!CalledAppStartup)
            {
                CalledAppStartup = true;
            }
            else
            {
                return;
            }
            if (IsActive)
            {
                //Start();
            }
        }

        private void Start()
        {
            if (!IsInitialized)
            {
                Initialize();
                IsInitialized = true;
            }
            Smile.Run();
        }

        private void Stop()
        {
            Smile.Stop();
        }



        [JsonObject]
        private class SmileMessage
        {
            [JsonObject]
            public class floatData
            {
                [JsonProperty("0")]
                public float Vad { get; set; }
            }

            [JsonProperty("msgname", Required = Required.Always)]
            public string Msgname { get; set; }
            [JsonProperty("msgtype", Required = Required.Always)]
            public string Msgtype { get; set; }
            [JsonProperty("smileTime")]
            public float SmileTime { get; set; }
            [JsonProperty("floatData")]
            public floatData FloatData { get; set; }
        }

        private class FeatureListener : SmileJNI.Listener
        {
            private readonly IAudioFeaturesService _service;
            public FeatureListener(AudioFeaturesService service)
            {
                _service = service;
            }
            public void onSmileMessageReceived(string text)
            {
                var json = text.Replace("(null)", "null");

                SmileMessage msg = Newtonsoft.Json.JsonConvert.DeserializeObject<SmileMessage>(json);

                var dbRecord = new AudioFeatures();
                dbRecord.Vad = msg.FloatData.Vad;
                dbRecord.Timestamp = DateTime.UtcNow;

                _service.VadMeasures.Add(dbRecord.Vad);
                // BluetoothWorker.GetInstance().SendNotification(UuidHelper.DataExchangeNotifyCharacteristicUuid, new DataExchangeInitMessage());


            }
        }

        private void Initialize()
        {
            var Fl = new FeatureListener(this);
            Smile.RegisterListener(Fl);
            Debug.WriteLine("Registered audio feature listener");
        }
    }
}