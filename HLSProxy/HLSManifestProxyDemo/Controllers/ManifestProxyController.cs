// -----------------------------------------------------------------------------
//  Copyright (C) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------------

namespace Microsoft.Azure.CloudVideo.VideoManagementService.Cms.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Http;

    namespace HLSManifestProxyDemo.Controllers 
    { 
        public class ManifestProxyController : ApiController
    {
        /// <summary>
        ///     Support for: GET /api/ManifestProxy(playbackUrl,token)
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Web API request will dispose of this object when thread completes")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "These are format strings, not URIs")]
        public HttpResponseMessage Get(string playbackUrl, string token)
        {          
            //if (token.StartsWith("Bearer=", StringComparison.OrdinalIgnoreCase))
            //{
            //    token = token.Substring("Bearer=".Length).Trim();
            //}
            var collection = HttpUtility.ParseQueryString(token);
            var authToken = collection.ToQueryString();
            string armoredAuthToken = HttpUtility.UrlEncode(authToken);

            var httpRequest = (HttpWebRequest)WebRequest.Create(new Uri(playbackUrl));
            httpRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            httpRequest.Timeout = 30000;
            var httpResponse = httpRequest.GetResponse();
            var response = this.Request.CreateResponse();

            try
            {
                var stream = httpResponse.GetResponseStream();
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        const string qualityLevelRegex = @"(QualityLevels\(\d+\))";
                        const string fragmentsRegex = @"(Fragments\([\w\d=-]+,[\w\d=-]+\))";
                        const string urlRegex = @"("")(https?:\/\/[\da-z\.-]+\.[a-z\.]{2,6}[\/\w \.-]*\/?[\?&][^&=]+=[^&=#]*)("")";

                        var baseUrl = playbackUrl.Substring(0, playbackUrl.IndexOf(".ism", System.StringComparison.OrdinalIgnoreCase)) + ".ism";
                        var content = reader.ReadToEnd();

                        var newContent = Regex.Replace(content, urlRegex, string.Format(CultureInfo.InvariantCulture, "$1$2&token={0}$3", armoredAuthToken));
                        var match = Regex.Match(playbackUrl, qualityLevelRegex);
                        if (match.Success)
                        {
                            var qualityLevel = match.Groups[0].Value;
                            newContent = Regex.Replace(newContent, fragmentsRegex, m => string.Format(CultureInfo.InvariantCulture, baseUrl + "/" + qualityLevel + "/" + m.Value));
                        }

                        response.Content = new StringContent(newContent, Encoding.UTF8, "application/vnd.apple.mpegurl");
                    }
                }
            }
            finally
            {
                httpResponse.Close();
            }
            return response;
        }
    }

        public static class QueryExtensions
        {
            public static string ToQueryString(this NameValueCollection collection)
            {           
                IEnumerable<string> segments = from key in collection.AllKeys
                                               from value in collection.GetValues(key)
                                               select string.Format(CultureInfo.InvariantCulture, "{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value));

                return string.Join("&", segments);
            }
        }
    }
}

