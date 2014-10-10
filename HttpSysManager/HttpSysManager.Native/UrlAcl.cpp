#include "HttpSysManager.h"
#include "HttpInitializeScope.cpp"
#include "HttpApiHelper.cpp"
#include "ManagedSet.cpp"

typedef ProcessHacker::Native::Security::AccessControl::SecurityDescriptor TSecurityDescriptor;

BeginHttpSysManager

	public ref class UrlAcl : public ManagedSet<HTTP_SERVICE_CONFIG_URLACL_SET>, public ISecurable
{
public: UrlAcl()
			:ManagedSet(HttpServiceConfigUrlAclInfo)
	{
	}
public: UrlAcl(mauto_handle<HTTP_SERVICE_CONFIG_URLACL_SET> urlAcl):
	ManagedSet(urlAcl, HttpServiceConfigUrlAclInfo)
	{
	}


	LocalMemoryAlloc^ _Prefix;
public: property String^ Prefix
		{
			String^ get()
			{
				return Marshal::PtrToStringUni(IntPtr(Set->KeyDesc.pUrlPrefix));
			}
			void set(String^ value)
			{
				_Prefix =  gcnew LocalMemoryAlloc(IntPtr((PWSTR)Marshal::StringToHGlobalUni(value).ToPointer()));
				Set->KeyDesc.pUrlPrefix = (PWSTR)(void*)_Prefix;
			}
		}
		LocalMemoryAlloc^ _SecurityDescriptor;
public: property TSecurityDescriptor^ SecurityDescriptor
		{
			TSecurityDescriptor^ get()
			{
				return TSecurityDescriptor::FromSDDL(Marshal::PtrToStringUni(IntPtr(Set->ParamDesc.pStringSecurityDescriptor)));
			}
			void set(TSecurityDescriptor^ value)
			{
				if(value == nullptr)
					throw gcnew ArgumentNullException("value");
				_SecurityDescriptor = gcnew LocalMemoryAlloc(Marshal::StringToHGlobalUni(value->ToString()));
				Set->ParamDesc.pStringSecurityDescriptor = (PWSTR)(void*)_SecurityDescriptor;
			}
		}

public: static UrlAcl^ Get(int index)
		{
			HTTP_SERVICE_CONFIG_URLACL_QUERY query;
			query.dwToken = index;
			query.QueryDesc = HttpServiceConfigQueryNext;
			ULONG result;
			auto buffer = 
				HttpApiHelper::HttpQueryServiceConfiguration<HTTP_SERVICE_CONFIG_URLACL_QUERY, HTTP_SERVICE_CONFIG_URLACL_SET>(HttpServiceConfigUrlAclInfo, query, &result);
			if(result == ERROR_NO_MORE_ITEMS)
				return nullptr;
			ASSERT_SUCCESS(result);
			return gcnew UrlAcl(buffer);
		}
public: static UrlAcl^ Get(String^ prefix)
		{
			HTTP_SERVICE_CONFIG_URLACL_QUERY query;
			query.QueryDesc = HttpServiceConfigQueryExact;
			UrlAcl info;
			info.Prefix = prefix;
			query.KeyDesc.pUrlPrefix = info.Set->KeyDesc.pUrlPrefix;
			ULONG result;
			auto buffer = 
				HttpApiHelper::HttpQueryServiceConfiguration<HTTP_SERVICE_CONFIG_URLACL_QUERY, HTTP_SERVICE_CONFIG_URLACL_SET>(HttpServiceConfigUrlAclInfo, query, &result);
			if(result == ERROR_FILE_NOT_FOUND)
				return nullptr;
			ASSERT_SUCCESS(result);
			return gcnew UrlAcl(buffer);
		}

private: virtual TSecurityDescriptor^ GetSecurity(SecurityInformation securityInformation) sealed =  ISecurable::GetSecurity
		 {
			 return SecurityDescriptor;
		 }

private:  virtual void SetSecurity(SecurityInformation securityInformation, TSecurityDescriptor^ securityDescriptor) sealed = ISecurable::SetSecurity 
		 {
			 if(securityDescriptor->ToString() == TSecurityDescriptor::Empty->ToString())
				 return;
			 HttpInitializeScope scope;
			 SecurityDescriptor = securityDescriptor;
			 Update();
		 }
public:
	void SetSecurity(TSecurityDescriptor^ securityDescriptor)
	{
		((ISecurable^)this)->SetSecurity(SecurityInformation::Dacl, securityDescriptor);
	}

};

EndHttpSysManager