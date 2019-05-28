using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ludo_MasterServer.Enums;

namespace Ludo_MasterServer
{
    public static class MessageConstructor
    {
        public static NetworkMessage Welcome()
        {
            NetworkMessage l_message = new NetworkMessage();
            l_message.Build(MessageType.welcome);
            return l_message;
        }
        public static NetworkMessage LogInSucess(int clientID)
        {
            int l_clientScore = Program.m_server.m_database.GetClientScore(clientID);

            NetworkMessage l_message = new NetworkMessage();
            l_message.Write(l_clientScore);
            l_message.Build(MessageType.logIn);
            return l_message;
        }

        public static NetworkMessage LogInFailed()
        {
            NetworkMessage l_message = new NetworkMessage();
            l_message.Build(MessageType.loginFailed);
            return l_message;
        }
        public static NetworkMessage StartNewMatch(int idGame, List<GameServer.PlayerInfo> clientList)
        {
            NetworkMessage l_message = new NetworkMessage();

            l_message.Write(idGame);
            for (int i = 0; i < clientList.Count; i++)
            {
                l_message.Write(clientList[i].m_id);
                l_message.Write(clientList[i].m_name);
                l_message.Write((int)clientList[i].m_color);
                l_message.Write(clientList[i].m_currentTurn);

                for (int j = 0; j < 4; j++) //StartPiecesPos
                    l_message.Write(clientList[i].m_piecePos[j]);
            }
            l_message.Build(MessageType.startNewGame);
            return l_message;
        }
        public static NetworkMessage ChangeTurn(Enums.Colors turnColor, bool clientTurn)
        {
            NetworkMessage l_message = new NetworkMessage();
            l_message.Write((int)turnColor);
            l_message.Write(clientTurn);
            l_message.Build(MessageType.changeTurn);
            return l_message;
        }

        public static NetworkMessage RollDice(int result)
        {
            NetworkMessage l_message = new NetworkMessage();
            l_message.Write(result);
            l_message.Build(MessageType.rollDice);
            return l_message;
        }

        public static NetworkMessage ChoosePiece()
        {
            NetworkMessage l_message = new NetworkMessage();
            l_message.Build(MessageType.choosePiece);
            return l_message;
        }

        public static NetworkMessage MovePiece(Colors color, int originID, int destID)
        {
            NetworkMessage l_message = new NetworkMessage();
            l_message.Write((int)color);
            l_message.Write(originID);
            l_message.Write(destID);

            l_message.Build(MessageType.movePiece);
            return l_message;
        }
        public static NetworkMessage EndMatch(int position)
        {
            NetworkMessage l_message = new NetworkMessage();
            l_message.Write(position);
            l_message.Build(MessageType.endMatch);
            return l_message;
        }
        public static NetworkMessage CurrentGames(Dictionary<int, GameServer> gameServersPerID, int clientID)
        {
            NetworkMessage l_message = new NetworkMessage();
            List<GameServer> l_gameServers = new List<GameServer>();

            foreach (var keyValue in gameServersPerID)
                if (keyValue.Value.m_clientList.FirstOrDefault(x => x.m_id == clientID) != null)
                    l_gameServers.Add(keyValue.Value);

            l_message.Write(l_gameServers.Count); //Number of GameServers Assosiated with clientID

            if(l_gameServers.Count > 0)
                foreach (var gs in l_gameServers)
                {
                    l_message.Write(gs.m_roomID); //Id of the gameServer
                    l_message.Write(gs.m_playersPerID[clientID].m_currentTurn); //has the client the current turn
                    foreach (var player in gs.m_playersPerID)
                    {
                        if (player.Key != clientID)
                           l_message.Write(player.Value.m_name); //names of the others 3 players
                    }
                }

            l_message.Build(MessageType.currentGames);
            return l_message;
        }

        public static NetworkMessage Ranking(int amount)
        {
            NetworkMessage l_message = new NetworkMessage();
            List<PlayerRankingInfo> l_rankingList = Program.m_server.m_database.GetRankingTop(amount);

            l_message.Write(l_rankingList.Count);
            for (int i = 0; i < l_rankingList.Count; i++)
            {
                l_message.Write(l_rankingList[i].m_playerName);
                l_message.Write(l_rankingList[i].m_playerScore);
                l_message.Write(l_rankingList[i].m_rankingPos);
            }


            l_message.Build(MessageType.ranking);
            return l_message;
        }
    }
}
