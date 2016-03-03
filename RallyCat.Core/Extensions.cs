using System;
using System.Collections.Generic;
using System.IO;
using RallyCat.Core.Services;


namespace RallyCat.Core
{
    public static class Extensions
    {
        public static Stream ToStream(this String str)
        {
            // todo: RESEARCH: using blocks ?
            //using (var stream = new MemoryStream())
            //{
            //    using (var sw = new StreamWriter(stream))
            //    {
            //        // do the stuff
            //    }
            //}

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static String HtmlToPlainText(this String htmlString)
        {
            var h = new HtmlConvertService();
            return h.ConvertFromString(htmlString);
        }

        public static List<String> GetAllImageSrcs(this String htmlString)
        {
            var h = new HtmlConvertService();
            return h.GetAllImageSrcs(htmlString);
        }
    }
}