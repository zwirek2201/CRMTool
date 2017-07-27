using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Licencjat_new_server
{
    public class Program
    {
        private TcpListener _server;
        private bool _isRunning;
        private int _port = 2001;

        private List<Client> _connectedClients = new List<Client>();

        public X509Certificate2 Certificate = new X509Certificate2("CRMToolServer.pfx", "");


        static void Main(string[] args)
        {
            new Program();
            Console.ReadLine();
        }

        public Program()
        {
            //byte[] data = Cryptography.EncodeString("marcin");
            //byte[] salt = new byte[100];
            //byte[] hashData = new byte[data.Length - salt.Length];

            //Buffer.BlockCopy(data, 0, salt, 0, 100);
            //Buffer.BlockCopy(data, 100, hashData, 0, data.Length - salt.Length);


            //string encodedData = Cryptography.DecodeBytes(hashData, salt);
            ErrorHelper.CreateErrorLogFile();
            try
                {
                Logger.Log("Server setup started");
                _server = new TcpListener(new IPEndPoint(IPAddress.Any, _port));
                _server.Start();
                Logger.Log("Server setup succedeed");
                _isRunning = true;
                Listen();
            }
            catch (Exception ex)
            {
                Logger.Log("ERROR: " + ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void Listen()
        {
            while (_isRunning)
            {
                TcpClient tcpClient = _server.AcceptTcpClient();
                Client client = new Client(tcpClient, this);
                client.UserLoggedIn += Client_UserLoggedIn;
                client.UserLoggedOut += Client_UserLoggedOut;
            }
        }

        public Client GetClientById(string userId)
        {
            Client client = _connectedClients.Find(obj => obj.UserInfo.UserId == userId);
            return client;
        }

        public void DestroyClient(Client client)
        {
            if (_connectedClients.Contains(client))
            {
                _connectedClients.Remove(client);
                client = null;
            }
        }

        private void Client_UserLoggedOut(object sender)
        {
            Client client = (Client)sender;
            Logger.Log("User disconnected");
            _connectedClients.Remove(client);
        }

        private void Client_UserLoggedIn(object sender)
        {
            Client client = (Client)sender;
            _connectedClients.Add(client);
        }
    }
}
