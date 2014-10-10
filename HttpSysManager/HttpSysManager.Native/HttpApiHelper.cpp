#include "HttpSysManager.h"


BeginHttpSysManager

public class HttpApiHelper
{
public: 
	template<class TQueryConfig, class TOutputConfig>
	static mauto_handle<TOutputConfig> HttpQueryServiceConfiguration(HTTP_SERVICE_CONFIG_ID configId, TQueryConfig& query, PULONG result)
	{
		ULONG length;
		ULONG firstCallResult = ::HttpQueryServiceConfiguration(NULL, 
			configId,
			&query,
			sizeof(TQueryConfig),
			NULL,
			0,
			&length,
			NULL);
		ASSERT_HTTPAPI_INITIALIZED(firstCallResult);
		mauto_handle<TOutputConfig> buffer((TOutputConfig*)new BYTE[length]);
		ZeroMemory(buffer.get(),length);
		 *result = ::HttpQueryServiceConfiguration(NULL, 
			configId,
			&query,
			sizeof(TQueryConfig),
			buffer.get(),
			length,
			&length,
			NULL);
		return buffer;
	}
};

EndHttpSysManager