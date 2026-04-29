using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace X990TesterCore
{
    public static class AppSession
    {
        public static TcpClientService TcpClient;
        public static RSACryptoServiceProvider PcRsa;
        public static RSACryptoServiceProvider TerminalRsa;
    }
}
