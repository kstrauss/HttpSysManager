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
using ProcessHacker.Native.Security.AccessControl;
using ProcessHacker.Native.Security;
using ProcessHacker.Native.Api;
using System.Runtime.InteropServices;
using ProcessHacker.Native;
using HttpSysManager.HttpAPI;
using System.Net;
using System.ComponentModel;
using HttpSysManager.Native;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Microsoft.Win32;

namespace HttpSysManager
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainWindowViewModel();
		}

		public MainWindowViewModel ViewModel
		{
			set
			{
				DataContext = value;
			}
			get
			{
				return (MainWindowViewModel)DataContext;
			}
		}
		private void Permissions_Click(object sender, RoutedEventArgs e)
		{
			var acl = (UrlAcl)((FrameworkElement)sender).DataContext;
			SecurityEditor.EditSecurity(null, acl, acl.Prefix, GetAccessEntries());
		}

		private IEnumerable<AccessEntry> GetAccessEntries()
		{
			yield return new AccessEntry("Register", StandardRights.GenericExecute, true, true);
			yield return new AccessEntry("Delegate", StandardRights.GenericWrite, true, true);
		}

		private void Delete_Click(object sender, RoutedEventArgs e)
		{
			var acl = (UrlAcl)((FrameworkElement)sender).DataContext;
			RemoveACL(acl);
		}

		private void RemoveACL(UrlAcl acl)
		{
			new HttpAPIManager().RemoveUrlAcl(acl.Prefix);
			ViewModel.Acls.Remove(acl);
		}

		private void DeleteCertInfo_Click(object sender, RoutedEventArgs e)
		{
			var vm = (SSLInfoViewModel)(((Button)sender).DataContext);
			vm.DeleteSSLInfo();
		}
		private void OptionSSLInfo_Click(object sender, RoutedEventArgs e)
		{
			var vm = (SSLInfoViewModel)(((Button)sender).DataContext);
			SSLInfoOptionWindow options = new SSLInfoOptionWindow();
			options.SSLInfo = vm.Info;
			options.ShowDialog();
		}
		

		private void DeleteSSLInfo_KeyDown(object sender, KeyEventArgs e)
		{
			ListView view = (ListView)sender;
			if(e.Key == Key.Delete)
			{
				foreach(SSLInfoViewModel sslInfo in view.SelectedItems.OfType<SSLInfoViewModel>().ToList())
					sslInfo.DeleteSSLInfo();
				e.Handled = true;
			}
		}


		private void DeleteUrlACL_KeyDown(object sender, KeyEventArgs e)
		{
			ListView view = (ListView)sender;
			if(e.Key == Key.Delete)
			{
				foreach(UrlAcl acl in view.SelectedItems.OfType<UrlAcl>().ToList())
					RemoveACL(acl);
				e.Handled = true;
			}
		}

		private void OnNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
		const string DefaultFileName = "NewCTL";

		private void CreateCTLClick(object sender, RoutedEventArgs e)
		{
			CreateCTLWizard wizard = new CreateCTLWizard();
			var builder = new CTLContextBuilder();
			builder.CTLInfo.ListIdentifier = "New CTL";
			wizard.DataContext = builder;
			var result = wizard.ShowDialog();
			if(result.HasValue && result.Value)
			{
				SaveFileDialog save = new SaveFileDialog();
				var name = builder.CTLInfo.ListIdentifier;
				save.FileName = (String.IsNullOrEmpty(name) ? DefaultFileName : name) + ".stl";
				var saveResult = save.ShowDialog();
				if(saveResult.HasValue && saveResult.Value)
				{
					using(var context = builder.ToCTLContext())
					{
						File.WriteAllBytes(save.FileName, context.ToBytes());
					}
				}
			}
		}

	}
}
