using System;
using System.Diagnostics.CodeAnalysis;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using SnmpMonitorLib.MIB;

namespace SnmpMonitorLib.Models
{
    public class OidUnit : ScalarObject, IOidUnit, ICloneable
    {
        private readonly int _index;
        private readonly string _path;
        private OctetString _data;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public OidUnit(string path, OctetString data)
            : base(path)
        {
            _path = path;
            _data = data;
            _index = SNMPHelper.GetObjectIndex(new ObjectIdentifier(path));
        }

        public string Path
        {
            get { return _path; }
        }

        #region ICloneable Members

        public object Clone()
        {
            return new OidUnit(Path, _data);
        }

        #endregion

        #region IOidUnit Members

        public int Index
        {
            get { return _index; }
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

                if (value.TypeCode != SnmpType.OctetString)
                {
                    throw new ArgumentException("data");
                }

                _data = (OctetString) value;

                if (ValueChanged != null)
                    ValueChanged.Invoke(this, null);
            }
        }

        public event EventHandler ValueChanged;

        public void AssignData(OctetString val)
        {
            _data = val;
        }

        #endregion

        public void AssignData(string val)
        {
            _data = new OctetString(val);
        }

        public void AssignData(ISnmpData val)
        {
            _data = (OctetString) val;
        }

        public override string ToString()
        {
            return _data == null ? "null" : _data.ToString();
        }
    }
}