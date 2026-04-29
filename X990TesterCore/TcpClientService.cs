using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace X990TesterCore
{
    public class TcpClientService
    {
        private string _ipAddress;
        private int _port = 7800;

        public TcpClientService(string ip, int port)
        {
            _ipAddress = ip;
            _port = port;
        }

        public async Task TestConnectionAsync()
        {
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(_ipAddress, _port);
            }
        }

        public async Task<string> SendRequestAsync(string json)
        {
            using (TcpClient client = new TcpClient(_ipAddress, _port))
            using (NetworkStream stream = client.GetStream())
            {
                string framed = PcNcFrame.Wrap(json);
                FileLoggingService.Log("befor Send", framed);
                byte[] data = Encoding.UTF8.GetBytes(framed);

                await stream.WriteAsync(data, 0, data.Length);

                // Read response with dynamic buffer
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    do
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        ms.Write(buffer, 0, bytesRead);
                    } while (stream.DataAvailable);

                    string framedResponse = Encoding.UTF8.GetString(ms.ToArray());
                    FileLoggingService.Log("framedResponse", framedResponse);
                    return PcNcFrame.Unwrap(framedResponse);
                }
            }
        }

    }
}
