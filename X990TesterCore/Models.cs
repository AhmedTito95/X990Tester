using Newtonsoft.Json;

namespace X990TesterCore
{
    public class EncryptedPacket
    {
        [JsonProperty("k")]
        public string EncryptedAesKey { get; set; } // Encrypted by RSA

        [JsonProperty("d")]
        public string EncryptedData { get; set; }   // Encrypted by AES
    }

    public class BaseRequest
    {
        [JsonProperty("lang")]
        public int Lang { get; set; } = 0; // 0: English, 1: Arabic [cite: 100]

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class InitRequest : BaseRequest
    {
        [JsonProperty("rsaPubKey")]
        public string RsaPubKey { get; set; }

        public InitRequest()
        {
            Type = "INIT";
        }
    }

    public class SaleRequest : BaseRequest
    {
        [JsonProperty("amt")]
        public string Amount { get; set; } // No decimals, e.g., "100" for 1.00

        [JsonProperty("cur")]
        public int Currency { get; set; } // 376 for ILS, 840 for USD

        [JsonProperty("rcpt")]
        public string ReceiptNumber { get; set; } // Unique tracking ID

        [JsonProperty("cshr")]
        public int CashierId { get; set; }

        [JsonProperty("print")]
        public int Print { get; set; } = 1;

        public SaleRequest()
        {
            Type = "SALE";
        }
    }

    public class RefundRequest : BaseRequest
    {
        [JsonProperty("amt")]
        public string Amount { get; set; }

        [JsonProperty("cur")]
        public int Currency { get; set; } // 376 for ILS, 840 for USD

        [JsonProperty("rcpt")]
        public string ReceiptNumber { get; set; } // Unique tracking ID

        [JsonProperty("cshr")]
        public int CashierId { get; set; }

        [JsonProperty("origSeq")]
        public int OrgSequenceNumber { get; set; }

        [JsonProperty("authCode")]
        public string OrgAuthCode { get; set; }
        
        [JsonProperty("datim")]
        public string OrgDate { get; set; }

        public RefundRequest()
        {
            Type = "RETURN";
        }
    }

    public class PosResponse
    {
        [JsonProperty("resp_code")]
        public int ResponseCode { get; set; } // 0 = Success [cite: 138]

        [JsonProperty("error_msg")]
        public string ErrorMessage { get; set; }

        [JsonProperty("rsaPubKey")]
        public string TerminalRsaPubKey { get; set; } // Only in INIT response

        // Sale specific fields
        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }

        [JsonProperty("trans")]
        public object TransactionDetails { get; set; }

        [JsonProperty("is_qr_pay")]
        public object IsQrPay { get; set; }
        
        [JsonProperty("app_ver")]
        public object AppVersion { get; set; }
        
        [JsonProperty("pos_code")]
        public object TerminalID { get; set; }
        
        [JsonProperty("datim")]
        public object TransactionDate { get; set; }
        
        [JsonProperty("seq")]
        public object SequenceNumber { get; set; }

    }
}
