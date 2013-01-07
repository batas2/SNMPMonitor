using System;
using System.Reflection;
using Agent.Listeners;
using Lextm.SharpSnmpLib;
using SnmpMonitorLib.MIB;
using SnmpMonitorLib.Models;
using log4net;

namespace Agent.MIB
{
    public class MibController : SnmpMonitorLib.MIB.MibController
    {
        private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void StartTests()
        {
            _logger.Info("Start Tests");
            foreach (var testModel in TestModels)
            {
                try
                {
                    if (!ValidTestModel(testModel.Value))
                    {
                        _logger.ErrorFormat("TestModel not valid for test; ID: {0}; IpDest: {1}", (testModel.Key*3),
                                            testModel.Value.IpDest);
                    }
                    else
                    {
                        bool result = TestListener.StartTest(testModel.Value);
                        _logger.InfoFormat("Test Result Index: {0} Result: {1}", testModel.Key, result.ToString());

                        testModel.Value.TestResult.AssignData(new OctetString(result.ToString()));
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Error while test" + e + testModel);
                    testModel.Value.TestResult.AssignData(new OctetString(bool.FalseString));
                }
            }
        }

        protected override bool ValidTestModel(TestModel testModel)
        {
            try
            {
                if (testModel.Id != null && testModel.IpDest != null && testModel.TestResult != null)
                {
                    if (!SNMPHelper.ValidIpHost(testModel.IpDest.ToString()))
                    {
                        logger.Error("Test IpDest not avaible; TestModel" + testModel);
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error while validating Model" + e + testModel);
            }
            return false;
        }
    }
}