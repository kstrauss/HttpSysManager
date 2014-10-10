using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.ComponentModel;

namespace HttpSysManager
{
	public class Extensions
	{
		public static string GetSortBy(DependencyObject obj)
		{
			return (string)obj.GetValue(SortByProperty);
		}

		public static void SetSortBy(DependencyObject obj, string value)
		{
			obj.SetValue(SortByProperty, value);
		}

		public static readonly DependencyProperty SortByProperty =
			DependencyProperty.RegisterAttached("SortBy", typeof(string), typeof(Extensions), new PropertyMetadata(OnSortByChanged));

		static void OnSortByChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			CollectionViewSource sender = (CollectionViewSource)source;
			sender.SortDescriptions.Clear();
			if(args.NewValue != null)
				sender.SortDescriptions.Add(new SortDescription((string)args.NewValue, ListSortDirection.Ascending));
		}
		
		
	}
}
