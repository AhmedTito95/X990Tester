#include "pch.h"
#include "framework.h"
#include "afxdialogex.h"
#include "X990TesterMFC.h"
#include "X990TesterMFCDlg.h"
#include "FileLogService.h"
#include "TransactionService.h"


BEGIN_MESSAGE_MAP(CX990TesterMFCDlg, CDialogEx)
ON_WM_SYSCOMMAND()
ON_WM_PAINT()
ON_WM_QUERYDRAGICON()
ON_BN_CLICKED(IDC_BTN_CONNECT, &CX990TesterMFCDlg::OnBnClickedBtnConnect)
ON_BN_CLICKED(IDC_BTN_INIT, &CX990TesterMFCDlg::OnBnClickedBtnInit)
ON_BN_CLICKED(IDC_BTN_SALE, &CX990TesterMFCDlg::OnBnClickedBtnSale)
ON_BN_CLICKED(IDC_BTN_REFUND, &CX990TesterMFCDlg::OnBnClickedBtnRefund)
END_MESSAGE_MAP()

CX990TesterMFCDlg::CX990TesterMFCDlg(CWnd *pParent /*=nullptr*/)
    : CDialogEx(IDD_X990TESTERMFC_DIALOG, pParent) {
  m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CX990TesterMFCDlg::DoDataExchange(CDataExchange *pDX) {
  CDialogEx::DoDataExchange(pDX);

  DDX_Control(pDX, IDC_IPADDRESS, m_editIp);
  DDX_Control(pDX, IDC_PORT, m_editPort);
  DDX_Control(pDX, IDC_LOG, m_editLog);
  DDX_Control(pDX, IDC_STATUS, m_lblStatus);
  DDX_Control(pDX, IDC_AMOUNT, m_editAmt);
  DDX_Control(pDX, IDC_SEQ_NUM, m_editSeqNum);
  DDX_Control(pDX, IDC_AUTH_CODE, m_editAuthCode);
  DDX_Control(pDX, IDC_DATE, m_editDate);
  DDX_Control(pDX, IDC_CHK_PRINT, m_chkPrint);
}

BOOL CX990TesterMFCDlg::OnInitDialog() {
  CDialogEx::OnInitDialog();

  SetIcon(m_hIcon, TRUE);  // Set big icon
  SetIcon(m_hIcon, FALSE); // Set small icon

  // Load saved settings or use default
  CString ip = AfxGetApp()->GetProfileString(_T("Settings"), _T("IP"),
                                             _T("192.168.0.199"));
  CString port =
      AfxGetApp()->GetProfileString(_T("Settings"), _T("Port"), _T("7800"));

  m_editIp.SetWindowText(ip);
  m_editPort.SetWindowText(port);

  // Check for existing keys
  if (m_crypto.Initialize()) {
    if (m_crypto.LoadTerminalPublicKey()) {
      m_isInitialized = true;
      SetStatus(_T("Keys Loaded"));
      Log(_T("Ready. Keys loaded from storage."));
    } else {
      Log(_T("Ready. PC Key loaded. Run INIT for Terminal Key."));
    }
  }

  return TRUE;
}

void CX990TesterMFCDlg::OnSysCommand(UINT nID, LPARAM lParam) {
  CDialogEx::OnSysCommand(nID, lParam);
}

void CX990TesterMFCDlg::OnPaint() {
  if (IsIconic()) {
    CPaintDC dc(this);
    SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()),
                0);

    int cxIcon = GetSystemMetrics(SM_CXICON);
    int cyIcon = GetSystemMetrics(SM_CYICON);
    CRect rect;
    GetClientRect(&rect);
    int x = (rect.Width() - cxIcon + 1) / 2;
    int y = (rect.Height() - cyIcon + 1) / 2;

    dc.DrawIcon(x, y, m_hIcon);
  } else {
    CDialogEx::OnPaint();
  }
}

HCURSOR CX990TesterMFCDlg::OnQueryDragIcon() {
  return static_cast<HCURSOR>(m_hIcon);
}

std::string CStrToStr(const CString &cstr) {
  CT2A ascii(cstr);
  return std::string(ascii.m_psz);
}

void CX990TesterMFCDlg::OnBnClickedBtnConnect() {
  CString ip;
  m_editIp.GetWindowText(ip);
  CString port;
  m_editPort.GetWindowText(port);
  int p = _ttoi(port);

  // Save successful input (or just last executed input)
  AfxGetApp()->WriteProfileString(_T("Settings"), _T("IP"), ip);
  AfxGetApp()->WriteProfileString(_T("Settings"), _T("Port"), port);

  Log(_T("Testing Connection..."));
  try {
    if (m_comm.TestConnection(CStrToStr(ip), p)) {
      Log(_T("Connection Available"));
      SetStatus(_T("Connected"));
    } else {
      Log(_T("Connection Error"));
      SetStatus(_T("Disconnected"));
    }
  } catch (const std::exception &) {
    Log(_T("exception Connection Error"));
  }
}

void CX990TesterMFCDlg::OnBnClickedBtnInit() {
  m_isInitialized = false;

  // Initialize Crypto (loads existing keys)
  if (!m_crypto.Initialize()) {
    Log(_T("Crypto Init Failed"));
    return;
  }

  Log(_T("Sending INIT..."));

  CString ip;
  m_editIp.GetWindowText(ip);
  CString portVal;
  m_editPort.GetWindowText(portVal);
  int port = _ttoi(portVal);

  // Call service
  PosResponse resp =
      CTransactionService::Init(m_comm, m_crypto, CStrToStr(ip), port);

  if (resp.ResponseCode == 0) {
    if (m_crypto.SaveTerminalPublicKey(resp.TerminalRsaPubKey)) {
      m_isInitialized = true;
      Log(_T("INIT Success. Keys Saved."));
    } else {
      m_isInitialized = true; // Still initialized in memory
      Log(_T("INIT Success. (Save Failed)"));
    }
  } else {
    CString err(resp.ErrorMessage.c_str());
    Log(_T("INIT Failed: ") + err);
  }
}

void CX990TesterMFCDlg::OnBnClickedBtnSale() {
  if (!m_isInitialized) {
    AfxMessageBox(_T("Run INIT first"));
    return;
  }

  CString amt;
  m_editAmt.GetWindowText(amt);
  double d = _ttof(amt);
  int cents = (int)(d * 100);

  CString ip;
  m_editIp.GetWindowText(ip);
  CString portVal;
  m_editPort.GetWindowText(portVal);
  int port = _ttoi(portVal);

  int print = (m_chkPrint.GetCheck() == BST_CHECKED) ? 1 : 3;

  Log(_T("Sending SALE..."));

  PosResponse resp = CTransactionService::Sale(
      m_comm, m_crypto, CStrToStr(ip), port, cents, 376, "DEMO-123", print, 1);

  if (resp.ResponseCode == 0) {
    Log(_T("Transaction APPROVED"));
    m_editAuthCode.SetWindowText(CString(resp.AuthCode.c_str()));
    m_editSeqNum.SetWindowText(
        CString(std::to_string(resp.SequenceNumber).c_str()));
    m_editDate.SetWindowText(CString(resp.TransactionDate.c_str()));
  } else {
    Log(_T("Transaction DECLINED: ") + CString(resp.ErrorMessage.c_str()));
  }
}

void CX990TesterMFCDlg::OnBnClickedBtnRefund() {
  if (!m_isInitialized) {
    AfxMessageBox(_T("Run INIT first"));
    return;
  }

  CString amt;
  m_editAmt.GetWindowText(amt);
  double d = _ttof(amt);
  int cents = (int)(d * 100);

  CString seq;
  m_editSeqNum.GetWindowText(seq);
  CString auth;
  m_editAuthCode.GetWindowText(auth);
  CString date;
  m_editDate.GetWindowText(date);

  CString ip;
  m_editIp.GetWindowText(ip);
  CString portVal;
  m_editPort.GetWindowText(portVal);
  int port = _ttoi(portVal);

  Log(_T("Sending REFUND..."));

  PosResponse resp = CTransactionService::Refund(
      m_comm, m_crypto, CStrToStr(ip), port, cents, 376, "DEMO-REF", _ttoi(seq),
      CStrToStr(auth), CStrToStr(date), 1);

  if (resp.ResponseCode == 0) {
    Log(_T("Refund APPROVED"));
  } else {
    Log(_T("Refund DECLINED: ") + CString(resp.ErrorMessage.c_str()));
  }
}

void CX990TesterMFCDlg::Log(const CString &msg) {
  CString current;
  m_editLog.GetWindowText(current);
  CTime now = CTime::GetCurrentTime();
  CString stamp = now.Format(_T("%H:%M:%S: "));

  current += stamp + msg + _T("\r\n");
  m_editLog.SetWindowText(current);
  m_editLog.LineScroll(m_editLog.GetLineCount());

  // File Log (UI Only, service handles req/resp logs)
  CFileLogService::Log("UI", CStrToStr(msg));
}

void CX990TesterMFCDlg::SetStatus(const CString &status) {
  m_lblStatus.SetWindowText(status);
}
