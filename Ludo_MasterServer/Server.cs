using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Ludo_MasterServer
{
    public class Server
    {
        private TcpListener m_tcpListener;
        private List<Client> m_clientList = new List<Client>();
        private Thread m_threadListening;
        private Thread m_threadReading;

        public SQLiteManager m_database;

        public List<Client> ClientsList { get { return m_clientList; } }


        public Server()
        {
            m_tcpListener = new TcpListener(IPAddress.Any, 8130);
            m_tcpListener.Start();

            m_threadListening = new Thread(Listening);
            m_threadListening.Start();

            m_threadReading = new Thread(Reading);
            m_threadReading.Start();

            m_database = new SQLiteManager();
            Console.WriteLine("STARTING SERVER!!");


            String strHostName = string.Empty;
            // Getting Ip address of local machine...
            // First get the host name of local machine.
            strHostName = Dns.GetHostName();
            Console.WriteLine("Local Machine's Host Name: " + strHostName);
            // Then using host name, get the IP address list..
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            for (int i = 0; i < addr.Length; i++)
            {
                Console.WriteLine("IP Address {0}: {1} ", i, addr[i].ToString());
            }
        }

        public void Listening()
        {
            while (true)
            {
                TcpClient l_tcpClient = m_tcpListener.AcceptTcpClient();
                Client l_client = new Client(l_tcpClient);
                m_clientList.Add(l_client);
            }
        }
        public void Reading()
        {
            int l_currentClients = 0;
            while (true)
            {
                for (int i = m_clientList.Count - 1; i >= 0; i--)
                {
                    m_clientList[i].Reading();
                }

                if (m_clientList.Count != l_currentClients)
                {
                    l_currentClients = m_clientList.Count;
                    Console.WriteLine("currentClients|" + l_currentClients);
                }
                Thread.Sleep(20); //Small delay to evade CPU overheat
            }
        }

        public void Leave(Client client)
        {
            m_clientList.Remove(client);
        }
    }
}
