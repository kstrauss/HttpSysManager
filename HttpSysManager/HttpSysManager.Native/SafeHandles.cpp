#include "HttpSysManager.h"

template<class THandle>
public ref class MySafeHandle
{
public:MySafeHandle(THandle handle):_Handle(handle)
	   {

	   }
private: THandle _Handle;
public: property THandle Handle
		{
			THandle get()
			{
				return _Handle;
			}
			void set(THandle value)
			{
				_Handle = value;
			}
		}
		~MySafeHandle()
		{
			this->!MySafeHandle();
		}
		!MySafeHandle()
		{
			if(Handle != NULL)
				Release();
		}
protected: virtual void Release()
		   {}
};

public ref class SafeStoreHandle : MySafeHandle<HCERTSTORE>
{
public:SafeStoreHandle(HCERTSTORE handle):MySafeHandle(handle)
	   {

	   }
protected: virtual void Release() override
		   {
			   CertCloseStore(Handle,0);
		   }
};
public ref class SafeCertHandle : MySafeHandle<PCCERT_CONTEXT>
{
public:SafeCertHandle(PCCERT_CONTEXT handle):MySafeHandle(handle)
	   {
	   }
protected: virtual  void Release() override
		   {
			   CertFreeCertificateContext(Handle);
		   }
};

public ref class SafeCTLHandle : MySafeHandle<PCCTL_CONTEXT>
{
public:SafeCTLHandle(PCCTL_CONTEXT handle):MySafeHandle(handle)
	   {

	   }
protected: virtual void Release() override
		   {
			   CertFreeCTLContext(Handle);
		   }
};

public ref class HGlobalHandle : MySafeHandle<void*>
{
public:HGlobalHandle(System::IntPtr handle):MySafeHandle(handle.ToPointer())
	   {

	   }
public:HGlobalHandle(void* handle):MySafeHandle(handle)
	   {

	   }
protected: virtual void Release() override
		   {
			   System::Runtime::InteropServices::Marshal::FreeHGlobal(System::IntPtr(Handle));
		   }
};