using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Franglais.Tests
{
    [TestClass]
    public class GoogleTranslateApiTests
    {
        [TestMethod]
        public async Task TranslateHelloWorld()
        {
            GoogleTranslator translator = new GoogleTranslator();

            var translation = await translator.TranslateMessage("Hello World", "en", "fr");

            Assert.AreEqual("Bonjour Le Monde", translation);
        }
    }
}
