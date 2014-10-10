using System;
using System.Collections.Generic;
using System.IO;
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
using HttpSysManager.Native;
using Microsoft.Win32;

namespace HttpSysManager
{
	/// <summary>
	/// Interaction logic for CreateCTLWizard.xaml
	/// </summary>
	public partial class CreateCTLWizard : Window
	{
		public CreateCTLWizard()
		{
			InitializeComponent();
		}


		private void Save_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

	}
}
