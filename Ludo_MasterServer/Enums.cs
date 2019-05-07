using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ludo_MasterServer
{
    public static class Enums
    {
        public enum Colors{ red = 0, blue, green, yellow}

        public enum MessageType : byte
        {
            welcome = 0x00,
            ping = 0x01,
            logIn = 0x02,
            joinNewGame = 0x03,
            startNewGame = 0x04,
            loginFailed = 0x05,
            changeTurn =0x06,
            movePiece = 0x07,
            rollDice = 0x08,
            choosePiece = 0x09,
        }
    }
}
