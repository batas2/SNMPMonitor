using System;

namespace ControlUnit.Models
{
    public class SectionDeletedEventArgs : EventArgs
    {
        public SectionDeletedEventArgs(string masterAgetIp)
        {
            MasterAgentIp = masterAgetIp;
        }

        public string MasterAgentIp { get; set; }
    }
}