using Newtonsoft.Json;
using System.Security.Cryptography;

namespace X990TesterCore
{
    public static class TransactionService
    {
        public static async Task<PosResponse> InitAsync()
        {
            // Load or create the PC key pair from the Windows key store.
            // On the first run this generates a new 4096-bit key; on later
            // runs it reloads the same key from the named container.
            AppSession.PcRsa = KeyStorageService.LoadOrCreatePcKey();

            var request = new InitRequest
            {
                Lang = 0,
                RsaPubKey = Convert.ToBase64String(AppSession.PcRsa.ExportSubjectPublicKeyInfo())
            };

            string json = JsonConvert.SerializeObject(request);

            string responseJson = await AppSession.TcpClient.SendRequestAsync(json);
            FileLoggingService.Log("InitresponseJson", responseJson);
            var response = JsonConvert.DeserializeObject<PosResponse>(responseJson);

            // Persist terminal public key on success
            if (response?.ResponseCode == 0 && !string.IsNullOrEmpty(response.TerminalRsaPubKey))
            {
                KeyStorageService.SaveTerminalPublicKey(response.TerminalRsaPubKey);
            }

            return response;
        }

        public static async Task<PosResponse> SaleAsync(int amount, int currency, string receipt, int print)
        {
            var saleRequest = new SaleRequest
            {
                Lang = 0,
                Amount = amount.ToString(),
                Currency = currency,
                ReceiptNumber = receipt,
                Print = print
            };

            string plainJson = JsonConvert.SerializeObject(saleRequest);
            EncryptedPacket encryptedReq = CryptoService.Encrypt(plainJson);
            string encryptedJson = JsonConvert.SerializeObject(encryptedReq);

            string responseJson = await AppSession.TcpClient.SendRequestAsync(encryptedJson);

            var encryptedResponse = JsonConvert.DeserializeObject<EncryptedPacket>(responseJson);
            string clearResponseJson = CryptoService.Decrypt(encryptedResponse);
            var saleResponse = JsonConvert.DeserializeObject<PosResponse>(clearResponseJson);
            FileLoggingService.Log("saleResponse", clearResponseJson);

            return saleResponse;
        }

        public static async Task<PosResponse> RefundAsync(int amount, int currency, string receipt,
            int seqNumber, string authCode, string orgDate)
        {
            var refundRequest = new RefundRequest
            {
                Lang = 0,
                Amount = amount.ToString(),
                Currency = currency,
                ReceiptNumber = receipt,
                OrgSequenceNumber = seqNumber,
                OrgAuthCode = authCode,
                OrgDate = orgDate
                //OrgDate = orgDate.ToString("yyMMddHHmmss")
            };

            string plainJson = JsonConvert.SerializeObject(refundRequest);
            EncryptedPacket encryptedReq = CryptoService.Encrypt(plainJson);
            string encryptedJson = JsonConvert.SerializeObject(encryptedReq);

            string responseJson = await AppSession.TcpClient.SendRequestAsync(encryptedJson);

            var encryptedResponse = JsonConvert.DeserializeObject<EncryptedPacket>(responseJson);
            string clearResponseJson = CryptoService.Decrypt(encryptedResponse);
            var refundResponse = JsonConvert.DeserializeObject<PosResponse>(clearResponseJson);

            FileLoggingService.Log("refundResponse", clearResponseJson);

            return refundResponse;
        }
    }
}