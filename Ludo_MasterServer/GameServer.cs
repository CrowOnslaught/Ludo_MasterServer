using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Ludo_MasterServer.Enums;


namespace Ludo_MasterServer
{
    public class GameServer
    {
        public List<Client> m_clientList = new List<Client>();
        public Dictionary<int, PlayerInfo> m_playersPerID = new Dictionary<int, PlayerInfo>();
        public int m_roomID;

        public struct PlayerInfo
        {
            public string m_name;
            public Colors m_color;
            public int m_id;
            public Client m_client;
            public int[] m_piecePos;

            public bool m_currentTurn;
        }

        public void SetUp()
        {
            List<PlayerInfo> l_ListToSend = new List<PlayerInfo>();
            for (int i = 0; i < m_clientList.Count; i++)
            {
                PlayerInfo l_pi = new PlayerInfo();
                l_pi.m_name = m_clientList[i].m_name;
                l_pi.m_color = (Colors)i;
                l_pi.m_id = m_clientList[i].m_id;
                l_pi.m_client = m_clientList[i];
                l_pi.m_currentTurn = (i == 0);
                l_pi.m_piecePos = new int[4] {1, 0, 0, 0};
                if(m_playersPerID.ContainsKey(l_pi.m_id))//forTesting
                    m_playersPerID.Add(l_pi.m_id+ i, l_pi);
                else
                    m_playersPerID.Add(l_pi.m_id, l_pi);
                l_ListToSend.Add(l_pi);
            }

            NetworkMessage l_message = MessageConstructor.StartNewMatch(l_ListToSend);
            for (int i = 0; i < m_clientList.Count; i++)
            {
                m_clientList[i].Send(l_message);
            }

            Thread.Sleep(10000);
            for (int i = 0; i < m_clientList.Count; i++)
            {
                NetworkMessage l_message2 = MessageConstructor.ChangeTurn((Colors)0, (int)l_ListToSend[i].m_color == 0);
                m_clientList[i].Send(l_message2);
            }
        }
    }
}
