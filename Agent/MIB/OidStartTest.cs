using System;
using System.Diagnostics.CodeAnalysis;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using SnmpMonitorLib.MIB;

namespace Agent.MIB
{
    internal sealed class OidStartTest : ScalarObject
    {
        private ISnmpData _data;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public OidStartTest()
            : base(SNMPHelper.StartTestPath)
        {
            _data = new Integer32(0);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public OidStartTest(string path, int value)
            : base(path)
        {
            _data = new Integer32(value);
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

                if (value.TypeCode != SnmpType.Integer32)
                {
                    throw new ArgumentException("data");
                }

                _data = value;

                if (StartTest != null)
                {
                    StartTest.Invoke(this, null);
                }

                _data = new Integer32(0);
            }
        }

        public event EventHandler StartTest;
    }
}