using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SnmpMonitorLib.Models;
using log4net;

namespace SnmpMonitorLib.MIB
{
    public abstract class MibController
    {
        protected readonly Dictionary<int, TestModel> TestModels;
        protected readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected MibController()
        {
            TestModels = new Dictionary<int, TestModel>();
        }

        public Dictionary<int, string> TestResults
        {
            get
            {
                return TestModels.ToDictionary(testModel => testModel.Key,
                                               testModel => testModel.Value.TestResult.Data.ToString());
            }
        }

        protected event EventHandler<TestModel> ValidTestModelAdded;

        protected virtual int getTestIdFromIndex(int index)
        {
            return index/3;
        }

        public virtual TestTypeEnum getTestTypeFromIndex(int index)
        {
            return (TestTypeEnum) (index%3);
        }

        private TestModel findTestModelByIndex(int index)
        {
            int testId = getTestIdFromIndex(index);
            if (!TestModels.Keys.Contains(testId))
            {
                TestModels.Add(testId, new TestModel());
            }
            return TestModels[testId];
        }

        public void UpdateOid(OidUnit value)
        {
            _logger.InfoFormat("Controller - Update Oid - index: {0}; data: {1}", value.Index, value.Data.ToString());
            TestModel testModel = findTestModelByIndex(value.Index);
            switch (getTestTypeFromIndex(value.Index))
            {
                case TestTypeEnum.Id:
                    testModel.Id = value;
                    break;
                case TestTypeEnum.IpDest:
                    testModel.IpDest = value;
                    break;
                case TestTypeEnum.Result:
                    testModel.TestResult = value;
                    break;
                case TestTypeEnum.IpSrc:
                    testModel.IpSrc = value;
                    break;
            }

            if (ValidTestModelAdded != null && ValidTestModel(testModel))
            {
                ValidTestModelAdded.Invoke(this, testModel);
            }
        }

        public abstract void StartTests();
        protected abstract bool ValidTestModel(TestModel testModel);
    }
}