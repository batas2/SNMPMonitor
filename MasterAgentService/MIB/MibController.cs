using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Lextm.SharpSnmpLib;
using SnmpMonitorLib.MIB;
using SnmpMonitorLib.Models;
using log4net;

namespace MasterAgent.MIB
{
    public class MibController : SnmpMonitorLib.MIB.MibController
    {
        private readonly TestController _testController;
        private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MibController()
        {
            _testController = new TestController(this);
            ValidTestModelAdded += OnValidTestModelAdded;
        }

        public IPAddress ControlUnitIP { get; set; }

        private void OnValidTestModelAdded(object sender, TestModel testModel)
        {
            IPAddress testSrcIp = IPAddress.Parse(testModel.IpSrc.ToString());
            var receiver = new IPEndPoint(testSrcIp, 161);
            try
            {
                logger.Debug("Valid Model Added: " + testModel);
                List<Variable> vList = testModel.SerializeToAgent();
                SNMPHelper.SNMPSet(vList, receiver);

                if (!_testController.isAlive)
                    _testController.StartController();
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        protected override int getTestIdFromIndex(int index)
        {
            return index / 4;
        }

        public override TestTypeEnum getTestTypeFromIndex(int index)
        {
            return (TestTypeEnum)(index % 4);
        }

        public override void StartTests()
        {
            logger.Info("Start tests");
            foreach (var testModel in TestModels)
            {
                if (!ValidTestModel(testModel.Value))
                {
                    _logger.ErrorFormat("TestModel not valid for test; {0}", testModel);
                }
                else
                {
                    IPAddress testSrcIp = IPAddress.Parse(testModel.Value.IpSrc.ToString());
                    var receiver = new IPEndPoint(testSrcIp, 161);

                    try
                    {
                        var vList = new List<Variable>();
                        vList.AddRange(testModel.Value.SerializeToAgent());
                        vList.Add(new Variable(new ObjectIdentifier(SNMPHelper.StartTestPath), new Integer32(1)));

                        SNMPHelper.SNMPSet(vList, receiver);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Error while starts testing" + e);
                    }
                }
            }
        }

        public void UpdateResults()
        {
            logger.Info("Start Update results");
            foreach (var testModel in TestModels)
            {
                if (!ValidTestModel(testModel.Value))
                {
                    _logger.ErrorFormat("TestModel not valid for result update; {0}", testModel);
                }
                else
                {
                    IPAddress testSrcIp = IPAddress.Parse(testModel.Value.IpSrc.ToString());
                    var receiver = new IPEndPoint(testSrcIp, 161);
                    var testState = (OidUnit)testModel.Value.TestResult.Clone();

                    try
                    {
                        int testId = 0;
                        if (!int.TryParse(testModel.Value.Id.Data.ToString(), out testId))
                        {
                            logger.Error("Error, cannot convert id to int; Model" + ToString());
                        }

                        var vList = new List<Variable>
                            {
                                new Variable(
                                    new ObjectIdentifier(String.Format(SNMPHelper.TestTreePathFormat, testId*3 + 2)))
                            };
                        IList<Variable> resultList = SNMPHelper.SNMPGet(vList, receiver);
                        if (resultList.Count > 0)
                        {
                            ISnmpData result = resultList[0].Data;
                            testModel.Value.TestResult.AssignData(result);
                        }
                        else
                        {
                            logger.Warn("SNMP get return 0 result");
                            testModel.Value.TestResult.AssignData(bool.FalseString);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("Error while updating Result" + e);
                        testModel.Value.TestResult.AssignData(bool.FalseString);
                    }
                    if (testState.Data.ToString() != testModel.Value.TestResult.Data.ToString())
                        _testController.InformControlUnit(testModel.Value);
                }
            }
        }

        protected override bool ValidTestModel(TestModel testModel)
        {
            try
            {
                if (testModel.Id != null && testModel.IpDest != null && testModel.TestResult != null &&
                    testModel.IpSrc != null)
                {
                    if (!SNMPHelper.ValidIpHost(testModel.IpSrc.ToString()))
                    {
                        logger.Error("Test IpSrc not avaible; TestModel" + testModel);
                        return false;
                    }
                    return true;
                }
                logger.Warn("TestModel not valid" + testModel);
            }
            catch (Exception e)
            {
                _logger.Error("Error while validating Model" + e + testModel);
            }
            return false;
        }
    }
}