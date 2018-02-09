using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Media;
using Happimeter.Core.Database;
using Happimeter.Watch.Droid.Database;

namespace Happimeter.Watch.Droid.Workers
{
    public class MicrophoneWorker : AbstractWorker
    {
        private const int SampleRate = 8000;
        private const ChannelIn Channel = ChannelIn.Mono;
        private const Encoding AudioEncoding = Encoding.Pcm16bit;
        private const int RecordingDurationSec = 1;

        private static MicrophoneWorker Instance;

        public static MicrophoneWorker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new MicrophoneWorker();
            }

            return Instance;
        }

        //public bool IsRunning { get; private set; }

        private MicrophoneWorker()
        {
            
        }

        public override void Start() {
            IsRunning = true;
            RunAsync();
        }

        public override void Stop() {
            IsRunning = false;
        }

        private async Task RunAsync()
        {
            try
            {
                int bufferSize = AudioRecord.GetMinBufferSize(SampleRate,
                          Channel,
                          AudioEncoding);

                if (bufferSize == 0)
                {
                    bufferSize = SampleRate * 2;
                }
                short[] audioBuffer = new short[bufferSize / 2];
                AudioRecord record = new AudioRecord(AudioSource.Default,
                                                     SampleRate,
                                                     Channel,
                                                     AudioEncoding,
                                                     bufferSize);

                while (IsRunning)
                {
                    record.StartRecording();
                    var bytesRead = 0;
                    var bigAudioBuffer = new List<short>();
                    while (bytesRead < SampleRate * RecordingDurationSec)
                    {

                        int numberOfShort = record.Read(audioBuffer, 0, audioBuffer.Count());
                        bytesRead += numberOfShort;
                        //System.Diagnostics.Debug.WriteLine(string.Concat(audioBuffer));
                        for (var i = 0; i < numberOfShort; i++) {
                            bigAudioBuffer.Add(audioBuffer[i]);    
                        }
                    }
                    var volume = CalculateVolumeForData(bigAudioBuffer.ToArray());
                    //System.Diagnostics.Debug.WriteLine("Volume: " + volume);
                    var measure = new MicrophoneMeasurement
                    {
                        Volumne = volume,
                        TimeStamp = DateTime.UtcNow
                    };
                    ServiceLocator.Instance.Get<IDatabaseContext>().Add(measure);
                    record.Stop();
                    await Task.Delay(TimeSpan.FromSeconds(RecordingDurationSec));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while Getting Microphone measurement. Stopping the worker. Error Message {e.Message}");
                IsRunning = false;
            }
        }

        private double CalculateVolumeForData(short[] data)
        {
            long totalSquare = 0;
            for (var i = 0; i < data.Length; i += 2)
            {
                var sample = data[i];
                totalSquare += sample * sample;
            }
            double meanSquare = 2 * totalSquare / data.Length;
            var rms = Math.Sqrt(meanSquare);
            var volume = rms / 32768.0;

            return volume;
        }


    }
}
