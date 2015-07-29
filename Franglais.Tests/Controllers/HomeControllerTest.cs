using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Franglais;
using Franglais.Controllers;

namespace Franglais.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Lobby()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Lobby() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Profile()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.UserProfile() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ChatRoom()
        {
            ChatHub.ChatRooms.Add(new Models.ChatRoom() { Id = 1 });
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.ChatRoom(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
