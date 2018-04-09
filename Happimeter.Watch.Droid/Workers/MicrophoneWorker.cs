using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Media;
using Happimeter.Core.Database;
using Happimeter.Watch.Droid.Database;
using System.Diagnostics;
using Java.Lang;
using System.Threading;

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

        private CancellationTokenSource _cancelationTokenSource { get; set; }

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

        public async void StartFor(int seconds)
        {
            _cancelationTokenSource = new CancellationTokenSource();
            IsRunning = true;
            await RunAsync(seconds);
        }

        public void Start()
        {
            _cancelationTokenSource = new CancellationTokenSource();
            IsRunning = true;
            RunAsync();
        }

        public void Stop()
        {
            IsRunning = false;
            _cancelationTokenSource.Cancel();
        }

        private async Task RunAsync(int? seconds = null)
        {
            int runFor = RecordingDurationSecRunning;
            var stopAfterSeconds = false;
            if (seconds != null)
            {
                runFor = seconds.Value;
                stopAfterSeconds = true;
            }


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
                    if (stopwatch.Elapsed.Seconds > runFor)
                    {
                        stopwatch.Stop();
                        stopwatch.Reset();
                        if (stopAfterSeconds)
                        {
                            break;
                        }
                        await Task.Delay(TimeSpan.FromSeconds(RecordingDurationSecPausing), _cancelationTokenSource.Token);
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
                        _cancelationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                    var volume = CalculateVolumeForData(bigAudioBuffer.ToArray());
                    var measure = new MicrophoneMeasurement
                    {
                        Volumne = volume,
                        TimeStamp = DateTime.UtcNow
                    };
                    MicrophoneMeasures.Add(volume);
                    record.Stop();
                    await Task.Delay(TimeSpan.FromSeconds(RecordingDurationSecSample), _cancelationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Operation was cancelled");
                //return without further doing
                return;
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"Error while Getting Microphone measurement. Stopping the worker. Error Message {e.Message}");
                IsRunning = false;
            }

            Stop();
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
            var rms = System.Math.Sqrt(meanSquare);
            var volume = rms / 32768.0;

            return volume;
        }


    }
}
