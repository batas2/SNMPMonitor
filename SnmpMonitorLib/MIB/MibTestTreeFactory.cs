using System;
using System.Collections.Generic;
using System.Reflection;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using SnmpMonitorLib.Models;
using log4net;

namespace SnmpMonitorLib.MIB
{
    public class MibTestTreeFactory : TableObject
    {
        protected readonly IList<ScalarObject> _elements;
        protected readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MibTestTreeFactory(IList<ScalarObject> elements)
        {
            _elements = elements;
        }

        public MibTestTreeFactory()
        {
            _elements = new List<ScalarObject>();
        }

        protected override IEnumerable<ScalarObject> Objects
        {
            get { return _elements; }
        }

        public event EventHandler TestValueChanged;

        public override ScalarObject MatchGet(ObjectIdentifier id)
        {
            ScalarObject result = base.MatchGet(id);
            if (result == null && id.ToString().Contains(SNMPHelper.TestTreePath))
            {
                logger.InfoFormat("Create OID id: {0}", id);
                IOidUnit oidUnit = CreateOidUnit(id);
                oidUnit.ValueChanged +=
                    (sender, args) => { if (TestValueChanged != null) TestValueChanged.Invoke(sender, args); };

                result = (ScalarObject) oidUnit;
                _elements.Add(result);
            }
            return result;
        }

        protected IOidUnit CreateOidUnit(ObjectIdentifier id)
        {
            var oidUnit = new OidUnit(id.ToString(), new OctetString("NULL"));

            return oidUnit;
        }
    }
}