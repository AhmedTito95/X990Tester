using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace X990TerminalSimulator
{
    public partial class MainForm : Form
    {
        private TcpServerService _server;
        private TransactionContext _activeContext;
        private SimulatedTransaction _lastTransaction;
        private BindingSource _bindingSource;

        public MainForm()
        {
            InitializeComponent();

            _bindingSource = new BindingSource();
            _bindingSource.DataSource = TransactionHistory.Transactions;
            dgvHistory.DataSource = _bindingSource;

            // Generate initial Terminal keys quietly if needed
            Task.Run(() => {
                try { KeyStorageService.LoadOrCreateTerminalKey(); } catch { }
            });
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (_server == null)
            {
                if (!int.TryParse(txtPort.Text, out int port) || port <= 0 || port > 65535)
                {
                    MessageBox.Show("Please enter a valid port number (1-65535).", "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    _server = new TcpServerService(port);
                    _server.OnLogReceived += Server_OnLogReceived;
                    _server.OnLogSent += Server_OnLogSent;
                    _server.OnStatusChanged += Server_OnStatusChanged;
                    _server.OnTransactionRequest += Server_OnTransactionRequest;

                    _server.Start();

                    btnStartStop.Text = "STOP SERVER";
                    btnStartStop.BackColor = Color.FromArgb(239, 68, 68); // Red
                    txtPort.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not start TCP server: {ex.Message}", "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _server = null;
                }
            }
            else
            {
                _server.Stop();
                _server = null;

                btnStartStop.Text = "START SERVER";
                btnStartStop.BackColor = Color.FromArgb(16, 185, 129); // Green
                txtPort.Enabled = true;
            }
        }

        private void Server_OnStatusChanged(object sender, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Server_OnStatusChanged(sender, status)));
                return;
            }

            lblServerStatus.Text = $"Status: {status}";
            if (status.Contains("listening"))
            {
                lblServerStatus.ForeColor = Color.FromArgb(34, 197, 94); // Green
                UpdateScreen("X990 SIMULATOR", "READY FOR SALE\nINSERT/TAP CARD OR QR", "");
                lblMockStatus.Text = $"WiFi [OK]   Battery 100%   Port {txtPort.Text}";
            }
            else
            {
                lblServerStatus.ForeColor = Color.FromArgb(239, 68, 68); // Red
                UpdateScreen("VERIFONE X990", "READY FOR SALE\nSTART SERVER", "");
            }
        }

        private void Server_OnLogReceived(object sender, string log)
        {
            AppendLog($"RECV: {log}");
        }

        private void Server_OnLogSent(object sender, string log)
        {
            AppendLog($"SEND: {log}");
        }

        private void Server_OnTransactionRequest(object sender, TransactionContext context)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Server_OnTransactionRequest(sender, context)));
                return;
            }

            _activeContext = context;
            PlayTone(1200, 100); // Beep to notify transaction arrived

            // Update LCD UI based on transaction type
            if (context.Type == "SALE" || context.Type == "SALE_CB" || context.Type == "LOAN")
            {
                double amtDouble = double.Parse(context.Amount) / 100.0;
                string typeLabel = context.Type == "SALE" ? "SALE" : (context.Type == "SALE_CB" ? "SALE CASHBACK" : "LOAN");
                UpdateScreen(typeLabel, "INSERT/TAP CARD\nOR SCAN QR CODE", $"{amtDouble:F2} {context.Currency}");

                // Enable customer interaction buttons
                btnTapVisa.Enabled = true;
                btnInsertMaster.Enabled = true;
                btnScanQR.Enabled = true;
                btnDecline.Enabled = true;
                btnTimeout.Enabled = true;

                // If QR/Sale, show the mock QR Code visual
                picScreenQR.Visible = true;
                picScreenQR.Invalidate();
            }
            else if (context.Type == "VOID" || context.Type == "RETURN")
            {
                var refundReq = Newtonsoft.Json.JsonConvert.DeserializeObject<RefundRequest>(context.RawRequest);
                double amtDouble = double.Parse(context.Amount) / 100.0;
                UpdateScreen("VOID TRANSACTION", $"APPROVE VOID FOR\nSEQ: {refundReq?.OrgSequenceNumber}?", $"{amtDouble:F2} {context.Currency}");

                // For void/return, card is not required, tap/insert represents approval
                btnTapVisa.Text = "APPROVE VOID";
                btnTapVisa.Enabled = true;
                btnInsertMaster.Enabled = false;
                btnScanQR.Enabled = false;
                btnDecline.Text = "DECLINE VOID";
                btnDecline.Enabled = true;
                btnTimeout.Enabled = true;
                picScreenQR.Visible = false;
            }
        }

        private void btnTapVisa_Click(object sender, EventArgs e)
        {
            if (_activeContext == null) return;

            if (_activeContext.Type == "VOID" || _activeContext.Type == "RETURN")
            {
                ApproveVoidTransaction();
            }
            else
            {
                ApproveSaleTransaction("VISA", "CTLS", "411111******1111", "VALUED CUSTOMER", "PIN Verified");
            }
        }

        private void btnInsertMaster_Click(object sender, EventArgs e)
        {
            if (_activeContext == null) return;
            ApproveSaleTransaction("MASTERCARD", "ICC", "541111******9999", "JOHN DOE", "Signature Verified");
        }

        private void btnScanQR_Click(object sender, EventArgs e)
        {
            if (_activeContext == null) return;
            ApproveQrTransaction();
        }

        private void btnDecline_Click(object sender, EventArgs e)
        {
            if (_activeContext == null) return;

            var response = new PosResponse
            {
                Type = _activeContext.Type,
                ResponseCode = 912,
                ErrorMessage = "Issuer not found\n(code 912)",
                SequenceNumber = TransactionHistory.GetNextSequenceNumber(),
                Stan = TransactionHistory.GetNextStan(),
                TransactionDate = DateTime.Now.ToString("yyMMddHHmmss"),
                Lang = _activeContext.RawRequest.Contains("\"lang\":1") ? 1 : 0,
                ReceiptNumber = _activeContext.ReceiptNumber
            };

            _activeContext.ResponseSource.SetResult(response);
            FinishTransaction(response, false, "DECLINED", Color.FromArgb(132, 32, 41)); // Crimson Red Background
        }

        private void btnTimeout_Click(object sender, EventArgs e)
        {
            if (_activeContext == null) return;

            var response = new PosResponse
            {
                Type = _activeContext.Type,
                ResponseCode = 999,
                ErrorMessage = "TIMEOUT / CANCELLED BY USER",
                SequenceNumber = TransactionHistory.GetNextSequenceNumber(),
                Stan = TransactionHistory.GetNextStan(),
                TransactionDate = DateTime.Now.ToString("yyMMddHHmmss"),
                Lang = _activeContext.RawRequest.Contains("\"lang\":1") ? 1 : 0,
                ReceiptNumber = _activeContext.ReceiptNumber
            };

            _activeContext.ResponseSource.SetResult(response);
            FinishTransaction(response, false, "CANCELLED", Color.FromArgb(75, 85, 99)); // Grey Background
        }

        private void ApproveSaleTransaction(string cardType, string entryMode, string maskedPan, string holder, string verif)
        {
            int amountVal = int.Parse(_activeContext.Amount);
            int currencyCode = _activeContext.Currency == "ILS" ? 376 : 840;

            var response = new PosResponse
            {
                Type = _activeContext.Type,
                ResponseCode = 0,
                ErrorMessage = "",
                AuthCode = GenerateRandomAuthCode(),
                SequenceNumber = TransactionHistory.GetNextSequenceNumber(),
                Stan = TransactionHistory.GetNextStan(),
                TransactionDate = DateTime.Now.ToString("yyMMddHHmmss"),
                IsQrPay = false,
                QrRef = "",
                Lang = _activeContext.RawRequest.Contains("\"lang\":1") ? 1 : 0,
                Print = _activeContext.RawRequest.Contains("\"print\":3") ? 3 : (_activeContext.RawRequest.Contains("\"print\":2") ? 2 : 1),
                ReceiptNumber = _activeContext.ReceiptNumber,
                Sale = new SaleDetails { Amount = amountVal, Currency = currencyCode, CurrencyName = _activeContext.Currency },
                Recon = new SaleDetails { Amount = amountVal, Currency = currencyCode, CurrencyName = _activeContext.Currency },
                Card = new CardDetails
                {
                    Type = cardType,
                    Entry = entryMode,
                    Pan = maskedPan,
                    PanSeq = 1,
                    Holder = holder,
                    Verif = verif
                },
                Icc = entryMode == "ICC" || entryMode == "CTLS" ? new IccDetails
                {
                    Aid = "A0000000031010",
                    Tvr = "0000000000",
                    Tsi = "0000",
                    Cvm = "420300",
                    Cid = "80",
                    TrmType = "22"
                } : null,
                TrxType = _activeContext.Type,
                Voided = false
            };

            SaveAndFinish(response, cardType, entryMode);
        }

        private void ApproveQrTransaction()
        {
            int amountVal = int.Parse(_activeContext.Amount);
            int currencyCode = _activeContext.Currency == "ILS" ? 376 : 840;

            var response = new PosResponse
            {
                Type = _activeContext.Type,
                ResponseCode = 0,
                ErrorMessage = "",
                AuthCode = GenerateRandomAuthCode(),
                SequenceNumber = TransactionHistory.GetNextSequenceNumber(),
                Stan = TransactionHistory.GetNextStan(),
                TransactionDate = DateTime.Now.ToString("yyMMddHHmmss"),
                IsQrPay = true,
                QrRef = GenerateRandomQrRef(),
                Lang = _activeContext.RawRequest.Contains("\"lang\":1") ? 1 : 0,
                Print = _activeContext.RawRequest.Contains("\"print\":3") ? 3 : (_activeContext.RawRequest.Contains("\"print\":2") ? 2 : 1),
                ReceiptNumber = _activeContext.ReceiptNumber,
                Sale = new SaleDetails { Amount = amountVal, Currency = currencyCode, CurrencyName = _activeContext.Currency },
                Recon = new SaleDetails { Amount = amountVal, Currency = currencyCode, CurrencyName = _activeContext.Currency },
                TrxType = _activeContext.Type,
                Voided = false
            };

            SaveAndFinish(response, "QR", "PalPay");
        }

        private void ApproveVoidTransaction()
        {
            var refundReq = Newtonsoft.Json.JsonConvert.DeserializeObject<RefundRequest>(_activeContext.RawRequest);
            var originalTx = TransactionHistory.FindBySeq(refundReq?.OrgSequenceNumber ?? 0);

            if (originalTx == null)
            {
                var failResp = new PosResponse
                {
                    Type = _activeContext.Type,
                    ResponseCode = 912,
                    ErrorMessage = "Original transaction not found",
                    SequenceNumber = TransactionHistory.GetNextSequenceNumber(),
                    Stan = TransactionHistory.GetNextStan(),
                    TransactionDate = DateTime.Now.ToString("yyMMddHHmmss"),
                    ReceiptNumber = _activeContext.ReceiptNumber
                };
                _activeContext.ResponseSource.SetResult(failResp);
                FinishTransaction(failResp, false, "VOID FAILED", Color.FromArgb(132, 32, 41));
                return;
            }

            if (originalTx.Voided)
            {
                var failResp = new PosResponse
                {
                    Type = _activeContext.Type,
                    ResponseCode = 912,
                    ErrorMessage = "Transaction already voided",
                    SequenceNumber = TransactionHistory.GetNextSequenceNumber(),
                    Stan = TransactionHistory.GetNextStan(),
                    TransactionDate = DateTime.Now.ToString("yyMMddHHmmss"),
                    ReceiptNumber = _activeContext.ReceiptNumber
                };
                _activeContext.ResponseSource.SetResult(failResp);
                FinishTransaction(failResp, false, "VOID FAILED", Color.FromArgb(132, 32, 41));
                return;
            }

            // Mark original transaction as voided
            originalTx.Voided = true;
            originalTx.ResponseObject.Voided = true;

            // Formulate response
            var response = new PosResponse
            {
                Type = _activeContext.Type,
                ResponseCode = 0,
                ErrorMessage = "",
                AuthCode = originalTx.AuthCode,
                SequenceNumber = TransactionHistory.GetNextSequenceNumber(),
                Stan = TransactionHistory.GetNextStan(),
                TransactionDate = DateTime.Now.ToString("yyMMddHHmmss"),
                IsQrPay = originalTx.IsQrPay,
                QrRef = originalTx.ResponseObject.QrRef,
                Lang = _activeContext.RawRequest.Contains("\"lang\":1") ? 1 : 0,
                Print = originalTx.ResponseObject.Print,
                ReceiptNumber = _activeContext.ReceiptNumber,
                Sale = originalTx.ResponseObject.Sale,
                Recon = originalTx.ResponseObject.Recon,
                Card = originalTx.Card,
                Icc = originalTx.Icc,
                TrxType = _activeContext.Type,
                Voided = true
            };

            // Set dynamic original trans details wrapper
            response.TransactionDetails = originalTx.ResponseObject;

            SaveAndFinish(response, originalTx.Card?.Type ?? "QR", originalTx.Card?.Entry ?? "QR");
        }

        private void SaveAndFinish(PosResponse response, string cardType, string entryMode)
        {
            var simulatedTx = new SimulatedTransaction
            {
                Type = response.Type,
                SequenceNumber = response.SequenceNumber,
                Stan = response.Stan,
                Amount = response.Sale.Amount,
                Currency = response.Sale.Currency,
                ReceiptNumber = response.ReceiptNumber,
                AuthCode = response.AuthCode,
                DateString = response.TransactionDate,
                Voided = response.Voided,
                IsQrPay = response.IsQrPay,
                Card = response.Card,
                Icc = response.Icc,
                ResponseObject = response
            };

            TransactionHistory.Add(simulatedTx);
            _lastTransaction = simulatedTx;

            _activeContext.ResponseSource.SetResult(response);

            // Print slip
            txtReceipt.Text = FormatReceiptSlip(response, false);
            btnReprintCopy.Enabled = true;

            // Update UI list
            RefreshHistoryGrid();

            FinishTransaction(response, true, "APPROVED", Color.FromArgb(15, 81, 50)); // Emerald Green Background
        }

        private void FinishTransaction(PosResponse response, bool success, string statusText, Color screenColor)
        {
            // Reset customer inputs
            ResetKeyboardButtons();

            picScreenQR.Visible = false;

            // Flash screen color
            pnlScreen.BackColor = screenColor;
            lblScreenInstruction.Text = statusText;
            if (success)
            {
                lblScreenAmount.Text = $"AUTH: {response.AuthCode}";
                Task.Run(() => {
                    Console.Beep(2000, 150);
                    Console.Beep(2500, 200);
                });
            }
            else
            {
                lblScreenAmount.Text = response.ResponseCode == 999 ? "CANCELLED" : "DECLINED";
                Task.Run(() => {
                    Console.Beep(800, 400);
                });
            }

            // Restore screen to ready status after 3 seconds
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 3000;
            timer.Tick += (s, ev) =>
            {
                timer.Stop();
                timer.Dispose();

                pnlScreen.BackColor = Color.FromArgb(9, 9, 11); // Dark Gray
                UpdateScreen("VERIFONE X990 SIMULATOR", "READY FOR SALE\nINSERT/TAP CARD OR QR", "");
            };
            timer.Start();

            _activeContext = null;
        }

        private void ResetKeyboardButtons()
        {
            btnTapVisa.Text = "TAP VISA CARD (NFC CTLS)";
            btnDecline.Text = "DECLINE PAYMENT (Code 912)";

            btnTapVisa.Enabled = false;
            btnInsertMaster.Enabled = false;
            btnScanQR.Enabled = false;
            btnDecline.Enabled = false;
            btnTimeout.Enabled = false;
        }

        private void UpdateScreen(string title, string instruction, string amount = "")
        {
            lblScreenTitle.Text = title;
            lblScreenInstruction.Text = instruction;
            lblScreenAmount.Text = amount;
        }

        private void AppendLog(string message)
        {
            if (txtLogs.InvokeRequired)
            {
                txtLogs.Invoke(new Action(() => AppendLog(message)));
                return;
            }
            txtLogs.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            txtLogs.Clear();
        }

        private void RefreshHistoryGrid()
        {
            _bindingSource.ResetBindings(false);
        }

        private void dgvHistory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < TransactionHistory.Transactions.Count)
            {
                var tx = TransactionHistory.Transactions[e.RowIndex];
                _lastTransaction = tx;
                txtReceipt.Text = FormatReceiptSlip(tx.ResponseObject, tx.Voided);
                btnReprintCopy.Enabled = true;
            }
        }

        private void btnReprintCopy_Click(object sender, EventArgs e)
        {
            if (_lastTransaction != null)
            {
                txtReceipt.Text = FormatReceiptSlip(_lastTransaction.ResponseObject, true);
            }
        }

        private string FormatReceiptSlip(PosResponse resp, bool isCopy = false)
        {
            string title = isCopy ? "****    COPY    ****" : "****  ORIGINAL  ****";
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd           HH:mm:ss");
            if (resp.TransactionDate != null && resp.TransactionDate is string dStr && dStr.Length == 12)
            {
                try
                {
                    string yy = "20" + dStr.Substring(0, 2);
                    string mm = dStr.Substring(2, 2);
                    string dd = dStr.Substring(4, 2);
                    string hh = dStr.Substring(6, 2);
                    string min = dStr.Substring(8, 2);
                    string ss = dStr.Substring(10, 2);
                    dateStr = $"{yy}-{mm}-{dd}           {hh}:{min}:{ss}";
                }
                catch { }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("          BANK OF PALESTINE          ");
            sb.AppendLine("-------------------------------------");
            sb.AppendLine($"Merch Name: VERIFONE SIMULATED merch");
            sb.AppendLine("Location:   BOP Bethlehem            ");
            sb.AppendLine($"POS Code:   {resp.TerminalID}");
            sb.AppendLine($"Outlet No:  {resp.Outlet}");
            sb.AppendLine($"       {title}       ");
            sb.AppendLine("            CUSTOMER SLIP            ");
            sb.AppendLine($"----------------- {resp.Type} -----------------");
            sb.AppendLine($"Date/Time:  {dateStr}");
            if (resp.Card != null)
            {
                sb.AppendLine($"Card #:     {resp.Card.Pan}");
                sb.AppendLine("Exp. Dt:    **/**");
            }
            sb.AppendLine($"Seq. No:    {resp.SequenceNumber:D6}");
            sb.AppendLine($"Auth. Code: {resp.AuthCode}");
            if (resp.Card != null)
            {
                sb.AppendLine($"Card Type:  {resp.Card.Type}");
                sb.AppendLine($"Card Entry: {resp.Card.Entry}");
                sb.AppendLine("AID:        A0000000031010");
            }
            else if (resp.IsQrPay)
            {
                sb.AppendLine("Payment Type: QR CODE PAY");
                sb.AppendLine($"QR Ref Code:  {resp.QrRef}");
            }
            sb.AppendLine("-------------------------------------");
            sb.AppendLine(resp.ResponseCode == 0 ? "              APPROVED" : "              DECLINED");
            double amountVal = 0;
            if (resp.Sale != null)
            {
                amountVal = resp.Sale.Amount / 100.0;
            }
            string curName = resp.Recon?.CurrencyName ?? (resp.Sale?.Currency == 376 ? "ILS" : "USD");
            sb.AppendLine($"            {amountVal:F2} {curName}");
            if (resp.Card != null)
            {
                sb.AppendLine($"Verification: {resp.Card.Verif}");
            }
            sb.AppendLine("TVR: 0000000000   TSI: 0000");
            sb.AppendLine("CID: 80           Terminal Type: 22");
            sb.AppendLine("-------------------------------------");
            sb.AppendLine("              Thank You              ");
            sb.AppendLine("        No RETURN/CANCELLATION       ");
            sb.AppendLine("       Please, Keep the Receipt      ");
            sb.AppendLine($"       {title}       ");
            sb.AppendLine("Pal_PAY 05.00.17R - Mar 22 2023 09:35:43");
            return sb.ToString();
        }

        private string GenerateRandomAuthCode()
        {
            var rand = new Random();
            return rand.Next(100000, 999999).ToString();
        }

        private string GenerateRandomQrRef()
        {
            var rand = new Random();
            return "QR" + rand.Next(10000000, 99999999).ToString();
        }

        private void picScreenQR_Paint(object sender, PaintEventArgs e)
        {
            DrawMockQRCode(e.Graphics, picScreenQR.Width, picScreenQR.Height);
        }

        private void DrawMockQRCode(Graphics g, int width, int height)
        {
            g.Clear(Color.White);
            using (var brush = new SolidBrush(Color.Black))
            {
                int size = 20;
                // Finder Pattern Top-Left
                g.FillRectangle(brush, 4, 4, size, size);
                using (var whiteBrush = new SolidBrush(Color.White)) g.FillRectangle(whiteBrush, 7, 7, size - 6, size - 6);
                g.FillRectangle(brush, 9, 9, size - 10, size - 10);

                // Finder Pattern Top-Right
                g.FillRectangle(brush, width - size - 4, 4, size, size);
                using (var whiteBrush = new SolidBrush(Color.White)) g.FillRectangle(whiteBrush, width - size - 4 + 3, 7, size - 6, size - 6);
                g.FillRectangle(brush, width - size - 4 + 5, 9, size - 10, size - 10);

                // Finder Pattern Bottom-Left
                g.FillRectangle(brush, 4, height - size - 4, size, size);
                using (var whiteBrush = new SolidBrush(Color.White)) g.FillRectangle(whiteBrush, 7, height - size - 4 + 3, size - 6, size - 6);
                g.FillRectangle(brush, 9, height - size - 4 + 5, size - 10, size - 10);

                // Draw mock QR pixels
                var rand = new Random(12345);
                for (int x = 4; x < width - 4; x += 3)
                {
                    for (int y = 4; y < height - 4; y += 3)
                    {
                        if (x < size + 6 && y < size + 6) continue;
                        if (x > width - size - 6 && y < size + 6) continue;
                        if (x < size + 6 && y > height - size - 6) continue;

                        if (rand.Next(0, 2) == 0)
                        {
                            g.FillRectangle(brush, x, y, 2, 2);
                        }
                    }
                }
            }
        }

        private void PlayTone(int freq, int dur)
        {
            Task.Run(() =>
            {
                try { Console.Beep(freq, dur); } catch { }
            });
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _server?.Stop();
        }
    }
}
