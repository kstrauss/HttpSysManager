#include "HttpSysManager.h"
#include "SafeHandles.cpp"

BeginHttpSysManager
	public ref class Security
{
	
public: static X509Certificate2^ SelectCertificate()
		{
			SafeStoreHandle storeHandle(CertOpenStore(CERT_STORE_PROV_SYSTEM,0,NULL,CERT_SYSTEM_STORE_LOCAL_MACHINE, L"MY"));
			SafeCertHandle certHandle(CryptUIDlgSelectCertificateFromStore(storeHandle.Handle,NULL,NULL,NULL,0,0,0));
			if(certHandle.Handle == NULL)
				return nullptr;
			return gcnew X509Certificate2(IntPtr((void*)certHandle.Handle)); //X509Certificate2 duplicate the handle
		};
};
EndHttpSysManager