using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlasheneFramework;
using System.Windows.Input;
using SlasheneFramework.UI;
using HttpSysManager.HttpAPI;
using System.Collections.ObjectModel;
using ProcessHacker.Native.Security.AccessControl;
using ProcessHacker.Native.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using HttpSysManager.Native;

namespace HttpSysManager
{
	public class SSLInfoViewModel : NotifyPropertyChangedBase
	{
		MainWindowViewModel _Parent;
		public SSLInfoViewModel(SSLInfo info, MainWindowViewModel parent)
		{
			_Info = info;
			_Parent = parent;
		}

		
		private readonly SSLInfo _Info;
		public SSLInfo Info
		{
			get
			{
				return _Info;
			}
		}

		
	
		public void DeleteSSLInfo()
		{
			new HttpAPIManager().RemoveSSLInfo(Info.Endpoint);
			_Parent.SSLInfos.Remove(this);
		}
	}
	public class MainWindowViewModel : NotifyPropertyChangedBase
	{
		public static SecurityDescriptor CreateAllRightsToCurrentUser()
		{
			var descriptor = new SecurityDescriptor();
			descriptor.Dacl = new Acl(800);
			descriptor.Dacl.AddAccessAllowed((int)StandardRights.GenericExecute, Sid.CurrentUser);
			descriptor.Dacl.AddAccessAllowed((int)StandardRights.GenericWrite, Sid.CurrentUser);
			return descriptor;
		}
		class AddSSLCertCommand : ViewModelCommand<MainWindowViewModel>
		{
			public AddSSLCertCommand(MainWindowViewModel vm)
				: base(vm)
			{

			}
			public override bool CanExecute(object parameter)
			{
				IPAddress addr;
				return ViewModel.SelectedCertificate != null
					&& ViewModel.SSLIp != null
					&& IPAddress.TryParse(ViewModel.SSLIp, out addr)
					&& (ViewModel.SSLPort > 0 && ViewModel.SSLPort < UInt16.MaxValue)
					&& ViewModel.SSLInfos.Select(i=>i.Info.Endpoint).All(i=>!i.Equals(ViewModel.CreateSSLIPEndpoint()));
			}

			public override void Execute(object parameter)
			{
				if(CanExecute(null))
				{
					var info = new SSLInfo();
					info.SetCertificate(StoreName.My, ViewModel.SelectedCertificate.Thumbprint);
					new HttpAPIManager().SetSSLInfo(ViewModel.CreateSSLIPEndpoint(), info);
					ViewModel.SSLInfos.Add(new SSLInfoViewModel(info,ViewModel));
					ViewModel.SSLPort = 443;
					ViewModel.SSLIp = "";
					ViewModel.SelectedCertificate = null;
				}
			}
		}
		class CreateNewAclCommand : ViewModelCommand<MainWindowViewModel>
		{
			public CreateNewAclCommand(MainWindowViewModel vm)
				: base(vm)
			{
			}
			public override bool CanExecute(object parameter)
			{
				if(ViewModel.NewAcl == null)
					return false;
				var url = ViewModel.NewAcl;
				url = RemoveWildcards(url);
				var isUri = Uri.IsWellFormedUriString(url, UriKind.Absolute);
				if(!isUri)
					return false;
				var uri = new Uri(url, UriKind.Absolute);
				if(uri.Scheme != "https" && uri.Scheme != "http")
					return false;
				if(ViewModel.Acls.Any(a => a.Prefix.Equals(WithPort(WithTrailingSlash(ViewModel.NewAcl)), StringComparison.InvariantCultureIgnoreCase)))
					return false;
				return true;
			}


			private string RemoveWildcards(string url)
			{
				var parts = url.Split(new char[] { '/' });
				if(parts.Length < 3)
					return url;
				parts[2] = parts[2].Replace("*", "StAr").Replace("+", "PlUs");
				return String.Join("/", parts);
			}

			public override void Execute(object parameter)
			{
				ViewModel.NewAcl = WithPort(WithTrailingSlash(ViewModel.NewAcl));
				var manager = new HttpAPIManager();
				manager.SetUrlAcl(ViewModel.NewAcl, MainWindowViewModel.CreateAllRightsToCurrentUser());
				ViewModel.Acls.Insert(0, manager.GetAclInfo(ViewModel.NewAcl));
				ViewModel.NewAcl = "";
			}

			private string WithPort(string url)
			{
				var parts = url.Split(new char[] { '/' });
				if(parts.Length < 3)
					return url;
				if(!parts[2].Contains(':'))
					parts[2] = parts[2] + ':' + DefaultPort(parts[0]);
				return string.Join("/", parts);
			}

			private string DefaultPort(string scheme)
			{
				if(scheme.Equals("http:", StringComparison.InvariantCultureIgnoreCase))
					return "80";
				if(scheme.Equals("https:", StringComparison.InvariantCultureIgnoreCase))
					return "443";
				throw new NotSupportedException(scheme + " is not supported");
			}

			private string WithTrailingSlash(string url)
			{
				if(!url.EndsWith("/") && !url.EndsWith("\\"))
				{
					return url + "/";
				}
				return url;
			}
		}

		public MainWindowViewModel()
		{
			SSLPort = 443;
			_Acls = new ObservableCollection<UrlAcl>(new HttpAPIManager().GetAclInfo());
			_SSLInfos = new ObservableCollection<SSLInfoViewModel>(new HttpAPIManager().GetSSLInfos().Select(i => new SSLInfoViewModel(i,this)));
		}

		private ICommand _AddSSLCert;
		public ICommand AddSSLCert
		{
			get
			{
				if(_AddSSLCert == null)
					_AddSSLCert = new AddSSLCertCommand(this);
				return _AddSSLCert;
			}
		}
		private X509Certificate2 _SelectedCertificate;
		public X509Certificate2 SelectedCertificate
		{
			get
			{
				return _SelectedCertificate;
			}
			set
			{
				if(value != _SelectedCertificate)
				{
					_SelectedCertificate = value;
					OnPropertyChanged(() => this.SelectedCertificate);
				}
			}
		}
		private string _SSLIp;
		public string SSLIp
		{
			get
			{
				return _SSLIp;
			}
			set
			{
				if(value != _SSLIp)
				{
					_SSLIp = value;
					OnPropertyChanged(() => this.SSLIp);
				}
			}
		}
		private int _SSLPort;
		public int SSLPort
		{
			get
			{
				return _SSLPort;
			}
			set
			{
				if(value != _SSLPort)
				{
					_SSLPort = value;
					OnPropertyChanged(() => this.SSLPort);
				}
			}
		}

		private readonly ObservableCollection<SSLInfoViewModel> _SSLInfos;
		public ObservableCollection<SSLInfoViewModel> SSLInfos
		{
			get
			{
				return _SSLInfos;
			}
		}
		private string _NewAcl;
		public string NewAcl
		{
			get
			{
				return _NewAcl;
			}
			set
			{
				if(value != _NewAcl)
				{
					_NewAcl = value;
					OnPropertyChanged(() => this.NewAcl);
				}
			}
		}

		private ICommand _CreateNewAcl;
		public ICommand CreateNewAcl
		{
			get
			{
				if(_CreateNewAcl == null)
					_CreateNewAcl = new CreateNewAclCommand(this);
				return _CreateNewAcl;
			}
		}

		private ObservableCollection<UrlAcl> _Acls;
		public ObservableCollection<UrlAcl> Acls
		{
			get
			{
				return _Acls;
			}
		}

		internal IPEndPoint CreateSSLIPEndpoint()
		{
			return new IPEndPoint(IPAddress.Parse(SSLIp), SSLPort);
		}
	}
}
