using System;
using System.Collections.Generic;


namespace RallyCat.Core.Rally
{
    public class RallySlackMapping
    {
        public Int32        Id               { get; set; }
        public String       TeamName         { get; set; }
        public Int64        ProjectId        { get; set; }
        public Int64        WorkspaceId      { get; set; }
        public String       KanbanSortColumn { get; set; }
        public Boolean      EnableKanban     { get; set; }
        public List<String> Channels         { get; set; }
    }
}