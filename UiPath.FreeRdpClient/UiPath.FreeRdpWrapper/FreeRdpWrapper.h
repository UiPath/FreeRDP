#pragma once

#include "pch.h"

namespace FreeRdpClient
{
	typedef struct
	{
		BOOL IsSmartCardLogon;
		BSTR ReaderName;
		BSTR ContainerName;
		BSTR CspName;
	} SmartcardSettings;

	typedef struct
	{
		long Width;
		long Height;
		long Depth;
		BOOL FontSmoothing;
		BSTR User;
		BSTR Domain;
		BSTR Pass;
		BSTR ScopeName;
		BSTR ClientName;
		BSTR HostName;
		long Port;
		
		SmartcardSettings smartcardSettings;
	} ConnectOptions;

	using pFreeRdpDisconnectedCallback = void (*)(BSTR);

	EXTERN_C __declspec(dllexport) HRESULT STDAPICALLTYPE
	    RdpLogon(
			ConnectOptions* rdpOptions,
			BSTR& releaseEventName);
	EXTERN_C __declspec(dllexport) HRESULT STDAPICALLTYPE RdpRelease(BSTR releaseEventName);
	EXTERN_C __declspec(dllexport) HRESULT STDAPICALLTYPE SetDisconnectCallback(pFreeRdpDisconnectedCallback disconnectCallback);
}