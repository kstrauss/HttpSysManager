using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using HttpSysManager.HttpAPI;
using ProcessHacker.Native.Security.AccessControl;
using ProcessHacker.Native.Security;
using System.Runtime.InteropServices;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using HttpSysManager.Native;
using System.IO;
using HttpSysManager.Core;

namespace HttpSysManager.Tests
{
	public class TestAcl : IDisposable
	{
		readonly string _Url;
		public string Url
		{
			get
			{
				return _Url;
			}
		}
		public TestAcl()
		{
			_Url = "http://+:9393/";
			HttpAPIManager manager = new HttpAPIManager();
			manager.SetUrlAcl(_Url, SecurityDescriptor.FromSDDL("D:(A;;GX;;;WD)"));
		}
		#region IDisposable Members

		public void Dispose()
		{
			HttpAPIManager manager = new HttpAPIManager();
			manager.RemoveUrlAcl(_Url);
		}

		#endregion
	}

	public class TestClass
	{
		[Fact]
		public void CanQueryAcls()
		{
			HttpAPIManager manager = new HttpAPIManager();
			var testUrl = "http://+:9399/CanQueryAclsTest";
			manager.SetUrlAcl(testUrl, SecurityDescriptor.FromSDDL("D:(A;;;;;WD)"));
			try
			{
				var containsTemporaryUrl = manager.GetAclInfo().Any(url => url.Prefix.Contains("CanQueryAclsTest"));
				Assert.True(containsTemporaryUrl, "You should be able to retrieve Acls");
			}
			finally
			{
				manager.RemoveUrlAcl(testUrl);
			}
		}
		[Fact]
		public void CanAddAndRemoveAcl()
		{
			var testUrl = "http://+:9393/";
			HttpAPIManager manager = new HttpAPIManager();
			manager.SetUrlAcl(testUrl, SecurityDescriptor.FromSDDL("D:(A;;GX;;;WD)"));
			try
			{
				var acl = manager.GetAclInfo(testUrl);
				Assert.NotNull(acl);
				var fakeAcl = manager.GetAclInfo("unknown");
				Assert.Null(fakeAcl);
			}
			finally
			{
				manager.RemoveUrlAcl(testUrl);
			}
			var oldAcl = manager.GetAclInfo(testUrl);
			Assert.Null(oldAcl);
		}

		[Fact]
		public void HasMeaningfulErrorMessageIfHttpApiNotInitialized()
		{
			try
			{
				SSLInfo.Get(0);
			}
			catch(InvalidOperationException ex)
			{
				Assert.Equal("You should first initialize HTTP Server API with HttpInitializeScope", ex.Message);
			}
		}
		[Fact]
		public void CanSetUrlTwiceAndSetToNullToRemove()
		{
			var testUrl = "http://+:9394/";
			HttpAPIManager manager = new HttpAPIManager();
			manager.SetUrlAcl(testUrl, SecurityDescriptor.FromSDDL("D:(A;;GX;;;WD)"));
			manager.SetUrlAcl(testUrl, SecurityDescriptor.FromSDDL("D:(A;;;;;WD)"));
			Assert.Equal("D:(A;;;;;WD)", manager.GetAclInfo(testUrl).SecurityDescriptor.ToString());
			manager.SetUrlAcl(testUrl, null);
			Assert.Null(manager.GetAclInfo(testUrl));
		}
		
		[Fact]
		public void CanSetCertificates()
		{
			var testEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 462);
			HttpAPIManager manager = new HttpAPIManager();
			manager.SetSSLInfo(testEndpoint, new SSLInfo(TestCertificate.Thumbprint));
			try
			{
				var sslInfo = manager.GetSSLInfo(testEndpoint);
				Assert.NotNull(sslInfo);
				Assert.Equal(TestCertificate.Thumbprint, sslInfo.Certificate.Thumbprint);
			}
			finally
			{
				manager.RemoveSSLInfo(testEndpoint);
				var sslInfo = manager.GetSSLInfo(testEndpoint);
				Assert.Null(sslInfo);
			}
		}
		[Fact]
		public void CanListCertificates()
		{
			var testEndpoints = new[]
			{ 
				new IPEndPoint(IPAddress.Parse("127.0.0.1"), 462),
				new IPEndPoint(IPAddress.Parse("127.0.0.2"), 462)
			};
			HttpAPIManager manager = new HttpAPIManager();
			var oldSslCount = manager.GetSSLInfos().Count();
			try
			{
				foreach(var endpoint in testEndpoints)
					manager.SetSSLInfo(endpoint, new SSLInfo(TestCertificate.Thumbprint));
				var newSslCount = manager.GetSSLInfos().Count();
				Assert.Equal(oldSslCount + testEndpoints.Length, newSslCount);
			}
			finally
			{
				foreach(var endpoint in testEndpoints)
					manager.RemoveSSLInfo(endpoint);
			}
		}

		[Fact]
		public void CanSetSSLInfoTwiceAndSetToNullToRemove()
		{
			var testEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 464);
			var info = new SSLInfo(TestCertificate.Thumbprint);

			HttpAPIManager manager = new HttpAPIManager();

			manager.SetSSLInfo(testEndpoint, info);
			try
			{
				Assert.NotNull(manager.GetSSLInfo(testEndpoint));
				Assert.Equal(false, manager.GetSSLInfo(testEndpoint).NegotiateClientCert);
				info.NegotiateClientCert = true;
				manager.SetSSLInfo(testEndpoint, info);
				Assert.NotNull(manager.GetSSLInfo(testEndpoint));
				Assert.Equal(true, manager.GetSSLInfo(testEndpoint).NegotiateClientCert);
			}
			finally
			{
				manager.SetSSLInfo(testEndpoint, null);
				Assert.Null(manager.GetSSLInfo(testEndpoint));
			}
		}
		public static X509Certificate2 TestCertificate
		{
			get
			{
				return new X509Certificate2("../../Certificates/HttpSysManager.pfx", "", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);
			}
		}
		public static X509Certificate2 Verisign
		{
			get
			{
				return CertificateHelper.GetCertificate(StoreLocation.LocalMachine, StoreName.Root, X509FindType.FindByThumbprint, "13 2d 0d 45 53 4b 69 97 cd b2 d5 c3 39 e2 55 76 60 9b 5c c6");
			}
		}

		public static X509Certificate2 KisaRoot1
		{
			get
			{
				return CertificateHelper.GetCertificate(StoreLocation.LocalMachine, StoreName.Root, X509FindType.FindByThumbprint, "02 72 68 29 3e 5f 5d 17 aa a4 b3 c3 e6 36 1e 1f 92 57 5e aa");
			}
		}
		

		[Fact]
		public void CanAttachCTLToSSLInfo()
		{
			var testEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2924);
			CTLContextBuilder builder = new CTLContextBuilder();
			builder.CTLInfo.ListIdentifier = "my test list binding";
			builder.CTLInfo.Certificates.Add(Verisign);
			builder.CTLInfo.Certificates.Add(KisaRoot1);
			builder.Signers.Add(TestCertificate);
			HttpAPIManager manager = new HttpAPIManager();
			var context = builder.ToCTLContext();

			try
			{
				context.ImportInStore(StoreLocation.LocalMachine, StoreName.Root);
				SSLInfo info = new SSLInfo();
				info.CheckClientCertificate = false;
				info.NegotiateClientCert = true;
				info.CTLIdentifier = context.CTLInfo.ListIdentifier;
				info.CTLStoreName = StoreName.Root;
				info.Certificate = TestCertificate;
				manager.SetSSLInfo(testEndpoint, info);
			}
			finally
			{
				context.RemoveFromStore(StoreLocation.LocalMachine, StoreName.Root);
				manager.RemoveSSLInfo(testEndpoint);
			}
		}

		public byte[] AuthRootCTL
		{
			get
			{
				return File.ReadAllBytes("../../authroot.stl");
			}
		}


		[Fact]
		public void CanReadExistingCTL()
		{
			var ctl = CTLContext.Load(AuthRootCTL);
			Assert.NotNull(ctl);
		}
		[Fact]
		public void CanCreateCTL()
		{
			var ctlBuilder = new CTLInfoBuilder();
			ctlBuilder.ListIdentifier = "Test list";
			ctlBuilder.Certificates.Add(TestCertificate);
			var ctlInfo = ctlBuilder.ToCTLInfo();
			var bytes = ctlInfo.ToBytes();
			var loadedCtlInfo = CTLInfo.Load(bytes);
			AssertCollectionEquals(TestCertificate.GetCertHash(), loadedCtlInfo.Entries[0].SubjectIdentifier);
			Assert.Equal("Test list", ctlInfo.ListIdentifier);
		}
		[Fact]
		public void CanCreateSerializeAndReloadCTLContext()
		{
			var ctlBuilder = new CTLContextBuilder();
			ctlBuilder.CTLInfo.Certificates.Add(TestCertificate);
			ctlBuilder.CTLInfo.ListIdentifier = "test list";
			var ctlContext = ctlBuilder.ToCTLContext();
			Assert.Equal("test list", ctlContext.FriendlyName);
			var bytes = ctlContext.ToBytes();
			var loadedCtlContext = CTLContext.Load(bytes);
			Assert.Equal("test list", loadedCtlContext.FriendlyName);
			AssertCollectionEquals(TestCertificate.GetCertHash(), loadedCtlContext.CTLInfo.Entries[0].SubjectIdentifier);
		}

		[Fact]
		public void CanImportLoadThenDeleteCTLInStore()
		{
			var ctlBuilder = new CTLContextBuilder();
			ctlBuilder.CTLInfo.Certificates.Add(TestCertificate);
			ctlBuilder.CTLInfo.ListIdentifier = "test list import";
			var ctlContext = ctlBuilder.ToCTLContext();
			ctlContext.ImportInStore(StoreLocation.CurrentUser, StoreName.My);
			try
			{
				Assert.NotNull(CTLContext.Get(StoreLocation.CurrentUser, StoreName.My, "test list import"));
				ctlContext.RemoveFromStore(StoreLocation.CurrentUser, StoreName.My);
				Assert.Null(CTLContext.Get(StoreLocation.CurrentUser, StoreName.My, "test list import"));
				ctlContext.RemoveFromStore(StoreLocation.CurrentUser, StoreName.My); //Does not throw if does not exist
			}
			finally
			{
				ctlContext.RemoveFromStore(StoreLocation.CurrentUser, StoreName.My);
			}
		}


		void AssertCollectionEquals(byte[] expected, byte[] actual)
		{
			Assert.True(expected.Length == actual.Length, "both array should be same size");
			for(int i = 0 ; i < expected.Length ; i++)
				Assert.True(expected[i] == actual[i], "Difference spotted at index " + i.ToString());
		}
		//[Fact]
		//public void CanAddEmptyAcl()
		//{
		//	var testUrl = "http://test2:3827/";
		//	HttpAPIManager manager = new HttpAPIManager();
		//	try
		//	{
		//		manager.SetUrlAcl(testUrl, SecurityDescriptor.Empty);
		//		var acl = manager.GetAclInfo(testUrl);
		//		Assert.Equal(SecurityDescriptor.Empty.ToString(), acl.SecurityDescriptor.ToString());
		//	}
		//	finally
		//	{
		//		manager.RemoveUrlAcl(testUrl);
		//	}
		//}

		//[Fact]
		//public void CanBrowseCertificates()
		//{
		//	HttpAPIManager manager = new HttpAPIManager();
		//	Assert.True(1 <= manager.GetSSLInfos().Count());
		//}
	}
}
