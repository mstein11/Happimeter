﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Happimeter.Interfaces;
using Happimeter.Models.ApiResultModels;
using Happimeter.Models.ServiceModels;
using Newtonsoft.Json.Linq;

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

        private const string ApiPathPostMood = "/v1/moods";
        private const string ApiPathPostSensor = "/v1/sensors";


        private static string GetUrlForPath(string path) {
            return ApiUrl + path;
        }

        public HappimeterApiService()
        {
            _restService = ServiceLocator.Instance.Get<IRestService>();
            _accountStore = ServiceLocator.Instance.Get<IAccountStoreService>();
            if (_accountStore.IsAuthenticated()) {
                _restService.AddAuthorizationTokenToInstance(_accountStore.GetAccountToken());
            }
        }

        public async Task<RegisterUserResultModel> CreateAccount(string email, string password) {

            var methodResult = new RegisterUserResultModel();

            var url = GetUrlForPath(ApiPathRegister);
            var data = new { mail = email, password };

            HttpResponseMessage result = null;
            try
            {
                result = await _restService.Post(url, data);
            }
            catch (WebException e)
            {
                methodResult.ResultType = RegisterUserResultTypes.ErrorNoInternet;
                return methodResult;
            }
            catch (Exception e)
            {
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
                else if (apiResult.Status == 410) {
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
            try {
                result = await _restService.Post(url, data);    
            } catch (WebException e) {
                methodResult.ResultType = AuthResultTypes.ErrorNoInternet;
                return methodResult;
            } catch (Exception e) {
                methodResult.ResultType = AuthResultTypes.ErrorUnknown;
                return methodResult;
            }


            if (result.IsSuccessStatusCode) {
                var responseString = await result.Content.ReadAsStringAsync();
                var apiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthApiResponseModel>(responseString);

                //we got the token
                if (apiResult.Status == 200) {
                    //IsSuccessStatusCode does not work properly because of server
                    _restService.AddAuthorizationTokenToInstance(apiResult.Token);
                    var me = await GetMe();

                    _accountStore.SaveAccount(email, apiResult.Token, me.Id, apiResult.Expires);
                    var authenticated = _accountStore.IsAuthenticated();




                    if (authenticated) {
                        methodResult.ResultType = AuthResultTypes.Success;    
                    } else {
                        methodResult.ResultType = AuthResultTypes.ErrorUnknown; 
                    }
                //authentication error, probably wrong username/password
                } else if (apiResult.Status == 510) {
                    methodResult.ResultType = AuthResultTypes.ErrorWrongCredentials;
                //Unknown error
                } else {
                    methodResult.ResultType = AuthResultTypes.ErrorUnknown;
                }
            }


            return methodResult;
        }

        public async Task<GetMeResultModel> GetMe() {
            var methodResult = new GetMeResultModel();
            var url = GetUrlForPath(ApiPathAuth);
            HttpResponseMessage result = null;
            try
            {
                result = await _restService.Get(url);
            }
            catch (WebException e)
            {
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
            } else if (status == 510) {
                methodResult.ResultType = HappimeterApiResultInformation.AuthenticationError;
            } else {
                methodResult.ResultType = HappimeterApiResultInformation.UnknownError;
            }

            return methodResult;
        }

        public async Task<HappimeterApiResultInformation> UploadMoad()
        {
            var measurementService = ServiceLocator.Instance.Get<IMeasurementService>();
            var toSend = measurementService.GetSurveyModelForServer();

            var url = GetUrlForPath(ApiPathPostMood);

            HttpResponseMessage result = null;
            try
            {
                foreach (var moodEntry in toSend) {
                    result = await _restService.Post(url, moodEntry);
                    if (result.IsSuccessStatusCode)
                    {
                        measurementService.SetIsUploadedToServerForSurveys(moodEntry);
                    } else {
                        return HappimeterApiResultInformation.UnknownError;
                    }
                }

                return HappimeterApiResultInformation.Success;
            }
            catch (WebException e)
            {
                return HappimeterApiResultInformation.NoInternet;
            }
            catch (Exception e)
            {
                return HappimeterApiResultInformation.UnknownError;
            }
        }

        public async Task<HappimeterApiResultInformation> UploadSensor() {
            var measurementService = ServiceLocator.Instance.Get<IMeasurementService>();
            var toSend = measurementService.GetSensorDataForServer();

            var url = GetUrlForPath(ApiPathPostSensor);

            HttpResponseMessage result = null;
            try
            {
                foreach (var sensorEntry in toSend)
                {
                    result = await _restService.Post(url, sensorEntry);
                    if (result.IsSuccessStatusCode)
                    {
                        measurementService.SetIsUploadedToServerForSensorData(sensorEntry);
                    }
                    else
                    {
                        return HappimeterApiResultInformation.UnknownError;
                    }
                }

                return HappimeterApiResultInformation.Success;
            }
            catch (WebException e)
            {
                return HappimeterApiResultInformation.NoInternet;
            }
            catch (Exception e)
            {
                return HappimeterApiResultInformation.UnknownError;
            }
        }
    }
}
