using Microsoft.AspNetCore.Mvc;
using RemoteConnect;

namespace RemoteDesktop.Controllers
{
    [Route("home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [Route("Index")]
        public void Index()
        {
            RDP.MessageLoopApartment.I.Run(() =>
            {
                var ca = new RDP();
                ca.Connect("Test", "Aa123456@", "192.168.3.86");
            }, CancellationToken.None);

        }
    }
}
