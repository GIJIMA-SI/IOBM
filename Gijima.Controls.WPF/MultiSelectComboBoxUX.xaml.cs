using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Gijima.Controls.WPF
{
    /// <summary>
    /// Interaction logic for MultiSelectComboBox.xaml
    /// </summary>
    public partial class MultiSelectComboBoxUX : UserControl
    {
        #region Properties and Attributes

        private ObservableCollection<Node> _nodeList;

        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", 
                                                                        typeof(Dictionary<string, object>), 
                                                                        typeof(MultiSelectComboBoxUX), 
                                                                        new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ItemsSourceChanged)));

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", 
                                                                          typeof(Dictionary<string, object>), 
                                                                          typeof(MultiSelectComboBoxUX), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(SelectedItemsChanged)));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", 
                                                                 typeof(string), 
                                                                 typeof(MultiSelectComboBoxUX), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefaultTextProperty = DependencyProperty.Register("DefaultText", 
                                                                        typeof(string), 
                                                                        typeof(MultiSelectComboBoxUX), new UIPropertyMetadata(string.Empty));

        #endregion

        #region Public Properties

        public Dictionary<string, object> ItemsSource
        {
            get { return (Dictionary<string, object>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public Dictionary<string, object> SelectedItems
        {
            get { return (Dictionary<string, object>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        #endregion

        #endregion

        #region Events

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBoxUX control = (MultiSelectComboBoxUX)d;
            control.DisplayInControl();
            control.SetText();
        }

        private static void SelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBoxUX control = (MultiSelectComboBoxUX)d;
            control.SelectNodes();
            control.SetText();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox clickedBox = (CheckBox)sender;
            int _selectedCount = 0;

            foreach (Node s in _nodeList)
            {
                if (s.IsSelected && s.Title != DefaultText)
                    _selectedCount++;
            }

            SetSelectedItems();
            SetText();
        }

        #endregion

        #region Methods

        public MultiSelectComboBoxUX()
        {
            InitializeComponent();
            _nodeList = new ObservableCollection<Node>();
        }

        private void SelectNodes()
        {
            foreach (KeyValuePair<string, object> keyValue in SelectedItems)
            {
                Node node = _nodeList.FirstOrDefault(i => i.Title == keyValue.Key);
                if (node != null)
                    node.IsSelected = true;
            }
        }

        private void SetSelectedItems()
        {
            if (SelectedItems == null)
                SelectedItems = new Dictionary<string, object>();
            else
                SelectedItems.Clear();

            foreach (Node node in _nodeList)
            {
                if (node.IsSelected && node.Title != DefaultText)
                {
                    if (ItemsSource.Count > 0)
                        SelectedItems.Add(node.Title, ItemsSource[node.Title]);
                }
            }
        }

        private void DisplayInControl()
        {
            _nodeList.Clear();

            // Add the default text if specified
            if (!string.IsNullOrEmpty(DefaultText))
                _nodeList.Add(new Node(DefaultText));

            foreach (KeyValuePair<string, object> keyValue in ItemsSource)
            {
                _nodeList.Add(new Node(keyValue.Key));
            }
            
            MultiSelectComboBox.ItemsSource = _nodeList;
        }

        private void SetText()
        {
            if (SelectedItems != null && SelectedItems.Count > 0)
            {
                StringBuilder displayText = new StringBuilder();

                foreach (Node s in _nodeList)
                {
                    if (s.IsSelected == true && s.Title != DefaultText)
                    {
                        displayText.Append(s.Title);
                        displayText.Append(',');
                    }
                }

                Text = displayText.ToString().TrimEnd(new char[] { ',' });                
            }
            else
            { 
                Text = DefaultText;
            }
        }

        #endregion
    }

    public class Node : INotifyPropertyChanged
    {
        private string _title;
        private bool _isSelected;

        public Node(string title)
        {
            Title = title;
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged("Title"); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyPropertyChanged("IsSelected"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
