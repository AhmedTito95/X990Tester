using Newtonsoft.Json;
using System.Security.Cryptography;

namespace X990TesterCore
{
    public partial class MainForm : Form
    {
        private bool _isInitialized = false;
        private bool _isTransactionStarted = false;

        public MainForm()
        {
            InitializeComponent();
            txtIp.Text = AppConfig.DefaultIpAddress;
            txtPort.Text = AppConfig.DefaultPort.ToString();

            // Attempt to restore keys from storage so INIT is not required every launch
            TryLoadPersistedKeys();
        }

        private void TryLoadPersistedKeys()
        {
            try
            {
                // Load PC key pair from Windows key store (creates it if missing)
                AppSession.PcRsa = KeyStorageService.LoadOrCreatePcKey();

                // Load terminal public key from file if it was saved after a previous INIT
                var terminalRsa = KeyStorageService.LoadTerminalPublicKey();
                if (terminalRsa != null)
                {
                    AppSession.TerminalRsa = terminalRsa;
                    _isInitialized = true;
                    Log("Keys loaded from storage. Ready for transactions.");
                }
                else
                {
                    Log("PC key loaded. Run INIT to exchange terminal key.");
                }
            }
            catch (Exception ex)
            {
                Log($"Key load warning: {ex.Message}");
            }
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                AppSession.TcpClient = new TcpClientService(txtIp.Text, AppConfig.DefaultPort);
                lblStatus.Text = "Connecting";
                await AppSession.TcpClient.TestConnectionAsync();
                AppConfig.SaveTerminalIP(txtIp.Text);

                lblStatus.Text = "Connected";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Disconnected";
                Log($"Error: {ex.Message}");
            }
        }

        private async void btnInit_Click(object sender, EventArgs e)
        {
            // Allow re-INIT even if already initialized (terminal might have restarted)
            if (_isTransactionStarted)
            {
                MessageBox.Show("Another Transaction Already Started");
                return;
            }

            Log($"Init Transaction Started.");
            _isInitialized = false;
            _isTransactionStarted = true;
            try
            {
                var response = await TransactionService.InitAsync();

                if (response?.ResponseCode == 0 && !string.IsNullOrEmpty(response.TerminalRsaPubKey))
                {
                    // Load the terminal key that InitAsync saved to disk
                    AppSession.TerminalRsa = KeyStorageService.LoadTerminalPublicKey();

                    _isInitialized = true;
                    Log($"INIT Success. Keys Saved.");
                }
                else
                {
                    Log($"INIT Failed: {response?.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
            _isTransactionStarted = false;
        }

        private async void btnSale_Click(object sender, EventArgs e)
        {
            if (!_isInitialized)
            {
                MessageBox.Show("Please run INIT first.");
                return;
            }

            if (_isTransactionStarted)
            {
                MessageBox.Show("Another Transaction Alrady Started");
                return;
            }

            Log($"Sale Transaction Started.");
            _isTransactionStarted = true;
            try
            {
                float.TryParse(txtSaleAmt.Text, out float amt);

                var response = await TransactionService.SaleAsync(
                    amount: (int)(amt * 100),
                    currency: AppConfig.DefaultCurrency,
                    receipt: "ABC-123",
                    print: chkPrint.Checked ? 1 : 0
                );

                if (response.ResponseCode == 0)
                    Log($"Transaction APPROVED.");
                else
                    Log("Transaction DECLINED: " + response.ErrorMessage);
            }
            catch (Exception ex)
            {
                Log($"Transaction Error: {ex.Message}");
            }
            _isTransactionStarted = false;
        }

        private async void btnRefund_Click(object sender, EventArgs e)
        {
            if (!_isInitialized)
            {
                MessageBox.Show("Please run INIT first.");
                return;
            }

            if (_isTransactionStarted)
            {
                MessageBox.Show("Another Transaction Alrady Started");
                return;
            }

            Log($"Refund Transaction Started.");
            _isTransactionStarted = true;
            try
            {
                float.TryParse(txtRefundAmt.Text, out float amt);
                int.TryParse(txtSeqNum.Text, out int seqNum);

                var response = await TransactionService.RefundAsync(
                    amount: (int)(amt * 100),
                    currency: AppConfig.DefaultCurrency,
                    receipt: "ABC-123",
                    seqNumber: seqNum,
                    authCode: txtAuthCode.Text,
                    orgDate: txtDate.Text
                );

                if (response.ResponseCode == 0)
                    Log($"Transaction APPROVED.");
                else
                    Log("Transaction DECLINED: " + response.ErrorMessage);
            }
            catch (Exception ex)
            {
                Log($"Transaction Error: {ex.Message}");
            }
            _isTransactionStarted = false;
        }

        private void Log(string msg)
        {
            txtLog.AppendText($"{DateTime.Now}: {msg}{Environment.NewLine}");
            FileLoggingService.Log("result", msg);
        }
    }
}
