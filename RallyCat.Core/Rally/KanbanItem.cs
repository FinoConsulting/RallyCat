using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RallyCat.Core.Rally
{
    public class KanbanItem
    { 
        public String  AssignedTo       { get; set; }
        public String  BlockedReason    { get; set; }
        public String  DisplayColor     { get; set; }
        public String  FormattedId      { get; set; }
        public Boolean IsBlocked        { get; set; }
        public String  KanbanState      { get; set; }
        public String  FullDescription  { get; set; }
        public String  StoryDescription { get; set; }

        public KanbanItem(String kanbanState, String formattedId, String displayColor, String assignedTo, String fullDescription, String storyDescription)
        {
            KanbanState      = kanbanState;
            FormattedId      = formattedId;
            DisplayColor     = displayColor;
            AssignedTo       = assignedTo;
            FullDescription  = fullDescription;
            StoryDescription = storyDescription;
            IsBlocked        = false;
          
        }

        public KanbanItem(String kanbanState, String formattedId, String displayColor, String assignedTo, String fullDescription, String storyDescription, String blockedReason)
        {
            KanbanState      = kanbanState;
            FormattedId      = formattedId;
            DisplayColor     = displayColor;
            AssignedTo       = assignedTo;
            FullDescription  = fullDescription;
            StoryDescription = storyDescription;
            IsBlocked        = true;
            BlockedReason    = blockedReason;
        }

        public static KanbanItem ConvertFrom(dynamic queryResult, String kanbanStateKeyWord)
        {

            var s = queryResult;
            var kanbanState  = s[kanbanStateKeyWord] ?? "None";
            var formattedId  = s["FormattedID"];
            var defaultColor = "#00A9E0";

            if (formattedId.ToLower().Contains("de"))
            {
                defaultColor = "#F9A814";
            }

            var displayColor    = s["DisplayColor"] ?? defaultColor;
            var owner           = s["Owner"] == null? "(None)": s["Owner"]["_refObjectName"];
            var name            = s["Name"];
            var blocked         = s["Blocked"];
            var blockedReason   = s["BlockedReason"];
            var fullDescription = ((String)s["Description"]).HtmlToPlainText();

            if (blocked)
            {
                return new KanbanItem(kanbanState, formattedId, displayColor, owner, fullDescription, name, blockedReason);
            }
            return new KanbanItem(kanbanState, formattedId, displayColor, owner, fullDescription, name);
        }
    }
}
