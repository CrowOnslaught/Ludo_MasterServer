using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ludo_MasterServer
{
    public class GameManager
    {

        public List<Client> m_enqueuedClients = new List<Client>();
        public Dictionary<int, GameServer> m_gameServersPerID = new Dictionary<int, GameServer>();


        public void EnqueueClient(Client c)
        {
            m_enqueuedClients.Add(c);
            if (m_enqueuedClients.Count >= 4)
            {
                int l_roomID = m_gameServersPerID.Count;
                GameServer l_gameServer = new GameServer(l_roomID);
                for (int i = 0; i < 4; i++)
                {
                    Client l_client = m_enqueuedClients[0];
                    l_gameServer.m_clientList.Add(l_client);

                    m_enqueuedClients.RemoveAt(0);
                    Console.Write(i + "|" + l_client.m_name + "|Dequeued");
                }

                m_gameServersPerID.Add(l_roomID, l_gameServer);

                l_gameServer.SetUp();
            }
        }

        public void DequeueClient(Client c)
        {
            if (m_enqueuedClients.Contains(c))
            {
                m_enqueuedClients.Remove(c);
                Console.Write("|" + c.m_name + "|Dequeued");
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

        public void EndMatch(int roomID)
        {
            Console.WriteLine("GAME WITH ID {0} ENDED", roomID);
            m_gameServersPerID.Remove(roomID);
        }
    }

}
