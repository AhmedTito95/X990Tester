namespace X990TesterCore
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnInit;
        private System.Windows.Forms.TextBox txtSaleAmt;
        private System.Windows.Forms.Button btnSale;
        private System.Windows.Forms.TextBox txtRefundAmt;
        private System.Windows.Forms.TextBox txtSeqNum;
        private System.Windows.Forms.TextBox txtAuthCode;
        private System.Windows.Forms.TextBox txtDate;
        private System.Windows.Forms.Button btnRefund;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.CheckBox chkPrint;
        private System.Windows.Forms.Label lblprint;
        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 550);
            this.Text = "X990 Transaction Tester";

            txtIp = new TextBox() { Left = 10, Top = 10, Width = 150 };
            txtPort = new TextBox() { Left = 170, Top = 10, Width = 70 };
            btnConnect = new Button() { Left = 250, Top = 8, Width = 100, Text = "Connect" };
            lblStatus = new Label() { Left = 360, Top = 12, Width = 200, Text = "Disconnected" };
            btnInit = new Button() { Left = 10, Top = 40, Width = 120, Text = "INIT" };
            txtSaleAmt = new TextBox() { Left = 10, Top = 70, Width = 150 };
            btnSale = new Button() { Left = 170, Top = 70, Width = 120, Text = "SALE" };
            txtRefundAmt = new TextBox() { Left = 10, Top = 100, Width = 100 };
            txtSeqNum = new TextBox() { Left = 120, Top = 100, Width = 100 };
            txtAuthCode = new TextBox() { Left = 230, Top = 100, Width = 100 };
            txtDate = new TextBox() { Left = 340, Top = 100, Width = 100 };
            btnRefund = new Button() { Left = 450, Top = 100, Width = 120, Text = "REFUND" };
            chkPrint = new CheckBox() { Left = 10, Top = 130, Text = "Print" };
            txtLog = new TextBox() { Left = 10, Top = 170, Width = 550, Height = 350, Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true };

            btnConnect.Click += btnConnect_Click;
            btnInit.Click += btnInit_Click;
            btnSale.Click += btnSale_Click;
            btnRefund.Click += btnRefund_Click;

            Controls.AddRange(new Control[] { txtIp, txtPort, btnConnect, btnInit, txtSaleAmt, btnSale,
                txtRefundAmt, txtSeqNum, txtAuthCode,txtDate, btnRefund, chkPrint, txtLog, lblStatus });

        }

        #endregion
    }
}
