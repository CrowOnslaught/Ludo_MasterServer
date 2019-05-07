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

#region structs
        public struct PlayerInfo
        {
            public string m_name;
            public Colors m_color;
            public int m_id;
            public Client m_client;
            public int[] m_piecePos;

            public bool m_currentTurn;
        }

        public struct TileInfo
        {
            public int m_redID;
            public int m_blueID;
            public int m_greenID;
            public int m_yellowID;

            public List<Colors> m_currentPieces;

            public TileInfo(int red, int blue, int yellow, int green)
            {
                m_redID = red;
                m_blueID = blue;
                m_yellowID = yellow;
                m_greenID = green;

                m_currentPieces = new List<Colors>();
            }
        }
        #endregion
        public GameServer(int roomID)
        {
            m_roomID = roomID;
            Console.WriteLine("GameServer Created with id:" + m_roomID);
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

                for (int j = 0; j < l_pi.m_piecePos.Length; j++)
                    GetTileByColorAndID(l_pi.m_color, j==0?1:0).m_currentPieces.Add(l_pi.m_color);
            }

            NetworkMessage l_message = MessageConstructor.StartNewMatch(m_roomID, l_ListToSend);
            for (int i = 0; i < m_clientList.Count; i++)
            {
                m_clientList[i].Send(l_message);
            }

            Thread.Sleep(5000);
            for (int i = 0; i < m_clientList.Count; i++)
            {
                NetworkMessage l_message2 = MessageConstructor.ChangeTurn((Colors)0, (int)l_ListToSend[i].m_color == 0);
                m_clientList[i].Send(l_message2);
            }
        }

        public void OnRollDice(Client client)
        {
            Random l_random = new Random();
            int l_rollResult = l_random.Next(7);

            NetworkMessage l_message = MessageConstructor.RollDice(l_rollResult);
            for (int i = 0; i < m_clientList.Count; i++)
                m_clientList[i].Send(l_message);

            Thread.Sleep(1000);

            NetworkMessage l_message2 = MessageConstructor.ChoosePiece();
            client.Send(l_message2);
        }
        public void OnSelectPiece(Client c, int tileID)
        {
            Colors l_color = m_playersPerID[m_clientList.FirstOrDefault(x => x == c).m_id].m_color;
            //TileInfo l_tile = m_tiles.FirstOrDefault(x => x.)
            Console.WriteLine("Selected piece in tile " + tileID);
        }

#region TileManagement
        public TileInfo GetTileByColorAndID(Colors pieceColor, int ID)
        {
            TileInfo l_tile = new TileInfo(-1, -1, -1, -1);
            switch (pieceColor)
            {
                case Colors.red:
                    l_tile = m_tiles.First(x => x.m_redID == ID);
                    break;
                case Colors.blue:
                    l_tile = m_tiles.First(x => x.m_blueID == ID);
                    break;
                case Colors.green:
                    l_tile = m_tiles.First(x => x.m_greenID == ID);
                    break;
                case Colors.yellow:
                    l_tile = m_tiles.First(x => x.m_yellowID == ID);
                    break;
            }

            return l_tile;
        }
        public TileInfo[] m_tiles = new TileInfo[]
        {
            new TileInfo(1,18,35,52),
            new TileInfo(2,19,36,53),
            new TileInfo(3,20,37,54),
            new TileInfo(4,21,38,55),
            new TileInfo(5,22,39,56),
            new TileInfo(6,23,40,57),
            new TileInfo(7,24,41,58),
            new TileInfo(8,25,42,59),
            new TileInfo(9,26,43,60),
            new TileInfo(10,27,44,61),
            new TileInfo(11,28,45,62),
            new TileInfo(12,29,46,63),
            new TileInfo(13,30,47,64),
            new TileInfo(14,31,48,-1),
            new TileInfo(15,32,49,-1),
            new TileInfo(16,33,50,-1),
            new TileInfo(17,34,51,-1),
            new TileInfo(18,35,52,1),
            new TileInfo(19,36,53,2),
            new TileInfo(20,37,54,3),
            new TileInfo(21,38,55,4),
            new TileInfo(22,39,56,5),
            new TileInfo(23,40,57,6),
            new TileInfo(24,41,58,7),
            new TileInfo(25,42,59,8),
            new TileInfo(26,43,60,9),
            new TileInfo(27,44,61,10),
            new TileInfo(28,45,62,11),
            new TileInfo(29,46,63,12),
            new TileInfo(30,47,64,13),
            new TileInfo(31,48,-1,14),
            new TileInfo(32,49,-1,15),
            new TileInfo(33,50,-1,16),
            new TileInfo(34,51,-1,17),
            new TileInfo(35,52,1,18),
            new TileInfo(36,53,2,19),
            new TileInfo(37,54,3,20),
            new TileInfo(38,55,4,21),
            new TileInfo(39,56,5,22),
            new TileInfo(40,57,6,23),
            new TileInfo(41,58,7,24),
            new TileInfo(42,59,8,25),
            new TileInfo(43,60,9,26),
            new TileInfo(44,61,10,27),
            new TileInfo(45,62,11,28),
            new TileInfo(46,63,12,29),
            new TileInfo(47,64,13,30),
            new TileInfo(48,-1,14,31),
            new TileInfo(49,-1,15,32),
            new TileInfo(50,-1,16,33),
            new TileInfo(51,-1,17,34),
            new TileInfo(52,1,18,35),
            new TileInfo(53,2,19,36),
            new TileInfo(54,3,20,37),
            new TileInfo(55,4,21,38),
            new TileInfo(56,5,22,39),
            new TileInfo(57,6,23,40),
            new TileInfo(58,7,24,41),
            new TileInfo(59,8,25,42),
            new TileInfo(60,9,26,43),
            new TileInfo(61,10,27,44),
            new TileInfo(62,11,28,45),
            new TileInfo(63,12,29,46),
            new TileInfo(64,13,30,47),
            new TileInfo(65,-1,-1,-1),
            new TileInfo(66,-1,-1,-1),
            new TileInfo(67,-1,-1,-1),
            new TileInfo(68,-1,-1,-1),
            new TileInfo(69,-1,-1,-1),
            new TileInfo(70,-1,-1,-1),
            new TileInfo(71,-1,-1,-1),
            new TileInfo(72,-1,-1,-1),
            new TileInfo(-1,14,31,48),
            new TileInfo(-1,15,32,49),
            new TileInfo(-1,16,33,50),
            new TileInfo(-1,17,34,51),
            new TileInfo(-1,65,-1,-1),
            new TileInfo(-1,66,-1,-1),
            new TileInfo(-1,67,-1,-1),
            new TileInfo(-1,68,-1,-1),
            new TileInfo(-1,69,-1,-1),
            new TileInfo(-1,70,-1,-1),
            new TileInfo(-1,71,-1,-1),
            new TileInfo(-1,72,-1,-1),
            new TileInfo(-1,-1,65,-1),
            new TileInfo(-1,-1,66,-1),
            new TileInfo(-1,-1,67,-1),
            new TileInfo(-1,-1,68,-1),
            new TileInfo(-1,-1,69,-1),
            new TileInfo(-1,-1,70,-1),
            new TileInfo(-1,-1,71,-1),
            new TileInfo(-1,-1,72,-1),
            new TileInfo(-1,-1,-1,65),
            new TileInfo(-1,-1,-1,66),
            new TileInfo(-1,-1,-1,67),
            new TileInfo(-1,-1,-1,68),
            new TileInfo(-1,-1,-1,69),
            new TileInfo(-1,-1,-1,70),
            new TileInfo(-1,-1,-1,71),
            new TileInfo(-1,-1,-1,72),
            new TileInfo(-1,-1,-1,0),
            new TileInfo(-1,-1,0,-1),
            new TileInfo(-1,0,-1,-1),
            new TileInfo(0,-1,-1,-1),
        };
#endregion
    }
}
