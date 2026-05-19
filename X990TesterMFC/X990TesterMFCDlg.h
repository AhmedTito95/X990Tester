#pragma once

#include "KeyStorageService.h"
#include "Models.h"
#include "TcpClient.h"

class CX990TesterMFCDlg : public CDialogEx {
  // Construction
public:
  CEdit m_editIp;
  CEdit m_editPort;
  CEdit m_editLog;
  CEdit m_editAmt;
  CEdit m_editSeqNum;
  CEdit m_editAuthCode;
  CEdit m_editDate;
  CButton m_chkPrint;
  CStatic m_lblStatus;

  CX990TesterMFCDlg(CWnd *pParent = nullptr); // standard constructor

  afx_msg void OnBnClickedBtnConnect();
  afx_msg void OnBnClickedBtnInit();
  afx_msg void OnBnClickedBtnSale();
  afx_msg void OnBnClickedBtnRefund();

  void Log(const CString &msg);
  void SetStatus(const CString &status);

private:
  CKeyStorageService m_keys;
  CTcpClient m_comm;
  bool m_isInitialized = false;

protected:
  HICON m_hIcon;

  virtual void DoDataExchange(CDataExchange *pDX); // DDX/DDV support
  // Generated message map functions
  virtual BOOL OnInitDialog();
  afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
  afx_msg void OnPaint();
  afx_msg HCURSOR OnQueryDragIcon();

  DECLARE_MESSAGE_MAP()
};
