// -----------------------------------------------------------------------------
//  Copyright (C) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------------

namespace Microsoft.Azure.CloudVideo.VideoManagementService.Cms.Controllers
{
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
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

            private static readonly string Issuer = "JWTIssuer";
            private static readonly string Audience = "myAudience2";
            private static readonly string PrimaryVerificationKey = "dGhpc2lzbXlzeW1tZXRyaWNrZXlhbmRpdGJlbG9uZ3N0b21l";
            private static byte[] TokenSigningKey = new byte[40];
            private static readonly string ContentKeyPolicyName = "AESwithToken";
            /// <summary>
            ///     Support for: GET /api/ManifestProxy(playbackUrl,token)
            /// </summary>
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Web API request will dispose of this object when thread completes")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "These are format strings, not URIs")]
        public HttpResponseMessage Get(string playbackUrl, string token)
        {
                //if (token.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
                //{
                // token = token.Substring("Bearer".Length).Trim();
                //}
                // Set a token signing key that you want to use.   This is used to encrypt assets and is required
                TokenSigningKey = Convert.FromBase64String(PrimaryVerificationKey);  //SymmetricKey
                token = GetTokenAsync(Issuer, Audience, String.Empty, TokenSigningKey);

                token = "Bearer=" + token;
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

            /// <summary>
            /// Create a token that will be used to protect your stream.
            /// Only authorized clients would be able to play the video.  
            /// </summary>
            /// <param name="issuer">The issuer is the secure token service that issues the token. </param>
            /// <param name="audience">The audience, sometimes called scope, describes the intent of the token or the resource the token authorizes access to. </param>
            /// <param name="keyIdentifier">The content key ID.</param>
            /// <param name="tokenVerificationKey">Contains the key that the token was signed with. </param>
            /// <returns></returns>
            // <GetToken>
            private static string GetTokenAsync(string issuer, string audience, string keyIdentifier, byte[] tokenVerificationKey)
            {
                var tokenSigningKey = new SymmetricSecurityKey(tokenVerificationKey);

                SigningCredentials cred = new SigningCredentials(
                    tokenSigningKey,
                    // Use the  HmacSha256 and not the HmacSha256Signature option, or the token will not work!
                    SecurityAlgorithms.HmacSha256,
                    SecurityAlgorithms.Sha256Digest);

                //Claim[] claims = new Claim[]
                //{
                //    new Claim(ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim.ClaimType, keyIdentifier)
                //};

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: null,
                    notBefore: DateTime.Now.AddMinutes(-5),
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: cred
                    );

                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

                return handler.WriteToken(token);
            }
            // </GetToken>
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

