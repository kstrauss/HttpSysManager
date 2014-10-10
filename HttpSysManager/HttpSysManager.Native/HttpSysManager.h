#pragma once

#define BeginHttpSysManager namespace HttpSysManager { namespace Native {
#define EndHttpSysManager } }


#include "mauto_ptr.h"
#define WIN32_LEAN_AND_MEAN 
#include <Windows.h>
#include <http.h>
#include <cryptuiapi.h>
#include <WinCrypt.h>
#include<msclr/auto_handle.h>
#include <memory>

BeginHttpSysManager
#define ASSERT_SUCCESS(result) ULONG r = result; if(r != ERROR_SUCCESS) Win32::Throw(r);
#define ASSERT_HTTPAPI_INITIALIZED(result) ULONG r = result; if(r == ERROR_INVALID_HANDLE) throw gcnew System::InvalidOperationException("You should first initialize HTTP Server API with HttpInitializeScope");
#define ASSERT_TRUE(result) if(!result) Win32::Throw();

using namespace System::Security::Cryptography::X509Certificates;
using namespace System;
using namespace System::Net;
using namespace System::Net::Sockets;
using namespace System::Runtime::InteropServices;
using namespace HttpSysManager::Core;
using namespace ProcessHacker::Native::Api;
using namespace ProcessHacker::Native;
using namespace System::ComponentModel;
using namespace ProcessHacker::Native::Security;
using namespace ProcessHacker::Native::Security::AccessControl;
using namespace System::Collections::Generic;

typedef System::Security::Cryptography::X509Certificates::StoreName TStoreName;
typedef System::Security::Cryptography::X509Certificates::StoreLocation TStoreLocation;
typedef System::Security::Cryptography::X509Certificates::X509FindType TX509FindType;

typedef HttpSysManager::Core::Thumbprint TThumbprint;
typedef System::Net::Sockets::AddressFamily TAddressFamily;


EndHttpSysManager
