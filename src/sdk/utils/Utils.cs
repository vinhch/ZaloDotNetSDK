using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#if (NETSTANDARD2_0)
using Microsoft.Extensions.DependencyInjection;
#endif

namespace ZaloDotNetSDK
{
    internal static class Utils
    {
        private static readonly Regex
            _querystringRegex = new Regex(@"[?|&]([\w\.]+)=([^?|^&]+)", RegexOptions.Compiled);

        /// <remarks>
        /// Source for this method: https://stackoverflow.com/a/22046389
        /// </remarks>
        internal static NameValueCollection ParseQueryString(string qs)
        {
            // Must use an absolute uri, else Uri.Query throws an InvalidOperationException
            var uri = new UriBuilder("http://example:3000")
            {
                Query = Uri.UnescapeDataString(qs)
            }.Uri;
            var match = _querystringRegex.Match(uri.PathAndQuery);
            var nvc = new NameValueCollection();
            while (match.Success)
            {
                nvc.Add(match.Groups[1].Value, match.Groups[2].Value);
                match = match.NextMatch();
            }

            return nvc;
        }

        internal static string ToQueryString(NameValueCollection nvc)
        {
            var queryDictionary = new Dictionary<string, string>();
            foreach (var key in nvc.AllKeys)
            {
                queryDictionary[key] = nvc[key];
            }
            var kvps = queryDictionary.Select(kvp => $"{kvp.Key}={kvp.Value}");
            return string.Join("&", kvps);
        }

        private static readonly TaskFactory _taskFactory = new
            TaskFactory(CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        internal static TResult RunSync<TResult>(Func<Task<TResult>> func)
            => _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();

        internal static void RunSync(Func<Task> func)
            => _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();

#if (NETSTANDARD2_0)
        private static readonly ServiceCollection _currentServiceCollection = new ServiceCollection();
        private static readonly ServiceProvider _currentServiceProvider =
            _currentServiceCollection.AddHttpClient()
                .BuildServiceProvider();
        private static readonly IHttpClientFactory _currentHttpClientFactory =
            _currentServiceProvider.GetService<IHttpClientFactory>();
        internal static HttpClient CreateHttpClient() => _currentHttpClientFactory.CreateClient();
#else
        private static readonly HttpClient _currentHttpClient = new HttpClient();
        internal static HttpClient CreateHttpClient() => _currentHttpClient;
#endif

        internal static byte[] DownloadFile(string url) => RunSync(async () =>
        {
            var client = CreateHttpClient();
            using (var result = await client.GetAsync(url))
            {
                if (result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsByteArrayAsync();
                }
            }
            return null;
        });
    }
}
