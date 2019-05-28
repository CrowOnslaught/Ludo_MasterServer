using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static Ludo_MasterServer.Enums;
using static Ludo_MasterServer.GameServer;

namespace Ludo_MasterServer
{
    public class Client
    {
        private TcpClient m_tcpClient;
        private NetworkStream m_clientStream;
        private DateTime last_message = DateTime.Now;

        public string m_name = "clientName";
        public int m_id = 888;

        public Client(TcpClient tcpClient)
        {
            if (m_tcpClient == null)
            {
                m_tcpClient = tcpClient;
                m_clientStream = m_tcpClient.GetStream(); //Canal de entrada de datos
            }
            else
                Close();
        }

        public void Reading() //Lectura de paquetes
        {
            if (m_clientStream.DataAvailable)
            {
                byte[] l_size = new byte[4];
                m_clientStream.Read(l_size, 0, 4);

                byte[] l_messageRecived = new byte[BitConverter.ToUInt32(l_size, 0)];
                m_clientStream.Read(l_messageRecived, 0, l_messageRecived.Length);

                NetworkMessage l_message = new NetworkMessage(this, l_messageRecived);
                Execute(l_message);
                last_message = DateTime.Now;
            }

            if ((DateTime.Now - last_message).TotalSeconds > 5)
                Close();
        }

        public void Send(NetworkMessage message)
        {
            byte[] l_messageSize;
            byte[] l_messageResult = new byte[message.m_raw.Length + 4];

            try
            {
                l_messageSize = BitConverter.GetBytes(message.m_raw.Length);
                Array.Copy(l_messageSize, 0, l_messageResult, 0, 4);
                Array.Copy(message.m_raw, 0, l_messageResult, 4, message.m_raw.Length);

                m_clientStream.Write(l_messageResult,0, l_messageResult.Length);
                Console.WriteLine("{0} Mensage sent to {1} with ID {2}", message.m_type, this.m_name, this.m_id);
            }
            catch
            {
                Console.WriteLine("Error 001: Sending Message Error");
            }
        }

        public void Execute(NetworkMessage message)
        {
            switch (message.m_type)
            {
                case MessageType.welcome:
                    Console.Write("Client Connected|");
                    break;
                case MessageType.logIn:
                    string l_name = message.ReadString();
                    string l_pass = message.ReadString();

                    if (TryToLogIn(l_name, l_pass))
                    {
                        this.m_name = l_name;
                        this.m_id = Program.m_server.m_database.GetClientID(m_name);
                        Send(MessageConstructor.LogInSucess(m_id));

                        Console.WriteLine("User loged in Succes| Name: {0}|Pass: {1}", l_name, l_pass);

                        Send(MessageConstructor.CurrentGames(Program.m_gameManager.m_gameServersPerID, this.m_id));
                    }
                    else
                    {
                        Send(MessageConstructor.LogInFailed());
                        Console.WriteLine("User LogIn Failed| Name: {0}|Pass: {1}", l_name, l_pass);
                    }
                   
                    break;
                case MessageType.ping:
                    break;
                case MessageType.joinNewGame:
                    Program.m_gameManager.EnqueueClient(this);
                    Console.WriteLine("Client Enqueued! Current Clients in queue: {0}", Program.m_gameManager.m_enqueuedClients.Count);
                    break;
                case MessageType.quitQueue:
                    Program.m_gameManager.DequeueClient(this);
                    break;
                case MessageType.rollDice:
                    Program.m_gameManager.OnClientRollDice(this, message.ReadInt());
                    break;
                case MessageType.choosePiece:
                    Program.m_gameManager.OnClientSelectPiece(this, message.ReadInt(), message.ReadInt());
                    break;
                case MessageType.rejoinGame:
                    GameServer l_gs = Program.m_gameManager.m_gameServersPerID[message.ReadInt()];
                    PlayerInfo l_pi = l_gs.m_playersPerID[this.m_id];
                    l_pi.m_client = this;

                    Client l_client = l_gs.m_clientList.FirstOrDefault(x => x.m_id == this.m_id);
                    if (l_client != null)
                        l_gs.m_clientList.Remove(l_client);

                    l_gs.m_clientList.Add(this);

                    Console.WriteLine();
                    for (int i = 0; i < l_gs.m_clientList.Count; i++)
                        Console.Write(l_gs.m_clientList[i].m_name + "|");

                    this.Send(MessageConstructor.StartNewMatch(l_gs.m_roomID, l_gs.PlayerInfoList()));
                    if (l_pi.m_currentTurn)
                    {
                        Thread.Sleep(800);
                        this.Send(MessageConstructor.ChangeTurn(l_gs.PlayerInfoList().Find(x => x.m_currentTurn).m_color, l_pi.m_currentTurn));
                    }

                    break;
                case MessageType.refreshCurrentGames:
                    Send(MessageConstructor.CurrentGames(Program.m_gameManager.m_gameServersPerID, this.m_id));
                    break;
                case MessageType.ranking:
                    Send(MessageConstructor.Ranking(5));
                    break;
                default:
                    Console.WriteLine("Error 002: Unknown type of Message");
                    break;
            }
        }

        private bool TryToLogIn(string name, string password)
        {
            bool success = false;
            if (name != null && password != null)
            {
                if (Program.m_server.m_database.ExistsName(name)) //That name Exists in Database
                {
                    if (Program.m_server.m_database.GetClientPassword(name) == password) //Sent password coincide with database's client pass
                    {
                        int l_id = Program.m_server.m_database.GetClientID(name);
                        if (Program.m_server.ClientsList.FirstOrDefault(x => x.m_id == l_id) == null) //There is no client with that ID already connected
                            success = true;
                        else
                            success = false;
                    }
                    else
                        success = false;
                }
                else //Name doesn't exists in database
                {
                    Program.m_server.m_database.AddClient(name, password);
                    success = true;
                }

            }

            return success;
        }

        private void Close()
        {
            Program.m_gameManager.DequeueClient(this);
            Program.m_server.Leave(this);
            m_tcpClient.Close();
        }
    }
}
