using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using HttpSysManager.HttpAPI;
using System.Security.Cryptography.X509Certificates;

namespace HttpSysManager.Tests
{
	class CertificateInstallation : IDisposable
	{
		private X509Certificate2 certificate;
		StoreName storeName;
		StoreLocation location;

		public CertificateInstallation(X509Certificate2 certificate, StoreLocation location, StoreName storeName)
		{
			this.certificate = certificate;
			this.location = location;
			this.storeName = storeName;
			Install(certificate);
		}

		private void Install(X509Certificate2 certificate)
		{
			X509Store store = new X509Store(storeName, location);
			store.Open(OpenFlags.ReadWrite);
			try
			{
				var foundCert = store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, false).OfType<X509Certificate2>().FirstOrDefault();
				if(foundCert != null)
					return;
				store.Add(certificate);
			}
			finally
			{
				store.Close();
			}
		}
		#region IDisposable Members

		public void Dispose()
		{
			X509Store store = new X509Store(storeName, location);
			store.Open(OpenFlags.ReadWrite);
			try
			{
				var foundCert = store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, false).OfType<X509Certificate2>().FirstOrDefault();
				if(foundCert != null)
				{
					store.Remove(foundCert);
				}
			}
			finally
			{
				store.Close();
			}
		}

		#endregion
	}
	class Program : ITestMethodRunnerCallback
	{
		static void Main(string[] args)
		{
			using(AssertIsInstalled(TestClass.TestCertificate))
			{
				var env = new MultiAssemblyTestEnvironment();
				env.Load(Assembly.GetExecutingAssembly().Location);
				var methods = env.EnumerateTestMethods();
				env.Run(methods, new Program());
			}
		}

		private static IDisposable AssertIsInstalled(X509Certificate2 certificate)
		{
			return new CertificateInstallation(certificate, StoreLocation.LocalMachine, StoreName.My);
		}


		#region ITestMethodRunnerCallback Members

		public void AssemblyFinished(TestAssembly testAssembly, int total, int failed, int skipped, double time)
		{

		}

		public void AssemblyStart(TestAssembly testAssembly)
		{

		}

		public bool ClassFailed(Xunit.TestClass testClass, string exceptionType, string message, string stackTrace)
		{
			return false;
		}

		public void ExceptionThrown(TestAssembly testAssembly, Exception exception)
		{

		}

		public bool TestFinished(TestMethod testMethod)
		{
			if(testMethod.RunStatus == TestStatus.Passed)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				WriteLine(testMethod.MethodName + " passed");
			}
			if(testMethod.RunStatus == TestStatus.Failed)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				WriteLine(testMethod.MethodName + " failed");
			}
			Console.ForegroundColor = ConsoleColor.White;
			WriteLine(GetLog(testMethod));
			return true;
		}

		private string GetLog(TestMethod testMethod)
		{
			var result = testMethod.RunResults.First();
			if(result is TestPassedResult)
			{
				return ((TestPassedResult)result).Output;
			}
			else if(result is TestFailedResult)
			{
				return ((TestFailedResult)result).Output;
			}
			return "";
		}

		private void WriteLine(string str)
		{
			Console.WriteLine(str);
			Trace.WriteLine(str);
		}

		public bool TestStart(TestMethod testMethod)
		{
			return true;
		}

		#endregion
	}
}
