using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography.X509Certificates;
using HttpSysManager.HttpAPI;
using System.Text.RegularExpressions;
using HttpSysManager.Core;

namespace HttpSysManager
{
	/// <summary>
	/// Interaction logic for CertificateSelector.xaml
	/// </summary>
	public partial class CertificateSelector : UserControl
	{
		public CertificateSelector()
		{
			InitializeComponent();
			UpdateButton();
		}



		public X509Certificate2 SelectedCertificate
		{
			get
			{
				return (X509Certificate2)GetValue(SelectedCertificateProperty);
			}
			set
			{
				SetValue(SelectedCertificateProperty, value);
			}
		}

		public static readonly DependencyProperty SelectedCertificateProperty =
			DependencyProperty.Register("SelectedCertificate", typeof(X509Certificate2), typeof(CertificateSelector), new UIPropertyMetadata(OnSelectedCertificateChanged));

		static void OnSelectedCertificateChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			CertificateSelector sender = (CertificateSelector)source;
			sender.UpdateButton();
		}

		private void UpdateButton()
		{
			if(SelectedCertificate == null)
				button.Text = "Select a certificate";
			else
			{
				button.Text = CertificateHelper.GetFriendlyNameOrName(SelectedCertificate);
			}
		}
		private void ModifyCertificate_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new CertificateSelectorWindow();
			var result = dialog.ShowDialog();
			if(result != null && result.Value)
			{
				SelectedCertificate = dialog.SelectedCertificate;
			}
		}


	}
}
