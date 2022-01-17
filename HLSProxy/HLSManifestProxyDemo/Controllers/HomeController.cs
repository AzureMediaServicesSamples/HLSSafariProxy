using HLSManifestProxyDemo.Services;
using Microsoft.Azure.CloudVideo.VideoManagementService.Cms.Controllers.HLSManifestProxyDemo.Controllers;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace HLSManifestProxyDemo.Controllers
{
    public class HomeController : Controller
    {
        private const string C_MANIFESTPROXYURLTEMPLATE = "http://{0}/api/ManifestProxy";
        private static readonly string Issuer = "JWTIssuer";
        private static readonly string Audience = "myAudience2";
        private static readonly string PrimaryVerificationKey = "dGhpc2lzbXlzeW1tZXRyaWNrZXlhbmRpdGJlbG9uZ3N0b21l";
        private static byte[] TokenSigningKey = new byte[40];
        private static readonly string ContentKeyPolicyName = "AESwithToken";

        public HomeController()
        {            
        
        }

        public ActionResult Index(string playbackurl)
        {
            ViewBag.Title = "Home Page";
            ViewData["playbackurl"] = playbackurl;


            // Set a token signing key that you want to use.   This is used to encrypt assets and is required
            TokenSigningKey = Convert.FromBase64String(PrimaryVerificationKey);  //SymmetricKey
            var token = GetTokenAsync(Issuer, Audience, String.Empty, TokenSigningKey);
            ViewData["webtoken"] = token;

            return View();
        }

        public ActionResult Manifest(string playbackUrl, string webtoken)
        //public ActionResult Manifest(string playbackUrl)
        {
            //var token = getTokenFor(playbackUrl);

            //var collection = HttpUtility.ParseQueryString(webtoken);
            //var ttoken = collection.ToQueryString();
            //string token = HttpUtility.UrlEncode(ttoken);

            var token = "Bearer=" + webtoken;

            var manifestRetriever = getManifestRetriever();                      

            var modifiedTopLeveLManifest = manifestRetriever.GetTopLevelManifestForToken(playbackUrl, token);          

            var response = new ContentResult();
            response.Content = modifiedTopLeveLManifest;
            response.ContentType = @"application/vnd.apple.mpegurl";
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("X-Content-Type-Options", "nosniff");
            Response.AppendHeader("Cache-Control", "max-age=259200");

            return response;            
        }       

        private TopLevelManifestRetriever _manifestRetriever;
        private TopLevelManifestRetriever getManifestRetriever()
        {
            if (_manifestRetriever == null)
            {
                var hostPortion = Request.Url.Host;
                var manifestProxyUrl = String.Format(C_MANIFESTPROXYURLTEMPLATE, hostPortion);                
                _manifestRetriever = new TopLevelManifestRetriever(manifestProxyUrl);                
            }

            return _manifestRetriever;
        }
                
        private string getTokenFor(string playbackUrl)
        {
            //TODO: implement your own token acquisition logic:        
            //return @"urn%3amicrosoft%3aazure%3amediaservices%3acontentkeyidentifier=8d6f52e7-c545-46e9-bbdd-19ee5e87fd55&Audience=urn%3atest&ExpiresOn=1483149817&Issuer=http%3a%2f%2ftestacs&HMACSHA256=v87VmN1z7890WtM6xauT%2bpo3k%2fsaq0yhlVASkcr5RJM%3d";
            return @"Audience=urn%3atest&ExpiresOn=1464936320&Issuer=http%3a%2f%2ftestacs&HMACSHA256=iuUh%2bS3RP7rKyOxF1wytfqQcN0CWZ3T%2fCwGh%2bNIJQmE%3d";                               
            //return @"Audience=urn%3Acaa&Issuer=http%3A%2F%2Fwww.caa.com%2F&ExpiresOn=3329668306&HMACSHA256=NPbPmS5bL1fKrjJ0QoJpQfY8iXT9Ycl89QUIPUImcZg%3D";                     
            //return @"Audience=urn:caa&Issuer=http://www.caa.com/&ExpiresOn=3329668306&HMACSHA256=NPbPmS5bL1fKrjJ0QoJpQfY8iXT9Ycl89QUIPUImcZg=";                     
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
}
