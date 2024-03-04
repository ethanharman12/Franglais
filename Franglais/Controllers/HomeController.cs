using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Franglais.Controllers
{
    public class HomeController : Controller
    {
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
    }
}