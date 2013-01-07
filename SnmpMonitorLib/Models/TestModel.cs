using System;
using System.Collections.Generic;
using System.Reflection;
using Lextm.SharpSnmpLib;
using SnmpMonitorLib.MIB;
using log4net;

namespace SnmpMonitorLib.Models
{
    public class TestModel : EventArgs, ICloneable
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public OidUnit Id { get; set; }
        public OidUnit IpDest { get; set; }
        public OidUnit TestResult { get; set; }
        public OidUnit IpSrc { get; set; }

        #region ICloneable Members

        public object Clone()
        {
            return new TestModel {Id = Id, IpDest = IpDest, TestResult = TestResult, IpSrc = IpSrc};
        }

        #endregion

        public override string ToString()
        {
            return String.Format("ID: {0}, IpSrc: {1}, IpDest: {2}, Result: {3}", Id, IpSrc, IpDest, TestResult);
        }

        public List<Variable> SerializeToAgent()
        {
            int agentTestID = 0;
            if (!int.TryParse(Id.Data.ToString(), out agentTestID))
            {
                _logger.Error("Error, cannot convert id to int; Model" + ToString());
            }

            return new List<Variable>
                {
                    new Variable(
                        new ObjectIdentifier(String.Format(SNMPHelper.TestTreePathFormat, agentTestID*3)),
                        new OctetString(Id.ToString())),
                    new Variable(
                        new ObjectIdentifier(String.Format(SNMPHelper.TestTreePathFormat, agentTestID*3 + 1)),
                        new OctetString(IpDest.ToString())),
                    new Variable(
                        new ObjectIdentifier(String.Format(SNMPHelper.TestTreePathFormat, agentTestID*3 + 2)),
                        new OctetString(TestResult.ToString()))
                };
        }
    }
}