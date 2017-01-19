using Prism.Mvvm;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Gijima.Controls.WPF
{
    /// <summary>
    /// Interaction logic for DualListBoxUX.xaml
    /// </summary>
    public partial class DualListBoxUX : UserControl
    {
		#region Properties and Attributes

		private object _data = null;
		private ListBox _dragSource = null;

        /// <summary>
        /// Set the availiable items listbox header
        /// </summary>
        public string AvailableItemsHeader
        {
            get { return (string)GetValue(AvailableItemsHeaderProperty); }
            set { SetValue(AvailableItemsHeaderProperty, value); }
        }
        public static readonly DependencyProperty AvailableItemsHeaderProperty = DependencyProperty.Register("AvailableItemsHeader",
                                                                                 typeof(string), 
                                                                                 typeof(DualListBoxUX), 
                                                                                 new UIPropertyMetadata("Available Items"));

        /// <summary>
        /// Set the selected items listbox header
        /// </summary>
        public string SelectedItemsHeader
        {
            get { return (string)GetValue(SelectedItemsHeaderProperty); }
            set { SetValue(SelectedItemsHeaderProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemsHeaderProperty = DependencyProperty.Register("SelectedItemsHeader",
                                                                                typeof(string),
                                                                                typeof(DualListBoxUX),
                                                                                new UIPropertyMetadata("Selected Items"));

        /// <summary>
        /// Set the Selected display member path of the source and destination controls
        /// </summary>
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set
            {
                SetValue(DisplayMemberPathProperty, value);
                ListBoxAvailableItems.DisplayMemberPath = value;
                ListBoxSelectedItems.DisplayMemberPath = value;
            }
        }
        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath",
                                                                              typeof(string),
                                                                              typeof(DualListBoxUX),
                                                                              new UIPropertyMetadata(string.Empty));

        /// <summary>
        /// Set the Selected Value Path of the source and destination controls
        /// </summary>
        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set
            {
                SetValue(SelectedValuePathProperty, value);
                ListBoxAvailableItems.SelectedValuePath = value;
                ListBoxSelectedItems.SelectedValuePath = value;
            }
        }
        public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath",
                                                                              typeof(string),
                                                                              typeof(DualListBoxUX),
                                                                              new UIPropertyMetadata(string.Empty));

        /// <summary>
        /// Get or set the available items data source
        /// </summary>
        public IEnumerable AvailableItems
        {
            get { return ListBoxAvailableItems.Items; }
            set
            {
                SetValue(AvailableItemsProperty, value);
                ListBoxAvailableItems.Items.Clear();
                if (value != null)
                {
                    foreach (object item in value)
                    {
                        ListBoxAvailableItems.Items.Add(item);
                    }
                }
            }
        }
        public static readonly DependencyProperty AvailableItemsProperty = DependencyProperty.Register("AvailableItems",
                                                                           typeof(IEnumerable),
                                                                           typeof(DualListBoxUX),
                                                                           new UIPropertyMetadata(null));

        /// <summary>
        /// Get or set the selected items data source
        /// </summary>
        public IEnumerable SelectedItems
        {
            get { return ListBoxSelectedItems.Items; }
            set
            {
                SetValue(SelectedItemsProperty, value);
                ListBoxSelectedItems.Items.Clear();
                if (value != null)
                {
                    foreach (object item in value)
                    {
                        ListBoxSelectedItems.Items.Add(item);
                    }
                }
            }
        }
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems",
                                                                          typeof(IEnumerable),
                                                                          typeof(DualListBoxUX),
                                                                          new UIPropertyMetadata(null));

        /// <summary>
        /// Set the controls tab order
        /// </summary>
        public int TabIndexStart
		{
            get { return (int)GetValue(TabIndexStartProperty); }
            set
            {
                SetValue(TabIndexStartProperty, value);
                ListBoxAvailableItems.TabIndex = value;
                ButtonAdd.TabIndex = ++value;
                ButtonRemove.TabIndex = ++value;
                ListBoxSelectedItems.TabIndex = ++value;
            }
        }
        public static readonly DependencyProperty TabIndexStartProperty = DependencyProperty.Register("TabIndexStart",
                                                                          typeof(int),
                                                                          typeof(DualListBoxUX),
                                                                          new UIPropertyMetadata(0));

		#region Public Events

		//public event EventHandler<ApplicationMessageEventArgs> Feedback;

		#endregion

		#endregion

		#region Event Handlers

		/// <summary>
		/// When a item get selected to be added to the selected items list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			if (_data != null && ListBoxAvailableItems.Items.IndexOf(_data) >= 0)
			{
				ListBoxSelectedItems.Items.Add(_data);
				ListBoxAvailableItems.Items.RemoveAt(ListBoxAvailableItems.Items.IndexOf(_data));
				ListBoxSelectedItems.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription(DisplayMemberPath, System.ComponentModel.ListSortDirection.Ascending));
			}

			ButtonAdd.IsEnabled = ButtonRemove.IsEnabled = false;
		}

		/// <summary>
		/// When a item get selected to be removed from the selected items list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RemoveButton_Click(object sender, RoutedEventArgs e)
		{
			if (_data != null && ListBoxSelectedItems.Items.IndexOf(_data) >= 0)
			{
				ListBoxAvailableItems.Items.Add(_data);
				ListBoxSelectedItems.Items.RemoveAt(ListBoxSelectedItems.Items.IndexOf(_data));
				ListBoxAvailableItems.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription(DisplayMemberPath, System.ComponentModel.ListSortDirection.Ascending));
			}

			ButtonAdd.IsEnabled = ButtonRemove.IsEnabled = false;
		}

		/// <summary>
		/// When a item is selected in the available items list
		/// to be dragged to the selected items list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListBoxSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ListBox parent = (ListBox)sender;
			_dragSource = parent;
			_data = GetDataFromListBox(_dragSource, e.GetPosition(parent));

			if (_data != null)
			{
				if (parent.Name == "ListBoxAvailableItems")
				{
					ButtonAdd.IsEnabled = parent.Items.Count > 0 ? true : false;
					ButtonRemove.IsEnabled = false;
				}
				else
				{
					ButtonRemove.IsEnabled = parent.Items.Count > 0 ? true : false; 
					ButtonAdd.IsEnabled = false;
				}
				DragDrop.DoDragDrop(_dragSource, _data, DragDropEffects.Move);
			}
		}

		/// <summary>
		/// When the mouse drag leaves the source list box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListBoxSource_DragLeave(object sender, DragEventArgs e)
		{
			ButtonAdd.IsEnabled = ButtonRemove.IsEnabled = false;
		}

		/// <summary>
		/// When a item from the selected items list
		/// is dropped into the available items list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListBoxDestination_Drop(object sender, DragEventArgs e)
		{
			ListBox parent = (ListBox)sender;
			object data = e.Data.GetData(typeof(object));
			_dragSource.Items.Remove(_data);
			parent.Items.Add(_data);
			parent.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription(DisplayMemberPath, System.ComponentModel.ListSortDirection.Ascending));
		}

		#endregion

		#region Methods

		public DualListBoxUX()
		{
			InitializeComponent();
            DataContext = this;
        }

		private object GetDataFromListBox(ListBox source, Point point)
		{
			UIElement element = source.InputHitTest(point) as UIElement;
			if (element != null)
			{
				object data = DependencyProperty.UnsetValue;
				while (data == DependencyProperty.UnsetValue)
				{
					data = source.ItemContainerGenerator.ItemFromContainer(element);

					if (data == DependencyProperty.UnsetValue)
					{
						element = VisualTreeHelper.GetParent(element) as UIElement;
					}

					if (element == source)
					{
						return null;
					}
				}

				if (data != DependencyProperty.UnsetValue)
				{
					return data;
				}
			}

			return null;
		}

		#endregion
	}
}
