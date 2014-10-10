using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HttpSysManager.Native;
using Microsoft.Win32;
using SlasheneFramework;
using SlasheneFramework.UI;

namespace HttpSysManager
{
	/// <summary>
	/// Interaction logic for SSLInfoOptionWindow.xaml
	/// </summary>
	public partial class SSLInfoOptionWindow : Window
	{
		class SSLInfoViewModel : DynamicViewModel<SSLInfo>
		{
			class SaveCommand : ViewModelCommand<SSLInfoViewModel>
			{
				public SaveCommand(SSLInfoViewModel vm)
					: base(vm)
				{

				}
				public override bool CanExecute(object parameter)
				{
					return ViewModel.IsDirty && ViewModel.HasValidCTL;
				}

				public override void Execute(object parameter)
				{
					ViewModel.ApplyChanges();
					using(new HttpInitializeScope())
					{
						try
						{
							ViewModel.Instance.Update();
							ViewModel.IsDirty = false;

						}
						catch(Exception ex)
						{
							MessageBox.Show("Error " + ex.Message + ".\r\n This error comes most likely from a bug in HTTP.sys. To fix it, download and install KB981506 (http://support.microsoft.com/kb/981506)");
						}
					}
				}
			}

			public SSLInfoViewModel(SSLInfo info)
				: base(info)
			{

			}

			public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
			{
				IsDirty = true;
				_ChangedTracking[binder.Name] = value;
				return true;
			}
			public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
			{

				if(!_ChangedTracking.TryGetValue(binder.Name, out result))
				{
					base.TryGetMember(binder, out result);
				}
				return true;
			}

			Dictionary<string, object> _ChangedTracking = new Dictionary<string, object>();
			private bool _IsDirty;
			public bool IsDirty
			{
				get
				{
					return _IsDirty;
				}
				set
				{
					if(value != _IsDirty)
					{
						_IsDirty = value;
						OnPropertyChanged(() => this.IsDirty);
					}
				}
			}
			private ICommand _SaveCommand;
			public ICommand Save
			{
				get
				{
					if(_SaveCommand == null)
						_SaveCommand = new SaveCommand(this);
					return _SaveCommand;
				}
			}

			public class StoreNameViewModel
			{
				public StoreName? store;
				public StoreNameViewModel()
				{
				}
				public StoreNameViewModel(StoreName? store)
				{
					this.store = store;
				}

				public override string ToString()
				{
					if(!store.HasValue)
						return "Not assigned";
					return store.ToString();
				}
				public override bool Equals(object obj)
				{
					StoreNameViewModel item = obj as StoreNameViewModel;
					if(item == null)
						return false;
					return store.Equals(item.store);
				}
				public override int GetHashCode()
				{
					return store.GetHashCode();
				}
			}
			public IEnumerable<StoreNameViewModel> StoreNames
			{
				get
				{
					yield return new StoreNameViewModel();
					foreach(var value in Enum.GetValues(typeof(StoreName)).OfType<StoreName>())
						yield return new StoreNameViewModel(value);
				}
			}

			public StoreNameViewModel CTLStoreNameProxy
			{
				get
				{
					dynamic thisDynamic = this;
					return new StoreNameViewModel(thisDynamic.CTLStoreName);
				}
				set
				{
					dynamic thisDynamic = this;
					thisDynamic.CTLStoreName = value.store;
					if(value.store == null)
						CTLIdentifierProxy = null;
					OnPropertyChanged(() => this.HasCTLStore);
					OnPropertyChanged(() => this.HasValidCTL);
				}
			}

			public string CTLIdentifierProxy
			{
				get
				{
					dynamic thisDynamic = this;
					return thisDynamic.CTLIdentifier;
				}
				set
				{
					dynamic thisDynamic = this;
					thisDynamic.CTLIdentifier = value;
					CTLError = null;
					OnPropertyChanged(() => this.CTLIdentifierProxy);
					OnPropertyChanged(() => this.HasValidCTL);
				}
			}

			public bool HasCTLStore
			{
				get
				{
					return CTLStoreNameProxy.store != null;
				}
			}
			public bool HasValidCTL
			{
				get
				{
					var result = CTLStoreNameProxy.store == null || CTLContext.Get(StoreLocation.LocalMachine, CTLStoreNameProxy.store.Value, this.CTLIdentifierProxy) != null;
					return result;
				}
			}

			internal void ApplyChanges()
			{
				foreach(var change in _ChangedTracking)
				{
					Instance.GetType().GetProperty(change.Key).SetValue(
						Instance, change.Value, null);
				}
				_ChangedTracking.Clear();
			}



			internal void CreateAndImportCTL(CTLContextBuilder builder)
			{
				if(String.IsNullOrEmpty(builder.CTLInfo.ListIdentifier))
				{
					builder.CTLInfo.ListIdentifier = "New CTL";
				}


				using(var context = builder.ToCTLContext())
				{
					ImportCTL(context);
				}
			}

			internal void ImportCTL(CTLContext context)
			{
				if(context.CTLInfo.ListIdentifier == null)
				{
					CTLError = "The ListIdentifier of the CTL should be an unicode string to work correctly with http.sys";
					return;
				}
				CTLError = null;
				CTLIdentifierProxy = context.CTLInfo.ListIdentifier;
				context.ImportInStore(StoreLocation.LocalMachine, CTLStoreNameProxy.store.Value);
				OnPropertyChanged(() => this.HasValidCTL);
			}

			private string _CTLError;
			public string CTLError
			{
				get
				{
					return _CTLError;
				}
				set
				{
					if(value != _CTLError)
					{
						_CTLError = value;
						OnPropertyChanged(() => this.CTLError);
					}
				}
			}
		}
		public SSLInfoOptionWindow()
		{
			InitializeComponent();
		}

		public SSLInfo SSLInfo
		{
			get
			{
				return ViewModel.Instance;
			}
			set
			{
				root.DataContext = new SSLInfoViewModel(value);
			}
		}

		SSLInfoViewModel ViewModel
		{
			get
			{
				return ((SSLInfoViewModel)root.DataContext);
			}
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Import_CTL(object sender, RoutedEventArgs e)
		{
			if(ViewModel.CTLStoreNameProxy.store == null)
				return;
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "STL files (*.stl)|*.stl";
			fileDialog.CheckFileExists = true;
			fileDialog.CheckPathExists = true;
			var result = fileDialog.ShowDialog();
			if(result.HasValue && result.Value)
			{
				var context = CTLContext.Load(File.ReadAllBytes(fileDialog.FileName));
				ViewModel.ImportCTL(context);
			}
		}
		private void Create_CTL(object sender, RoutedEventArgs e)
		{
			if(ViewModel.CTLStoreNameProxy.store == null)
				return;
			CreateCTLWizard wizard = new CreateCTLWizard();
			CTLContextBuilder builder = new CTLContextBuilder();
			builder.CTLInfo.ListIdentifier = "New CTL";
			wizard.DataContext = builder;

			var result = wizard.ShowDialog();
			if(result.HasValue && result.Value)
			{
				ViewModel.CreateAndImportCTL(builder);
			}
		}
	}
}
