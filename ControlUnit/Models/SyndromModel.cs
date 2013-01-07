using System;

namespace ControlUnit.Models
{
    public class SyndromModel
    {
        public string MasterAgentIp { get; set; }
        public string Hosts { get; set; }
        public string Syndrom { get; set; }

        #region Overrides of Object

        public override string ToString()
        {
            return String.Format("SyndromModel; Master Agent {0}; Hosts: {1}; Syndrom: {2}", MasterAgentIp, Hosts,
                                 Syndrom);
        }

        #endregion
    }
}