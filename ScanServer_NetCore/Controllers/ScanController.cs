using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScanServer_NetCore.Controllers
{
    /// <summary>
    /// Controller to handle scan action and returning available quality options
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ScanController : ControllerBase
    {        

        private readonly ILogger<ScanController> _logger;

        public ScanController(ILogger<ScanController> logger)
        {
            _logger = logger;
        }
    }
}
