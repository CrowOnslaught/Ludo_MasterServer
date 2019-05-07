using System;

namespace Ludo_MasterServer
{
    class Program
    {
        public static Server m_server;
        public static GameManager m_gameManager;

        static void Main(string[] args)
        {
            m_server = new Server();
            m_gameManager = new GameManager();
        }
    }
}
