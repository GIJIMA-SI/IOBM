using Prism.Mvvm;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager
{
    /// <summary>
    /// Interaction logic for DualListBoxUX.xaml
    /// </summary>
    public partial class DualListBoxUX : UserControl
    {
		#region Declarations

		private object _data = null;
		private ListBox _dragSource = null;

        #region Public Properties

        public static readonly DependencyProperty AvailableItemsSource = DependencyProperty.Register("AvailableItems", typeof(IEnumerable), typeof(DualListBoxUX));

        /// <summary>
        /// Get or set the available items data source
        /// </summary>
        public IEnumerable AvailableItems
        {
            get { return GetType(AvailableItemsSource); }
            set
            {
                SetValue(AvailableItemsSource, value);
                ListBoxAvailableItems.Items.Clear();
                if (value != null)
                    foreach (object item in value)
                    {
                        ListBoxAvailableItems.Items.Add(item);
                    }
            }
        }

        private void SetValue(DependencyProperty availableItemsSource, IEnumerable value)
        {
        }

        private IEnumerable GetType(DependencyProperty availableItemsSource)
        {
            return GetType(availableItemsSource);
        }

        /// <summary>
        /// Get or set the available items data source
        /// </summary>
        public IEnumerable SelectedItems
		{
			get { return ListBoxSelectedItems.Items; }
			set
			{
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

		/// <summary>
		/// Get or set the available items data source
		/// </summary>
		public string DisplayMemberPath
		{
			get { return ListBoxAvailableItems.DisplayMemberPath; }
			set
			{
				ListBoxAvailableItems.DisplayMemberPath = value;
				ListBoxSelectedItems.DisplayMemberPath = value;
			}
		}

		/// <summary>
		/// Set the Selected Value Path of the source and destination controls
		/// </summary>
		public string SelectedValuePath
		{
			set
			{
				ListBoxAvailableItems.SelectedValuePath = value;
				ListBoxSelectedItems.SelectedValuePath = value;
			}
		}

		/// <summary>
		/// Set the availiable items listbox header
		/// </summary>
		public string AvailableItemsHeader
		{
			set { TextBlockAvailableItemsHeader.Text = value; }
		}

		/// <summary>
		/// Set the selected items listbox header
		/// </summary>
		public string SelectedItemsHeader
		{
			set { TextBlockSelectedItemsHeader.Text = value; }
		}

		/// <summary>
		/// Set the controls tab order
		/// </summary>
		public int TabIndexStart
		{
			set
			{ 
				ListBoxAvailableItems.TabIndex = value;
				ButtonAdd.TabIndex = ++value;
				ButtonRemove.TabIndex = ++value;
				ListBoxSelectedItems.TabIndex = ++value;
			}
		}

		#endregion

		#region Public Events

		//public event EventHandler<MessageEventArgs> Feedback;

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
