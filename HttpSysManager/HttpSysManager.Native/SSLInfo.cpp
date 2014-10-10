#include "HttpSysManager.h"
#include "HttpApiHelper.cpp"
#include "ManagedSet.cpp"
#include "CTLInfo.cpp"

BeginHttpSysManager

	public ref class SSLInfo : public ManagedSet<HTTP_SERVICE_CONFIG_SSL_SET>
{

private: static System::Guid ToCLRGuid(GUID& guid)
		 {
			 return *reinterpret_cast<System::Guid*>(&guid);
		 }
private: static GUID ToNativeGuid(System::Guid& guid)
		 {
			 return *reinterpret_cast<GUID*>(&guid);
		 }


public: static SSLInfo^ Get(int index)
		{
			HTTP_SERVICE_CONFIG_SSL_QUERY query;
			query.dwToken = index;
			query.QueryDesc = HttpServiceConfigQueryNext;
			ULONG result;
			auto buffer = 
				HttpApiHelper::HttpQueryServiceConfiguration<HTTP_SERVICE_CONFIG_SSL_QUERY, HTTP_SERVICE_CONFIG_SSL_SET>(HttpServiceConfigSSLCertInfo, query, &result);
			if(result == ERROR_NO_MORE_ITEMS)
				return nullptr;
			ASSERT_SUCCESS(result);
			return gcnew SSLInfo(buffer);
		}
public: static SSLInfo^ Get(IPEndPoint^ endpoint)
		{
			HTTP_SERVICE_CONFIG_SSL_QUERY query;
			query.QueryDesc = HttpServiceConfigQueryExact;
			SSLInfo info;
			info.Endpoint = endpoint;
			query.KeyDesc.pIpPort = info.Set->KeyDesc.pIpPort;
			ULONG result;
			auto buffer = 
				HttpApiHelper::HttpQueryServiceConfiguration<HTTP_SERVICE_CONFIG_SSL_QUERY, HTTP_SERVICE_CONFIG_SSL_SET>(HttpServiceConfigSSLCertInfo, query, &result);
			if(result == ERROR_FILE_NOT_FOUND)
				return nullptr;
			ASSERT_SUCCESS(result);
			return gcnew SSLInfo(buffer);
		}
public: SSLInfo()
			:ManagedSet(HttpServiceConfigSSLCertInfo)
		{
			Init(TStoreName::My, nullptr);
		}
public: SSLInfo(String^ thumbprint)
			:ManagedSet(HttpServiceConfigSSLCertInfo)
		{
			Init(TStoreName::My, thumbprint);
		}
public: SSLInfo(TStoreName storeName, String^ thumbprint)
			:ManagedSet(HttpServiceConfigSSLCertInfo)
		{
			Init(storeName, thumbprint);
		}

public: SSLInfo(mauto_handle<HTTP_SERVICE_CONFIG_SSL_SET> set)
			:ManagedSet(set, HttpServiceConfigSSLCertInfo),
			marshal(gcnew msclr::interop::marshal_context())
		{
			InternalSetCertificate(Thumbprint,false);
		}

private: void Init(TStoreName storeName, String^ thumbprint)
		 {
			 marshal = auto_handle<msclr::interop::marshal_context>(gcnew msclr::interop::marshal_context());
			 StoreName = storeName;
			 if(thumbprint != nullptr)
				 Thumbprint = gcnew TThumbprint(thumbprint);
		 }



private: LocalMemoryAlloc^ _Thumbprint;
public: property TThumbprint^ Thumbprint
		{
			TThumbprint^ get()
			{
				return gcnew TThumbprint((BYTE*)Set->ParamDesc.pSslHash, Set->ParamDesc.SslHashLength);
			}
			void set(TThumbprint^ value)
			{
				InternalSetCertificate(value);
				SetThumbprint(value);
			}
		}

		typedef CTLInfo TCTLInfo;

private: CTLContext^ _CTLContext;

private: LocalMemoryAlloc^ _CTLListIdentifier;
		 //public: property CTLContext^ CTLInfo
		 //		{
		 //			CTLContext^ get()
		 //			{
		 //				return _CTLContext;
		 //			}
		 //			void set(CTLContext^ value)
		 //			{
		 //				_CTLContext = value;
		 //				if(_CTLContext == nullptr || _CTLContext->Store == nullptr)
		 //				{
		 //					Set->ParamDesc.pDefaultSslCtlStoreName = NULL;
		 //					Set->ParamDesc.pDefaultSslCtlIdentifier = NULL;
		 //				}
		 //				else
		 //				{
		 //					if(_CTLContext->Store->Name != nullptr)
		 //					{
		 //						_CTLStoreName = gcnew LocalMemoryAlloc(Marshal::StringToHGlobalUni(_CTLContext->Store->Name));
		 //						Set->ParamDesc.pSslCertStoreName = (PWSTR)(void*)_CTLStoreName;
		 //					}
		 //					else
		 //					{
		 //						Set->ParamDesc.pDefaultSslCtlStoreName = NULL;
		 //					}
		 //					//_CTLStoreName = gcnew LocalMemoryAlloc(InteropHelper::ToCArray(_CTLInfo->ListIdentifier));
		 //				}
		 //				if(_CTLContext != nullptr && !String::IsNullOrEmpty(_CTLContext->CTLInfo->ListIdentifier))
		 //				{
		 //					_CTLListIdentifier = gcnew LocalMemoryAlloc(Marshal::StringToHGlobalUni(_CTLContext->CTLInfo->ListIdentifier));
		 //					Set->ParamDesc.pDefaultSslCtlIdentifier = (PWSTR)(void*)_CTLListIdentifier;
		 //				}
		 //				else
		 //				{
		 //					Set->ParamDesc.pDefaultSslCtlIdentifier = NULL;
		 //				}
		 //			}
		 //		}

		 auto_handle<msclr::interop::marshal_context> marshal;
public: property String^ CTLIdentifier
		{
			String^ get()
			{
				if(Set->ParamDesc.pDefaultSslCtlIdentifier == NULL)
					return nullptr;
				return Marshal::PtrToStringUni(IntPtr(Set->ParamDesc.pDefaultSslCtlIdentifier));
			}
			void set(String^ value)
			{
				if(String::IsNullOrEmpty(value))
				{
					Set->ParamDesc.pDefaultSslCtlIdentifier = NULL;
					return;
				}
				Set->ParamDesc.pDefaultSslCtlIdentifier = (PWSTR)marshal->marshal_as<const wchar_t*, String^>(value); 
			}
		}

		private: LocalMemoryAlloc^ _CTLStoreName;
public: property Nullable<TStoreName> CTLStoreName
		{
			Nullable<TStoreName> get()
			{
				if(Set->ParamDesc.pDefaultSslCtlStoreName == NULL)
				{
					return Nullable<TStoreName>();
				}
				String^ store = Marshal::PtrToStringUni(IntPtr(Set->ParamDesc.pDefaultSslCtlStoreName));
				return Nullable<TStoreName>((TStoreName)Enum::Parse(TStoreName::typeid,store,true));
			}
			void set(Nullable<TStoreName> value)
			{
				if(!value.HasValue)
				{
					Set->ParamDesc.pDefaultSslCtlStoreName = NULL;
					return;
				}
				String^ str = Enum::GetName(TStoreName::typeid, value.Value)->ToUpper();
				_CTLStoreName =  gcnew LocalMemoryAlloc(IntPtr((PWSTR)Marshal::StringToHGlobalUni(str).ToPointer()));
				Set->ParamDesc.pDefaultSslCtlStoreName = (PWSTR)(void*)_CTLStoreName;
			}
		}

private: void SetThumbprint(TThumbprint^ value)
		 {
			 if(value == nullptr)
			 {
				 Set->ParamDesc.SslHashLength = 0;
				 return;
			 }

			 Set->ParamDesc.SslHashLength = value->Data->Length;
			 _Thumbprint = InteropHelper::ToCArray(value->Data);
			 Set->ParamDesc.pSslHash = (void*)_Thumbprint;
		 }
private: void InternalSetCertificate(TThumbprint^ thumbprint)
		 {
			 InternalSetCertificate(thumbprint,true);
		 }
private: void InternalSetCertificate(TThumbprint^ thumbprint, bool throwOnNotFound)
		 {
			 if(thumbprint == nullptr)
			 {
				 _Certificate = nullptr;
				 return;
			 }
			 auto cert = CertificateHelper::GetCertificate(TStoreLocation::LocalMachine, StoreName, TX509FindType::FindByThumbprint, thumbprint->ToString());
			 if(cert == nullptr && throwOnNotFound)
			 {
				 throw gcnew ArgumentException("Certificate " + thumbprint + " not found in the store " + Enum::GetName(TStoreName::typeid, StoreName) + " of the local machine");
			 }
			 _Certificate = cert;
		 }

public: void SetCertificate(TStoreName store, String^ thumbprint)
		{
			Certificate = CertificateHelper::GetCertificate(TStoreLocation::LocalMachine, StoreName, TX509FindType::FindByThumbprint, thumbprint);
		}
private: X509Certificate2^ _Certificate;
public: property X509Certificate2^ Certificate
		{
			X509Certificate2^ get()
			{
				return _Certificate;
			}
			void set(X509Certificate2^ value)
			{
				TThumbprint^ thumbprint = gcnew TThumbprint(value->Thumbprint);
				InternalSetCertificate(thumbprint);
				SetThumbprint(thumbprint);
			}
		}

private: IPEndPoint^ _Endpoint;
public: property IPEndPoint^ Endpoint
		{
			IPEndPoint^ get()
			{
				BYTE* socketAddress = (BYTE*)Set->KeyDesc.pIpPort;
				auto family = (TAddressFamily)((int)socketAddress[0] | (int)socketAddress[1] << 8);
				if(family == TAddressFamily::InterNetworkV6)
				{
					array<Byte>^ arr = gcnew array<Byte>(16);
					for(int i = 0 ; i < arr->Length ; i++)
					{
						arr[i] = socketAddress[i + 8];
					}
					int port = ((int)socketAddress[2] << 8 & 65280) | (int)socketAddress[3];
					UInt64 scopeid = (UInt64)(((int)socketAddress[27] << 24) + ((int)socketAddress[26] << 16) + ((int)socketAddress[25] << 8) + (int)socketAddress[24]);
					return gcnew IPEndPoint(gcnew IPAddress(arr, scopeid), port);
				}
				if(family != TAddressFamily::InterNetwork)
					throw gcnew NotSupportedException(family.ToString());
				int port2 = ((int)socketAddress[2] << 8 & 65280) | (int)socketAddress[3];
				long address = (long)((int)(socketAddress[4] & 255) | ((int)socketAddress[5] << 8 & 65280) | ((int)socketAddress[6] << 16 & 16711680) | (int)socketAddress[7] << 24) & (UInt64)-1;
				return gcnew IPEndPoint(address, port2);
			}
			void set(IPEndPoint^ value)
			{
				SocketAddress^ address = value->Serialize();
				_SocketAddress = mauto_handle<BYTE>(new BYTE[address->Size]());
				Set->KeyDesc.pIpPort = (PSOCKADDR)_SocketAddress.get();
				BYTE* socketBytes=(BYTE*)_SocketAddress.get();
				for(int i = 0; i < address->Size;i++)
					socketBytes[i] = address[i];
			}
		}
		mauto_handle<BYTE> _SocketAddress;
public: property Guid AppId
		{
			Guid get()
			{
				return ToCLRGuid(Set->ParamDesc.AppId);
			}
			void set(Guid value)
			{
				Set->ParamDesc.AppId = ToNativeGuid(value);
			}
		}

private: LocalMemoryAlloc^ _StoreName;
public: property TStoreName StoreName
		{
			TStoreName get()
			{
				if(Set->ParamDesc.pSslCertStoreName == NULL)
				{
					return TStoreName::My;
				}
				String^ store = Marshal::PtrToStringUni(IntPtr(Set->ParamDesc.pSslCertStoreName));
				return (TStoreName)Enum::Parse(TStoreName::typeid,store,true);
			}
			void set(TStoreName value)
			{
				String^ str = Enum::GetName(TStoreName::typeid, value)->ToUpper();
				_StoreName =  gcnew LocalMemoryAlloc(IntPtr((PWSTR)Marshal::StringToHGlobalUni(str).ToPointer()));
				Set->ParamDesc.pSslCertStoreName = (PWSTR)(void*)_StoreName;
			}
		}
public: property bool NegotiateClientCert
		{
			bool get()
			{
				return (Set->ParamDesc.DefaultFlags & HTTP_SERVICE_CONFIG_SSL_FLAG_NEGOTIATE_CLIENT_CERT) != 0;
			}
			void set(bool value)
			{
				if(value)
					Set->ParamDesc.DefaultFlags |= HTTP_SERVICE_CONFIG_SSL_FLAG_NEGOTIATE_CLIENT_CERT;
				else
					Set->ParamDesc.DefaultFlags &= ~HTTP_SERVICE_CONFIG_SSL_FLAG_NEGOTIATE_CLIENT_CERT;
			}
		}
public: property bool CertificateAccountMapping
		{
			bool get()
			{
				return (Set->ParamDesc.DefaultFlags & HTTP_SERVICE_CONFIG_SSL_FLAG_USE_DS_MAPPER) != 0;
			}
			void set(bool value)
			{
				if(value)
					Set->ParamDesc.DefaultFlags |= HTTP_SERVICE_CONFIG_SSL_FLAG_USE_DS_MAPPER;
				else
					Set->ParamDesc.DefaultFlags &= ~HTTP_SERVICE_CONFIG_SSL_FLAG_USE_DS_MAPPER;
			}
		}
public: property bool CheckClientCertificate
		{
			bool get()
			{
				return Set->ParamDesc.DefaultCertCheckMode == 0;
			}
			void set(bool value)
			{
				Set->ParamDesc.DefaultCertCheckMode = value ? 0 : 1;
			}
		}
};

EndHttpSysManager