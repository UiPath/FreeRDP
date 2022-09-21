#ifndef FreeRdpWrapper_H
#define FreeRdpWrapper_H
#include "pch.h"

#define CHECK_HRESULT_RET_HR(Stmt) CHECK_HRESULT_ACTION_(Stmt, return hrTmp)
#define CHECK_HRESULT_ACTION_(Stmt, Action)                                           \
	{                                                                                 \
		HRESULT hrTmp = Stmt;                                                         \
		if (FAILED(hrTmp))                                                            \
		{                                                                             \
			DT_ERROR(L"%S:%d: error: %u [%x]", __FUNCTION__, __LINE__, hrTmp, hrTmp); \
			Action;                                                                   \
		}                                                                             \
	}
namespace FreeRdpClient
{
	typedef struct
	{
		long Width;
		long Height;
		long Depth;
		BOOL FontSmoothing;
		LPCWSTR User;
		LPCWSTR Domain;
		LPCWSTR Pass;
		LPCWSTR ClientName;
	} ConnectOptions;

	EXTERN_C __declspec(dllexport) HRESULT STDAPICALLTYPE
	    RdpLogon(ConnectOptions* rdpOptions, BSTR& releaseEventName);
	EXTERN_C __declspec(dllexport) HRESULT STDAPICALLTYPE RdpRelease(LPCWSTR releaseEventName);
}
#endif