using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using ScanServer_NetCore.Services.Interfaces;
using System;
using System.Net;

namespace ScanServer_NetCore.Controllers
{
    /// <summary>
    /// Controller to handle updates if an old version of an app asks for one
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private IUpdateService _updateService;

        public StatusController(IUpdateService updateService)
        {
            _updateService = updateService;
        }

        /// <summary>
        /// Returns if the app with the requesting version is old or not. 
        /// </summary>
        /// <param name="versionString">current app version</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public bool UpdateAvailable(string versionString)
        {            
            return _updateService.NewVersionAvailable(versionString);
        }

        /// <summary>
        /// Returns the newest version as apk file
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public ActionResult GetUpdateFile()
        {
            var newestFileInfo = _updateService.GetNewestFile();
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(newestFileInfo.Name, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return File(newestFileInfo.OpenRead(), contentType, newestFileInfo.Name, true);
        }

        /// <summary>
        /// Return true, used to see if server is reachable/running
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(bool), (int) HttpStatusCode.OK)]
        public bool Ping()
        {
            return true;
        }

        
    }
}
