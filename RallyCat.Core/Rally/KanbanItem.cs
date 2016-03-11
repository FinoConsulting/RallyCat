using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RallyCat.Core.Rally
{
    public class KanbanItem
    {
        public string FormattedId { get; set; }
        public string AssignedTo { get; set; }
        public string StoryDescription { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockedReason { get; set; }
        public string KanbanState { get; set; }
        public string DisplayColor { get; set; }

        public KanbanItem(string kanbanState, string formattedId, string displayColor, string assignedTo, string storyDescription)
        {
            KanbanState = kanbanState;
            FormattedId = formattedId;
            AssignedTo = assignedTo;
            StoryDescription = storyDescription;
            IsBlocked = false;
            DisplayColor = displayColor;

        }

        public KanbanItem(string kanbanState, string formattedId, string displayColor, string assignedTo, string storyDescription, string blockedReason)
        {
            KanbanState = kanbanState;
            FormattedId = formattedId;
            AssignedTo = assignedTo;
            StoryDescription = storyDescription;
            IsBlocked = true;
            BlockedReason = blockedReason;
            DisplayColor = displayColor;
        }

        public static KanbanItem ConvertFrom(dynamic queryResult, string kanbanStateKeyWord)
        {
            var s = queryResult;
            var defaultColor = "#00A9E0";
          
            if (s["FormattedID"].ToLower().Contains("de"))
            {
                defaultColor = "#F9A814";
                
            }

            if (s["Blocked"])
            {
                return new KanbanItem(s[kanbanStateKeyWord] ?? "None", s["FormattedID"], s["DisplayColor"] ?? defaultColor, s["Owner"] == null ? "(None)" : s["Owner"]["_refObjectName"], s["Name"], s["BlockedReason"]);
            }
        
            return new KanbanItem(s[kanbanStateKeyWord] ?? "None", s["FormattedID"], s["DisplayColor"] ?? defaultColor, s["Owner"] == null ? "(None)" : s["Owner"]["_refObjectName"], s["Name"]);
        
        }
    }
}
