using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Text;

namespace ZaloDotNetSDK {
    public class ZaloBaseClient {

        public bool isDebug = false;

        protected string sendHttpGetRequest(string endpoint, Dictionary<string, dynamic> param, Dictionary<string, string> header) => Utils.RunSync(async () =>
        {
            UriBuilder builder = new UriBuilder(endpoint);
            var query = Utils.ParseQueryString(builder.Query);
            if (param != null) {
                foreach (KeyValuePair<string, dynamic> entry in param) {
                    if (entry.Value is string) {
                        query[entry.Key] = entry.Value;
                    }
                }
            }
            builder.Query = Utils.ToQueryString(query);

            var httpClient = Utils.CreateHttpClient();
            if (header != null) {
                foreach (KeyValuePair<string, string> entry in header) {
                    httpClient.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                }
            }
            if (isDebug)
            {
                Console.WriteLine("GET: "+ builder.ToString());
            }
            return await httpClient.GetStringAsync(builder.ToString());
        });

        protected string sendHttpPostRequest(string endpoint, Dictionary<string, dynamic> param, Dictionary<string, string> header) => Utils.RunSync(async () =>
        {
            Dictionary<string, string> paramsUrl = new Dictionary<string, string>();
            var httpClient = Utils.CreateHttpClient();
            if (header != null)
            {
                foreach (KeyValuePair<string, string> entry in header)
                {
                    httpClient.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                }
            }
            if (param != null)
            {
                foreach (KeyValuePair<string, dynamic> entry in param)
                {
                    if (entry.Value is string)
                    {
                        paramsUrl[entry.Key] = entry.Value;
                    }
                }
            }
            FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(paramsUrl);
            if (isDebug)
            {
                UriBuilder builder = new UriBuilder(endpoint);
                var query = Utils.ParseQueryString(builder.Query);
                foreach (KeyValuePair<string, string> entry in paramsUrl)
                {
                    query[entry.Key] = entry.Value;
                }
                builder.Query = Utils.ToQueryString(query);
                Console.WriteLine("POST: " + builder.ToString());
            }

            using (var response = await httpClient.PostAsync(endpoint, formUrlEncodedContent))
            {
                return await response.Content.ReadAsStringAsync();
            }
        });

        protected string sendHttpPostRequestWithBody(string endpoint, Dictionary<string, dynamic> param, string body, Dictionary<string, string> header) => Utils.RunSync(async () =>
        {
            var httpClient = Utils.CreateHttpClient();
            if (header != null) {
                foreach (KeyValuePair<string, string> entry in header) {
                    httpClient.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                }
            }

            UriBuilder builder = new UriBuilder(endpoint);
            var query = Utils.ParseQueryString(builder.Query);
            if (param != null)
            {
                foreach (KeyValuePair<string, dynamic> entry in param)
                {
                    if (entry.Value is string && !entry.Key.Equals("body")) {
                        query[entry.Key] = entry.Value;
                    }
                }
            }
            builder.Query = Utils.ToQueryString(query);

            if (body == null) {
                body = "";
            }
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            if (isDebug)
            {
                Console.WriteLine("POST: " + builder.ToString());
                Console.WriteLine("body: " + body);
                Console.WriteLine("body content: " + content);
            }

            using (var response = await httpClient.PostAsync(builder.ToString(), content))
            {
                return await response.Content.ReadAsStringAsync();
            }
        });

        protected string sendHttpUploadRequest(string endpoint, Dictionary<string, dynamic> param, Dictionary<string, string> header) => Utils.RunSync(async () =>
        {
            MultipartFormDataContent form = new MultipartFormDataContent();

            UriBuilder builder = new UriBuilder(endpoint);
            var query = Utils.ParseQueryString(builder.Query);
            if (param != null)
            {
                foreach (KeyValuePair<string, dynamic> entry in param)
                {
                    if (entry.Value is string) {
                        query[entry.Key] = entry.Value;
                    }
                }
            }
            builder.Query = Utils.ToQueryString(query);

            ZaloFile file = param["file"];
            form.Add(file.GetData(), "file", file.GetName());

            var httpClient = Utils.CreateHttpClient();
            if (header != null)
            {
                foreach (KeyValuePair<string, string> entry in header)
                {
                    httpClient.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                }
            }

            using (var response = await httpClient.PostAsync(builder.ToString(), form))
            {
                return await response.Content.ReadAsStringAsync();
            }
        });
    }
}