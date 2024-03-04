using FranglaisChat.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;

namespace FranglaisChat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public ActionResult ChatRoom(int id)
        {
            var room = ChatHub.ChatRooms.FirstOrDefault(rm => rm.Id == id);

            if (room == null)
            {
                return Redirect("../Lobby");
            }

            return View(room);
        }

        public ActionResult Lobby()
        {
            return View();
        }

        public ActionResult UserProfile()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
