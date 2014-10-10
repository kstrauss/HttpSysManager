#include "HttpSysManager.h"
#include <vector>
#include <msclr\marshal.h>
#include <memory>
#include "SafeHandles.cpp"


typedef std::vector<std::shared_ptr<CTL_ENTRY>> CTL_ENTRIES;


BeginHttpSysManager
#define DEFAULT_ENCODING X509_ASN_ENCODING | PKCS_7_ASN_ENCODING
	//http://msdn.microsoft.com/en-us/library/windows/desktop/aa381487(v=vs.85).aspx
	public ref class CTLEntry
{
private:CTL_ENTRY& entry;
public: CTLEntry(CTL_ENTRY& entry):entry(entry)
		{
		}
private: array<Byte>^ _SubjectIdentifier;
public: property array<Byte>^ SubjectIdentifier
		{
			array<Byte>^ get()
			{
				return InteropHelper::ToCSharpArray(entry.SubjectIdentifier.pbData, entry.SubjectIdentifier.cbData);
			}
		}

};

//http://msdn.microsoft.com/en-us/library/windows/desktop/aa381491(v=vs.85).aspx
public ref class CTLInfo
{
internal: mauto_handle<CTL_INFO> nativeCtlInfo;
private : mauto_handle<CTL_ENTRIES> allocatedEntries;
private: mauto_handle<CTL_ENTRY> nativeCtlInfoEntries;
private: mauto_handle<LPSTR> allocatedUsages;
private: LocalMemoryAlloc^ listIdentifier;
private: static DWORD encoding = X509_ASN_ENCODING | PKCS_7_ASN_ENCODING;


public: CTLInfo(PCTL_INFO nativeCtlInfo)
			:nativeCtlInfo(new CTL_INFO(*nativeCtlInfo))
		{
			Init();
		}
public: CTLInfo(mauto_handle<CTL_INFO> nativeCtlInfo)
			:nativeCtlInfo(nativeCtlInfo)
		{
			Init();
		}
public: CTLInfo(mauto_handle<CTL_INFO> nativeCtlInfo, mauto_handle<CTL_ENTRY> nativeCtlInfoEntries, mauto_handle<CTL_ENTRIES> allocatedEntries, mauto_handle<LPSTR> allocatedUsages, LocalMemoryAlloc^ listIdentifier)
			:nativeCtlInfo(nativeCtlInfo),
			allocatedEntries(allocatedEntries),
			nativeCtlInfoEntries(nativeCtlInfoEntries),
			allocatedUsages(allocatedUsages),
			listIdentifier(listIdentifier)
		{
			Init();
		}
private: void Init()
		 {
			 _Entries = gcnew List<CTLEntry^>();
			 for(DWORD i = 0; i < nativeCtlInfo->cCTLEntry; i++)
			 {
				 CTLEntry^ entry = gcnew CTLEntry(nativeCtlInfo->rgCTLEntry[i]);
				 _Entries->Add(entry);
			 }
		 }
private: List<CTLEntry^>^ _Entries;
public: property IList<CTLEntry^>^ Entries
		{
			IList<CTLEntry^>^ get()
			{
				return _Entries;
			};
		}

public: property String^ ListIdentifier
		{
			String^ get()
			{
				if(!nativeCtlInfo->ListIdentifier.pbData || !nativeCtlInfo->ListIdentifier.cbData)
					return nullptr;
				return Marshal::PtrToStringUni(IntPtr(nativeCtlInfo->ListIdentifier.pbData));
			}
		}

public: static CTLInfo^ Load(array<Byte>^ bytes)
		{
			auto encoded = InteropHelper::ToCArray(bytes);
			DWORD size;
			CryptDecodeObject(encoding, PKCS_CTL,(BYTE*)(void*)encoded,bytes->Length,0,NULL, &size);
			mauto_handle<CTL_INFO> ctlInfo((CTL_INFO*)new Byte[size]);
			ASSERT_TRUE(CryptDecodeObject(DEFAULT_ENCODING, PKCS_CTL,(BYTE*)(void*)encoded,bytes->Length,0,ctlInfo.get(), &size));
			return gcnew CTLInfo(ctlInfo);
		};
		//Creating, Signing, and Storing a CTL  http://msdn.microsoft.com/en-us/library/windows/desktop/aa379867(v=vs.85).aspx
public: array<Byte>^ ToBytes()
		{
			DWORD size;
			std::auto_ptr<Byte> encoded(ToCBytes(&size));
			return InteropHelper::ToCSharpArray(encoded.get(),size);
		}
private: std::auto_ptr<Byte> ToCBytes(DWORD* size)
		 {
			 CryptEncodeObject(DEFAULT_ENCODING,PKCS_CTL,  nativeCtlInfo.get(),NULL,size);
			 std::auto_ptr<Byte> encoded(new Byte[*size]);
			 ASSERT_TRUE(CryptEncodeObject(DEFAULT_ENCODING,PKCS_CTL, nativeCtlInfo.get(),encoded.get(),size));
			 return encoded;
		 }
};

public ref class CTLInfoBuilder
{
public: CTLInfoBuilder()
		{
			_Certificates = gcnew List<X509Certificate2^>();
		}

private: List<X509Certificate2^>^ _Certificates;
public: property IList<X509Certificate2^>^ Certificates
		{
			IList<X509Certificate2^>^ get()
			{
				return _Certificates;
			}
		}
private: String^ _ListIdentifier;
public: property String^ ListIdentifier
		{
			String^ get()
			{
				return _ListIdentifier;
			}
			void set(String^ value)
			{
				_ListIdentifier = value;
			}
		}

private: mauto_handle<LPSTR> CreateArrayFromVector(std::vector<LPSTR> vector)
		 {
			 mauto_handle<LPSTR> output(new LPSTR[vector.size()]);
			 memcpy(output.get(),&vector[0],sizeof(LPSTR) * vector.size());
			 return output;
		 }
		 //Add certificates http://msdn.microsoft.com/en-us/library/windows/desktop/aa376038(v=vs.85).aspx
public:CTLInfo^ ToCTLInfo()
	   { 
		   mauto_handle<CTL_INFO> ctl(new CTL_INFO());
		   ZeroMemory(ctl.get(),sizeof(CTL_INFO));



		   std::vector<LPSTR> usages;
		   usages.push_back(szOID_ROOT_LIST_SIGNER);
		   usages.push_back(szOID_CTL);
		   usages.push_back(szOID_TRUSTED_CLIENT_AUTH_CA_LIST);
		   usages.push_back(szOID_PKIX_KP_CLIENT_AUTH);
		   usages.push_back(szOID_PKIX_KP_SERVER_AUTH);
		   //usages.push_back(szOID_IIS_VIRTUAL_SERVER);
		   
		   mauto_handle<LPSTR> usagesArray(CreateArrayFromVector(usages));


		   ctl->dwVersion = CTL_V1;

		   ctl->SubjectUsage.cUsageIdentifier = usages.size();
		   ctl->SubjectUsage.rgpszUsageIdentifier = usagesArray.get();

		   LocalMemoryAlloc^ listIdentifierPtr;
		   if(!String::IsNullOrEmpty(ListIdentifier))
		   {
			   listIdentifierPtr = gcnew LocalMemoryAlloc(Marshal::StringToHGlobalUni(ListIdentifier));
			   ctl->ListIdentifier.cbData = (ListIdentifier->Length +1)* sizeof(WCHAR);
			   ctl->ListIdentifier.pbData = (BYTE*)(void*)listIdentifierPtr;
		   }

		   ctl->SubjectAlgorithm.pszObjId = szOID_OIWSEC_sha1;

		   ctl->cCTLEntry = Certificates->Count;

		   mauto_handle<CTL_ENTRY> ctlEntries(new CTL_ENTRY[ctl->cCTLEntry]);
		   ctl->rgCTLEntry = ctlEntries.get();

		   mauto_handle<CTL_ENTRIES> allocatedEntries(new CTL_ENTRIES());
		   
		   int i = 0;
		   for each (X509Certificate2^ cert in Certificates)
		   {
			   PCCERT_CONTEXT certContext = (PCCERT_CONTEXT)cert->Handle.ToPointer();
			   DWORD size;
			   CertCreateCTLEntryFromCertificateContextProperties(certContext,0,NULL,CTL_ENTRY_FROM_PROP_CHAIN_FLAG,NULL,NULL, &size);

			   std::shared_ptr<CTL_ENTRY> ctlEntry((CTL_ENTRY*)new Byte[size]);
			   allocatedEntries->push_back(ctlEntry);
			   ASSERT_TRUE(CertCreateCTLEntryFromCertificateContextProperties(certContext,0,NULL,CTL_ENTRY_FROM_PROP_CHAIN_FLAG,NULL,ctlEntry.get(), &size));
			   ctl->rgCTLEntry[i] = *(ctlEntry.get());
			   i++;
		   }
		   return gcnew CTLInfo(ctl, ctlEntries, allocatedEntries, usagesArray, listIdentifierPtr);
	   }
};

typedef CTLInfo TCTLInfo;
public ref class CTLContext
{
internal: SafeCTLHandle^ handle;


public: CTLContext(SafeCTLHandle^ handle)
		{
			this->handle=handle;
			_CTLInfo = gcnew TCTLInfo(handle->Handle->pCtlInfo);
			if(!String::IsNullOrEmpty(_CTLInfo->ListIdentifier))
				FriendlyName = _CTLInfo->ListIdentifier;
		}

		private:
			void SetContextProperty(DWORD dwPropId, String^ value)
			{
				HGlobalHandle identifier(Marshal::StringToHGlobalUni(value));
				CRYPT_DATA_BLOB blob;
				blob.cbData = (value->Length + 1) * sizeof(WCHAR);
				blob.pbData = (BYTE*)identifier.Handle;
				CertSetCTLContextProperty(handle->Handle,dwPropId,NULL, &blob);
			}
			String^ GetContextProperty(DWORD dwPropId)
			{
				DWORD size;
				CertGetCTLContextProperty(handle->Handle, dwPropId,NULL,&size);
				std::auto_ptr<WCHAR> buffer((WCHAR*)new BYTE[size]);
				if(CertGetCTLContextProperty(handle->Handle, dwPropId,buffer.get(),&size))
				{
					return Marshal::PtrToStringUni(IntPtr(buffer.get()));
				}
				auto lastError = GetLastError();
				if(lastError == CRYPT_E_NOT_FOUND)
					return nullptr;
				Win32::Throw((int)lastError);
				return nullptr;
			}

public: property String^ FriendlyName
		{
			String^ get()
			{
				return GetContextProperty(CERT_FRIENDLY_NAME_PROP_ID);
			}
			void set(String^ value)
			{
				SetContextProperty(CERT_FRIENDLY_NAME_PROP_ID, value);
			}
		}
			
public: array<Byte>^ ToBytes()
		{
			DWORD size;
			std::auto_ptr<Byte> encoded(ToCBytes(&size));
			return InteropHelper::ToCSharpArray(encoded.get(),size);
		}
private: std::auto_ptr<Byte> ToCBytes(DWORD* size)
		 {
			 *size = handle->Handle->cbCtlEncoded;
			 std::auto_ptr<Byte> cpy(new Byte[*size]);
			 memcpy(cpy.get(), handle->Handle->pbCtlEncoded, *size);
			 return cpy;
		 }
public: static CTLContext^ Load(Byte* bytes, DWORD size)
		{
			SafeCTLHandle^ ctlContext = gcnew SafeCTLHandle(CertCreateCTLContext(DEFAULT_ENCODING, bytes, size));
			if(ctlContext->Handle == NULL)
				Win32::Throw();
			return gcnew CTLContext(ctlContext);
		}
public: static CTLContext^ Load(array<Byte>^ bytes)
		{
			return Load((Byte*)(void*)InteropHelper::ToCArray(bytes), bytes->Length);
		}


public: void ImportInStore(TStoreLocation location, TStoreName storeName)
		{
			msclr::interop::marshal_context mashal;
			SafeStoreHandle storeHandle(CertOpenStore(CERT_STORE_PROV_SYSTEM,
				0,
				NULL,
				location == TStoreLocation::LocalMachine ? CERT_SYSTEM_STORE_LOCAL_MACHINE : CERT_SYSTEM_STORE_CURRENT_USER,
				mashal.marshal_as<const wchar_t*,String^>(Enum::GetName(TStoreName::typeid, storeName)->ToUpper())));
			ASSERT_TRUE(CertAddCTLContextToStore(storeHandle.Handle,handle->Handle,CERT_STORE_ADD_REPLACE_EXISTING,NULL));
		}

public: void RemoveFromStore(TStoreLocation storeLocation, TStoreName storeName)
		{
			CTLContext::RemoveFromStore(storeLocation,storeName, CTLInfo->ListIdentifier);
		}
public: static void RemoveFromStore(TStoreLocation location, TStoreName storeName, String^ listIdentifier)
		{
			auto_handle<CTLContext> context(Get(location,storeName,listIdentifier));
			if(context.get() == nullptr)
				return;
			auto handle = CertDuplicateCTLContext(context->handle->Handle); //CertDeleteCTLFromStore Free the handle
			if(handle == NULL)
				Win32::Throw();
			ASSERT_TRUE(CertDeleteCTLFromStore(handle));
		}
public: static CTLContext^ Get(TStoreLocation location, TStoreName storeName, String^ listIdentifier)
		{
			if(listIdentifier == nullptr)
				return nullptr;
			msclr::interop::marshal_context mashal;
			SafeStoreHandle storeHandle(CertOpenStore(CERT_STORE_PROV_SYSTEM,
				0,
				NULL,
				location == TStoreLocation::LocalMachine ? CERT_SYSTEM_STORE_LOCAL_MACHINE : CERT_SYSTEM_STORE_CURRENT_USER,
				mashal.marshal_as<const WCHAR*,String^>(Enum::GetName(TStoreName::typeid, storeName)->ToUpper())));

			auto result = mashal.marshal_as<const WCHAR*,String^>(listIdentifier);
			CTL_FIND_USAGE_PARA para;
			ZeroMemory(&para,sizeof(CTL_FIND_USAGE_PARA));
			para.cbSize = sizeof(CTL_FIND_USAGE_PARA);
			para.ListIdentifier.cbData = (listIdentifier->Length + 1) * sizeof(WCHAR);
			para.ListIdentifier.pbData = (BYTE*)result;

			auto context = gcnew SafeCTLHandle(CertFindCTLInStore(storeHandle.Handle, DEFAULT_ENCODING,0,CTL_FIND_USAGE,&para,NULL));
			if(context->Handle == NULL)
			{
				auto error = GetLastError();
				if(error == CRYPT_E_NOT_FOUND)
					return nullptr;
				Win32::Throw(error);
			}
			return gcnew CTLContext(context);
		}
private: CTLInfo^ _CTLInfo;
public: property CTLInfo^ CTLInfo
		{
			TCTLInfo^ get()
			{
				return _CTLInfo;
			}
		}


		~CTLContext()
		{
			this->!CTLContext();
		}
		!CTLContext()
		{
			delete handle;
		}
};
public ref class CTLContextBuilder
{
public : CTLContextBuilder()
		 {
			 _CTLInfo = gcnew CTLInfoBuilder();
			 _Signers = gcnew List<X509Certificate2^>();
		 }


private: List<X509Certificate2^>^ _Signers;
public: property IList<X509Certificate2^>^ Signers
		{
			IList<X509Certificate2^>^ get()
			{
				return _Signers;
			}
		}

public: CTLContext^ ToCTLContext()
		{
			auto_handle<TCTLInfo> ctlInfo(_CTLInfo->ToCTLInfo());
			CMSG_SIGNED_ENCODE_INFO encodeInfo;
			ZeroMemory(&encodeInfo, sizeof(CMSG_SIGNED_ENCODE_INFO));
			encodeInfo.cbSize = sizeof(CMSG_SIGNED_ENCODE_INFO);

			mauto_handle<CERT_BLOB> certificates;
			if(_CTLInfo->Certificates->Count > 0)
			{
				certificates = mauto_handle<CERT_BLOB>(new CERT_BLOB[_CTLInfo->Certificates->Count]);
				ZeroMemory(certificates.get(), sizeof(CERT_BLOB) * _CTLInfo->Certificates->Count);
				encodeInfo.rgCertEncoded = certificates.get();
				encodeInfo.cCertEncoded = _CTLInfo->Certificates->Count;
				for(int i = 0; i < _CTLInfo->Certificates->Count; i++)
				{
					PCCERT_CONTEXT certHandle = (PCCERT_CONTEXT)_CTLInfo->Certificates[i]->Handle.ToPointer();
					encodeInfo.rgCertEncoded[i].cbData = certHandle->cbCertEncoded;
					encodeInfo.rgCertEncoded[i].pbData = certHandle->pbCertEncoded;
				}
			}
			mauto_handle<CMSG_SIGNER_ENCODE_INFO> signers;
			if(Signers->Count > 0)
			{
				signers = mauto_handle<CMSG_SIGNER_ENCODE_INFO>(new CMSG_SIGNER_ENCODE_INFO[Signers->Count]);
				ZeroMemory(signers.get(), sizeof(CMSG_SIGNER_ENCODE_INFO) * Signers->Count);
				encodeInfo.rgSigners = signers.get();
				encodeInfo.cSigners = Signers->Count;
				for(int i = 0; i < Signers->Count; i++)
				{
					PCCERT_CONTEXT certHandle = (PCCERT_CONTEXT)Signers[i]->Handle.ToPointer();
					HCRYPTPROV privateKey;
					BOOL freePrivateKey;
					DWORD privateKeyType;
					encodeInfo.rgSigners[i].cbSize = sizeof(CMSG_SIGNER_ENCODE_INFO);
					encodeInfo.rgSigners[i].pCertInfo = certHandle->pCertInfo;
					ASSERT_TRUE(CryptAcquireCertificatePrivateKey(
						certHandle,
						CRYPT_ACQUIRE_CACHE_FLAG,
						NULL,
						&privateKey,
						&privateKeyType,
						&freePrivateKey));
					encodeInfo.rgSigners[i].hCryptProv = privateKey;
					encodeInfo.rgSigners[i].dwKeySpec = privateKeyType;
					encodeInfo.rgSigners[i].HashAlgorithm.pszObjId = szOID_OIWSEC_sha1RSASign;
				}
			}

			DWORD flags = CMSG_ENCODE_SORTED_CTL_FLAG| CMSG_ENCODE_HASHED_SUBJECT_IDENTIFIER_FLAG;
			DWORD size;
			CryptMsgEncodeAndSignCTL(DEFAULT_ENCODING, (PCTL_INFO)ctlInfo->nativeCtlInfo.get(), &encodeInfo, flags,NULL,&size);
			std::auto_ptr<BYTE> encoded(new BYTE[size]);
			ASSERT_TRUE(CryptMsgEncodeAndSignCTL(DEFAULT_ENCODING, (PCTL_INFO)ctlInfo->nativeCtlInfo.get(), &encodeInfo, flags,encoded.get(),&size));
			return CTLContext::Load(encoded.get(), size);
		};

private: CTLInfoBuilder^ _CTLInfo;
public: property CTLInfoBuilder^ CTLInfo
		{
			CTLInfoBuilder^ get()
			{
				return _CTLInfo;
			}
		}
};



EndHttpSysManager