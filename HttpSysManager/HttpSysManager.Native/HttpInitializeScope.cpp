#include "HttpSysManager.h"

BeginHttpSysManager

	public ref class HttpInitializeScope
{
	ULONG flags;
public:
	HttpInitializeScope()
	{
		HTTPAPI_VERSION v;
		v.HttpApiMajorVersion = 2;
		v.HttpApiMinorVersion = 0;
		Init(v,HTTP_INITIALIZE_CONFIG);
	}
	HttpInitializeScope(HTTPAPI_VERSION& version,ULONG flags)
	{
		Init(version, flags);
	}
	void Init(HTTPAPI_VERSION& version,ULONG flags)
	{
		ASSERT_SUCCESS(HttpInitialize(version,flags,NULL));
		this->flags = flags;
	}

	~HttpInitializeScope()
	{
		this->!HttpInitializeScope();
	}

	!HttpInitializeScope()
	{
		ASSERT_SUCCESS(HttpTerminate(flags,NULL));
	}
};

EndHttpSysManager