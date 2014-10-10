using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace HttpSysManager
{
	public class CertificateHelper
	{
		public static X509Certificate2 GetCertificate(StoreLocation storeLocation, System.Security.Cryptography.X509Certificates.StoreName storeName, X509FindType x509FindType, object value)
		{
			X509Store store = new X509Store(storeLocation);
			store.Open(OpenFlags.ReadOnly);
			try
			{
				var certs = store.Certificates.Find(x509FindType, value, false);
				if(certs.Count == 0)
					return null;
				return certs[0];
			}
			finally
			{
				store.Close();
			}
		}
	}
}
