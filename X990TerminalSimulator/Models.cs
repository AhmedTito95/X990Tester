using Newtonsoft.Json;

namespace X990TerminalSimulator
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
        public int Lang { get; set; } = 0;

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class InitRequest : BaseRequest
    {
        [JsonProperty("rsaPubKey")]
        public string RsaPubKey { get; set; }
    }

    public class SaleRequest : BaseRequest
    {
        [JsonProperty("amt")]
        public string Amount { get; set; } // No decimals, e.g., "1295" for 12.95

        [JsonProperty("cur")]
        public int Currency { get; set; } // 376 for ILS, 840 for USD

        [JsonProperty("rcpt")]
        public string ReceiptNumber { get; set; }

        [JsonProperty("cshr")]
        public int CashierId { get; set; }

        [JsonProperty("print")]
        public int Print { get; set; } = 1;
    }

    public class RefundRequest : BaseRequest
    {
        [JsonProperty("amt")]
        public string Amount { get; set; }

        [JsonProperty("cur")]
        public int Currency { get; set; }

        [JsonProperty("rcpt")]
        public string ReceiptNumber { get; set; }

        [JsonProperty("cshr")]
        public int CashierId { get; set; }

        [JsonProperty("origSeq")]
        public int OrgSequenceNumber { get; set; }

        [JsonProperty("authCode")]
        public string OrgAuthCode { get; set; }

        [JsonProperty("datim")]
        public string OrgDate { get; set; } // yyMMddHHmmss
    }

    public class QueryRequest : BaseRequest
    {
        [JsonProperty("origSeq")]
        public int OrgSequenceNumber { get; set; }

        [JsonProperty("rcpt")]
        public string ReceiptNumber { get; set; }

        [JsonProperty("cshr")]
        public int CashierId { get; set; }

        [JsonProperty("isQr")]
        public bool IsQr { get; set; }
    }

    public class BatchTimeRequest : BaseRequest
    {
        [JsonProperty("time")]
        public string Time { get; set; } // HH:mm
    }

    public class SaleDetails
    {
        [JsonProperty("amt")]
        public int Amount { get; set; }

        [JsonProperty("cur")]
        public int Currency { get; set; }

        [JsonProperty("cname")]
        public string CurrencyName { get; set; }
    }

    public class CardDetails
    {
        [JsonProperty("type")]
        public string Type { get; set; } // VISA, MASTERCARD, DEBIT, etc.

        [JsonProperty("entry")]
        public string Entry { get; set; } // Swipe, ICC, CTLS

        [JsonProperty("pan")]
        public string Pan { get; set; } // e.g. 411111******1111

        [JsonProperty("pan_seq")]
        public int PanSeq { get; set; } = 1;

        [JsonProperty("holder")]
        public string Holder { get; set; }

        [JsonProperty("verif")]
        public string Verif { get; set; } // NONE, PIN, Signature, etc.
    }

    public class IccDetails
    {
        [JsonProperty("aid")]
        public string Aid { get; set; }

        [JsonProperty("tvr")]
        public string Tvr { get; set; }

        [JsonProperty("tsi")]
        public string Tsi { get; set; }

        [JsonProperty("cvm")]
        public string Cvm { get; set; }

        [JsonProperty("cid")]
        public string Cid { get; set; }

        [JsonProperty("trm_type")]
        public string TrmType { get; set; } = "22";
    }

    public class PosResponse
    {
        [JsonProperty("resp_code")]
        public int ResponseCode { get; set; } // 000 = Success, others = Fail

        [JsonProperty("error_msg")]
        public string ErrorMessage { get; set; }

        [JsonProperty("rsaPubKey")]
        public string TerminalRsaPubKey { get; set; } // Only in INIT response

        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }

        [JsonProperty("is_qr_pay")]
        public bool IsQrPay { get; set; }

        [JsonProperty("qr_ref")]
        public string QrRef { get; set; }

        [JsonProperty("app_ver")]
        public string AppVersion { get; set; } = "38000001,X990_00V1E0188660,05.01.00D\nMar 26 2023";

        [JsonProperty("pos_code")]
        public string TerminalID { get; set; } = "38000001";

        [JsonProperty("outlet")]
        public string Outlet { get; set; } = "38000001";

        [JsonProperty("merch")]
        public string MerchantName { get; set; } = "VERIFONE SIMULATED MERCHANT";

        [JsonProperty("datim")]
        public string TransactionDate { get; set; } // YYMMDDHHmmss

        [JsonProperty("batch")]
        public int Batch { get; set; } = 1;

        [JsonProperty("seq")]
        public int SequenceNumber { get; set; }

        [JsonProperty("stan")]
        public int Stan { get; set; }

        [JsonProperty("instlmt")]
        public int Installment { get; set; }

        [JsonProperty("lang")]
        public int Lang { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("print")]
        public int Print { get; set; }

        [JsonProperty("rcpt")]
        public string ReceiptNumber { get; set; }

        [JsonProperty("cshr")]
        public int CashierId { get; set; }

        [JsonProperty("sale")]
        public SaleDetails Sale { get; set; }

        [JsonProperty("recon")]
        public SaleDetails Recon { get; set; }

        [JsonProperty("card")]
        public CardDetails Card { get; set; }

        [JsonProperty("icc")]
        public IccDetails Icc { get; set; }

        [JsonProperty("trx_type")]
        public string TrxType { get; set; }

        [JsonProperty("voided")]
        public bool Voided { get; set; }

        [JsonProperty("dcc")]
        public string Dcc { get; set; }

        [JsonProperty("trans")]
        public object TransactionDetails { get; set; } // Used in QUERY response to return original response
    }

    /// <summary>
    /// Holds the stored history details of a transaction for QUERY and Refund/VOID/RETURN lookups.
    /// </summary>
    public class SimulatedTransaction
    {
        public string Type { get; set; }
        public int SequenceNumber { get; set; }
        public int Stan { get; set; }
        public int Amount { get; set; }
        public int Currency { get; set; }
        public string ReceiptNumber { get; set; }
        public string AuthCode { get; set; }
        public string DateString { get; set; }
        public bool Voided { get; set; }
        public bool IsQrPay { get; set; }
        public CardDetails Card { get; set; }
        public IccDetails Icc { get; set; }
        public PosResponse ResponseObject { get; set; }

        public string AmountFormatted => $"{(Amount / 100.0):F2} {(Currency == 376 ? "ILS" : "USD")}";
        public string StatusFormatted => Voided ? "VOIDED" : (ResponseObject?.ResponseCode == 0 ? "APPROVED" : "DECLINED");
    }

    public static class TransactionHistory
    {
        public static readonly System.Collections.Generic.List<SimulatedTransaction> Transactions = 
            new System.Collections.Generic.List<SimulatedTransaction>();

        private static int _seqCounter = 1;
        private static int _stanCounter = 1;

        public static int GetNextSequenceNumber() => _seqCounter++;
        public static int GetNextStan() => _stanCounter++;

        public static void Add(SimulatedTransaction tx)
        {
            Transactions.Add(tx);
        }

        public static SimulatedTransaction FindBySeq(int seq)
        {
            return Transactions.Find(t => t.SequenceNumber == seq);
        }
    }
}
