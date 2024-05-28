using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RemoteController.Server.Controllers
{
    public class EquipmentsController : ControllerBase
    {
        private readonly ILogger<EquipmentsController> _logger;
        public EquipmentsController(ILogger<EquipmentsController> logger)
        {
            _logger = logger;
        }


    }
}
