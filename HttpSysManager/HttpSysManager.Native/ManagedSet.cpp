#include "HttpSysManager.h"

BeginHttpSysManager

	template<class TSet>
public ref class ManagedSet
{
protected: mauto_handle<TSet> Set;
		   HTTP_SERVICE_CONFIG_ID ConfigId;

public: ManagedSet(mauto_handle<TSet> set, HTTP_SERVICE_CONFIG_ID configId)
			:Set(set), ConfigId(configId)
		{
		}
public: ManagedSet(HTTP_SERVICE_CONFIG_ID configId)
			:Set(new TSet()),ConfigId(configId)
		{
			ZeroMemory(Set.get(),sizeof(TSet));
		}


public: virtual void Add()
		{
			ULONG result = HttpSetServiceConfiguration(NULL, 
				ConfigId,
				Set.get(),
				sizeof(TSet),
				0);
			ASSERT_HTTPAPI_INITIALIZED(result);
			if(result == ERROR_NO_SUCH_LOGON_SESSION)
				throw gcnew ArgumentException("Invalid Certificate");
			if(result != 0)
				Win32::Throw(result);
		}
public: virtual void Update()
		{
			try
			{
				Add();
			}catch(WindowsException^ ex)
			{
				if((LONG)ex->ErrorCode != ERROR_ALREADY_EXISTS)
					throw;
				Delete();
				Add();
			}
		}
public: virtual void Delete()
		{
			auto result = HttpDeleteServiceConfiguration(NULL, 
				ConfigId,
				Set.get(),
				sizeof(TSet),
				0);
			ASSERT_HTTPAPI_INITIALIZED(result);
			if(result != 0)
				Win32::Throw(result);
		}
};

EndHttpSysManager