using AWorkFlow.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.Retry;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AWorkFlow.RestfulApiAction
{
    /// <summary>
    /// a class provides operations of http/https call
    /// </summary>
    public class HttpProvider
    {
        /// <summary>
        /// start a helper and set base url of the call
        /// </summary>
        /// <param name="url">url</param>
        /// <returns>an instance of HttpProvider</returns>
        public static HttpProvider CallUrl(string url)
        {
            return new HttpProvider(url);
        }
        private readonly RestClient _client;
        private readonly RestSharp.RestRequest _request;
        private AsyncRetryPolicy _retryPolicy;

        /// <summary>
        /// acceptable status code(s), default by 200 only
        /// </summary>
        public List<HttpStatusCode> SuccessStatusCodes { get; private set; } = new List<HttpStatusCode> { HttpStatusCode.OK };

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="url">base url</param>
        public HttpProvider(string url)
        {
            _client = new RestClient(url);

            _request = new RestSharp.RestRequest
            {
                JsonSerializer = new NewtonsoftJsonSerializer(JsonSerializer.Create
                      (
                          new JsonSerializerSettings()
                          {
                              ContractResolver = new CamelCasePropertyNamesContractResolver()
                              //ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                          }
                      ))
            };

            _retryPolicy = Policy
                  .Handle<Exception>()
                  .WaitAndRetryAsync(1, count => TimeSpan.FromSeconds(2 * count));

        }

        #region parameters
        /// <summary>
        /// add an bearer authorization item in header, which key is "Authorization" and value is "Bearer {token}"
        /// </summary>
        /// <param name="token">bearer token</param>
        /// <returns>this HttpProvider</returns>
        public HttpProvider WithBearerToken(string token)
        {
            _request.AddHeader("Authorization", $"Bearer {token}");
            return this;
        }

        /// <summary>
        /// add an basic authenticator with username and password
        /// </summary>
        /// <param name="userName">username</param>
        /// <param name="password">password</param>
        /// <returns>this HttpProvider</returns>
        public HttpProvider WithBasicToken(string userName, string password)
        {
            _client.Authenticator = new HttpBasicAuthenticator(userName, password);
            return this;
        }

        /// <summary>
        /// use retry policy when the call failed
        /// </summary>
        /// <param name="times">retry times(default by 1: do once and no retry)</param>
        /// <param name="interval">the duration to wait for for a particular retry attempt</param>
        /// <returns>this HttpProvider</returns>
        public HttpProvider WithRetry(int times, TimeSpan? interval = null)
        {
            if (times <= 0)
            {
                times = 1;
            }

            _retryPolicy = Policy
                  .Handle<Exception>()
                  .WaitAndRetryAsync(times, count => interval ?? TimeSpan.Zero);
            return this;
        }

        /// <summary>
        /// add a query parameter to the request
        /// </summary>
        /// <param name="key">parameter key</param>
        /// <param name="value">value</param>
        /// <returns>this HttpProvider</returns>
        public HttpProvider WithQueryParameter(string key, string value)
        {
            _request.AddQueryParameter(key, value);
            return this;
        }

        /// <summary>
        /// add a custom header to the request
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>this HttpProvider</returns>
        public HttpProvider WithHeader(string key, string value)
        {
            _request.AddHeader(key, value);
            return this;
        }

        /// <summary>
        /// Serializes obj to JSON format and adds it to the request body.
        /// </summary>
        /// <param name="body">The object to serialize</param>
        /// <returns>this HttpProvider</returns>
        public HttpProvider WithJsonBody(object body)
        {
            _request.AddJsonBody(body);
            return this;
        }

        /// <summary>
        /// Serializes obj to XML format and adds it to the request body.
        /// </summary>
        /// <param name="body">The object to serialize</param>
        /// <returns>this HttpProvider</returns>
        public HttpProvider WithXmlBody(object body)
        {
            _request.AddXmlBody(body);
            return this;
        }

        /// <summary>
        /// Adds a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and
        /// HEAD; Encoded form for POST and PUT)
        /// </summary>
        /// <param name="key">parameter key</param>
        /// <param name="value">parameter data</param>
        /// <returns>this HttpProvider</returns>
        public HttpProvider WithHttpParameter(string key, object value)
        {
            _request.AddParameter(key, value);
            return this;
        }

        /// <summary>
        /// Add bytes to the Files collection as if it was a file of specific type
        /// </summary>
        /// <param name="key">A form parameter name</param>
        /// <param name="filename">The file name to use for the uploaded file</param>
        /// <param name="content">The file data</param>
        /// <param name="contentType">Specific content type. Es: application/x-gzip</param>
        /// <returns>this HttpProvider</returns>
        public HttpProvider WithFile(string key, string filename, byte[] content, string contentType)
        {
            _request.AddFileBytes(key, content, filename, contentType);
            return this;
        }

        /// <summary>
        /// set timeout of the request
        /// </summary>
        /// <param name="timeout">time before request time out</param>
        /// <param name="readWriteTimeout">time before the writing or reading times out</param>
        /// <returns>this HttpProvider</returns>
        public HttpProvider WithTimeout(TimeSpan timeout, TimeSpan? readWriteTimeout = null)
        {
            _client.Timeout = (int)timeout.TotalMilliseconds;
            if (readWriteTimeout.HasValue)
            {
                _client.ReadWriteTimeout = (int)readWriteTimeout.Value.TotalMilliseconds;
            }
            return this;
        }

        /// <summary>
        /// acceptable status code(s) in which the request whill be treat as success, default by 200 only
        /// </summary>
        /// <param name="statusCodes">http status code(s)</param>
        /// <returns>this HttpProvider</returns>
        public HttpProvider AcceptStatusCodes(params HttpStatusCode[] statusCodes)
        {
            if (statusCodes?.Any() == true)
            {
                SuccessStatusCodes = statusCodes?.ToList();
            }
            return this;
        }
        #endregion

        #region call
        /// <summary>
        /// request as GET and retrun original body as string
        /// </summary>
        /// <returns>operation result with original body as string</returns>
        public Task<OperationResult<string>> GetAsync()
        {
            return ExecuteAsync<string>(Method.GET);
        }

        /// <summary>
        /// request as GET and retreive body as specific model
        /// </summary>
        /// <typeparam name="T">model of body data</typeparam>
        /// <returns>operation result with data</returns>
        public Task<OperationResult<T>> GetAsync<T>()
        {
            return ExecuteAsync<T>(Method.GET);
        }

        /// <summary>
        /// request as POST
        /// </summary>
        /// <returns>operation result</returns>
        public async Task<OperationResult> PostAsync()
        {
            return await ExecuteAsync<string>(Method.POST);
        }
        /// <summary>
        /// request as POST and retreive body as specific model
        /// </summary>
        /// <typeparam name="T">model of body data</typeparam>
        /// <returns>operation result</returns>
        public Task<OperationResult<T>> PostAsync<T>()
        {
            return ExecuteAsync<T>(Method.POST);
        }

        /// <summary>
        /// request as PUT
        /// </summary>
        /// <returns>operation result</returns>
        public async Task<OperationResult> PutAsync()
        {
            return await ExecuteAsync<string>(Method.PUT);
        }
        /// <summary>
        /// request as PUT and retreive body as specific model
        /// </summary>
        /// <typeparam name="T">model of body data</typeparam>
        /// <returns>operation result with data</returns>
        public Task<OperationResult<T>> PutAsync<T>()
        {
            return ExecuteAsync<T>(Method.PUT);
        }

        /// <summary>
        /// request as DELETE
        /// </summary>
        /// <returns>operation result</returns>
        public async Task<OperationResult> DeleteAsync()
        {
            return await ExecuteAsync<string>(Method.DELETE);
        }
        /// <summary>
        /// request as DELETE and retreive body as specific model
        /// </summary>
        /// <typeparam name="T">model of body data</typeparam>
        /// <returns>operation result with data</returns>
        public Task<OperationResult<T>> DeleteAsync<T>()
        {
            return ExecuteAsync<T>(Method.DELETE);
        }

        /// <summary>
        /// request with specific http method
        /// </summary>
        /// <param name="method">http verb</param>
        /// <returns>operation result</returns>
        public async Task<OperationResult> ExecuteAsync(Method method)
        {
            return await ExecuteAsync<string>(method);
        }

        /// <summary>
        /// request as GET and retreive data as downloaded file
        /// </summary>
        /// <returns>operation result with downloaded file</returns>
        public Task<OperationResult<FileContent>> DownloadAsync()
        {
            return ExecuteDownloadAsync();
        }

        /// <summary>
        /// request with specific http method
        /// </summary>
        /// <param name="method">http verb</param>
        /// <returns>operation result with data</returns>
        public async Task<OperationResult<T>> ExecuteAsync<T>(Method method)
        {
            try
            {
                _request.Method = method;
                var retryResult = await _retryPolicy.ExecuteAndCaptureAsync(async () =>
                {
                    var response = await _client.ExecuteTaskAsync<T>(_request);
                    if (SuccessStatusCodes.Contains(response.StatusCode))
                    {
                        try
                        {
                            if (response.Data == null && !string.IsNullOrEmpty(response.Content))
                            {
                                return JsonConvert.DeserializeObject<T>(response.Content);
                            }
                        }
                        catch { }
                        return response.Data;
                    }
                    else
                    {
                        throw new Exception($"{method} {_client.BaseUrl} failed with {response.StatusCode}: {response.Content}");
                    }
                });

                if (retryResult.Outcome == OutcomeType.Successful)
                {
                    return new OperationResult<T> { Success = true, Data = retryResult.Result };
                }
                else
                {
                    return new OperationResult<T> { Success = false, Message = retryResult.FinalException.Message };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<T> { Success = false, Message = ex.Message, Exception = ex };
            }
        }
        private async Task<OperationResult<FileContent>> ExecuteDownloadAsync()
        {
            try
            {
                _request.Method = Method.GET;
                var retryResult = await _retryPolicy.ExecuteAndCaptureAsync(async () =>
                {
                    var response = await _client.ExecuteTaskAsync(_request);
                    if (SuccessStatusCodes.Contains(response.StatusCode))
                    {
                        return new FileContent
                        {
                            Content = response.RawBytes,
                            ContentType = response.ContentType
                        };
                    }
                    else
                    {
                        throw new Exception($"Download file content failed with {response.StatusCode}");
                    }
                });

                if (retryResult.Outcome == OutcomeType.Successful)
                {
                    return new OperationResult<FileContent> { Success = true, Data = retryResult.Result };
                }
                else
                {
                    return new OperationResult<FileContent> { Success = false, Message = retryResult.FinalException.Message };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<FileContent> { Success = false, Message = ex.Message, Exception = ex };
            }
        }
        #endregion
    }

    /// <summary>
    /// downloaded file model
    /// </summary>
    public class FileContent
    {
        /// <summary>
        /// content type
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// file data
        /// </summary>
        public byte[] Content { get; set; }
    }
}
