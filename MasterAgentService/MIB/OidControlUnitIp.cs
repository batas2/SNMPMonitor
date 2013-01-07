using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using SnmpMonitorLib.MIB;

namespace MasterAgent.MIB
{
    internal sealed class OidControlUnitIp : ScalarObject
    {
        private OctetString _data;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public OidControlUnitIp()
            : base(SNMPHelper.MasterAgentIpPath)
        {
            _data = new OctetString("");
        }

        public override ISnmpData Data
        {
            get { return _data; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (value.TypeCode != SnmpType.OctetString)
                {
                    throw new ArgumentException("data");
                }

                _data = (OctetString) value;

                if (ControlUnitIpAdded != null && SNMPHelper.ValidIpHost(_data.ToString()))
                {
                    ControlUnitIpAdded.Invoke(this, null);
                }
            }
        }

        public IPAddress Ip
        {
            get { return IPAddress.Parse(_data.ToString()); }
        }

        public event EventHandler ControlUnitIpAdded;
    }
}