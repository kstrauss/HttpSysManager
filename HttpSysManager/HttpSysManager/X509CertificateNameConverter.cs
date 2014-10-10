using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Data;
using HttpSysManager.Core;

namespace HttpSysManager
{
	public class X509CertificateNameConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var cert = value as X509Certificate2;
			if(cert == null)
				return Binding.DoNothing;
			return CertificateHelper.GetFriendlyNameOrName(cert);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
