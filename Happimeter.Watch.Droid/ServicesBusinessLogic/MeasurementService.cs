using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.ViewModels;
using Android.Graphics;
using System.Reactive.Subjects;
using Happimeter.Core.Services;
using Happimeter.Watch.Droid.Workers;
using AltBeaconOrg.Bluetooth;
using Android.App;
using System.Threading.Tasks;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
    public class MeasurementService : IMeasurementService
    {
        public MeasurementService()
        {
        }

        public void AddSurveyMeasurement(SurveyMeasurement measurement)
        {
            var pairing = ServiceLocator.Instance.Get<IDeviceService>().GetBluetoothPairing();
            measurement.UserId = pairing.PairedWithUserId;
            var appVersion = ServiceLocator.Instance.Get<IDeviceService>().AppVersion();
            measurement.WatchAppVersion = appVersion;
            var batteryLevel = ServiceLocator.Instance.Get<IDeviceService>().BatteryPercent();
            measurement.WatchBatteryPercentage = batteryLevel;
            var dbContext = ServiceLocator.Instance.Get<IDatabaseContext>();
            dbContext.AddGraph(measurement);

            Task.Factory.StartNew(() =>
            {
                if (Android.OS.Looper.MyLooper() == null)
                {
                    Android.OS.Looper.Prepare();
                }

                var scanRes = BluetoothMedic.Instance.RunScanTest(Application.Context);
                var transRes = BluetoothMedic.Instance.RunTransmitterTest(Application.Context);
                if (!scanRes || !transRes)
                {
                    ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.BluetoothInBadState);
                }
                BluetoothWorker.GetInstance().SendNotification(UuidHelper.DataExchangeNotifyCharacteristicUuid, new DataExchangeInitMessage());
            });
        }

        public void AddSensorMeasurement(SensorMeasurement measurement)
        {
            var pairing = ServiceLocator.Instance.Get<IDeviceService>().GetBluetoothPairing();
            measurement.UserId = pairing.PairedWithUserId;
            var appVersion = ServiceLocator.Instance.Get<IDeviceService>().AppVersion();
            measurement.WatchAppVersion = appVersion;
            var batteryLevel = ServiceLocator.Instance.Get<IDeviceService>().BatteryPercent();
            measurement.WatchBatteryPercentage = batteryLevel;
            var dbContext = ServiceLocator.Instance.Get<IDatabaseContext>();
            dbContext.AddGraph(measurement);
            SaveInfoMeasurement(measurement);

            Task.Factory.StartNew(() =>
            {
                if (Android.OS.Looper.MyLooper() == null)
                {
                    Android.OS.Looper.Prepare();
                }

                var scanRes = BluetoothMedic.Instance.RunScanTest(Application.Context);
                var transRes = BluetoothMedic.Instance.RunTransmitterTest(Application.Context);
                if (!scanRes || !transRes)
                {
                    ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.BluetoothInBadState);
                }
                BluetoothWorker.GetInstance().SendNotification(UuidHelper.DataExchangeNotifyCharacteristicUuid, new DataExchangeInitMessage());
            });
        }

        public void SaveInfoMeasurement(SensorMeasurement measurement)
        {
            var dbContext = ServiceLocator.Instance.Get<IDatabaseContext>();
            var oldInstance = dbContext.GetAll<InfoScreenMeasurements>().OrderBy(x => x.Timestamp).FirstOrDefault();
            var isNew = false;
            if (oldInstance == null)
            {
                oldInstance = new InfoScreenMeasurements();
                isNew = true;
            }

            oldInstance.Heartrate = measurement.SensorItemMeasures.FirstOrDefault(x => x.Type == MeasurementItemTypes.HeartRateClean)?.Average ?? default(double);
            oldInstance.CloseTo = measurement.SensorItemMeasures.Count(x => x.Type.Contains(MeasurementItemTypes.ProximityCm));
            oldInstance.Timestamp = measurement.Timestamp;
            oldInstance.Steps = (int)(measurement.SensorItemMeasures.FirstOrDefault(x => x.Type == MeasurementItemTypes.Step)?.Magnitude ?? default(double));
            if (isNew)
            {
                dbContext.Add(oldInstance);
            }
            else
            {
                dbContext.Update(oldInstance);
            }
            InfoScreenMeasurementsUpdatedSubject.OnNext(oldInstance);
        }
        private Subject<InfoScreenMeasurements> InfoScreenMeasurementsUpdatedSubject = new Subject<InfoScreenMeasurements>();
        public IObservable<InfoScreenMeasurements> WhenInfoScreenMeasurementUpdated()
        {
            return InfoScreenMeasurementsUpdatedSubject;
        }

        public InfoScreenMeasurements GetInfoScreenMeasurements()
        {
            var dbContext = ServiceLocator.Instance.Get<IDatabaseContext>();
            return dbContext.GetAll<InfoScreenMeasurements>().OrderBy(x => x.Timestamp).FirstOrDefault();
        }


        public SurveyViewModel GetSurveyQuestions(int? pleasance = null, int? activation = null)
        {
            var questions = new SurveyViewModel();

            var generics = ServiceLocator.Instance.Get<IDatabaseContext>().GetAll<GenericQuestion>(x => !x.Deactivated);
            foreach (var generic in generics)
            {
                var viewModel = new SurveyFragmentViewModel(questions)
                {
                    Question = generic.Question,
                    QuestionShort = generic.QuestionShort,
                    QuestionId = generic.QuestionId,
                    Answer = 50
                };
                if (generic.QuestionId == 2 && activation != null)
                {
                    viewModel.Answer = activation.Value == 0 ? viewModel.Answer = 16 : activation.Value == 1 ? viewModel.Answer = 50 : viewModel.Answer = 83;
                    viewModel.IsAnswered = true;
                }
                else if (generic.QuestionId == 1 && pleasance != null)
                {
                    viewModel.Answer = pleasance.Value == 0 ? viewModel.Answer = 16 : pleasance.Value == 1 ? viewModel.Answer = 50 : viewModel.Answer = 83;
                    viewModel.IsAnswered = true;
                }
                questions.Questions.Add(viewModel);
            }
            if (!questions.Questions.Any())
            {
                //if we don't have any questions. Add the default ones.
                var question1 = new SurveyFragmentViewModel(questions)
                {
                    Question = "How Pleasant do you feel?",
                    QuestionShort = "Pleasance",
                    IsHardcodedQuestion = true,
                    QuestionId = 2,
                    Answer = 50
                };
                var question2 = new SurveyFragmentViewModel(questions)
                {
                    Question = "How Active do you feel?",
                    QuestionShort = "Activation",
                    IsHardcodedQuestion = true,
                    QuestionId = 1,
                    Answer = 50
                };
                questions.Questions.Add(question1);
                questions.Questions.Add(question2);
            }

            var sortedQuestions = new List<SurveyFragmentViewModel>();

            var first = questions.Questions.FirstOrDefault(x => x.QuestionId == 2);
            if (first != null)
            {
                sortedQuestions.Add(first);
            }
            var second = questions.Questions.FirstOrDefault(x => x.QuestionId == 1);
            if (second != null)
            {
                sortedQuestions.Add(second);
            }
            sortedQuestions.AddRange(questions.Questions.Where(x => x.QuestionId != 1 && x.QuestionId != 2));
            questions.Questions = sortedQuestions;

            return questions;
        }

        public void AddGenericQuestions(List<GenericQuestion> questions)
        {
            ServiceLocator.Instance.Get<IDatabaseContext>().DeleteAll<GenericQuestion>();
            foreach (var question in questions)
            {
                question.Id = 0;
                ServiceLocator.Instance.Get<IDatabaseContext>().Add(question);
            }
        }

        public DataExchangeMessage GetMeasurementsForDataTransfer()
        {
            //used to account for time differences between phone and watch
            var referenceTime = DateTime.UtcNow;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var allMoods = ServiceLocator.Instance.Get<IDatabaseContext>().GetAllWithChildren<SurveyMeasurement>();
            var moods = allMoods.Take(UtilHelper.NumberOfMeasurementToTransferPerOperation).ToList();
            var moodsCount = allMoods.Count();
            stopWatch.Stop();
            Debug.WriteLine($"Read {moods.Count} moods from db. took {stopWatch.Elapsed.Seconds} seconds");
            stopWatch.Reset();
            stopWatch.Start();
            var sensorMeasurementCount = ServiceLocator.Instance.Get<IDatabaseContext>().CountSensorMeasurements();
            var sensors = ServiceLocator.Instance.Get<IDatabaseContext>().GetSensorMeasurements(0, UtilHelper.NumberOfMeasurementToTransferPerOperation).ToList();
            Debug.WriteLine($"Read {sensors.Count} sensors from db. took {stopWatch.Elapsed.Seconds} seconds");
            //remove self referencing loop so that we can serialize it
            moods.ForEach(x => x.SurveyItemMeasurement.ForEach(y => y.SurveyMeasurement = null));
            sensors.ForEach(x => x.SensorItemMeasures.ForEach(y => y.SensorMeasurement = null));
            var needAnotherBatch =
                moodsCount > UtilHelper.NumberOfMeasurementToTransferPerOperation
                                       || sensorMeasurementCount > UtilHelper.NumberOfMeasurementToTransferPerOperation;

            return new DataExchangeMessage
            {
                SurveyMeasurements = moods,
                SensorMeasurements = sensors,
                CurrentTimeUtc = referenceTime,
                NeedAnotherBatch = needAnotherBatch
            };
        }

        public void DeleteSurveyMeasurement(DataExchangeMessage message)
        {
            var context = ServiceLocator.Instance.Get<IDatabaseContext>();
            foreach (var measruement in message.SurveyMeasurements)
            {
                context.Delete(measruement);
            }
            foreach (var measruement in message.SensorMeasurements)
            {
                context.Delete(measruement);
            }

            var pairing = context.Get<BluetoothPairing>(x => x.IsPairingActive);
            if (pairing != null)
            {
                pairing.LastDataSync = DateTime.UtcNow;
                context.Update(pairing);
            }
            ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.DataExchangeEnd);
        }
    }
}
