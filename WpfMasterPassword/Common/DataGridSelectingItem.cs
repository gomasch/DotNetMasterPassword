using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfMasterPassword.Common
{
    /// <summary>
    /// Situation: You have a datagrid, bound the SelectedItem to an MVVM property, 
    /// the selection is changed by code and by the user.
    /// Problem: How can you achieve to scroll the datagrid to the selection set by the code?
    /// Just doing a "scrollintoview" whenever the grid selection changed didn't work,
    /// so we try this: http://stackoverflow.com/questions/18019425/scrollintoview-for-wpf-datagrid-mvvm
    /// 
    /// Solution:
    /// Have an extra trigger in form of a attached property. The code first changes the bound SelectedItem
    /// in the VM and then also another artifical property in the VM, that bound to this special attached property
    /// in the DataGrid.
    /// 
    /// Usage in code:
    ///   SelectedItem.Value = newItem;
    ///   SelectedItemScrollTo.Value = newItem
    /// 
    /// Usage in XAML:
    ///   DataGrid 
    ///          SelectedItem="{Binding Path=SelectedItem.Value, Mode=OneWay}" 
    ///          common:DataGridSelectingItem.SelectingItem="{Binding Path=SelectedItemScrollTo.Value}"
    /// </summary>
    public class DataGridSelectingItem
    {
        public static readonly DependencyProperty SelectingItemProperty = DependencyProperty.RegisterAttached(
            "SelectingItem",
            typeof(object),
            typeof(DataGridSelectingItem),
            new PropertyMetadata(default(object), OnSelectingItemChanged));

        public static object GetSelectingItem(DependencyObject target)
        {
            return target.GetValue(SelectingItemProperty);
        }

        public static void SetSelectingItem(DependencyObject target, object value)
        {
            target.SetValue(SelectingItemProperty, value);
        }

        static void OnSelectingItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null || grid.SelectedItem == null)
                return;

            // Works with .Net 4.5
            grid.Dispatcher.InvokeAsync(() =>
            {
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.SelectedItem, null);
            });

            //// Works with .Net 4.0
            //grid.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    grid.UpdateLayout();
            //    grid.ScrollIntoView(grid.SelectedItem, null);
            //}));
        }
    }
}
