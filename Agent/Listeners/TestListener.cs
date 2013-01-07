using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using SnmpMonitorLib.Models;
using log4net;

namespace Agent.Listeners
{
    public class TestListener
    {
        private const int Port = 3000;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Thread _listenThread;
        private readonly TcpListener _tcpListener;
        private bool _active;


        public TestListener()
        {
            _tcpListener = new TcpListener(IPAddress.Any, Port);
            _listenThread = new Thread(ListenForClients);
        }

        public void StartListen()
        {
            _logger.Info("Start listening for Tests");
            _active = true;
            _listenThread.Start();
        }

        public void StopListen()
        {
            _tcpListener.Server.Close();
            _tcpListener.Stop();

            _active = false;
            _logger.Info("Stop listening for Tests");
        }

        private void ListenForClients()
        {
            _tcpListener.Start();

            while (_active)
            {
                try
                {
                    if (!_tcpListener.Pending())
                    {
                        Thread.Sleep(200);
                        continue;
                    }

                    TcpClient client = _tcpListener.AcceptTcpClient();

                    _logger.Info("New connection");
                    var clientThread = new Thread(HandleClientComm);
                    clientThread.Start(client);
                }
                catch (SocketException ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        private void HandleClientComm(object client)
        {
            var tcpClient = (TcpClient) client;
            NetworkStream clientStream = tcpClient.GetStream();

            var data = new byte[4096];
            int bytesRead = 0;

            while ((bytesRead = clientStream.Read(data, 0, data.Length)) != 0)
            {
                string msg = Encoding.ASCII.GetString(data, 0, bytesRead);
                _logger.Info(string.Format("Received: {0}", msg));

                TestValues testVal = TestValues.Deserialize(msg);

                byte[] response = Encoding.ASCII.GetBytes(Test(testVal.A, testVal.B));

                clientStream.Write(response, 0, response.Length);
                _logger.Info(string.Format("Sent: {0}", response));
            }


            tcpClient.Close();
        }

        private static string Test(int a, int b)
        {
            _logger.Debug(String.Format("Test {0} and {1}", a, b));
            Debug.Assert(b != 0);

            var sb = new StringBuilder();

            sb.Append(a + b);
            sb.Append(a - b);
            sb.Append(a*b);
            sb.Append(a/b);

            string md5Hash = Md5Helper.GetMd5Hash(sb.ToString());
            _logger.Debug("Test Hash: " + md5Hash);

            return md5Hash;
        }

        public static bool StartTest(TestModel testModel)
        {
            try
            {
                var client = new TcpClient(testModel.IpDest.Data.ToString(), Port);

                var testValues = new TestValues
                    {
                        A = new Random((int) DateTime.Now.Ticks).Next(1, 100),
                        B = new Random((int) DateTime.Now.AddDays(1).Ticks).Next(1, 100)
                    };
                byte[] data = Encoding.ASCII.GetBytes(testValues.Serialize());

                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);

                _logger.Info(string.Format("Sent: {0}", testValues.Serialize()));

                data = new byte[4096];
                String recvHash = String.Empty;

                Int32 bytes = stream.Read(data, 0, data.Length);
                recvHash = Encoding.ASCII.GetString(data, 0, bytes);
                _logger.Info(string.Format("Received: {0}", recvHash));

                stream.Close();
                client.Close();

                string inHash = Test(testValues.A, testValues.B);
                return inHash == recvHash;
            }
            catch (ArgumentNullException e)
            {
                _logger.Error("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                _logger.Error("SocketException: {0}", e);
            }
            return false;
        }
    }
}