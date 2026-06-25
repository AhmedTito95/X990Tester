namespace X990TerminalSimulator
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.grpHistory = new System.Windows.Forms.GroupBox();
            this.dgvHistory = new System.Windows.Forms.DataGridView();
            this.colSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSTAN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAuth = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpLogs = new System.Windows.Forms.GroupBox();
            this.txtLogs = new System.Windows.Forms.TextBox();
            this.btnClearLogs = new System.Windows.Forms.Button();
            this.grpServer = new System.Windows.Forms.GroupBox();
            this.lblServerStatus = new System.Windows.Forms.Label();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.pnlCenter = new System.Windows.Forms.Panel();
            this.pnlTerminalBody = new System.Windows.Forms.Panel();
            this.pnlPinpad = new System.Windows.Forms.Panel();
            this.btnTimeout = new System.Windows.Forms.Button();
            this.btnDecline = new System.Windows.Forms.Button();
            this.btnScanQR = new System.Windows.Forms.Button();
            this.btnInsertMaster = new System.Windows.Forms.Button();
            this.btnTapVisa = new System.Windows.Forms.Button();
            this.lblPinpadTitle = new System.Windows.Forms.Label();
            this.pnlScreen = new System.Windows.Forms.Panel();
            this.picScreenQR = new System.Windows.Forms.PictureBox();
            this.lblScreenAmount = new System.Windows.Forms.Label();
            this.lblScreenInstruction = new System.Windows.Forms.Label();
            this.lblScreenTitle = new System.Windows.Forms.Label();
            this.lblMockStatus = new System.Windows.Forms.Label();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.grpPrinter = new System.Windows.Forms.GroupBox();
            this.txtReceipt = new System.Windows.Forms.TextBox();
            this.pnlPrintButtons = new System.Windows.Forms.Panel();
            this.btnReprintCopy = new System.Windows.Forms.Button();
            this.pnlLeft.SuspendLayout();
            this.grpHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).BeginInit();
            this.grpLogs.SuspendLayout();
            this.grpServer.SuspendLayout();
            this.pnlCenter.SuspendLayout();
            this.pnlTerminalBody.SuspendLayout();
            this.pnlPinpad.SuspendLayout();
            this.pnlScreen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picScreenQR)).BeginInit();
            this.pnlRight.SuspendLayout();
            this.grpPrinter.SuspendLayout();
            this.pnlPrintButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(27)))));
            this.pnlLeft.Controls.Add(this.grpHistory);
            this.pnlLeft.Controls.Add(this.grpLogs);
            this.pnlLeft.Controls.Add(this.grpServer);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(12);
            this.pnlLeft.Size = new System.Drawing.Size(430, 711);
            this.pnlLeft.TabIndex = 0;
            // 
            // grpHistory
            // 
            this.grpHistory.Controls.Add(this.dgvHistory);
            this.grpHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpHistory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpHistory.ForeColor = System.Drawing.Color.White;
            this.grpHistory.Location = new System.Drawing.Point(12, 450);
            this.grpHistory.Name = "grpHistory";
            this.grpHistory.Padding = new System.Windows.Forms.Padding(8);
            this.grpHistory.Size = new System.Drawing.Size(406, 249);
            this.grpHistory.TabIndex = 2;
            this.grpHistory.TabStop = false;
            this.grpHistory.Text = "TRANSACTION HISTORY";
            // 
            // dgvHistory
            // 
            this.dgvHistory.AllowUserToAddRows = false;
            this.dgvHistory.AllowUserToDeleteRows = false;
            this.dgvHistory.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.dgvHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvHistory.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHistory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSeq,
            this.colSTAN,
            this.colType,
            this.colAmount,
            this.colAuth,
            this.colStatus});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvHistory.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvHistory.EnableHeadersVisualStyles = false;
            this.dgvHistory.Location = new System.Drawing.Point(8, 24);
            this.dgvHistory.Name = "dgvHistory";
            this.dgvHistory.ReadOnly = true;
            this.dgvHistory.RowHeadersVisible = false;
            this.dgvHistory.RowTemplate.Height = 25;
            this.dgvHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHistory.Size = new System.Drawing.Size(390, 217);
            this.dgvHistory.TabIndex = 0;
            this.dgvHistory.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvHistory_CellClick);
            // 
            // colSeq
            // 
            this.colSeq.DataPropertyName = "SequenceNumber";
            this.colSeq.HeaderText = "Seq";
            this.colSeq.Name = "colSeq";
            this.colSeq.ReadOnly = true;
            this.colSeq.Width = 40;
            // 
            // colSTAN
            // 
            this.colSTAN.DataPropertyName = "Stan";
            this.colSTAN.HeaderText = "STAN";
            this.colSTAN.Name = "colSTAN";
            this.colSTAN.ReadOnly = true;
            this.colSTAN.Width = 50;
            // 
            // colType
            // 
            this.colType.DataPropertyName = "Type";
            this.colType.HeaderText = "Type";
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            this.colType.Width = 60;
            // 
            // colAmount
            // 
            this.colAmount.DataPropertyName = "AmountFormatted";
            this.colAmount.HeaderText = "Amount";
            this.colAmount.Name = "colAmount";
            this.colAmount.ReadOnly = true;
            this.colAmount.Width = 90;
            // 
            // colAuth
            // 
            this.colAuth.DataPropertyName = "AuthCode";
            this.colAuth.HeaderText = "Auth";
            this.colAuth.Name = "colAuth";
            this.colAuth.ReadOnly = true;
            this.colAuth.Width = 60;
            // 
            // colStatus
            // 
            this.colStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStatus.DataPropertyName = "StatusFormatted";
            this.colStatus.HeaderText = "Status";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            // 
            // grpLogs
            // 
            this.grpLogs.Controls.Add(this.txtLogs);
            this.grpLogs.Controls.Add(this.btnClearLogs);
            this.grpLogs.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpLogs.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpLogs.ForeColor = System.Drawing.Color.White;
            this.grpLogs.Location = new System.Drawing.Point(12, 112);
            this.grpLogs.Name = "grpLogs";
            this.grpLogs.Padding = new System.Windows.Forms.Padding(8);
            this.grpLogs.Size = new System.Drawing.Size(406, 338);
            this.grpLogs.TabIndex = 1;
            this.grpLogs.TabStop = false;
            this.grpLogs.Text = "COMMUNICATION LOGS";
            // 
            // txtLogs
            // 
            this.txtLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(9)))), ((int)(((byte)(11)))));
            this.txtLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLogs.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.txtLogs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
            this.txtLogs.Location = new System.Drawing.Point(8, 24);
            this.txtLogs.Multiline = true;
            this.txtLogs.Name = "txtLogs";
            this.txtLogs.ReadOnly = true;
            this.txtLogs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLogs.Size = new System.Drawing.Size(390, 276);
            this.txtLogs.TabIndex = 0;
            // 
            // btnClearLogs
            // 
            this.btnClearLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(39)))), ((int)(((byte)(42)))));
            this.btnClearLogs.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnClearLogs.FlatAppearance.BorderSize = 0;
            this.btnClearLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearLogs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClearLogs.ForeColor = System.Drawing.Color.White;
            this.btnClearLogs.Location = new System.Drawing.Point(8, 300);
            this.btnClearLogs.Name = "btnClearLogs";
            this.btnClearLogs.Size = new System.Drawing.Size(390, 30);
            this.btnClearLogs.TabIndex = 1;
            this.btnClearLogs.Text = "Clear Log Panel";
            this.btnClearLogs.UseVisualStyleBackColor = false;
            this.btnClearLogs.Click += new System.EventHandler(this.btnClearLogs_Click);
            // 
            // grpServer
            // 
            this.grpServer.Controls.Add(this.lblServerStatus);
            this.grpServer.Controls.Add(this.btnStartStop);
            this.grpServer.Controls.Add(this.txtPort);
            this.grpServer.Controls.Add(this.lblPort);
            this.grpServer.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpServer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpServer.ForeColor = System.Drawing.Color.White;
            this.grpServer.Location = new System.Drawing.Point(12, 12);
            this.grpServer.Name = "grpServer";
            this.grpServer.Size = new System.Drawing.Size(406, 100);
            this.grpServer.TabIndex = 0;
            this.grpServer.TabStop = false;
            this.grpServer.Text = "TCP SERVER CONTROL";
            // 
            // lblServerStatus
            // 
            this.lblServerStatus.AutoSize = true;
            this.lblServerStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.lblServerStatus.Location = new System.Drawing.Point(15, 67);
            this.lblServerStatus.Name = "lblServerStatus";
            this.lblServerStatus.Size = new System.Drawing.Size(91, 15);
            this.lblServerStatus.TabIndex = 3;
            this.lblServerStatus.Text = "Status: Stopped";
            // 
            // btnStartStop
            // 
            this.btnStartStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnStartStop.FlatAppearance.BorderSize = 0;
            this.btnStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartStop.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnStartStop.ForeColor = System.Drawing.Color.White;
            this.btnStartStop.Location = new System.Drawing.Point(235, 23);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(155, 34);
            this.btnStartStop.TabIndex = 2;
            this.btnStartStop.Text = "START SERVER";
            this.btnStartStop.UseVisualStyleBackColor = false;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // txtPort
            // 
            this.txtPort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(39)))), ((int)(((byte)(42)))));
            this.txtPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPort.ForeColor = System.Drawing.Color.White;
            this.txtPort.Location = new System.Drawing.Point(85, 28);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(100, 23);
            this.txtPort.TabIndex = 1;
            this.txtPort.Text = "7800";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPort.Location = new System.Drawing.Point(15, 32);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(55, 15);
            this.lblPort.TabIndex = 0;
            this.lblPort.Text = "TCP Port:";
            // 
            // pnlCenter
            // 
            this.pnlCenter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(39)))), ((int)(((byte)(42)))));
            this.pnlCenter.Controls.Add(this.pnlTerminalBody);
            this.pnlCenter.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlCenter.Location = new System.Drawing.Point(430, 0);
            this.pnlCenter.Name = "pnlCenter";
            this.pnlCenter.Size = new System.Drawing.Size(370, 711);
            this.pnlCenter.TabIndex = 1;
            // 
            // pnlTerminalBody
            // 
            this.pnlTerminalBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.pnlTerminalBody.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTerminalBody.Controls.Add(this.pnlPinpad);
            this.pnlTerminalBody.Controls.Add(this.pnlScreen);
            this.pnlTerminalBody.Controls.Add(this.lblMockStatus);
            this.pnlTerminalBody.Location = new System.Drawing.Point(15, 12);
            this.pnlTerminalBody.Name = "pnlTerminalBody";
            this.pnlTerminalBody.Size = new System.Drawing.Size(340, 687);
            this.pnlTerminalBody.TabIndex = 0;
            // 
            // pnlPinpad
            // 
            this.pnlPinpad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(27)))));
            this.pnlPinpad.Controls.Add(this.btnTimeout);
            this.pnlPinpad.Controls.Add(this.btnDecline);
            this.pnlPinpad.Controls.Add(this.btnScanQR);
            this.pnlPinpad.Controls.Add(this.btnInsertMaster);
            this.pnlPinpad.Controls.Add(this.btnTapVisa);
            this.pnlPinpad.Controls.Add(this.lblPinpadTitle);
            this.pnlPinpad.Location = new System.Drawing.Point(20, 290);
            this.pnlPinpad.Name = "pnlPinpad";
            this.pnlPinpad.Padding = new System.Windows.Forms.Padding(12);
            this.pnlPinpad.Size = new System.Drawing.Size(300, 375);
            this.pnlPinpad.TabIndex = 2;
            // 
            // btnTimeout
            // 
            this.btnTimeout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.btnTimeout.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTimeout.Enabled = false;
            this.btnTimeout.FlatAppearance.BorderSize = 0;
            this.btnTimeout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTimeout.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnTimeout.ForeColor = System.Drawing.Color.White;
            this.btnTimeout.Location = new System.Drawing.Point(12, 287);
            this.btnTimeout.Name = "btnTimeout";
            this.btnTimeout.Size = new System.Drawing.Size(276, 52);
            this.btnTimeout.TabIndex = 5;
            this.btnTimeout.Text = "TIMEOUT / CANCEL TRANSACTION";
            this.btnTimeout.UseVisualStyleBackColor = false;
            this.btnTimeout.Click += new System.EventHandler(this.btnTimeout_Click);
            // 
            // btnDecline
            // 
            this.btnDecline.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.btnDecline.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnDecline.Enabled = false;
            this.btnDecline.FlatAppearance.BorderSize = 0;
            this.btnDecline.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDecline.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDecline.ForeColor = System.Drawing.Color.White;
            this.btnDecline.Location = new System.Drawing.Point(12, 225);
            this.btnDecline.Name = "btnDecline";
            this.btnDecline.Size = new System.Drawing.Size(276, 62);
            this.btnDecline.TabIndex = 4;
            this.btnDecline.Text = "DECLINE PAYMENT (Code 912)";
            this.btnDecline.UseVisualStyleBackColor = false;
            this.btnDecline.Click += new System.EventHandler(this.btnDecline_Click);
            // 
            // btnScanQR
            // 
            this.btnScanQR.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(150)))), ((int)(((byte)(105)))));
            this.btnScanQR.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnScanQR.Enabled = false;
            this.btnScanQR.FlatAppearance.BorderSize = 0;
            this.btnScanQR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScanQR.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnScanQR.ForeColor = System.Drawing.Color.White;
            this.btnScanQR.Location = new System.Drawing.Point(12, 163);
            this.btnScanQR.Name = "btnScanQR";
            this.btnScanQR.Size = new System.Drawing.Size(276, 62);
            this.btnScanQR.TabIndex = 3;
            this.btnScanQR.Text = "SIMULATE QR PAY SCAN (PalPay)";
            this.btnScanQR.UseVisualStyleBackColor = false;
            this.btnScanQR.Click += new System.EventHandler(this.btnScanQR_Click);
            // 
            // btnInsertMaster
            // 
            this.btnInsertMaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(119)))), ((int)(((byte)(6)))));
            this.btnInsertMaster.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnInsertMaster.Enabled = false;
            this.btnInsertMaster.FlatAppearance.BorderSize = 0;
            this.btnInsertMaster.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInsertMaster.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnInsertMaster.ForeColor = System.Drawing.Color.White;
            this.btnInsertMaster.Location = new System.Drawing.Point(12, 101);
            this.btnInsertMaster.Name = "btnInsertMaster";
            this.btnInsertMaster.Size = new System.Drawing.Size(276, 62);
            this.btnInsertMaster.TabIndex = 2;
            this.btnInsertMaster.Text = "INSERT MASTERCARD (Chip ICC)";
            this.btnInsertMaster.UseVisualStyleBackColor = false;
            this.btnInsertMaster.Click += new System.EventHandler(this.btnInsertMaster_Click);
            // 
            // btnTapVisa
            // 
            this.btnTapVisa.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnTapVisa.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTapVisa.Enabled = false;
            this.btnTapVisa.FlatAppearance.BorderSize = 0;
            this.btnTapVisa.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTapVisa.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnTapVisa.ForeColor = System.Drawing.Color.White;
            this.btnTapVisa.Location = new System.Drawing.Point(12, 39);
            this.btnTapVisa.Name = "btnTapVisa";
            this.btnTapVisa.Size = new System.Drawing.Size(276, 62);
            this.btnTapVisa.TabIndex = 1;
            this.btnTapVisa.Text = "TAP VISA CARD (NFC CTLS)";
            this.btnTapVisa.UseVisualStyleBackColor = false;
            this.btnTapVisa.Click += new System.EventHandler(this.btnTapVisa_Click);
            // 
            // lblPinpadTitle
            // 
            this.lblPinpadTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPinpadTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblPinpadTitle.Location = new System.Drawing.Point(12, 12);
            this.lblPinpadTitle.Name = "lblPinpadTitle";
            this.lblPinpadTitle.Size = new System.Drawing.Size(276, 27);
            this.lblPinpadTitle.TabIndex = 0;
            this.lblPinpadTitle.Text = "SIMULATED CUSTOMER INPUTS";
            this.lblPinpadTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlScreen
            // 
            this.pnlScreen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(9)))), ((int)(((byte)(11)))));
            this.pnlScreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlScreen.Controls.Add(this.picScreenQR);
            this.pnlScreen.Controls.Add(this.lblScreenAmount);
            this.pnlScreen.Controls.Add(this.lblScreenInstruction);
            this.pnlScreen.Controls.Add(this.lblScreenTitle);
            this.pnlScreen.Location = new System.Drawing.Point(20, 30);
            this.pnlScreen.Name = "pnlScreen";
            this.pnlScreen.Size = new System.Drawing.Size(300, 240);
            this.pnlScreen.TabIndex = 1;
            // 
            // picScreenQR
            // 
            this.picScreenQR.BackColor = System.Drawing.Color.White;
            this.picScreenQR.Location = new System.Drawing.Point(100, 65);
            this.picScreenQR.Name = "picScreenQR";
            this.picScreenQR.Size = new System.Drawing.Size(100, 100);
            this.picScreenQR.TabIndex = 3;
            this.picScreenQR.TabStop = false;
            this.picScreenQR.Visible = false;
            this.picScreenQR.Paint += new System.Windows.Forms.PaintEventHandler(this.picScreenQR_Paint);
            // 
            // lblScreenAmount
            // 
            this.lblScreenAmount.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblScreenAmount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(204)))), ((int)(((byte)(21)))));
            this.lblScreenAmount.Location = new System.Drawing.Point(3, 171);
            this.lblScreenAmount.Name = "lblScreenAmount";
            this.lblScreenAmount.Size = new System.Drawing.Size(292, 49);
            this.lblScreenAmount.TabIndex = 2;
            this.lblScreenAmount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblScreenInstruction
            // 
            this.lblScreenInstruction.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblScreenInstruction.ForeColor = System.Drawing.Color.White;
            this.lblScreenInstruction.Location = new System.Drawing.Point(3, 40);
            this.lblScreenInstruction.Name = "lblScreenInstruction";
            this.lblScreenInstruction.Size = new System.Drawing.Size(292, 114);
            this.lblScreenInstruction.TabIndex = 1;
            this.lblScreenInstruction.Text = "READY FOR SALE\r\nSTART SERVER";
            this.lblScreenInstruction.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblScreenTitle
            // 
            this.lblScreenTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblScreenTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(161)))), ((int)(((byte)(161)))), ((int)(((byte)(170)))));
            this.lblScreenTitle.Location = new System.Drawing.Point(0, 0);
            this.lblScreenTitle.Name = "lblScreenTitle";
            this.lblScreenTitle.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblScreenTitle.Size = new System.Drawing.Size(298, 23);
            this.lblScreenTitle.TabIndex = 0;
            this.lblScreenTitle.Text = "VERIFONE X990 SIMULATOR";
            this.lblScreenTitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblMockStatus
            // 
            this.lblMockStatus.AutoSize = true;
            this.lblMockStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblMockStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.lblMockStatus.Location = new System.Drawing.Point(20, 10);
            this.lblMockStatus.Name = "lblMockStatus";
            this.lblMockStatus.Size = new System.Drawing.Size(186, 13);
            this.lblMockStatus.TabIndex = 0;
            this.lblMockStatus.Text = "WiFi [OK]   Battery 100%   Port 7800";
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(27)))));
            this.pnlRight.Controls.Add(this.grpPrinter);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(800, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Padding = new System.Windows.Forms.Padding(12);
            this.pnlRight.Size = new System.Drawing.Size(370, 711);
            this.pnlRight.TabIndex = 2;
            // 
            // grpPrinter
            // 
            this.grpPrinter.Controls.Add(this.txtReceipt);
            this.grpPrinter.Controls.Add(this.pnlPrintButtons);
            this.grpPrinter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpPrinter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpPrinter.ForeColor = System.Drawing.Color.White;
            this.grpPrinter.Location = new System.Drawing.Point(12, 12);
            this.grpPrinter.Name = "grpPrinter";
            this.grpPrinter.Padding = new System.Windows.Forms.Padding(8);
            this.grpPrinter.Size = new System.Drawing.Size(346, 687);
            this.grpPrinter.TabIndex = 0;
            this.grpPrinter.TabStop = false;
            this.grpPrinter.Text = "VIRTUAL RECEIPT PRINTER";
            // 
            // txtReceipt
            // 
            this.txtReceipt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(249)))));
            this.txtReceipt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtReceipt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtReceipt.Font = new System.Drawing.Font("Consolas", 9.25F);
            this.txtReceipt.ForeColor = System.Drawing.Color.Black;
            this.txtReceipt.Location = new System.Drawing.Point(8, 24);
            this.txtReceipt.Multiline = true;
            this.txtReceipt.Name = "txtReceipt";
            this.txtReceipt.ReadOnly = true;
            this.txtReceipt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtReceipt.Size = new System.Drawing.Size(330, 605);
            this.txtReceipt.TabIndex = 0;
            // 
            // pnlPrintButtons
            // 
            this.pnlPrintButtons.Controls.Add(this.btnReprintCopy);
            this.pnlPrintButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlPrintButtons.Location = new System.Drawing.Point(8, 629);
            this.pnlPrintButtons.Name = "pnlPrintButtons";
            this.pnlPrintButtons.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.pnlPrintButtons.Size = new System.Drawing.Size(330, 50);
            this.pnlPrintButtons.TabIndex = 1;
            // 
            // btnReprintCopy
            // 
            this.btnReprintCopy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(39)))), ((int)(((byte)(42)))));
            this.btnReprintCopy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReprintCopy.Enabled = false;
            this.btnReprintCopy.FlatAppearance.BorderSize = 0;
            this.btnReprintCopy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReprintCopy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnReprintCopy.ForeColor = System.Drawing.Color.White;
            this.btnReprintCopy.Location = new System.Drawing.Point(0, 8);
            this.btnReprintCopy.Name = "btnReprintCopy";
            this.btnReprintCopy.Size = new System.Drawing.Size(330, 42);
            this.btnReprintCopy.TabIndex = 0;
            this.btnReprintCopy.Text = "REPRINT LAST SLIP (COPY)";
            this.btnReprintCopy.UseVisualStyleBackColor = false;
            this.btnReprintCopy.Click += new System.EventHandler(this.btnReprintCopy_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(9)))), ((int)(((byte)(11)))));
            this.ClientSize = new System.Drawing.Size(1170, 711);
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.pnlCenter);
            this.Controls.Add(this.pnlLeft);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "X990 POS Terminal Simulator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.pnlLeft.ResumeLayout(false);
            this.grpHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).EndInit();
            this.grpLogs.ResumeLayout(false);
            this.grpLogs.PerformLayout();
            this.grpServer.ResumeLayout(false);
            this.grpServer.PerformLayout();
            this.pnlCenter.ResumeLayout(false);
            this.pnlTerminalBody.ResumeLayout(false);
            this.pnlTerminalBody.PerformLayout();
            this.pnlPinpad.ResumeLayout(false);
            this.pnlScreen.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picScreenQR)).EndInit();
            this.pnlRight.ResumeLayout(false);
            this.grpPrinter.ResumeLayout(false);
            this.grpPrinter.PerformLayout();
            this.pnlPrintButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.GroupBox grpServer;
        private System.Windows.Forms.Label lblServerStatus;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.GroupBox grpLogs;
        private System.Windows.Forms.TextBox txtLogs;
        private System.Windows.Forms.Button btnClearLogs;
        private System.Windows.Forms.GroupBox grpHistory;
        private System.Windows.Forms.DataGridView dgvHistory;
        private System.Windows.Forms.Panel pnlCenter;
        private System.Windows.Forms.Panel pnlTerminalBody;
        private System.Windows.Forms.Label lblMockStatus;
        private System.Windows.Forms.Panel pnlScreen;
        private System.Windows.Forms.Label lblScreenAmount;
        private System.Windows.Forms.Label lblScreenInstruction;
        private System.Windows.Forms.Label lblScreenTitle;
        private System.Windows.Forms.Panel pnlPinpad;
        private System.Windows.Forms.Label lblPinpadTitle;
        private System.Windows.Forms.Button btnTimeout;
        private System.Windows.Forms.Button btnDecline;
        private System.Windows.Forms.Button btnScanQR;
        private System.Windows.Forms.Button btnInsertMaster;
        private System.Windows.Forms.Button btnTapVisa;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.GroupBox grpPrinter;
        private System.Windows.Forms.TextBox txtReceipt;
        private System.Windows.Forms.Panel pnlPrintButtons;
        private System.Windows.Forms.Button btnReprintCopy;
        private System.Windows.Forms.PictureBox picScreenQR;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSTAN;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAmount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAuth;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
    }
}
