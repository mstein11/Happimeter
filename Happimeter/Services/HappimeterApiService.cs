using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Happimeter.Core.Helper;
using Happimeter.Events;
using Happimeter.Interfaces;
using Happimeter.Models.ApiResultModels;
using Happimeter.Models.ServiceModels;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Xamarin.Forms;
using Happimeter.Core.Services;

namespace Happimeter.Services
{
	public class HappimeterApiService : IHappimeterApiService
	{
		private readonly IRestService _restService;
		private readonly IAccountStoreService _accountStore;

		private const string ApiUrl = "https://api.happimeter.org";
		//private const string ApiUrl = "http://localhost:4711";

		private const string ApiPathAuth = "/v1/auth";
		private const string ApiPathRegister = "/v1/users";
		private const string ApiPathGetMe = "/v1/me";
		private const string ApiPathGetGenericQuestion = "/v1/moods/genericquestions";

		private const string ApiPathPostMood = "/v1/moods-generic";
		private const string ApiPathPostSensor = "/v1/sensorslist";
		private const string ApiPathPostSensorV2 = "/v2/sensors";

		private const string ApiPathPredictions = "/v1/classifier/prediction";
		private const string ApiPathProximity = "/v1/proximity";
		private const string ApiPathSignals = "/v1/signals";


		private static string GetUrlForPath(string path)
		{
			return ApiUrl + path;
		}

		public HappimeterApiService()
		{
			_restService = ServiceLocator.Instance.Get<IRestService>();
			_accountStore = ServiceLocator.Instance.Get<IAccountStoreService>();
			if (_accountStore.IsAuthenticated())
			{
				_restService.AddAuthorizationTokenToInstance(_accountStore.GetAccountToken());
			}
		}

		public async Task<RegisterUserResultModel> CreateAccount(string email, string password)
		{

			var methodResult = new RegisterUserResultModel();

			var url = GetUrlForPath(ApiPathRegister);
			var data = new { mail = email, password };

			HttpResponseMessage result = null;
			try
			{
				result = await _restService.Post(url, data);
			}
			catch (Exception e) when (
				e is HttpRequestException
				|| e is WebException
			)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				methodResult.ResultType = RegisterUserResultTypes.ErrorNoInternet;
				return methodResult;
			}
			catch (Exception e)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				methodResult.ResultType = RegisterUserResultTypes.ErrorUnknown;
				return methodResult;
			}

			if (result.IsSuccessStatusCode)
			{
				var responseString = await result.Content.ReadAsStringAsync();
				var apiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthApiResponseModel>(responseString);

				//we got the token
				if (apiResult.Status == 200)
				{
					methodResult.ResultType = RegisterUserResultTypes.Success;
				}
				else if (apiResult.Status == 400)
				{
					methodResult.ResultType = RegisterUserResultTypes.ErrorPasswordInsufficient;
				}
				else if (apiResult.Status == 409)
				{
					methodResult.ResultType = RegisterUserResultTypes.ErrorUserAlreadyTaken;
				}
				else if (apiResult.Status == 410)
				{
					methodResult.ResultType = RegisterUserResultTypes.ErrorInvalidEmail;
				}
				else
				{
					methodResult.ResultType = RegisterUserResultTypes.ErrorUnknown;
				}
			}

			return methodResult;
		}

		public async Task<AuthResultModel> Auth(string email, string password)
		{
			var methodResult = new AuthResultModel();

			var url = GetUrlForPath(ApiPathAuth);
			var data = new { mail = email, password };
			HttpResponseMessage result = null;
			try
			{
				result = await _restService.Post(url, data);
			}
			catch (Exception e) when (
				e is HttpRequestException
				|| e is WebException
			)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				methodResult.ResultType = AuthResultTypes.ErrorNoInternet;
				return methodResult;
			}
			catch (Exception e)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				methodResult.ResultType = AuthResultTypes.ErrorUnknown;
				return methodResult;
			}


			if (result.IsSuccessStatusCode)
			{
				var responseString = await result.Content.ReadAsStringAsync();
				var apiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthApiResponseModel>(responseString);

				//we got the token
				if (apiResult.Status == 200)
				{
					//IsSuccessStatusCode does not work properly because of server
					_restService.AddAuthorizationTokenToInstance(apiResult.Token);
					var me = await GetMe();

					_accountStore.SaveAccount(email, apiResult.Token, me.Id, apiResult.Expires);
					var authenticated = _accountStore.IsAuthenticated();

					if (authenticated)
					{
						methodResult.ResultType = AuthResultTypes.Success;
					}
					else
					{
						methodResult.ResultType = AuthResultTypes.ErrorUnknown;
					}
					//authentication error, probably wrong username/password
				}
				else if (apiResult.Status == 510)
				{
					methodResult.ResultType = AuthResultTypes.ErrorWrongCredentials;
					//Unknown error
				}
				else
				{
					methodResult.ResultType = AuthResultTypes.ErrorUnknown;
				}
			}


			return methodResult;
		}

		public async Task<GetMeResultModel> GetMe()
		{
			var methodResult = new GetMeResultModel();
			var url = GetUrlForPath(ApiPathAuth);
			HttpResponseMessage result = null;
			try
			{
				result = await _restService.Get(url);
			}
			catch (Exception e) when (
				e is HttpRequestException
				|| e is WebException
			)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				methodResult.ResultType = HappimeterApiResultInformation.NoInternet;
				return methodResult;
			}
			var stringResult = await result.Content.ReadAsStringAsync();
			var jObjectResult = JObject.Parse(stringResult);
			var status = jObjectResult["status"].ToObject<int>();
			if (status == 200)
			{
				var apiResults = jObjectResult["auth"].ToObject<GetMeResultModel>();
				methodResult = apiResults;
				methodResult.ResultType = HappimeterApiResultInformation.Success;
			}
			else if (status == 510)
			{
				methodResult.ResultType = HappimeterApiResultInformation.AuthenticationError;
			}
			else
			{
				methodResult.ResultType = HappimeterApiResultInformation.UnknownError;
			}

			return methodResult;
		}

		public event EventHandler<SynchronizeDataEventArgs> UploadMoodStatusUpdate;
		public async Task<HappimeterApiResultInformation> UploadMood()
		{
			var measurementService = ServiceLocator.Instance.Get<IMeasurementService>();
			var toSend = measurementService.GetSurveyModelForServer();

			if (!toSend.Any())
			{
				return HappimeterApiResultInformation.Success;
			}

			var url = GetUrlForPath(ApiPathPostMood);

			HttpResponseMessage result = null;
			try
			{
				var counter = 0;
				UploadMoodStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
				{
					EntriesSent = counter,
					TotalEntries = toSend.Count,
					EventType = SyncronizeDataStates.UploadingMood
				});

				//if the new api is not available yet, we try to send with the old one.
				foreach (var moodEntry in toSend)
				{
					UploadMoodStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
					{
						EntriesSent = counter,
						TotalEntries = toSend.Count,
						EventType = SyncronizeDataStates.UploadingMood
					});
					result = await _restService.Post(url, moodEntry);
					if (result.IsSuccessStatusCode)
					{
						measurementService.SetIsUploadedToServerForSurveys(moodEntry);
					}
					else
					{
						UploadMoodStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
						{
							EntriesSent = counter,
							TotalEntries = toSend.Count,
							EventType = SyncronizeDataStates.UploadingError
						});
						return HappimeterApiResultInformation.UnknownError;
					}
					counter++;
				}

				UploadMoodStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
				{
					EntriesSent = counter,
					TotalEntries = toSend.Count,
					EventType = SyncronizeDataStates.UploadingSuccessful
				});

				return HappimeterApiResultInformation.Success;
			}
			catch (Exception e) when (
				e is HttpRequestException
				|| e is WebException
			)
			{
				Debug.WriteLine(e.Message);
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				UploadMoodStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
				{
					EventType = SyncronizeDataStates.NoInternetError
				});
				return HappimeterApiResultInformation.NoInternet;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				UploadMoodStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
				{
					EventType = SyncronizeDataStates.UploadingError
				});
				return HappimeterApiResultInformation.UnknownError;
			}
		}

		public event EventHandler<SynchronizeDataEventArgs> UploadSensorStatusUpdate;
		/// <summary>
		///     Uploads the sensordata to the happimetersever. 
		///     This method uploads the sensordata in two different formats. (old format and new format)
		///     For each 150 sensor entries there is one http request to the old and to the new ap
		///     We limit to 150 entries because if we have more, we get an exception
		/// </summary>
		/// <returns>The sensor.</returns>
		public async Task<HappimeterApiResultInformation> UploadSensor()
		{
			var measurementService = ServiceLocator.Instance.Get<IMeasurementService>();
			HttpResponseMessage result = null;
			try
			{
				var totalCount = measurementService.CountUnsynchronizedSensorData();
				var uploadCount = 0;
				while (measurementService.HasUnsynchronizedSensorData())
				{

					(var toSend, var toSendNewFormat) = measurementService.GetSensorDataForServer();

					var url = GetUrlForPath(ApiPathPostSensor);
					var newUrl = GetUrlForPath(ApiPathPostSensorV2);

					UploadSensorStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
					{
						EntriesSent = uploadCount,
						TotalEntries = totalCount,
						EventType = SyncronizeDataStates.UploadingSensor
					});

					//first upload in new format
					result = await _restService.Post(newUrl, toSendNewFormat);
					if (!result.IsSuccessStatusCode)
					{
						//if it did not work, lets log this.
						ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.CouldNotUploadSensorNewFormat);
						UploadSensorStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
						{
							EventType = SyncronizeDataStates.UploadingError
						});
						//if the new format upload does not work, we want to abort here, so the next time we can try again
						return HappimeterApiResultInformation.UnknownError;
					}
					var resultOldFormat = await _restService.Post(url, toSend);
					if (!resultOldFormat.IsSuccessStatusCode)
					{
						//if it did not work, lets log this.
						ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.CouldNotUploadSensorOldFormat);
						//we don't care so much if the old upload does not work
					}

					if (resultOldFormat.IsSuccessStatusCode || result.IsSuccessStatusCode)
					{
						//if any of the two methods did work, lets mark the entries as uploaded
						foreach (var sensorEntry in toSend)
						{
							measurementService.SetIsUploadedToServerForSensorData(sensorEntry);
						}
						uploadCount += toSend.Count();
					}
					else
					{
						//both methods did not work. :(
						UploadSensorStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
						{
							EntriesSent = uploadCount,
							TotalEntries = totalCount,
							EventType = SyncronizeDataStates.UploadingError
						});
						return HappimeterApiResultInformation.UnknownError;
					}
				}
				//after we upload date we might have new proximity info, lets download them
				await ServiceLocator.Instance.Get<IProximityService>().DownloadAndSaveProximity();

				//if we arrive here, than at least one method did work.
				UploadSensorStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
				{
					EntriesSent = uploadCount,
					TotalEntries = totalCount,
					EventType = SyncronizeDataStates.UploadingSuccessful
				});
				return HappimeterApiResultInformation.Success;
			}
			catch (Exception e) when (
				e is HttpRequestException
				|| e is WebException
			)
			{
				Debug.WriteLine(e.Message);
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				UploadSensorStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
				{
					EventType = SyncronizeDataStates.NoInternetError
				});
				return HappimeterApiResultInformation.NoInternet;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				if (e.InnerException != null)
				{
					Debug.WriteLine(e.InnerException.Message);
				}
				UploadSensorStatusUpdate?.Invoke(this, new SynchronizeDataEventArgs
				{
					EventType = SyncronizeDataStates.UploadingError
				});
				return HappimeterApiResultInformation.UnknownError;
			}
		}

		public async Task<GetGenericQuestionApiResult> GetGenericQuestions()
		{
			var methodResult = new GetGenericQuestionApiResult();
			var url = GetUrlForPath(ApiPathGetGenericQuestion);
			HttpResponseMessage result = null;
			GetGenericQuestionApiResult questions = null;
			try
			{
				result = await _restService.Get(url);
				var stringResult = await result.Content.ReadAsStringAsync();
				questions = Newtonsoft.Json.JsonConvert.DeserializeObject<GetGenericQuestionApiResult>(stringResult);
			}
			catch (Exception e) when (
				e is HttpRequestException
				|| e is WebException
			)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				Debug.WriteLine(e.Message);
				methodResult.ResultType = HappimeterApiResultInformation.NoInternet;
				return methodResult;
			}
			catch (Exception e)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				Debug.WriteLine(e.Message);
				Debug.WriteLine(e.GetType());
				methodResult.ResultType = HappimeterApiResultInformation.UnknownError;
				return methodResult;
			}

			if (result.IsSuccessStatusCode)
			{
				methodResult = questions;
				methodResult.ResultType = HappimeterApiResultInformation.Success;
			}
			else if (result.StatusCode == HttpStatusCode.Forbidden)
			{
				methodResult.ResultType = HappimeterApiResultInformation.AuthenticationError;
			}
			else
			{
				methodResult.ResultType = HappimeterApiResultInformation.UnknownError;
			}

			return methodResult;
		}

		public async Task<GetProximityResultModel> GetProximityData(DateTime since)
		{
			var url = GetUrlForPath(ApiPathProximity);

			url += $"/{since.ToString("yyyy-MM-dd HH:mm:ss")}";
			var methodResult = new GetProximityResultModel();
			HttpResponseMessage result = null;
			GetProximityResultModel proximity = null;
			try
			{
				result = await _restService.Get(url);
				var stringResult = await result.Content.ReadAsStringAsync();
				proximity = Newtonsoft.Json.JsonConvert.DeserializeObject<GetProximityResultModel>(stringResult);
			}
			catch (Exception e) when (
				e is HttpRequestException
				|| e is WebException
			)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				Debug.WriteLine(e.Message);
				methodResult.ResultType = HappimeterApiResultInformation.NoInternet;
				return methodResult;
			}
			catch (Exception e)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				Debug.WriteLine(e.Message);
				Debug.WriteLine(e.GetType());
				methodResult.ResultType = HappimeterApiResultInformation.UnknownError;
				return methodResult;
			}
			if (result.IsSuccessStatusCode && proximity.IsSuccess)
			{
				methodResult = proximity;
				methodResult.ResultType = HappimeterApiResultInformation.Success;
			}
			else if (result.StatusCode == HttpStatusCode.Forbidden)
			{
				methodResult.ResultType = HappimeterApiResultInformation.AuthenticationError;
			}
			else
			{
				methodResult.ResultType = HappimeterApiResultInformation.UnknownError;
			}
			return methodResult;
		}

		public async Task<GetSignalsModel> GetSignals(DateTime forDay)
		{
			var url = GetUrlForPath(ApiPathSignals);
			var from = forDay.Date.ToUniversalTime();
			var until = from.AddDays(1);
			url += $"?timestamps[]={from.ToString("yyyy-MM-dd HH:mm:ss")}&timestamps[]={until.ToString("yyyy-MM-dd HH:mm:ss")}";
			var methodResult = new GetSignalsModel();
			HttpResponseMessage result = null;
			GetSignalsModel signals = null;
			try
			{
				result = await _restService.Get(url);
				var stringResult = await result.Content.ReadAsStringAsync();
				signals = Newtonsoft.Json.JsonConvert.DeserializeObject<GetSignalsModel>(stringResult);
			}
			catch (Exception e) when (
				e is HttpRequestException
				|| e is WebException
			)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				Debug.WriteLine(e.Message);
				methodResult.ResultType = HappimeterApiResultInformation.NoInternet;
				return methodResult;
			}
			catch (Exception e)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				Debug.WriteLine(e.Message);
				Debug.WriteLine(e.GetType());
				methodResult.ResultType = HappimeterApiResultInformation.UnknownError;
				return methodResult;
			}
			if (result.IsSuccessStatusCode && signals.IsSuccess)
			{
				methodResult = signals;
				methodResult.ResultType = HappimeterApiResultInformation.Success;
			}
			else if (result.StatusCode == HttpStatusCode.Forbidden)
			{
				methodResult.ResultType = HappimeterApiResultInformation.AuthenticationError;
			}
			else
			{
				methodResult.ResultType = HappimeterApiResultInformation.UnknownError;
			}
			return methodResult;
		}


		public async Task<GetPredictionsResultModel> GetPredictions()
		{
			var url = GetUrlForPath(ApiPathPredictions);
			var methodResult = new GetPredictionsResultModel();
			HttpResponseMessage result = null;
			GetPredictionsResultModel predictions;
			try
			{
				result = await _restService.Get(url);
				var stringResult = await result.Content.ReadAsStringAsync();
				predictions = Newtonsoft.Json.JsonConvert.DeserializeObject<GetPredictionsResultModel>(stringResult);
			}
			catch (Exception e) when (
				e is HttpRequestException
				|| e is WebException
			)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				Debug.WriteLine(e.Message);
				methodResult.ResultType = HappimeterApiResultInformation.NoInternet;
				return methodResult;
			}
			catch (Exception e)
			{
				ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
				Debug.WriteLine(e.Message);
				Debug.WriteLine(e.GetType());
				methodResult.ResultType = HappimeterApiResultInformation.UnknownError;
				return methodResult;
			}
			if (result.IsSuccessStatusCode && predictions.IsSuccess)
			{
				methodResult = predictions;
				methodResult.ResultType = HappimeterApiResultInformation.Success;
			}
			else if (result.StatusCode == HttpStatusCode.Forbidden)
			{
				methodResult.ResultType = HappimeterApiResultInformation.AuthenticationError;
			}
			else
			{
				methodResult.ResultType = HappimeterApiResultInformation.UnknownError;
			}
			return methodResult;
		}
	}
}
