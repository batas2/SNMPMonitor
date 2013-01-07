using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using SnmpMonitorLib.MIB;
using SnmpMonitorLib.Models;
using log4net;

namespace MasterAgent.MIB
{
    public class TestController
    {
        private readonly MibController _mibController;
        private readonly Thread _thread;
        private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public TestController(MibController mibController)
        {
            _thread = new Thread(ExecuteTests);
            _thread.IsBackground = true;
            _mibController = mibController;
        }

        public bool isAlive
        {
            get { return _thread.IsAlive; }
        }

        private void ExecuteTests()
        {
            while (isAlive)
            {
                Thread.Sleep(new Random((int) DateTime.Now.Ticks).Next(5000, 15000));

                _mibController.StartTests();
                Thread.Sleep(2000);
                _mibController.UpdateResults();
            }
        }

        internal void StartController()
        {
            if (!isAlive)
                _thread.Start();
        }

        internal void InformControlUnit(TestModel testModel)
        {
            var vList = new List<Variable>
                {
                    new Variable(
                        new ObjectIdentifier(testModel.Id.Path),
                        new OctetString(testModel.Id.ToString())),
                    new Variable(
                        new ObjectIdentifier(testModel.IpDest.Path),
                        new OctetString(testModel.IpDest.ToString())),
                    new Variable(
                        new ObjectIdentifier(testModel.TestResult.Path),
                        new OctetString(testModel.TestResult.ToString())),
                    new Variable(
                        new ObjectIdentifier(testModel.IpSrc.Path),
                        new OctetString(testModel.IpSrc.ToString()))
                };
            foreach (Variable variable in vList)
            {
                logger.InfoFormat("Trap var: {0}", variable);
            }
            logger.Info("Trap sended");
            Messenger.SendTrapV1(new IPEndPoint(_mibController.ControlUnitIP, 162), IPAddress.Loopback,
                                 SNMPHelper.Community, new ObjectIdentifier(SNMPHelper.TratTreePath),
                                 GenericCode.ColdStart,
                                 0, 0, vList);
        }
    }
}