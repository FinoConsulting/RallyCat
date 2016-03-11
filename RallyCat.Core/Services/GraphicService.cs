using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using RallyCat.Core.Rally;

namespace RallyCat.Core.Services
{
    public class GraphicService
    {
        public Rectangle GetKanbanItemSize(Point startPoint, int recWidth, int headerHeight, int textMargin,
            KanbanItem item)
        {
            
            Image img = new Bitmap(1, 1);
            using (var g = Graphics.FromImage(img))
            {
                float currentHeight = 0;
                var halfMargin = textMargin/2;
                var headerRec = new Rectangle(new Point(startPoint.X, startPoint.Y),
                    new Size(recWidth + 1, headerHeight));
                var font = new Font("Segoe UI", 20, FontStyle.Regular);

                currentHeight += headerRec.Bottom + halfMargin;
                var formattedIdSize = g.MeasureString(item.FormattedId, font);
                var assignedToSize = g.MeasureString(item.AssignedTo, font);

                var descriptionSize = g.MeasureString(item.StoryDescription, font, headerRec.Width - 2*textMargin);
                var recDescription = new RectangleF(
                    new PointF(headerRec.Left + textMargin, currentHeight + textMargin), descriptionSize);

                currentHeight += formattedIdSize.Height + textMargin;
                currentHeight += assignedToSize.Height + textMargin;
                currentHeight += recDescription.Height + textMargin;

                if (item.IsBlocked)
                {
                    Image blockedIcon = Resources.blocked;
                    var blockedReasonSize = g.MeasureString(item.BlockedReason, font,
                        headerRec.Width - 2*textMargin - blockedIcon.Width - halfMargin);
                    var blockedReasonBackgroundSize = new SizeF(headerRec.Width - 2*textMargin, blockedReasonSize.Height);

                    currentHeight += blockedReasonBackgroundSize.Height + textMargin;
                }
                currentHeight += halfMargin;
                //currentBottom += halfMargin;
                //Frame, wire frame rectangle
                var recFrame = new Rectangle(startPoint, new Size(recWidth, (int) currentHeight));
                return recFrame;
            }
        }

        public Image DrawWholeKanban(int rectWidth, int headerHeight, int stackItemMargin,
            int textMargin, int categoryHeaderHeight, Dictionary<string, List<KanbanItem>> kanbanItems)
        {
            var next = new Point(0, 0);
            var maxHeight = 0;
            var width = 0;
            foreach (var key in kanbanItems.Keys)
            {
                var oneCol = GetOneKanbanColumnSize(next, rectWidth, headerHeight, stackItemMargin, textMargin,
                    categoryHeaderHeight, kanbanItems[key]);
                maxHeight = Math.Max(maxHeight, oneCol.Height);
                width += oneCol.Width;
            }

            Image img = new Bitmap(width, maxHeight);
            using (var g = Graphics.FromImage(img))
            {
                g.Clear(Color.White);
                next = new Point(0, 0);
                foreach (var key in kanbanItems.Keys)
                {
                    var oneCol = GetOneKanbanColumnSize(next, rectWidth, headerHeight, stackItemMargin, textMargin,
                        categoryHeaderHeight, kanbanItems[key]);
                    DrawOneKanbanColumn(g, next, rectWidth, headerHeight, stackItemMargin, textMargin, key,
                        categoryHeaderHeight, kanbanItems[key]);
                    next.X += oneCol.Width;
                }
                return img;
            }
        }


        public Rectangle DrawOneKanbanColumn(Graphics g, Point startPoint, int rectWidth, int headerHeight,
            int stackItemMargin,
            int textMargin, string category, int categoryHeight, List<KanbanItem> items)
        {
          
            var next = startPoint;
            next.X += stackItemMargin;
            next.Y += stackItemMargin;

            var font = new Font("Segoe UI", 25, FontStyle.Bold);
            var sf = new StringFormat {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center};
            Brush brush = new SolidBrush(Color.Black);
            g.DrawString(category, font, brush, new RectangleF(next.X, next.Y, rectWidth, categoryHeight), sf);

            next.Y += categoryHeight;
            if (items == null)
            {
                return new Rectangle(startPoint, new Size(stackItemMargin + rectWidth + stackItemMargin, next.Y));
            }
            foreach (var item in items)
            {
                var rec = DrawOneKanbanItem(g, next, rectWidth, headerHeight, textMargin, item);
                next.Y = rec.Bottom + stackItemMargin;
            }
            return new Rectangle(startPoint, new Size(stackItemMargin + rectWidth + stackItemMargin, next.Y));
        }

        public Rectangle GetOneKanbanColumnSize(Point startPoint, int rectWidth, int headerHeight, int stackItemMargin,
            int textMargin, int categoryHeight, List<KanbanItem> items)
        {
         
            var next = startPoint;
            next.X += stackItemMargin;
            next.Y += categoryHeight;
            next.Y += stackItemMargin;
            var lastHeight = 0;
            if (items == null)
            {
                return new Rectangle(startPoint, new Size(stackItemMargin + rectWidth + stackItemMargin, next.Y));
            }
            foreach (var item in items)
            {
                var rec = GetKanbanItemSize(next, rectWidth, headerHeight, textMargin, item);
                next.Y = rec.Height + stackItemMargin;
            }

            return new Rectangle(startPoint, new Size(stackItemMargin + rectWidth + stackItemMargin, next.Y));
        }


        public Rectangle DrawOneKanbanItem(Graphics g, Point startPoint, int recWidth, int headerHeight, int textMargin,
            KanbanItem item)
        {

            if (item == null)
            {
                return new Rectangle(new Point(0,0), new Size(0,0));
            }
            float currentHeight = 0;

            var halfMargin = textMargin/2;

            //Header, solid rectangle
            string htmlColor = item.DisplayColor;
            Color itemHeaderColor= ColorTranslator.FromHtml(htmlColor);
            Brush headerBrush = new SolidBrush(itemHeaderColor);
            var headerRec = new Rectangle(new Point(startPoint.X, startPoint.Y), new Size(recWidth + 1, headerHeight));

            var font = new Font("Segoe UI", 20, FontStyle.Regular);
            Debug.WriteLine("Draw:currentHeight0:" + currentHeight);
            currentHeight = headerRec.Bottom + halfMargin;
            Debug.WriteLine("Draw:currentHeight1:" + currentHeight);
            //FormattedId
            Brush formattedIdBrush = new SolidBrush(Color.DeepSkyBlue);
            var formattedIdSize = g.MeasureString(item.FormattedId, font);
            g.DrawString(item.FormattedId, font, formattedIdBrush, headerRec.Left + textMargin, currentHeight);

            currentHeight += formattedIdSize.Height + textMargin;
            Debug.WriteLine("Draw:currentHeight2:" + currentHeight);
            //AssignedToId
            Brush assignedToBrush = new SolidBrush(Color.Black);
            var assignedToSize = g.MeasureString(item.AssignedTo, font);
            g.DrawString(item.AssignedTo, font, assignedToBrush, headerRec.Left + textMargin, currentHeight);

            currentHeight += assignedToSize.Height + textMargin;
            Debug.WriteLine("Draw:currentHeight3:" + currentHeight);
            //Description
            Brush descriptionBrush = new SolidBrush(Color.Black);
            var descriptionSize = g.MeasureString(item.StoryDescription, font, headerRec.Width - 2*textMargin);
            var recDescription = new RectangleF(new PointF(headerRec.Left + textMargin, currentHeight), descriptionSize);


            g.DrawString(item.StoryDescription, font, descriptionBrush, recDescription);
            currentHeight += recDescription.Height + textMargin;
            Debug.WriteLine("Draw:currentHeight4:" + currentHeight);
            if (item.IsBlocked)
            {
                Image blockedIcon = Resources.blocked;

                var blockedReasonSize = g.MeasureString(item.BlockedReason, font,
                    headerRec.Width - 2*textMargin - blockedIcon.Width - halfMargin);
                var blockedReasonBackgroundSize = new SizeF(headerRec.Width - 2*textMargin, blockedReasonSize.Height);
                Brush blockedReasonBackgroundBrush = new SolidBrush(Color.LightGray);
                g.FillRectangle(blockedReasonBackgroundBrush,
                    new RectangleF(new PointF(headerRec.Left + textMargin, currentHeight), blockedReasonBackgroundSize));

                Brush blockedReasonBrush = new SolidBrush(Color.Black);
                var recBlockedReason =
                    new RectangleF(
                        new PointF(headerRec.Left + blockedIcon.Width + textMargin + halfMargin, currentHeight),
                        blockedReasonSize);
                g.DrawString(item.BlockedReason, font, blockedReasonBrush, recBlockedReason);

                var recBlockedIconSmall = new RectangleF(headerRec.Left + textMargin + halfMargin,
                    currentHeight + recBlockedReason.Height/2 - blockedIcon.Height/2, blockedIcon.Width,
                    blockedIcon.Height);
                g.DrawImage(blockedIcon, recBlockedIconSmall);

                currentHeight += blockedReasonBackgroundSize.Height + textMargin;
            }

            currentHeight += halfMargin;
            //Frame, wire frame rectangle

            var framePen = item.IsBlocked ? new Pen(Color.DarkRed, 2) : new Pen(Color.Gray, 2);
            //g.Clear(Color.White);
            var recFrame = new Rectangle(startPoint, new Size(recWidth, (int) currentHeight - startPoint.Y));
            g.DrawRectangle(framePen, recFrame);
            g.FillRectangle(headerBrush, headerRec);

            return recFrame;
        }
    }
}