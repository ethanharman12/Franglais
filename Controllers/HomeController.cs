using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Franglais.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ChatRoom(int id)
        {
            //ViewBag.roomId = id;

            var room = ChatHub.ChatRooms.FirstOrDefault(rm => rm.Id == id);

            if (room != null)
            {
                //ViewBag.users = room.Users;
            }

            return View(room);
        }

        public ActionResult Lobby()
        {
            return View();
        }
    }
}