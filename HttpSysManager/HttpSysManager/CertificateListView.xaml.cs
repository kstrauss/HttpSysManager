using System;
using System.Collections;
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

namespace HttpSysManager
{
	/// <summary>
	/// Interaction logic for CertificateListView.xaml
	/// </summary>
	public partial class CertificateListView : UserControl
	{
		public CertificateListView()
		{
			InitializeComponent();
			root.SetBinding(DataContextProperty, new Binding()
			{
				Source = this
			});
		}




		public string Label
		{
			get
			{
				return (string)GetValue(LabelProperty);
			}
			set
			{
				SetValue(LabelProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LabelProperty =
			DependencyProperty.Register("Label", typeof(string), typeof(CertificateListView), new PropertyMetadata(null));




		public IEnumerable ItemsSource
		{
			get
			{
				return (IEnumerable)GetValue(ItemsSourceProperty);
			}
			set
			{
				SetValue(ItemsSourceProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CertificateListView), new PropertyMetadata(null));




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
			DependencyProperty.Register("ShouldHavePrivateKey", typeof(bool), typeof(CertificateListView), new PropertyMetadata(true));

		private void AddClick(object sender, RoutedEventArgs e)
		{
			CertificateSelectorWindow selector = new CertificateSelectorWindow();
			selector.ShouldImportCertificate = false;
			selector.ShouldHavePrivateKey = ShouldHavePrivateKey;
			var result = selector.ShowDialog();
			if(result.HasValue && result.Value)
			{
				var certificate = selector.SelectedCertificate;
				if(certificate != null)
				{
					var source = ItemsSource as IList;
					if(source != null)
					{
						source.Add(certificate);
						list.ItemsSource = null;
						list.ItemsSource = ItemsSource;
					}
				}
			}
		}



	}
}
