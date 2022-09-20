#include "pch.h"
#include "FreeRdpWrapper.h"
#include "Logging.h"

#pragma warning( disable: 4324 4201 4245 )
#include <freerdp/freerdp.h>
#include <freerdp/cache/cache.h>
#pragma warning( default: 4324 4201 4245 )
#pragma once
using namespace Logging;
using namespace FreeRdpClient;

namespace FreeRdpClient
{
	struct instance_data
	{
		rdpContext* context;
		HANDLE transportStopEvent;
	};

	// constants for RD Service
	LPCSTR HOST_NAME = "localhost";

	inline void SecureFreeMemory(char*& data)
	{
		if (data != nullptr)
		{
			if (strlen(data) > 0)
				::SecureZeroMemory(data, strlen(data));

			free(data);

			data = nullptr;
		}
	}

	inline HRESULT SetErrorInfo(LPCWSTR szError)
	{
		CComPtr<ICreateErrorInfo> pICEI;
		CHECK_HRESULT_RET_HR(CreateErrorInfo(&pICEI));

		CHECK_HRESULT_RET_HR(pICEI->SetDescription((LPOLESTR)szError));

		CComPtr<IErrorInfo> pErrorInfo;
		CHECK_HRESULT_RET_HR(pICEI->QueryInterface(__uuidof(IErrorInfo), (void**)&pErrorInfo));
		CHECK_HRESULT_RET_HR(SetErrorInfo(0, pErrorInfo));
		return S_OK;
	}

	DWORD SetError(DWORD rdpError)
	{
		const char* rdpErrorString = freerdp_get_last_error_string(rdpError);

		DT_ERROR("Connection failed: %x, Message: '%s'", rdpError, rdpErrorString);

		WCHAR szMsgBuff[MAX_TRACE_MSG];
		swprintf_s(szMsgBuff, _countof(szMsgBuff),
			        L"Rdp connection failed: Message: %S, Last error: %d", rdpErrorString,
			        rdpError);
		SetErrorInfo(szMsgBuff);

		return ERROR_INTERNAL_ERROR;
	}

	freerdp* CreateFreeRdpInstance()
	{
		freerdp* instance = NULL;

		instance = freerdp_new();
		if (instance == NULL)
		{
			DT_ERROR("Failed create the rdp instance");
			return NULL;
		}

		if (freerdp_context_new(instance) == FALSE)
		{
			freerdp_free(instance);
			DT_ERROR("Failed create the rdp context");
			return NULL;
		}
		return instance;
	}

	void PrepareRdpContext(rdpContext* context, const ConnectOptions* rdpOptions)
	{
		std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>, wchar_t> convToUTF8;

		context->settings->SoftwareGdi = TRUE;
		context->settings->ServerHostname = _strdup(HOST_NAME);
		context->settings->Domain = _strdup(convToUTF8.to_bytes(rdpOptions->Domain).c_str());
		context->settings->Username = _strdup(convToUTF8.to_bytes(rdpOptions->User).c_str());
		context->settings->Password = _strdup(convToUTF8.to_bytes(rdpOptions->Pass).c_str());

		if (rdpOptions->ClientName)
			context->settings->ClientHostname =
			    _strdup(convToUTF8.to_bytes(rdpOptions->ClientName).c_str());

		context->settings->LocalConnection = TRUE;
		context->settings->ProxyType = PROXY_TYPE_IGNORE;

		// Increase the TcpAckTimeout to 60 seconds (default is 9 seconds). Used to wait for an
		// active tcp connection (CONNECTION_STATE_ACTIVE)
		// https://github.com/FreeRDP/FreeRDP/blob/fa3cf9417ffb67a3433ecb48d18a1c2b3190a03e/libfreerdp/core/connection.c#L380
		context->settings->TcpAckTimeout = 60000;

		// The freerdp is used only to create a session on local machine (localhost) => we ignore
		// certificate
		context->settings->IgnoreCertificate = TRUE;

		if (rdpOptions->Width > 0)
			context->settings->DesktopWidth = rdpOptions->Width;
		if (rdpOptions->Height > 0)
			context->settings->DesktopHeight = rdpOptions->Height;
		if (rdpOptions->Depth > 0)
			context->settings->ColorDepth = rdpOptions->Depth;

		context->settings->AllowFontSmoothing = rdpOptions->FontSmoothing;
	}

	DWORD ReleaseAll(instance_data* instanceData)
	{
		DT_TRACE("RdpRelease: Start");

		if (instanceData == NULL || instanceData->context == NULL)
		{
			DT_ERROR("RdpRelease: Invalid context data");
			return ERROR_INVALID_PARAMETER;
		}

		freerdp* instance = instanceData->context->instance;
		if (instance->context->cache != NULL)
		{
			cache_free(instance->context->cache);
		}

		freerdp_disconnect(instance);
		freerdp_context_free(instance);
		freerdp_free(instance);

		CloseHandle(instanceData->transportStopEvent);
		instanceData->transportStopEvent = NULL;

		free(instanceData);

		DT_TRACE("RdpRelease: Finish");
		return ERROR_SUCCESS;
	}

	// Freerdp async transport implementation
	// Was removed from freerdp core (https://github.com/FreeRDP/FreeRDP/pull/4815), and remains
	// only on freerdp clients Seems to still needed for Windows7 disconnected session
	// (https://github.com/UiPath/Driver/commit/dbc3ea9009b988471eee124ed379b02a63b993eb)

	DWORD WINAPI transport_thread(LPVOID pData)
	{
		instance_data* instanceData = (instance_data*)pData;
		if (instanceData == NULL || instanceData->context == NULL ||
		    instanceData->transportStopEvent == NULL)
		{
			DT_ERROR("Invalid freerdp instance data");
			return 1;
		}

		rdpContext* context = instanceData->context;
		context->cache = cache_new(context->instance->settings);

		HANDLE handles[64]{};
		handles[0] = instanceData->transportStopEvent;

		while (1)
		{
			DWORD nCount = 1; // transportStopEvent

			DWORD nCountTmp = freerdp_get_event_handles(context, &handles[nCount], 64 - nCount);
			if (nCountTmp == 0)
			{
				DT_ERROR("freerdp_get_event_handles failed");
				break;
			}

			nCount += nCountTmp;
			DWORD status = WaitForMultipleObjects(nCount, handles, FALSE, INFINITE);

			if (status == WAIT_OBJECT_0)
			{
				DT_TRACE("freerdp: transportStopEvent triggered");
				break;
			}

			if (status > WAIT_OBJECT_0 && status < (WAIT_OBJECT_0 + nCount))
			{
				freerdp_check_event_handles(context);
				if (freerdp_shall_disconnect(context->instance))
				{
					DT_TRACE("freerdp_shall_disconnect()");
					freerdp_set_error_info(context->rdp, ERRINFO_PEER_DISCONNECTED);
					break;
				}
			}
			else
			{
				DT_ERROR("WaitForMultipleObjects returned 0x%08", status);
				break;
			}
		}

		ReleaseAll(instanceData);
		return 0;
	}

	instance_data* transport_start(rdpContext* context, LPCWSTR eventName)
	{
		instance_data* instanceData;
		instanceData = (instance_data*)calloc(1, sizeof(instance_data));
		if (!instanceData)
			return NULL;

		instanceData->context = context;
		auto existingEvent = OpenEvent(NULL, false, eventName);
		if (existingEvent)
		{
			CloseHandle(existingEvent);
			DT_ERROR(_T("Failed to create freerdp transport stop event, error: alreadyExists: %s"),
			         eventName);
			free(instanceData);
			return NULL;
		}

		instanceData->transportStopEvent = CreateEvent(NULL, TRUE, FALSE, eventName);
		if (!instanceData->transportStopEvent)
		{
			DT_ERROR(_T("Failed to create freerdp transport stop event, error: %u"),
			         GetLastError());
			free(instanceData);
			return NULL;
		}

		auto transportThreadHandle = CreateThread(NULL, 0, transport_thread, instanceData, 0, NULL);
		if (!transportThreadHandle)
		{
			DT_ERROR(_T("Failed to create freerdp transport client thread, error: %u"),
			         GetLastError());
			CloseHandle(instanceData->transportStopEvent);
			free(instanceData);
			return NULL;
		}
		CloseHandle(transportThreadHandle);
		return instanceData;
	}
	HRESULT STDAPICALLTYPE RdpLogon(ConnectOptions* rdpOptions, BSTR& releaseEventName)
	{
		DT_TRACE(L"Start for user: [%s], domain: [%s], clientName: [%s]", rdpOptions->User,
		         rdpOptions->Domain, rdpOptions->ClientName);
		releaseEventName = NULL;
		auto instance = CreateFreeRdpInstance();
		if (!instance)
			return ERROR_INTERNAL_ERROR;

		rdpContext* context = instance->context;
		PrepareRdpContext(context, rdpOptions);

		auto connectResult = freerdp_connect(instance);
		DWORD rdpError = freerdp_get_last_error(context);

		// secure the password
		SecureFreeMemory(context->settings->Password);

		if (connectResult)
		{
			auto eventName = "Global\\" + (_bstr_t)(rdpOptions->ClientName);
			auto lpData = transport_start(context, eventName);
			if (lpData)
			{
				releaseEventName = ::SysAllocString(eventName);
				DT_TRACE("Connection succeeded");
				return ERROR_SUCCESS;
			}
			else
			{
				DT_ERROR("Failed start the freerdp transport thread");
			}
		}

		DWORD dwError = SetError(rdpError);

		freerdp_context_free(instance);
		freerdp_free(instance);

		return dwError;
	}

	HRESULT STDAPICALLTYPE RdpRelease(LPCWSTR releaseEventName)
	{
		DT_TRACE("RdpRelease");
		auto eventHandle = OpenEvent(EVENT_MODIFY_STATE, false, releaseEventName);

		if (!eventHandle)
			return ERROR_SUCCESS;
		if (!SetEvent(eventHandle))
		{
			auto lastError = GetLastError();
			CloseHandle(eventHandle);
			return lastError;
		}

		CloseHandle(eventHandle);
		return ERROR_SUCCESS;
	}
}