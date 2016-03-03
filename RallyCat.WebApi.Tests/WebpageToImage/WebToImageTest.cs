using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WatiN.Core;


namespace RallyCat.WebApi.Tests.WebpageToImage
{
    [TestClass]
    public class WebToImageTest
    {
        [TestMethod]
        public void WebPageToImageTest()
        {
            //Capture(null, null);
            using (var ie = new IE("http://watin.org/"))
            {
                ie.CaptureWebPageToFile(@"c:\temp\g1.jpg");
            }
        }

        protected void Capture(Object sender, EventArgs e)
        {
            const String url = "http://www.aspsnippets.com/Articles/Capture-Screenshot-Snapshot-Image-of-Website-Web-Page-in-ASPNet-using-C-and-VBNet.aspx";
            var thread = new Thread(delegate()
            {
                using (var browser = new WebBrowser())
                {
                    browser.ScrollBarsEnabled = false;
                    browser.AllowNavigation = true;
                    browser.Navigate(url);
                    browser.Width = 1024;
                    browser.Height = 768;
                    browser.DocumentCompleted += DocumentCompleted;
                    while (browser.ReadyState != WebBrowserReadyState.Complete)
                    {
                        Application.DoEvents();
                    }
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private static void DocumentCompleted(Object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var browser = sender as WebBrowser;
            var w = browser.Document.Body.ScrollRectangle.Width;
            var h = browser.Document.Body.ScrollRectangle.Height;
            browser.ScriptErrorsSuppressed = true;
            using (var bitmap = new Bitmap(w, h))
            {
                browser.DrawToBitmap(bitmap, new Rectangle(0, 0, browser.Width, browser.Height));
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    var bytes = stream.ToArray();
                    using (var sw = new FileStream(@"c:\temp\1.png", FileMode.OpenOrCreate))
                    {
                        using (var bw = new BinaryWriter(sw))
                        {
                            bw.Write(bytes);
                        }
                    }
                    //imgScreenShot.Visible = true;
                    //imgScreenShot.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(bytes);
                }
            }
        }
    }
}