using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScanServer_NetCore.Services.Enums;
using ScanServer_NetCore.Services.Interfaces;
using System.Net;
using System.Threading.Tasks;

namespace ScanServer_NetCore.Controllers
{
    /// <summary>
    /// Controller to handle scan action and returning available quality options
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ScanController : ControllerBase
    {        

        private readonly ILogger<ScanController> _logger;
        /// <summary>
        /// Service to execute scans
        /// </summary>
        private readonly IScanService _scanService;

        public ScanController(
            ILogger<ScanController> logger, 
            IScanService scanService)
        {
            _logger = logger;
            _scanService = scanService;
        }

        /// <summary>
        /// Scan a file to the given folder and fileName
        /// </summary>
        /// <param name="folderName">folder to put result in</param>
        /// <param name="fileName">name the file should get</param>
        /// <param name="scanQuality">quality to scan with</param>
        /// <returns>filename of the resultfile (may changed if original name was taken) or null if scanning failed</returns>
        [HttpPost]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> Scan(string folderName, string fileName, ScanQuality scanQuality)
        {            
            var result = await _scanService.Scan(folderName, fileName, scanQuality);
            if (string.IsNullOrEmpty(result))
            {
                return BadRequest($"Failed to scan file!");
            }
            return Ok(result);
        }
    }
}
