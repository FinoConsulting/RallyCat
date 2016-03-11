using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RallyCat.Core.Rally;
using RallyCat.Core.Services;

namespace RallyCat.Core.Tests
{
    [TestClass]
    public class GraphicServiceTest
    {
        [TestMethod]
        public void DrawKanbanItemFrameBlockedTest()
        {
            var service  = new GraphicService();
            var item     = new KanbanItem("C1", "US1234", "Cheng Huang",
                "aabbccddxxxxxxxxxxxxxxxxxxxxxddsssseeqqzxxxxxxxxxxxxxxxxxxxxxbbbbdeevsdfaefefadsfasefaefasdfsdfsafas",
                "blockedblockedblockedblockedblockedblockedblockedblockedblockedblockedblocked");
            var rec      = service.GetKanbanItemSize(new Point(0, 0), 400, 20, 20, item);
            var image    = new Bitmap(rec.Width, rec.Height);

            using (var g = Graphics.FromImage(image))
            {
                service.DrawOneKanbanItem(g, new Point(0, 0), 400, 20, 20, item);
                image.Save(@"c:\temp\pngTempBlocked.png", ImageFormat.Png);
                image.Dispose();
            }
        }

        [TestMethod]
        public void DrawKanbanItemFrameNonBlockedTest()
        {
            var service  = new GraphicService();
            var item     = new KanbanItem("C1", "US4321", "Cheng Huang",
                "aabbccddxxxxxxxxxxxxxxxxxxxxxddsssseeqqzxxxxxxxxxxxxxxxxxxxxxbbbbdeevsdfaefefadsfasefaefasdfsdfsafas");
            var rec      = service.GetKanbanItemSize(new Point(0, 0), 400, 20, 20, item);
            Image image  = new Bitmap(rec.Width, rec.Height);

            using (var g = Graphics.FromImage(image))
            {
                service.DrawOneKanbanItem(g, new Point(0, 0), 400, 20, 20, item);
                image.Save(@"c:\temp\pngTempNonBlocked.png", ImageFormat.Png);
            }
        }

        [TestMethod]
        public void DrawOneKanbanColumnTest()
        {
            var service        = new GraphicService();
            var item0          = new KanbanItem("C1", "US4321", "Cheng Huang",
                "aabbccddxxxxxxxxxxxxxxxxxxxxxddsssseeqqzxxxxxxxxxxxxxxxxxxxxxbbbbdeevsdfaefefadsfasefaefasdfsdfsafas");
            var item1          = new KanbanItem("C1", "US4321", "Cheng Huang",
                "aabbccddxxxxxxxxxxxxxxxxxxxxxddsssseeqqzxxxxxxxxxxxxxxxxxxxxxbbbbdeevsdfaefefadsfasefaefasdfsdfsafas",
                "BLOCKED BLOCKED BLOCKED BBBBBBBB");
            var items          = new List<KanbanItem> {item0, item1};
            var rec            = service.GetOneKanbanColumnSize(new Point(0, 0), 400, 20, 50, 20, 100, items);
            Image image        = new Bitmap(rec.Width, rec.Height);

            using (var g = Graphics.FromImage(image))
            {
                service.DrawOneKanbanColumn(g, new Point(0, 0), 400, 20, 50, 20, "Development", 100, items);
                image.Save(@"c:\temp\pngTempOneColumn.png", ImageFormat.Png);
            }
        }

        [TestMethod]
        public void DrawWholeKanbanTest()
        {
            var service = new GraphicService();
            var item0   = new KanbanItem("C1", "US1234", "Cheng Huang",
                "aabbccddxxxxxxxxxxxxxxxxxxxxxddsssseeqqzxxxxxxxxxxxxxxxxxxxxxbbbbdeevsdfaefefadsfasefaefasdfsdfsafas");
            var item1   = new KanbanItem("C1", "DE4321", "Cheng Huang",
                "aabbccddxxxxxxxxxxxxxxxxxxxxxddsssseeqqzxxxxxxxxxxxxxxxxxxxxxbbbbdeevsdfaefefadsfasefaefasdfsdfsafas",
                "BLOCKED BLOCKED BLOCKED BBBBBBBB");
            var item2   = new KanbanItem("C2", "US4321", "Cheng Huang",
                "aabbccddxxxxxxxxxxxxxxxxxxxxxddsssseeqqzxxxxxxxxxxxxxxxxxxxxxbbbbdeevsdfaefefadsfasefaefasdfsdfsafas");
            var item3   = new KanbanItem("C2", "US4321", "Cheng Huang", "aabbccddxxxxxxxxxxxefadsfasefaefasdfsdfsafas");
            var item4   = new KanbanItem("C3", "US22221", "Cheng Huang",
                "aabbccddxxxxxxxxxxxxxxxxxxxxxdsdfgsdfgsfgsdsssseeqqzxxxxxxxxxxxxxxxxxxxxxbbbbdeevsdfaefefadsfasefaefasdfsdfsafas");
            var item5   = new KanbanItem("C4", "US4321", "Cheng Huang",
                "aabbccddxxxxxxxxxxxxxxxxxxxxxxxxxbbbbdeevsdfaefefadsfasefaefasdfsdfsafas");
            var item6   = new KanbanItem("C4", "US4321", "Cheng Huang", "aabbccfaefasdfsdfsafas", "abeedde");

            var itemg1  = new List<KanbanItem> {item0, item1};
            var itemg2  = new List<KanbanItem> {item2, item3, item0, item5};
            var itemg3  = new List<KanbanItem> {item4};
            var itemg4  = new List<KanbanItem> {item5, item6};

            var idx = new Dictionary<String, List<KanbanItem>>
            {
                { "C1", itemg1 },
                { "C2", itemg2 },
                { "C3", itemg3 },
                { "C4", itemg4 }
            };
            var img = service.DrawWholeKanban(400, 20, 50, 20, 100, idx);

            img.Save(@"c:\temp\pngTempWholeKanban.png", ImageFormat.Png);
        }
    }
}