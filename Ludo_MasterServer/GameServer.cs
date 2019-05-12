﻿using System;
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
            public List<int> m_rollResults;
        }

        public struct TileInfo
        {
            public int m_redID;
            public int m_blueID;
            public int m_greenID;
            public int m_yellowID;

            public bool m_safeTile;
            public List<Colors> m_currentPieces;

            public TileInfo(int red, int blue, int yellow, int green, bool safeTile= false)
            {
                m_redID = red;
                m_blueID = blue;
                m_yellowID = yellow;
                m_greenID = green;

                m_safeTile = safeTile;
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
                l_pi.m_rollResults = new List<int>();

                if (m_playersPerID.ContainsKey(l_pi.m_id))//forTesting
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
                if (m_clientList[i] != null)
                    m_clientList[i].Send(l_message);
            }

            Thread.Sleep(2000);
            for (int i = 0; i < m_clientList.Count; i++)
            {
                if (m_clientList[i] != null)
                {
                    NetworkMessage l_message2 = MessageConstructor.ChangeTurn((Colors)0, (int)l_ListToSend[i].m_color == 0);
                    m_clientList[i].Send(l_message2);

                    m_playersPerID[m_clientList[i].m_id].m_rollResults.Clear();
                }
            }
        }

        public void OnRollDice(Client client)
        {
            Random l_random = new Random();
            int l_rollResult = l_random.Next(1,7);

            m_playersPerID[client.m_id].m_rollResults.Add(l_rollResult);

            NetworkMessage l_message = MessageConstructor.RollDice(l_rollResult);
            for (int i = 0; i < m_clientList.Count; i++)
                if (m_clientList[i] != null)
                    m_clientList[i].Send(l_message);

            NetworkMessage l_message2 = MessageConstructor.ChoosePiece();
            client.Send(l_message2);
        }
        public void OnSelectPiece(Client currentTurnClient, int originID)
        {
            NetworkMessage l_message2 = MessageConstructor.ChoosePiece();

            Colors l_color = m_playersPerID[currentTurnClient.m_id].m_color;
            TileInfo l_originTile = GetTileByColorAndID(l_color, originID);
            int l_rollResult = m_playersPerID[currentTurnClient.m_id].m_rollResults.Last();
            int l_destID = -1;

            if (originID == 0) //Moving from Home
            {
                if (l_rollResult == 5)
                {
                    TileInfo l_destTile = GetTileByColorAndID(l_color, 1);
                    if (l_destTile.m_currentPieces.Count < 2)
                    {
                        l_destID = 1;
                        MovePiece(currentTurnClient, l_color, originID, l_destID);
                    }
                    else
                    {
                        
                        currentTurnClient.Send(l_message2);
                        return;
                    }
                }
                else
                {
                    currentTurnClient.Send(l_message2);
                    return;
                }
            }
            else //Normal Roll
            {
                for (int i = originID+1; i <= originID + l_rollResult; i++) // Check for tiles in movement range
                {
                    TileInfo l_tile = GetTileByColorAndID(l_color, i);
                    if (l_tile.m_currentPieces.Count >= 2) // If tile is full
                    {
                        //Block path
                        l_destID = i-1;
                        MovePiece(currentTurnClient, l_color, originID, l_destID);
                        break;
 
                    }
                    else
                    {
                        if (i == (originID + l_rollResult)) // if tile is the last tile to check
                        {
                            l_destID = originID + l_rollResult;

                            if (l_destID > 72) //Move back if you overpassed the final tile
                                l_destID = 72-(l_destID - 72);

                            MovePiece(currentTurnClient, l_color, originID, originID + l_rollResult);
                        }
                    }
                }
            }

            if (l_destID != -1)
            {
              
                TileInfo l_destTile = GetTileByColorAndID(l_color, l_destID);

                if (!l_destTile.m_safeTile)
                    for (int i = 0; i < l_destTile.m_currentPieces.Count; i++)
                    {
                        if (l_destTile.m_currentPieces[i] != l_color)
                        {
                            //Kill Enemy piece
                            Colors l_enemyPieceColor = l_destTile.m_currentPieces[i];
                            MovePiece(currentTurnClient, l_enemyPieceColor, GetTileIDByColor(l_destTile, l_enemyPieceColor), 0);

                            //Make player move 10 tiles
                            m_playersPerID[currentTurnClient.m_id].m_rollResults.Add(10);
                            currentTurnClient.Send(l_message2);
                            return;
                        }
                    }
            }

            if (l_rollResult == 6)//Repeat Turn
            {
                for (int i = 0; i < m_clientList.Count; i++)
                {
                    Client l_client = m_clientList[i];
                    if (l_client != null)
                    {
                        NetworkMessage l_message3 = MessageConstructor.ChangeTurn(l_color, m_playersPerID[l_client.m_id].m_color == l_color);
                        l_client.Send(l_message3);
                    }
                }
            }
            else //Change to next player turn
            {
                Colors l_nextTurnColor = (Colors)((int)l_color + 1);
                if ((int)l_nextTurnColor >= 4)
                    l_nextTurnColor = (Colors)0;

                for (int i = 0; i < m_clientList.Count; i++)
                {
                    Client l_client = m_clientList[i];
                    if (l_client != null)
                    {
                        NetworkMessage l_message3 = MessageConstructor.ChangeTurn(l_nextTurnColor, m_playersPerID[l_client.m_id].m_color == l_nextTurnColor);
                        l_client.Send(l_message3);
                    }
                }
                m_playersPerID[currentTurnClient.m_id].m_rollResults.Clear();
            }
            Console.WriteLine("player {0}| origin {1}| dest {2}", currentTurnClient.m_name, originID, l_destID);
        }

        private void MovePiece(Client currentTurnClient, Colors color, int originID, int destID)
        {
            TileInfo l_originTile = GetTileByColorAndID(color, originID);
            TileInfo l_destTile = GetTileByColorAndID(color, destID);

            for (int i = 0; i < l_originTile.m_currentPieces.Count; i++)
                if (l_originTile.m_currentPieces[i] == color)
                {
                    l_originTile.m_currentPieces.RemoveAt(i);
                    l_destTile.m_currentPieces.Add(color);
                    break;
                }


            NetworkMessage l_message = MessageConstructor.MovePiece(color, originID, destID);
            for (int i = 0; i < m_clientList.Count; i++)
            {
                Client l_client = m_clientList[i];
                if (l_client != null)
                    l_client.Send(l_message);
            }
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
        public int GetTileIDByColor(TileInfo tile, Colors color)
        {
            int l_tileID = -1;
            switch (color)
            {
                case Colors.red:
                    l_tileID = tile.m_redID;
                    break;
                case Colors.blue:
                    l_tileID = tile.m_blueID;
                    break;
                case Colors.green:
                    l_tileID = tile.m_greenID;
                    break;
                case Colors.yellow:
                    l_tileID = tile.m_yellowID;
                    break;
            }

            return l_tileID;
        }
        public TileInfo[] m_tiles = new TileInfo[]
        {
            new TileInfo(1,18,35,52, true),
            new TileInfo(2,19,36,53),
            new TileInfo(3,20,37,54),
            new TileInfo(4,21,38,55),
            new TileInfo(5,22,39,56),
            new TileInfo(6,23,40,57),
            new TileInfo(7,24,41,58),
            new TileInfo(8,25,42,59, true),
            new TileInfo(9,26,43,60),
            new TileInfo(10,27,44,61),
            new TileInfo(11,28,45,62),
            new TileInfo(12,29,46,63),
            new TileInfo(13,30,47,64, true),
            new TileInfo(14,31,48,-1),
            new TileInfo(15,32,49,-1),
            new TileInfo(16,33,50,-1),
            new TileInfo(17,34,51,-1),
            new TileInfo(18,35,52,1, true),
            new TileInfo(19,36,53,2),
            new TileInfo(20,37,54,3),
            new TileInfo(21,38,55,4),
            new TileInfo(22,39,56,5),
            new TileInfo(23,40,57,6),
            new TileInfo(24,41,58,7),
            new TileInfo(25,42,59,8, true),
            new TileInfo(26,43,60,9),
            new TileInfo(27,44,61,10),
            new TileInfo(28,45,62,11),
            new TileInfo(29,46,63,12),
            new TileInfo(30,47,64,13, true),
            new TileInfo(31,48,-1,14),
            new TileInfo(32,49,-1,15),
            new TileInfo(33,50,-1,16),
            new TileInfo(34,51,-1,17),
            new TileInfo(35,52,1,18, true),
            new TileInfo(36,53,2,19),
            new TileInfo(37,54,3,20),
            new TileInfo(38,55,4,21),
            new TileInfo(39,56,5,22),
            new TileInfo(40,57,6,23),
            new TileInfo(41,58,7,24),
            new TileInfo(42,59,8,25, true),
            new TileInfo(43,60,9,26),
            new TileInfo(44,61,10,27),
            new TileInfo(45,62,11,28),
            new TileInfo(46,63,12,29),
            new TileInfo(47,64,13,30, true),
            new TileInfo(48,-1,14,31),
            new TileInfo(49,-1,15,32),
            new TileInfo(50,-1,16,33),
            new TileInfo(51,-1,17,34),
            new TileInfo(52,1,18,35, true),
            new TileInfo(53,2,19,36),
            new TileInfo(54,3,20,37),
            new TileInfo(55,4,21,38),
            new TileInfo(56,5,22,39),
            new TileInfo(57,6,23,40),
            new TileInfo(58,7,24,41),
            new TileInfo(59,8,25,42, true),
            new TileInfo(60,9,26,43),
            new TileInfo(61,10,27,44),
            new TileInfo(62,11,28,45),
            new TileInfo(63,12,29,46),
            new TileInfo(64,13,30,47, true),
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
