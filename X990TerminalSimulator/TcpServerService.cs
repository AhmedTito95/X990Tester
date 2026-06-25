using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace X990TerminalSimulator
{
    public class TransactionContext
    {
        public string Type { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ReceiptNumber { get; set; }
        public string RawRequest { get; set; }
        public TaskCompletionSource<PosResponse> ResponseSource { get; set; } = new TaskCompletionSource<PosResponse>();
    }

    public class TcpServerService
    {
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private bool _isRunning;

        public event EventHandler<string> OnLogReceived;
        public event EventHandler<string> OnLogSent;
        public event EventHandler<string> OnStatusChanged;
        public event EventHandler<TransactionContext> OnTransactionRequest;

        public int Port { get; private set; }

        public TcpServerService(int port = 7800)
        {
            Port = port;
        }

        public void Start()
        {
            if (_isRunning) return;

            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.IPv6Any, Port);
            _listener.Server.DualMode = true;
            _listener.Start();
            _isRunning = true;

            OnStatusChanged?.Invoke(this, $"Server listening on port {Port}...");
            Task.Run(() => AcceptConnectionsAsync(_cts.Token));
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cts?.Cancel();
            _listener?.Stop();

            OnStatusChanged?.Invoke(this, "Server stopped.");
        }

        private async Task AcceptConnectionsAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync(ct);
                    _ = Task.Run(() => HandleClientAsync(client, ct), ct);
                }
                catch (ObjectDisposedException)
                {
                    // Listener was stopped
                    break;
                }
                catch (Exception ex)
                {
                    if (!ct.IsCancellationRequested)
                    {
                        OnLogReceived?.Invoke(this, $"Error accepting client: {ex.Message}");
                    }
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                try
                {
                    // Read request using our frame reader
                    string requestJson = await ReadPcNcFrameAsync(stream, ct);
                    OnLogReceived?.Invoke(this, $"Incoming Frame: {requestJson}");

                    // Check if it's INIT or Encrypted
                    if (requestJson.Contains("\"type\":\"INIT\"") || requestJson.Contains("\"type\": \"INIT\""))
                    {
                        var initReq = JsonConvert.DeserializeObject<InitRequest>(requestJson);
                        if (initReq == null)
                        {
                            throw new Exception("Malformed INIT request");
                        }

                        // Process INIT (Key Exchange)
                        var response = ProcessInit(initReq);
                        string respJson = JsonConvert.SerializeObject(response);
                        string framed = $"~PCNC~{respJson.Length:D4}~{respJson}";

                        byte[] respBytes = Encoding.UTF8.GetBytes(framed);
                        await stream.WriteAsync(respBytes, 0, respBytes.Length, ct);
                        OnLogSent?.Invoke(this, $"Sent INIT Response: {framed}");
                    }
                    else
                    {
                        // Parse as EncryptedPacket
                        var encryptedPacket = JsonConvert.DeserializeObject<EncryptedPacket>(requestJson);
                        if (encryptedPacket == null || string.IsNullOrEmpty(encryptedPacket.EncryptedData))
                        {
                            throw new Exception("Invalid encrypted packet");
                        }

                        // Decrypt request
                        string decryptedJson = CryptoService.Decrypt(encryptedPacket);
                        OnLogReceived?.Invoke(this, $"Decrypted Request Payload: {decryptedJson}");

                        // Parse the decrypted request to find its type
                        var baseReq = JsonConvert.DeserializeObject<BaseRequest>(decryptedJson);
                        if (baseReq == null)
                        {
                            throw new Exception("Malformed request JSON");
                        }

                        // Handle non-interactive transactions automatically
                        if (baseReq.Type == "QUERY")
                        {
                            var queryReq = JsonConvert.DeserializeObject<QueryRequest>(decryptedJson);
                            var originalTx = TransactionHistory.FindBySeq(queryReq?.OrgSequenceNumber ?? 0);

                            var queryResp = new
                            {
                                type = "QUERY",
                                origSeq = 0,
                                found = originalTx != null,
                                trans = originalTx?.ResponseObject,
                                error_msg = originalTx != null ? "" : "Transaction not found"
                            };

                            string queryPlainResponseJson = JsonConvert.SerializeObject(queryResp);
                            OnLogSent?.Invoke(this, $"Auto Response (Decrypted): {queryPlainResponseJson}");

                            EncryptedPacket queryEncryptedResp = CryptoService.Encrypt(queryPlainResponseJson);
                            string queryEncryptedResponseJson = JsonConvert.SerializeObject(queryEncryptedResp);
                            string queryFramed = $"~PCNC~{queryEncryptedResponseJson.Length:D4}~{queryEncryptedResponseJson}";

                            byte[] queryRespBytes = Encoding.UTF8.GetBytes(queryFramed);
                            await stream.WriteAsync(queryRespBytes, 0, queryRespBytes.Length, ct);
                            OnLogSent?.Invoke(this, $"Sent Auto Response Frame: {queryFramed}");
                            return;
                        }

                        if (baseReq.Type == "BATCH_TIME")
                        {
                            var batchResp = new PosResponse
                            {
                                Type = "BATCH_TIME",
                                ResponseCode = 0,
                                ErrorMessage = ""
                            };

                            string batchPlainResponseJson = JsonConvert.SerializeObject(batchResp);
                            OnLogSent?.Invoke(this, $"Auto Response (Decrypted): {batchPlainResponseJson}");

                            EncryptedPacket batchEncryptedResp = CryptoService.Encrypt(batchPlainResponseJson);
                            string batchEncryptedResponseJson = JsonConvert.SerializeObject(batchEncryptedResp);
                            string batchFramed = $"~PCNC~{batchEncryptedResponseJson.Length:D4}~{batchEncryptedResponseJson}";

                            byte[] batchRespBytes = Encoding.UTF8.GetBytes(batchFramed);
                            await stream.WriteAsync(batchRespBytes, 0, batchRespBytes.Length, ct);
                            OnLogSent?.Invoke(this, $"Sent Auto Response Frame: {batchFramed}");
                            return;
                        }

                        var context = new TransactionContext
                        {
                            Type = baseReq.Type,
                            RawRequest = decryptedJson
                        };

                        // Populate details based on request type
                        if (baseReq.Type == "SALE" || baseReq.Type == "SALE_CB" || baseReq.Type == "LOAN")
                        {
                            var saleReq = JsonConvert.DeserializeObject<SaleRequest>(decryptedJson);
                            if (saleReq != null)
                            {
                                context.Amount = saleReq.Amount;
                                context.Currency = saleReq.Currency == 376 ? "ILS" : "USD";
                                context.ReceiptNumber = saleReq.ReceiptNumber;
                            }
                        }
                        else if (baseReq.Type == "VOID" || baseReq.Type == "RETURN")
                        {
                            var refundReq = JsonConvert.DeserializeObject<RefundRequest>(decryptedJson);
                            if (refundReq != null)
                            {
                                context.Amount = refundReq.Amount;
                                context.Currency = refundReq.Currency == 376 ? "ILS" : "USD";
                                context.ReceiptNumber = refundReq.ReceiptNumber;
                            }
                        }

                        // Dispatch to the UI to handle and wait for response
                        OnTransactionRequest?.Invoke(this, context);

                        // Await UI response
                        PosResponse responseObj = await context.ResponseSource.Task;

                        // Encrypt response
                        string plainResponseJson = JsonConvert.SerializeObject(responseObj);
                        OnLogSent?.Invoke(this, $"Decrypted Response Payload: {plainResponseJson}");

                        EncryptedPacket encryptedResp = CryptoService.Encrypt(plainResponseJson);
                        string encryptedResponseJson = JsonConvert.SerializeObject(encryptedResp);
                        string framed = $"~PCNC~{encryptedResponseJson.Length:D4}~{encryptedResponseJson}";

                        byte[] respBytes = Encoding.UTF8.GetBytes(framed);
                        await stream.WriteAsync(respBytes, 0, respBytes.Length, ct);
                        OnLogSent?.Invoke(this, $"Sent Encrypted Response Frame: {framed}");
                    }
                }
                catch (Exception ex)
                {
                    OnLogReceived?.Invoke(this, $"Error: {ex.Message}");

                    // Try to send error response if possible
                    try
                    {
                        var errResp = new PosResponse
                        {
                            ResponseCode = 999,
                            ErrorMessage = ex.Message
                        };
                        string respJson = JsonConvert.SerializeObject(errResp);
                        string framed = $"~PCNC~{respJson.Length:D4}~{respJson}";
                        byte[] respBytes = Encoding.UTF8.GetBytes(framed);
                        await stream.WriteAsync(respBytes, 0, respBytes.Length, ct);
                    }
                    catch { /* Ignore double fault */ }
                }
            }
        }

        private PosResponse ProcessInit(InitRequest request)
        {
            // 1. Save PC client public key to local file
            KeyStorageService.SaveClientPublicKey(request.RsaPubKey);

            // 2. Load or generate Terminal key pair
            using (var terminalRsa = KeyStorageService.LoadOrCreateTerminalKey())
            {
                // Export terminal public key as base64 DER
                string termPubKey = Convert.ToBase64String(terminalRsa.ExportSubjectPublicKeyInfo());

                return new PosResponse
                {
                    Type = "INIT",
                    ResponseCode = 0,
                    ErrorMessage = "",
                    TerminalRsaPubKey = termPubKey
                };
            }
        }

        private async Task<string> ReadPcNcFrameAsync(NetworkStream stream, CancellationToken ct)
        {
            // 1. Read "~PCNC~" prefix
            byte[] prefixBuffer = new byte[6];
            await ReadExactlyAsync(stream, prefixBuffer, 0, 6, ct);
            string prefix = Encoding.UTF8.GetString(prefixBuffer);
            if (prefix != "~PCNC~")
                throw new FormatException($"Invalid PCNC frame prefix: {prefix}");

            // 2. Read length until next '~'
            StringBuilder lenStr = new StringBuilder();
            byte[] singleByte = new byte[1];
            while (true)
            {
                int read = await stream.ReadAsync(singleByte, 0, 1, ct);
                if (read == 0) throw new EndOfStreamException("Connection closed while parsing length.");
                char c = (char)singleByte[0];
                if (c == '~')
                    break;
                lenStr.Append(c);
            }

            if (!int.TryParse(lenStr.ToString(), out int length) || length <= 0)
                throw new FormatException($"Invalid frame length: '{lenStr}'");

            // 3. Read exactly 'length' bytes of JSON payload
            byte[] jsonBuffer = new byte[length];
            await ReadExactlyAsync(stream, jsonBuffer, 0, length, ct);

            return Encoding.UTF8.GetString(jsonBuffer);
        }

        private async Task ReadExactlyAsync(NetworkStream stream, byte[] buffer, int offset, int count, CancellationToken ct)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = await stream.ReadAsync(buffer, offset + totalRead, count - totalRead, ct);
                if (read == 0)
                    throw new EndOfStreamException("Socket connection closed prematurely.");
                totalRead += read;
            }
        }
    }
}
