using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using ControlUnit.Models;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Security;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using SnmpMonitorLib.MIB;
using log4net;

namespace ControlUnit.Data
{
    public class SNMPProvider
    {
        private readonly SnmpEngine _engine;
        private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IPAddress _ipAddress;

        public SNMPProvider()
        {
            Container = new UnityContainer().LoadConfiguration("snmptrapd");
            var users = Container.Resolve<UserRegistry>();
            users.Add(new OctetString("neither"), DefaultPrivacyProvider.DefaultPair);
            users.Add(new OctetString("authen"),
                      new DefaultPrivacyProvider(new MD5AuthenticationProvider(new OctetString("authentication"))));
            users.Add(new OctetString("privacy"), new DESPrivacyProvider(new OctetString("privacyphrase"),
                                                                         new MD5AuthenticationProvider(
                                                                             new OctetString("authentication"))));

            var trapv1 = Container.Resolve<TrapV1MessageHandler>("TrapV1Handler");
            trapv1.MessageReceived += WatcherTrapV1Received;

            _engine = Container.Resolve<SnmpEngine>();
        }

        private IUnityContainer Container { get; set; }
        public event EventHandler<TestModel> TrapRecived;

        public string UpdateMasterAgentDB(TestModel testModel)
        {
            if (!SNMPHelper.ValidIpHost(testModel.MasterAgentIp))
            {
                return String.Format("Błąd! IP Master Agnet jest nie osiągalne. {0}", testModel);
            }

            IPAddress masterAgentIP = IPAddress.Parse(testModel.MasterAgentIp);
            var receiver = new IPEndPoint(masterAgentIP, 163);

            try
            {
                var vList = new List<Variable>();

                vList.AddRange(testModel.SerializeToMasterAgent());

                SNMPHelper.SNMPSet(vList, receiver);
                return "Master Agent zaktualizowany...";
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
            return String.Format("Błąd! Podczas aktualizacji Master Agent. {0}", testModel);
        }

        public string RefreshTestResults(TestModel testModel)
        {
            if (!SNMPHelper.ValidIpHost(testModel.MasterAgentIp))
            {
                return String.Format("Błąd! IP Master Agnet jest nie osiągalne. {0}", testModel);
            }

            IPAddress masterAgentIP = IPAddress.Parse(testModel.MasterAgentIp);
            var receiver = new IPEndPoint(masterAgentIP, 163);

            var vList = new List<Variable>
                {
                    new Variable(
                        new ObjectIdentifier(String.Format(SNMPHelper.TestTreePathFormat, testModel.TestId*4 + 2)))
                };
            IList<Variable> resultList = SNMPHelper.SNMPGet(vList, receiver);
            if (resultList.Count > 0)
            {
                ISnmpData result = resultList[0].Data;

                if (result.ToString() == bool.FalseString)
                {
                    return "Stan NIEpoprawny";
                }

                if (result.ToString() == bool.TrueString)
                {
                    return "Stan poprawny";
                }
            }
            return "Stan NIEpoprawny";
        }

        private void WatcherTrapV1Received(object sender, TrapV1MessageReceivedEventArgs e)
        {
            logger.Info("Trap recived");

            logger.Debug(e);
            foreach (Variable variable in e.TrapV1Message.Scope.Pdu.Variables)
            {
                logger.DebugFormat("Trap var: {0}", variable);
            }

            var testModel = new TestModel();

            //testModel.TestId = Convert.ToInt32(e.TrapV1Message.Scope.Pdu.Variables[0].Data.ToString());
            //testModel.IpDest = e.TrapV1Message.Scope.Pdu.Variables[1].Data.ToString();
            //testModel.Status = e.TrapV1Message.Scope.Pdu.Variables[2].Data.ToString();
            //testModel.IpSrc = e.TrapV1Message.Scope.Pdu.Variables[3].Data.ToString();

            if (TrapRecived != null)
            {
                TrapRecived.Invoke(this, testModel);
            }
        }

        internal void StartListen(IPAddress ipAddress)
        {
            _ipAddress = ipAddress;

            _engine.Listener.AddBinding(new IPEndPoint(IPAddress.Any, 162));
            _engine.Start();
            logger.Info("SNMP engine start");
        }
    }
}