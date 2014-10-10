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
using System.Diagnostics;

namespace HttpSysManager
{
	/// <summary>
	/// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
	///
	/// Step 1a) Using this custom control in a XAML file that exists in the current project.
	/// Add this XmlNamespace attribute to the root element of the markup file where it is 
	/// to be used:
	///
	///     xmlns:MyNamespace="clr-namespace:HttpSysManager"
	///
	///
	/// Step 1b) Using this custom control in a XAML file that exists in a different project.
	/// Add this XmlNamespace attribute to the root element of the markup file where it is 
	/// to be used:
	///
	///     xmlns:MyNamespace="clr-namespace:HttpSysManager;assembly=HttpSysManager"
	///
	/// You will also need to add a project reference from the project where the XAML file lives
	/// to this project and Rebuild to avoid compilation errors:
	///
	///     Right click on the target project in the Solution Explorer and
	///     "Add Reference"->"Projects"->[Browse to and select this project]
	///
	///
	/// Step 2)
	/// Go ahead and use your control in the XAML file.
	///
	///     <MyNamespace:HyperlinkButton/>
	///
	/// </summary>
	public class HyperlinkButton : Control
	{
		static HyperlinkButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(HyperlinkButton), new FrameworkPropertyMetadata(typeof(HyperlinkButton)));
		}

		public HyperlinkButton()
		{
			this.KeyDown += new KeyEventHandler(HyperlinkButton_KeyDown);
			this.IsTabStop = false;
		}

		void HyperlinkButton_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{
				link_Click(null, null);
				e.Handled = true;
			}
		}

		public string GoTo
		{
			get
			{
				return (string)GetValue(GoToProperty);
			}
			set
			{
				SetValue(GoToProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for GoTo.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty GoToProperty =
			DependencyProperty.Register("GoTo", typeof(string), typeof(HyperlinkButton), new UIPropertyMetadata(null));



		public ICommand Command
		{
			get
			{
				return (ICommand)GetValue(CommandProperty);
			}
			set
			{
				SetValue(CommandProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.Register("Command", typeof(ICommand), typeof(HyperlinkButton), new UIPropertyMetadata(OnPropertyChanged));


		public string Text
		{
			get
			{
				return (string)GetValue(TextProperty);
			}
			set
			{
				SetValue(TextProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(HyperlinkButton), new UIPropertyMetadata(OnPropertyChanged));

		static void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			HyperlinkButton sender = (HyperlinkButton)source;
			sender.RefreshLink();
		}

		public event RoutedEventHandler Click;

		Hyperlink link;
		Run run;
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			link = (Hyperlink)this.GetTemplateChild("link");
			run = (Run)this.GetTemplateChild("run");
			RefreshLink();
		}

		private void RefreshLink()
		{
			if(run != null)
			{
				run.Text = Text;
				link.Command = Command;
				link.Click -= link_Click;
				link.Click += link_Click;
			}
		}


		void link_Click(object sender, RoutedEventArgs e)
		{
			if(link != null)
				if(Click != null)
					Click(this, e);
				else if(GoTo != null)
					Process.Start(GoTo);

		}
	}
}
