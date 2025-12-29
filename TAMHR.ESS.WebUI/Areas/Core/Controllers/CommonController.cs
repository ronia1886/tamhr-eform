using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    [ApiController]
    [Route("api/common")]
    public class CommonApiController : ControllerBase
    {
        [HttpGet("keepalive")]
        //[Authorize]
        [AllowAnonymous]
        public IActionResult KeepAlive()
        {
            return Content("OK");
        }

        [HttpPost("get-time")]
        [Authorize]
        public IActionResult GetServerTime()
        {
            var serverTime = DateTime.UtcNow; // Waktu server dalam UTC
            return Ok(new { serverTime });
        }
    }
    #endregion
}