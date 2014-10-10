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
using System.Windows.Shapes;
using System.Security.Cryptography.X509Certificates;
using HttpSysManager.HttpAPI;
using SlasheneFramework;
using SlasheneFramework.UI;
using System.IO;
using HttpSysManager.Native;

namespace HttpSysManager
{
	/// <summary>
	/// Interaction logic for CertificateSelectorWindow.xaml
	/// </summary>
	public partial class CertificateSelectorWindow : Window
	{
		const string NoPrivateKeyAttached = "The selected certificate does not have any private key attached";
		class CertificateSelectorWindowVm : NotifyPropertyChangedBase
		{

			public CertificateSelectorWindow Window
			{
				get;
				private set;
			}

			public CertificateSelectorWindowVm(CertificateSelectorWindow parent)
			{
				CertificatePath = "";
				Password = "";
				Window = parent;
			}
			class SubmitCommand : ViewModelCommand<CertificateSelectorWindowVm>
			{
				public SubmitCommand(CertificateSelectorWindowVm vm)
					: base(vm)
				{
				}
				public override bool CanExecute(object parameter)
				{
					return File.Exists(ViewModel.CertificatePath);
				}

				public override void Execute(object parameter)
				{
					try
					{
						X509Certificate2 certificate = new X509Certificate2(ViewModel.CertificatePath, ViewModel.Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

						if(ViewModel.Window.ShouldHavePrivateKey && !certificate.HasPrivateKey)
						{
							ViewModel.ErrorMessage = CertificateSelectorWindow.NoPrivateKeyAttached;
							return;
						}

						if(ViewModel.Window.ShouldImportCertificate)
						{
							X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
							store.Open(OpenFlags.ReadWrite);
							try
							{
								store.Add(certificate);
							}
							finally
							{
								store.Close();
							}
						}
						ViewModel.SelectedCertificate = certificate;
					}
					catch(Exception ex)
					{
						ViewModel.ErrorMessage = ex.Message;
					}
				}
			}
			private ICommand _Submit;
			public ICommand Submit
			{
				get
				{
					if(_Submit == null)
						_Submit = new SubmitCommand(this);
					return _Submit;
				}
			}
			private String _CertificatePath;
			public String CertificatePath
			{
				get
				{
					return _CertificatePath;
				}
				set
				{
					if(value != _CertificatePath)
					{
						_CertificatePath = value;
						OnPropertyChanged(() => this.CertificatePath);
					}
				}
			}
			private string _Password;
			public string Password
			{
				get
				{
					return _Password;
				}
				set
				{
					if(value != _Password)
					{
						_Password = value;
						OnPropertyChanged(() => this.Password);
					}
				}
			}

			private string _ErrorMessage;
			public string ErrorMessage
			{
				get
				{
					return _ErrorMessage;
				}
				set
				{
					if(value != _ErrorMessage)
					{
						_ErrorMessage = value;
						OnPropertyChanged(() => this.ErrorMessage);
					}
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
		}
		public CertificateSelectorWindow()
		{
			InitializeComponent();
			ViewModel = new CertificateSelectorWindowVm(this);
		}




	

		public bool ShouldHavePrivateKey
		{
			get
			{
				return (bool)GetValue(ShouldHavePrivateKeyProperty);
			}
			set
			{
				SetValue(ShouldHavePrivateKeyProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for ShouldHavePrivateKey.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShouldHavePrivateKeyProperty =
			DependencyProperty.Register("ShouldHavePrivateKey", typeof(bool), typeof(CertificateSelectorWindow), new PropertyMetadata(true));




		public bool ShouldImportCertificate
		{
			get
			{
				return (bool)GetValue(ShouldImportCertificateProperty);
			}
			set
			{
				SetValue(ShouldImportCertificateProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for ShouldImportCertificate.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShouldImportCertificateProperty =
			DependencyProperty.Register("ShouldImportCertificate", typeof(bool), typeof(CertificateSelectorWindow), new PropertyMetadata(true));



		CertificateSelectorWindowVm ViewModel
		{
			get
			{
				return (CertificateSelectorWindowVm)DataContext;
			}
			set
			{
				value.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(value_PropertyChanged);
				DataContext = value;
			}
		}

		void value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "SelectedCertificate")
			{
				SelectedCertificate = ViewModel.SelectedCertificate;
				DialogResult = true;
			}
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
			DependencyProperty.Register("SelectedCertificate", typeof(X509Certificate2), typeof(CertificateSelectorWindow), new UIPropertyMetadata(null));

		private void SelectCertificate_Click(object sender, RoutedEventArgs e)
		{
			SelectedCertificate = Security.SelectCertificate();
			if(SelectedCertificate != null)
			{
				if(ShouldHavePrivateKey && !SelectedCertificate.HasPrivateKey)
				{
					storeErrorMessage.Text = NoPrivateKeyAttached;
					return;
				}
				DialogResult = true;
			}
		}


	}
}
