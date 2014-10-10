using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace HttpSysManager.Core
{
	public class CertificateHelper
	{
		public static X509Certificate2 GetCertificate(StoreLocation storeLocation, System.Security.Cryptography.X509Certificates.StoreName storeName, X509FindType x509FindType, object value)
		{
			X509Store store = new X509Store(storeName, storeLocation);
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

		public static string GetFriendlyNameOrName(X509Certificate2 SelectedCertificate)
		{
			if(!string.IsNullOrEmpty(SelectedCertificate.FriendlyName))
				return SelectedCertificate.FriendlyName;
			else
			{
				var match = Regex.Match(SelectedCertificate.Subject, "^(CN=)?(?<Name>[^,]*)", RegexOptions.IgnoreCase);
				if(!match.Success)
					return SelectedCertificate.Subject;
				return match.Groups["Name"].Value.Trim();
			}
		}
	}
}
