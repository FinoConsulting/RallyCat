using Microsoft.VisualStudio.TestTools.UnitTesting;
using RallyCat.WebApi.Controllers;


namespace RallyCat.WebApi.Tests.Controllers
{
    [TestClass]
    public class RallyControllerTest
    {
        [TestMethod]
        public void GetRallyItemTest()
        {
            var c    = new RallyController();
            var item = c.GetItem("DE1877", "de-support");
            Assert.IsNotNull(item);
        }

        [TestMethod]
        public void GetRallyKanbanTest()
        {
            //RallyController c = new RallyController();
            //Image img =c.GetKanban("de-support");
            //img.Save(@"c:\temp\RallyKanbanFull.png",ImageFormat.Png);
        }
    }
}