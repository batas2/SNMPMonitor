using System.Net;
using System.Reflection;
using Lextm.SharpSnmpLib.Pipeline;
using Microsoft.Practices.Unity;
using log4net;

namespace SnmpMonitorLib.Listeners
{
    public class SnmpListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly SnmpEngine _engine;
        private readonly IPAddress _ipAddress;
        private readonly int _port;

        public SnmpListener(IUnityContainer container, IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _engine = container.Resolve<SnmpEngine>();
            _engine.ExceptionRaised += (sender, e) => Logger.Error(e);
        }

        public void StartListen()
        {
            if (!_engine.Active)
            {
                _engine.Listener.ClearBindings();

                _engine.Listener.AddBinding(new IPEndPoint(_ipAddress, _port));
                _engine.Start();
                Logger.Info("Star Listening SNMP");
            }
        }

        public void StopListen()
        {
            if (_engine.Active)
            {
                _engine.Stop();
                Logger.Info("Stop Listening SNMP");
            }
        }
    }
}