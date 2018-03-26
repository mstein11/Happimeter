using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Media;
using Happimeter.Core.Database;
using Happimeter.Watch.Droid.Database;
using System.Diagnostics;

namespace Happimeter.Watch.Droid.Workers
{
    public class MicrophoneWorker : AbstractWorker
    {
        public ConcurrentBag<double> MicrophoneMeasures = new ConcurrentBag<double>();

        private const int SampleRate = 8000;
        private const ChannelIn Channel = ChannelIn.Mono;
        private const Encoding AudioEncoding = Encoding.Pcm16bit;
        private const int RecordingDurationSecSample = 1;
        private const int RecordingDurationSecRunning = 60;
        private const int RecordingDurationSecPausing = 60;

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

        public override void Start()
        {
            IsRunning = true;
            RunAsync();
        }

        public void StartOnce()
        {
            IsRunning = true;
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


                record.StartRecording();
                var bytesRead = 0;
                var bigAudioBuffer = new List<short>();
                while (bytesRead < SampleRate * RecordingDurationSecSample)
                {

                    int numberOfShort = record.Read(audioBuffer, 0, audioBuffer.Count());
                    bytesRead += numberOfShort;
                    for (var i = 0; i < numberOfShort; i++)
                    {
                        bigAudioBuffer.Add(audioBuffer[i]);
                    }
                }
                var volume = CalculateVolumeForData(bigAudioBuffer.ToArray());
                var measure = new MicrophoneMeasurement
                {
                    Volumne = volume,
                    TimeStamp = DateTime.UtcNow
                };
                MicrophoneMeasures.Add(volume);
                record.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while Getting Microphone measurement. Stopping the worker. Error Message {e.Message}");
                IsRunning = false;
            }
        }

        public override void Stop()
        {
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

                var stopwatch = new Stopwatch();
                while (IsRunning)
                {
                    if (stopwatch.Elapsed.Seconds > RecordingDurationSecRunning)
                    {
                        stopwatch.Stop();
                        stopwatch.Reset();
                        await Task.Delay(TimeSpan.FromSeconds(RecordingDurationSecPausing));
                    }
                    if (!stopwatch.IsRunning)
                    {
                        stopwatch.Start();
                    }
                    record.StartRecording();
                    var bytesRead = 0;
                    var bigAudioBuffer = new List<short>();
                    while (bytesRead < SampleRate * RecordingDurationSecSample)
                    {

                        int numberOfShort = record.Read(audioBuffer, 0, audioBuffer.Count());
                        bytesRead += numberOfShort;
                        //System.Diagnostics.Debug.WriteLine(string.Concat(audioBuffer));
                        for (var i = 0; i < numberOfShort; i++)
                        {
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
                    //ServiceLocator.Instance.Get<IDatabaseContext>().Add(measure);
                    MicrophoneMeasures.Add(volume);
                    record.Stop();
                    await Task.Delay(TimeSpan.FromSeconds(RecordingDurationSecSample));
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
