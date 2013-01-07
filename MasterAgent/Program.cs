using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Objects;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Security;
using MasterAgent.MIB;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using SnmpMonitorLib.Listeners;
using SnmpMonitorLib.MIB;
using SnmpMonitorLib.Models;
using log4net;
using log4net.Config;
using MibController = MasterAgent.MIB.MibController;

[assembly: XmlConfigurator(Watch = true)]

namespace MasterAgent
{
    internal class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static SnmpListener _snmpListenerlistener;
        private static MibTestTreeFactory _mibTestTreeFactory;

        private static readonly OidControlUnitIp _oidControlUnitIp = new OidControlUnitIp();
        private static readonly MibController MibController = new MibController();

        private static IUnityContainer Container { get; set; }

        private static void Main(string[] args)
        {
            Container = new UnityContainer();
            Container.LoadConfiguration("agent");

            _oidControlUnitIp.ControlUnitIpAdded += oidControlUnitIp_ControlUnitIpAdded;

            var oidElements = new List<ScalarObject>();
            oidElements.Add(_oidControlUnitIp);

            _snmpListenerlistener = new SnmpListener(Container, IPAddress.Any, 163);
            _mibTestTreeFactory = new MibTestTreeFactory(oidElements);

            _mibTestTreeFactory.TestValueChanged += TestValueChanged;

            var store = Container.Resolve<ObjectStore>();
            store.Add(new SysDescr());
            store.Add(new SysObjectId());
            store.Add(new SysUpTime());
            store.Add(new SysContact());
            store.Add(new SysName());
            store.Add(new SysLocation());
            store.Add(new SysServices());
            store.Add(new SysORLastChange());
            store.Add(new SysORTable());
            store.Add(_mibTestTreeFactory);

            var users = Container.Resolve<UserRegistry>();
            users.Add(new OctetString("neither"), DefaultPrivacyProvider.DefaultPair);
            users.Add(new OctetString("authen"),
                      new DefaultPrivacyProvider(new MD5AuthenticationProvider(new OctetString("authentication"))));
            users.Add(new OctetString("privacy"), new DESPrivacyProvider(new OctetString("privacyphrase"),
                                                                         new MD5AuthenticationProvider(
                                                                             new OctetString("authentication"))));


            _snmpListenerlistener.StartListen();

            Console.ReadLine();
            _snmpListenerlistener.StopListen();
        }

        private static void oidControlUnitIp_ControlUnitIpAdded(object sender, EventArgs e)
        {
            MibController.ControlUnitIP = _oidControlUnitIp.Ip;
        }

        private static void TestValueChanged(object sender, EventArgs e)
        {
            var s = (OidUnit) sender;
            MibController.UpdateOid(s);

            logger.InfoFormat("Index: {0}, Type: {1}; Value: {2}", s.Index, MibController.getTestTypeFromIndex(s.Index),
                              s.Data);
        }
    }
}