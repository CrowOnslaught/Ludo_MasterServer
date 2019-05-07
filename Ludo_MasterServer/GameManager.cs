using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ludo_MasterServer
{
    public class GameManager
    {

        public Queue<Client> m_enqueuedClients = new Queue<Client>();
        public Dictionary<int, GameServer> m_gameServersPerID = new Dictionary<int, GameServer>();


        public void EnqueueClient(Client c)
        {
            m_enqueuedClients.Enqueue(c);
            if (m_enqueuedClients.Count >= 4)
            {
                int l_roomID = m_gameServersPerID.Count;
                GameServer l_gameServer = new GameServer(l_roomID);
                for (int i = 0; i < 4; i++)
                {
                    Client l_client = m_enqueuedClients.Dequeue();
                    Console.Write(i + "|" + l_client.m_name + "|Dequeued");
                    l_gameServer.m_clientList.Add(l_client);
                }

                m_gameServersPerID.Add(l_roomID, l_gameServer);

                l_gameServer.SetUp();
            }
        }

        public void OnClientRollDice(Client c, int roomID)
        {
            m_gameServersPerID[roomID].OnRollDice(c);
        }
        public void OnClientSelectPiece(Client c, int roomID, int tileID)
        {
            m_gameServersPerID[roomID].OnSelectPiece(c, tileID);
        }
    }

}
