using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RallyCat.Core.Rally
{
    public class KanbanItem
    {
        public String  FormattedId      { get; set; }
        public String  AssignedTo       { get; set; }
        public String  StoryDescription { get; set; }
        public Boolean IsBlocked        { get; set; }
        public String  BlockedReason    { get; set; }
        public String  KanbanState      { get; set; }

        public KanbanItem(String kanbanState, String formattedId, String assignedTo, String storyDescription)
        {
            KanbanState      = kanbanState;
            FormattedId      = formattedId;
            AssignedTo       = assignedTo;
            StoryDescription = storyDescription;
            IsBlocked        = false;
            
        }

        public KanbanItem(String kanbanState, String formattedId, String assignedTo, String storyDescription, String blockedReason)
        {
            KanbanState      = kanbanState;
            FormattedId      = formattedId;
            AssignedTo       = assignedTo;
            StoryDescription = storyDescription;
            IsBlocked        = true;
            BlockedReason    = blockedReason;
        }

        public static KanbanItem ConvertFrom(dynamic queryResult, String kanbanStateKeyWord)
        {
            var s = queryResult;

            var formattedId = s["FormattedID"];
            // var owner
            // var name
            // etc.

            // todo: Jenny, please go make these not hurt to look at

            if (s["Blocked"])
            {
                return new KanbanItem(s[kanbanStateKeyWord] ?? "None", formattedId, s["Owner"] == null ? "(None)" : s["Owner"]["_refObjectName"], s["Name"], s["BlockedReason"]);
            }
            else
            {
                return new KanbanItem(s[kanbanStateKeyWord] ?? "None", formattedId, s["Owner"] == null ? "(None)" : s["Owner"]["_refObjectName"], s["Name"]);
            }
        }
    }
}
