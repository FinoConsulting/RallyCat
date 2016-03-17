using System;


namespace RallyCat.WebApi.ViewModels
{
    public class SlackErrorMessage
    {
        public String SlackErrorMessage()
        {
            return ":warning: Something went wrong. Type /rallycat help for available commands.";
        }

        public String ProjectNotFound(string projectName)
        {
            return ":no_entry: Project: " + projectName + " not found. Please try again.";
        }

        public String KanbanNotFound(string projectName)
        {
            return ":no_entry: Kanban for " + projectName + " not found. Please try again.";
        }

        public String UserStoryDefectNotFound(string formattedId)
        {
            return ":no_entry: User Story/Defect: " + formattedId + " not found.";
        }

        public String ProjectNotFound(string projectName, string color)
        {
            var errorColor = color ?? "#ff0033";
            return ":no_entry: Project: " + projectName + " not found. Please try again.\", \"color\": \"" + errorColor;
        }

        public String UserStoryDefectNotFound(string formattedId, string color)
        {
            var errorColor = color ?? "#ff0033";
            return ":no_entry: User Story/Defect: " + formattedId + " not found.\", \"color\": \"" + errorColor;
        }
    }

}
