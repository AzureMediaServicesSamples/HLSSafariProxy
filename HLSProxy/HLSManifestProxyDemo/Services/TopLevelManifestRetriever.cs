﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Web;

namespace HLSManifestProxyDemo.Services
{
    public class TopLevelManifestRetriever
    {
        private readonly string manifestProxyUrl;
        public TopLevelManifestRetriever(string manifestProxyUrl)
        {
            this.manifestProxyUrl = manifestProxyUrl;
        }

        public string GetTopLevelManifestForToken(string topLeveLManifestUrl, string token)
        {            
            var httpRequest = (HttpWebRequest)WebRequest.Create(new Uri(topLeveLManifestUrl));
            httpRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            httpRequest.Timeout = 30000;
            var httpResponse = httpRequest.GetResponse();            

            try
            {
                var stream = httpResponse.GetResponseStream();
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        const string qualityLevelRegex = @"(QualityLevels\(\d+\)/Manifest\(.+\))";

                        var toplevelmanifestcontent = reader.ReadToEnd();

                        var topLevelManifestBaseUrl = topLeveLManifestUrl.Substring(0, topLeveLManifestUrl.IndexOf(".ism", System.StringComparison.OrdinalIgnoreCase)) + ".ism";
                        var urlEncodedTopLeveLManifestBaseUrl = HttpUtility.UrlEncode(topLevelManifestBaseUrl);
                        var urlEncodedToken = HttpUtility.UrlEncode(token);
                      
                        var newContent = Regex.Replace(toplevelmanifestcontent,
                                                      qualityLevelRegex,
                                                      string.Format(CultureInfo.InvariantCulture,
                                                           "{0}?playbackUrl={1}/$1&token={2}",
                                                           manifestProxyUrl,
                                                           urlEncodedTopLeveLManifestBaseUrl,
                                                           urlEncodedToken));                  

                        return newContent;
                    }
                }
            }
            finally
            {
                httpResponse.Close();
            }
            return null;
        }
    }
}