#include "pch.h"
#include "FreeRdpWrapper.h"
#include "Logging.h"

#pragma warning(disable : 4324 4201 4245 4996)
#include <freerdp/freerdp.h>
#include <freerdp/cache/cache.h>
#include <freerdp/addin.h>
#include <freerdp/client/channels.h>
#include <freerdp/channels/rdpsnd.h>
#include <freerdp/channels/rdpdr.h>
#pragma warning(default : 4324 4201 4245 4996)
#pragma once
using namespace Logging;
using namespace FreeRdpClient;

namespace FreeRdpClient
{
	static pFreeRdpDisconnectedCallback _disconnectCallback = nullptr;

	char* ConvToUtf8(BSTR source)
	{
		std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>, wchar_t> convToUTF8;
		return _strdup(convToUTF8.to_bytes(source).c_str());
	}

	wchar_t* ConvToUtf16(const char* source)
	{
		if (!source) // Handle null input
		{
			return nullptr;
		}

		std::wstring_convert<std::codecvt_utf8<wchar_t>> converter;
		std::wstring wideString = converter.from_bytes(source);

		wchar_t* wstr = new wchar_t[wideString.size() + 1];
		std::wmemcpy(wstr, wideString.c_str(), wideString.size() + 1);
		return wstr;
	}

	class instance_data
	{
	  public:
		rdpContext* context;
		HANDLE transportStopEvent;
		char* scopeName;

		instance_data(rdpContext* context, ConnectOptions* rdpOptions)
		{
			transportStopEvent = NULL;
			this->context = context;
			this->scopeName = ConvToUtf8(rdpOptions->ScopeName);
		}
		~instance_data()
		{
			if (this->transportStopEvent)
			{
				CloseHandle(this->transportStopEvent);
				this->transportStopEvent = NULL;
			}
			if (this->scopeName)
			{
				free(this->scopeName);
				this->scopeName = nullptr;
			}
		}
		_bstr_t getEventName()
		{
			return "Global\\" + (_bstr_t)(this->scopeName);
		}
	};

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

	void SetLastError(rdpContext* context)
	{
		auto rdpError = freerdp_get_last_error(context);
		const char* rdpErrorString = freerdp_get_last_error_string(rdpError);


		WCHAR szMsgBuff[MAX_TRACE_MSG];
		swprintf_s(szMsgBuff, _countof(szMsgBuff),
		           L"Rdp connection failed: Message: %S Last error: %d", rdpErrorString, rdpError);
		SetErrorInfo(szMsgBuff);		
		DT_ERROR(szMsgBuff);
	}

	freerdp* CreateFreeRdpInstance()
	{
		freerdp* instance = NULL;

		instance = freerdp_new();
		if (instance == NULL)
		{
			DT_ERROR(L"Failed create the rdp instance");
			return NULL;
		}

		if (freerdp_context_new(instance) == FALSE)
		{
			freerdp_free(instance);
			DT_ERROR(L"Failed create the rdp context");
			return NULL;
		}
		return instance;
	}

	static BOOL LoadStaticChannelAddin(rdpChannels* channels,
		rdpSettings* settings, const char* name,
		void* data)
	{
		PVIRTUALCHANNELENTRY entry = NULL;
		PVIRTUALCHANNELENTRY pvce = freerdp_load_channel_addin_entry(
			name, NULL, NULL, FREERDP_ADDIN_CHANNEL_STATIC | FREERDP_ADDIN_CHANNEL_ENTRYEX);
		PVIRTUALCHANNELENTRYEX pvceex = WINPR_FUNC_PTR_CAST(pvce, PVIRTUALCHANNELENTRYEX);

		if (!pvceex)
			entry =
			freerdp_load_channel_addin_entry(name, NULL, NULL, FREERDP_ADDIN_CHANNEL_STATIC);

		if (pvceex)
		{
			if (freerdp_channels_client_load_ex(channels, settings, pvceex, data) == 0)
			{
				DT_TRACE(L"loading channelEx %s", name);
				return TRUE;
			}
		}
		else if (entry)
		{
			if (freerdp_channels_client_load(channels, settings, entry, data) == 0)
			{
				DT_TRACE(L"loading channel %s", name);
				return TRUE;
			}
		}

		return FALSE;
	}

	BOOL AddStaticChannel(rdpSettings* settings, size_t count, const char* const* params)
	{
		ADDIN_ARGV* _args = NULL;

		if (!settings || !params || !params[0] || (count > INT_MAX))
			return FALSE;

		if (freerdp_static_channel_collection_find(settings, params[0]))
			return TRUE;

		_args = freerdp_addin_argv_new(count, params);

		if (!_args)
			return FALSE;

		if (!freerdp_static_channel_collection_add(settings, _args))
		{
			freerdp_addin_argv_free(_args);
			return FALSE;
		}

		return TRUE;
	}

	static BOOL LoadChannels(rdpChannels* channels, rdpSettings* settings)
	{
		if (!LoadStaticChannelAddin(channels, settings, RDPDR_SVC_CHANNEL_NAME,
			settings))
			return FALSE;

		if (!freerdp_static_channel_collection_find(settings, RDPSND_CHANNEL_NAME))
		{
			const char* const params[] = { RDPSND_CHANNEL_NAME, "sys:fake" };

			if (!AddStaticChannel(settings, ARRAYSIZE(params), params))
				return FALSE;
		}

		if (freerdp_settings_get_bool(settings, FreeRDP_RedirectSmartCards))
		{
			if (!freerdp_device_collection_find_type(settings, RDPDR_DTYP_SMARTCARD))
			{
				RDPDR_DEVICE* smartcard = freerdp_device_new(RDPDR_DTYP_SMARTCARD, 0, NULL);

				if (!smartcard)
					return FALSE;

				if (!freerdp_device_collection_add(settings, smartcard))
				{
					freerdp_device_free(smartcard);
					return FALSE;
				}
			}
		}

		for (UINT32 i = 0; i < freerdp_settings_get_uint32(settings, FreeRDP_StaticChannelCount); i++)
		{
			ADDIN_ARGV* _args = static_cast<ADDIN_ARGV*>(freerdp_settings_get_pointer_array_writable(settings,
				FreeRDP_StaticChannelArray, i));

			if (!LoadStaticChannelAddin(channels, settings, _args->argv[0], _args))
				return FALSE;
		}
	}

	BOOL LoadChannelsCore(freerdp* instance)
	{
		return LoadChannels(instance->context->channels, instance->context->settings);
	}

	BOOL PrepareRdpContext(rdpContext* context, const ConnectOptions* rdpOptions)
	{
		context->settings->ServerHostname = ConvToUtf8(rdpOptions->HostName);

		if (rdpOptions->Port)
			context->settings->ServerPort = rdpOptions->Port;

		context->settings->Domain = ConvToUtf8(rdpOptions->Domain);
		context->settings->Username = ConvToUtf8(rdpOptions->User);
		context->settings->Password = ConvToUtf8(rdpOptions->Pass);
		
		if (rdpOptions->ClientName)
			context->settings->ClientHostname = ConvToUtf8(rdpOptions->ClientName);

		context->settings->SoftwareGdi = TRUE;
		context->settings->LocalConnection = TRUE;
		context->settings->ProxyType = PROXY_TYPE_IGNORE;

		// Without this setting the RDP session getting disconnected unexpectedly after a time
		// This issue can be reproduced using 2.5.0 freerdp version
		// (https://uipath.atlassian.net/browse/ROBO-2607) and seems to be introduced by this
		// commit:
		// https://github.com/FreeRDP/FreeRDP/pull/5151/commits/7610917a48e2ea4f1e1065bd226643120cbce4e5
		context->settings->BitmapCacheEnabled = TRUE;

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

		if (rdpOptions->smartcardSettings.IsSmartCardLogon)
		{
			context->settings->SmartcardLogon = TRUE;
			context->settings->PasswordIsSmartcardPin = TRUE;
			context->settings->RedirectSmartCards = TRUE;
			context->settings->DeviceRedirection = TRUE;
			context->settings->KeySpec = 1;

			context->settings->ReaderName = ConvToUtf8(rdpOptions->smartcardSettings.ReaderName);
			context->settings->CspName = ConvToUtf8(rdpOptions->smartcardSettings.CspName);
			context->settings->ContainerName = ConvToUtf8(rdpOptions->smartcardSettings.ContainerName);

			context->instance->LoadChannels = LoadChannelsCore;

			// Only called if multiple certificates are available for the same user
			context->instance->ChooseSmartcard = [](freerdp* instance,
			                                        SmartcardCertInfo** cert_list, DWORD count,
			                                        DWORD* choice, BOOL gateway) -> BOOL 
			{
				auto res = false;
				auto containerName = ConvToUtf16(instance->context->settings->ContainerName);
					
				for (DWORD idx = 0; idx < count; idx++)
				{
					if (wcscmp(cert_list[idx]->containerName, containerName) != 0)
						continue;

					*choice = idx;
					res = true;
					break;
				}

				delete[] containerName;
				return res;
			};

			if (freerdp_register_addin_provider(freerdp_channels_load_static_addin_entry, 0) != CHANNEL_RC_OK)
			{
				DT_ERROR(L"Failed to register addin provider");
				return FALSE;
			}
		}

		return TRUE;
	}

	DWORD ReleaseAll(instance_data* instanceData)
	{
		DT_TRACE(L"RdpRelease: Start");

		freerdp* instance = instanceData->context->instance;
		if (instance->context->cache != NULL)
		{
			cache_free(instance->context->cache);
		}

		freerdp_disconnect(instance);
		freerdp_context_free(instance);
		freerdp_free(instance);

		auto releaseObjectName = instanceData->getEventName();

		delete instanceData;

		if (_disconnectCallback)
		{
			_disconnectCallback(releaseObjectName);
		}

		DT_TRACE(L"RdpRelease: Finish");
		return ERROR_SUCCESS;
	}

	// Freerdp async transport implementation
	// Was removed from freerdp core (https://github.com/FreeRDP/FreeRDP/pull/4815), and remains
	// only on freerdp clients Seems to still needed for Windows7 disconnected session
	// (https://github.com/UiPath/Driver/commit/dbc3ea9009b988471eee124ed379b02a63b993eb)

	DWORD WINAPI transport_thread(LPVOID pData)
	{
		instance_data* instanceData = (instance_data*)pData;

		rdpContext* context = instanceData->context;

		Logging::RegisterCurrentThreadScope(instanceData->scopeName);

		context->cache = cache_new(context);

		HANDLE handles[64]{};
		handles[0] = instanceData->transportStopEvent;

		while (1)
		{
			DWORD nCount = 1; // transportStopEvent

			DWORD nCountTmp = freerdp_get_event_handles(context, &handles[nCount], 64 - nCount);
			if (nCountTmp == 0)
			{
				DT_ERROR(L"freerdp_get_event_handles failed");
				break;
			}

			nCount += nCountTmp;
			DWORD status = WaitForMultipleObjects(nCount, handles, FALSE, INFINITE);

			if (status == WAIT_OBJECT_0)
			{
				DT_TRACE(L"freerdp: transportStopEvent triggered");
				break;
			}

			if (status > WAIT_OBJECT_0 && status < (WAIT_OBJECT_0 + nCount))
			{
				freerdp_check_event_handles(context);
				if (freerdp_shall_disconnect(context->instance))
				{
					DT_TRACE(L"freerdp_shall_disconnect()");
					freerdp_set_error_info(context->rdp, ERRINFO_PEER_DISCONNECTED);
					break;
				}
			}
			else
			{
				DT_ERROR(L"WaitForMultipleObjects returned 0x%08", status);
				break;
			}
		}

		ReleaseAll(instanceData);
		return 0;
	}

	BOOL transport_start(
		rdpContext* context,
		ConnectOptions* rdpOptions,
		_bstr_t &eventName)
	{
		instance_data* instanceData = new instance_data(context, rdpOptions);

		eventName = instanceData->getEventName();
		auto existingEvent = OpenEvent(NULL, false, eventName.GetBSTR());
		if (existingEvent)
		{
			CloseHandle(existingEvent);
			DT_ERROR(L"Failed to create freerdp transport stop event, error: alreadyExists: %s", eventName.GetBSTR());
			delete instanceData;
			return FALSE;
		}

		instanceData->transportStopEvent = CreateEvent(NULL, TRUE, FALSE, eventName.GetBSTR());
		if (!instanceData->transportStopEvent)
		{
			DT_ERROR(L"Failed to create freerdp transport stop event, error: %u", GetLastError());
			delete instanceData;
			return FALSE;
		}

		auto transportThreadHandle = CreateThread(NULL, 0, transport_thread, instanceData, 0, NULL);
		if (!transportThreadHandle)
		{
			DT_ERROR(L"Failed to create freerdp transport client thread, error: %u", GetLastError());
			delete instanceData;
			return FALSE;
		}
		CloseHandle(transportThreadHandle);
		return TRUE;
	}

	HRESULT STDAPICALLTYPE RdpLogon(
		ConnectOptions* rdpOptions,
		BSTR& releaseEventName)
	{
		DT_TRACE(L"Start for user: [%s], domain: [%s], scopeName: [%s]", rdpOptions->User,
		         rdpOptions->Domain, rdpOptions->ScopeName);
		releaseEventName = NULL;
		auto instance = CreateFreeRdpInstance();
		if (!instance)
			return E_OUTOFMEMORY;

		rdpContext* context = instance->context;

		if (PrepareRdpContext(context, rdpOptions)
			&& freerdp_connect(instance))
		{
			_bstr_t eventName;
			if (transport_start(context, rdpOptions, eventName))
			{
				releaseEventName = eventName.Detach();
				DT_TRACE(L"Connection succeeded");
				return S_OK;
			}
			else
			{
				DT_ERROR(L"Failed start the freerdp transport thread");
			}
		}

		SetLastError(context);

		freerdp_context_free(instance);
		freerdp_free(instance);

		return E_FAIL;
	}

	HRESULT STDAPICALLTYPE RdpRelease(BSTR releaseEventName)
	{
		DT_TRACE(L"RdpRelease");
		auto eventHandle = OpenEvent(EVENT_MODIFY_STATE, false, releaseEventName);
		if (!eventHandle)
			return S_OK;

		if (!SetEvent(eventHandle))
		{
			auto lastError = GetLastError();
			CloseHandle(eventHandle);
			return HRESULT_FROM_WIN32(lastError);
		}

		CloseHandle(eventHandle);
		return S_OK;
	}

	HRESULT STDAPICALLTYPE SetDisconnectCallback(pFreeRdpDisconnectedCallback disconnectCallback)
	{
		_disconnectCallback = disconnectCallback;
		return S_OK;
	}
}