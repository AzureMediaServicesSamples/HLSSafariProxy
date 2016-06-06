using HLSManifestProxyDemo.Services;
using Microsoft.Azure.CloudVideo.VideoManagementService.Cms.Controllers.HLSManifestProxyDemo.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace HLSManifestProxyDemo.Controllers
{
    public class HomeController : Controller
    {
        private const string C_MANIFESTPROXYURLTEMPLATE = "http://{0}/api/ManifestProxy";
        
        public HomeController()
        {            
        
        }

        public ActionResult Index(string playbackurl)
        {
            ViewBag.Title = "Home Page";
            ViewData["playbackurl"] = playbackurl;

            return View();
        }

        public ActionResult Manifest(string playbackUrl, string webtoken)
        //public ActionResult Manifest(string playbackUrl)
        {
            //var token = getTokenFor(playbackUrl);

            //var collection = HttpUtility.ParseQueryString(webtoken);
            //var ttoken = collection.ToQueryString();
            //string token = HttpUtility.UrlEncode(ttoken);

            var token = webtoken;

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
            return @"urn%3amicrosoft%3aazure%3amediaservices%3acontentkeyidentifier=2f04925a-ff16-474a-87cc-c73bbb76ba20&Audience=urn%3atest&ExpiresOn=1461902688&Issuer=http%3a%2f%2ftestacs&HMACSHA256=5corSb9EJs3ns7auiu%2foclCCvyfuGdKEa%2fK1eZKDN7o%3d";                               
            //return @"Audience=urn%3Acaa&Issuer=http%3A%2F%2Fwww.caa.com%2F&ExpiresOn=3329668306&HMACSHA256=NPbPmS5bL1fKrjJ0QoJpQfY8iXT9Ycl89QUIPUImcZg%3D";                     
            //return @"Audience=urn:caa&Issuer=http://www.caa.com/&ExpiresOn=3329668306&HMACSHA256=NPbPmS5bL1fKrjJ0QoJpQfY8iXT9Ycl89QUIPUImcZg=";                     
        }
    }
}
