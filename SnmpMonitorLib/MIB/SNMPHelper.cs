using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using log4net;

namespace SnmpMonitorLib.MIB
{
    public static class SNMPHelper
    {
        public const string StartTestPath = "1.3.6.1.4.1";
        public const string MasterAgentIpPath = "1.3.6.1.4.2";
        public const string TestTreePath = "1.3.6.1.4.3";
        public const string TestTreePathFormat = "1.3.6.1.4.3.{0}";
        public const string TratTreePath = "1.3.6.1.4.4";
        public const VersionCode Version = VersionCode.V1;
        public const int Timeout = 1000;
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly OctetString Community = new OctetString("public");

        public static int GetObjectIndex(ObjectIdentifier id)
        {
            string[] dots = id.ToString().Split('.');
            return Convert.ToInt32(dots.Last());
        }

        public static bool ValidIpHost(String IpVal)
        {
            IPAddress ip;
            bool parsed = IPAddress.TryParse(IpVal, out ip);
            if (!parsed)
            {
                foreach (IPAddress address in
                    Dns.GetHostAddresses(IpVal).Where(
                        address => address.AddressFamily == AddressFamily.InterNetwork))
                {
                    ip = address;
                    break;
                }
            }
            return ip != null;
        }

        public static void SNMPSet(List<Variable> vList, IPEndPoint receiver)
        {
            foreach (
                Variable variable in
                    Messenger.Set(Version, receiver, Community, vList, Timeout))
            {
                logger.Info(variable);
            }
        }

        public static IList<Variable> SNMPGet(List<Variable> vList, IPEndPoint receiver)
        {
            return Messenger.Get(Version, receiver, Community, vList, Timeout);
        }
    }
}