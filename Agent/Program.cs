using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Agent.Listeners;
using Agent.MIB;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Objects;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Security;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using SnmpMonitorLib.Listeners;
using SnmpMonitorLib.MIB;
using SnmpMonitorLib.Models;
using log4net;
using log4net.Config;
using MibController = Agent.MIB.MibController;

[assembly: XmlConfigurator(Watch = true)]

namespace Agent
{
    internal class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static SnmpListener _snmpListenerlistener;
        private static readonly TestListener _testListener = new TestListener();
        private static readonly MibController _mibController = new MibController();
        private static MibTestTreeFactory _mibTestTreeFactory;

        internal static IUnityContainer Container { get; private set; }

        private static void Main(string[] args)
        {
            Container = new UnityContainer();
            Container.LoadConfiguration("agent");

            var oidStartTest = new OidStartTest();
            oidStartTest.StartTest += mibTestTree_StartTest;

            var oidElements = new List<ScalarObject>();
            oidElements.Add(oidStartTest);

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


            _snmpListenerlistener = new SnmpListener(Container, IPAddress.Any, 161);
            _snmpListenerlistener.StartListen();
            _testListener.StartListen();

            Console.ReadLine();
            _testListener.StopListen();
            _snmpListenerlistener.StopListen();
        }

        private static void TestValueChanged(object sender, EventArgs e)
        {
            var s = (OidUnit) sender;
            _mibController.UpdateOid(s);

            logger.InfoFormat("Index: {0}, Type: {1}; Value: {2}", s.Index, _mibController.getTestTypeFromIndex(s.Index),
                              s.Data);
        }

        private static void mibTestTree_StartTest(object sender, EventArgs e)
        {
            _mibController.StartTests();
        }
    }
}