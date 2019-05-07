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
        public static NetworkMessage LogInSucess()
        {
            NetworkMessage l_message = new NetworkMessage();
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
    }
}
