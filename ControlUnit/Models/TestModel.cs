using System;
using System.Collections.Generic;
using Lextm.SharpSnmpLib;
using SnmpMonitorLib.MIB;

namespace ControlUnit.Models
{
    public class TestModel : EventArgs
    {
        public int TestId { get; set; }
        public string MasterAgentIp { get; set; }
        public string ControlUnitIp { get; set; }
        public string IpSrc { get; set; }
        public string IpDest { get; set; }
        public string Status { get; set; }
        public int Number { get; set; }

        public override string ToString()
        {
            return string.Format("TestId: {0}; MasterAgentIp: {1}, IPsrc: {2}, IPDest: {3}",
                                 TestId, MasterAgentIp, IpSrc, IpDest);
        }

        public IEnumerable<Variable> SerializeToMasterAgent()
        {
            return new List<Variable>
                {
                    new Variable(new ObjectIdentifier(SNMPHelper.MasterAgentIpPath),
                                 new OctetString(ControlUnitIp)),
                    new Variable(
                        new ObjectIdentifier(String.Format(SNMPHelper.TestTreePathFormat, TestId*4)),
                        new OctetString(TestId.ToString())),
                    new Variable(
                        new ObjectIdentifier(String.Format(SNMPHelper.TestTreePathFormat, TestId*4 + 1)),
                        new OctetString(IpDest)),
                    new Variable(
                        new ObjectIdentifier(String.Format(SNMPHelper.TestTreePathFormat, TestId*4 + 2)),
                        new OctetString("")),
                    new Variable(
                        new ObjectIdentifier(String.Format(SNMPHelper.TestTreePathFormat, TestId*4 + 3)),
                        new OctetString(IpSrc))
                };
        }

        public override bool Equals(object obj)
        {
            if (obj is TestModel)
            {
                var val = (TestModel) obj;
                return val.MasterAgentIp == MasterAgentIp && IpSrc == val.IpSrc && IpDest == val.IpDest;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}