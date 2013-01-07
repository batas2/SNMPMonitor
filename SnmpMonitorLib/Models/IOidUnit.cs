using System;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;

namespace SnmpMonitorLib.Models
{
    public interface IOidUnit : ISnmpObject
    {
        int Index { get; }
        //TestTypeEnum Type { get; }
        ISnmpData Data { get; set; }
        event EventHandler ValueChanged;
        void AssignData(OctetString val);
    }
}