using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using Amaqele.Graphics.Format.Common;
using Amaqele.Graphics.Images;
using Amaqele.Graphics.WPF;
using Amaqele.Common.Property;
using Gijima.BrsMs.Common;
using Gijima.BrsMs.Common.Helper;
using Gijima.BrsMs.Common.Struct;
using Gijima.BrsMs.Controller.Core;
using Gijima.BrsMs.Controller.Entity;
using Gijima.BrsMs.Model.DAL.Entity;
using Gijima.BrsMs.Model.Data;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace Gijima.BrsMs.Controls
{
	/// <summary>
	/// Interaction logic for VolumeIndexUX.xaml
	/// </summary>
	public partial class VolumeIndexUX_old : UserControl
	{
        #region Declarations

        private Volume _currentVolume = null;
		private BitmapImage _frontImage = null;
		private BitmapImage _spineImage = null;
		private string _originalIndex = string.Empty;
		private bool _isSpineImage = false;
		private bool _allowIndexing = true;
		private IndexMode _mode;

		#region Public Properties

		/// <summary>
		/// Get or set the type of volumes handled.
		/// </summary>
		public MaterialType Material { get; set; }

		public Volume SelectedVolume
		{
			get { return _currentVolume; }
			set
			{
				if (value != null)
				{
					_currentVolume = value;
					_originalIndex = value.VolumeName;
					ShowVolumeName();
					this.IsEnabled = true;
				}
				else
				{
					TextBoxVolumeName.Text = "";
					this.IsEnabled = false;
				}
			}
		}

		/// <summary>
		/// Get or set an indicator if indexing (changing of the volume name) is allowed.
		/// Default is <see cref="true"/>.
		/// </summary>
		public bool AllowIndexing
		{
			get { return _allowIndexing; }
			set
			{
				_allowIndexing = value;
				ButtonEditSaveVolume.IsEnabled = value;
			}
		}

		public int LeftLabelWidth
		{
			set	{ TextBlockVolumeName.Width = TextBlockVolumeType.Width = value;	}
		}

		public int TabIndex
		{
			set
			{
				ButtonEditSaveVolume.TabIndex = value;
				ButtonViewVolumeImage.TabIndex = value + 1;
				ButtonVolumeCancel.TabIndex = value + 2;
				CheckBoxNameFormatted.TabIndex = value + 3;
				ComboBoxVolumeType.TabIndex = value + 4;
				TextBoxVolumeStartIndex.TabIndex = value + 5;
				TextBoxVolumeEndIndex.TabIndex = value + 6;
				TextBoxVolumeYear.TabIndex = value + 7;
				TextBoxVolumePostFix.TabIndex = value + 8;
			}
		}

		#endregion

		#region Public Events

		public event EventHandler IsBusy;
		public event EventHandler ShowVolumeImage;
		public event EventHandler<VolumeIndexOldEventArgs> IndexCompleted;

		#endregion

		#endregion

		#region Event Handlers

		/// <summary>
		/// When the user select to view volume spine or front image
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ButtonViewVolumeImage_Click(object sender, RoutedEventArgs e)
		{
			await GetVolumeImagesAsync();

			if (_isSpineImage)
			{
				_isSpineImage = false;
				ButtonViewVolumeImage.ToolTip = "View the volume front image.";

				if (ShowVolumeImage != null)
					ShowVolumeImage(_spineImage, null);
			}
			else
			{
				_isSpineImage = true;
				ButtonViewVolumeImage.ToolTip = "View the volume Spine image.";

				if (ShowVolumeImage != null)
					ShowVolumeImage(_frontImage, null);
			}
		}

		/// <summary>
		/// When the user edit or save the volume name
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonEditSaveVolume_Click(object sender, RoutedEventArgs e)
		{
			if (_mode == IndexMode.Edit)
			{
				ShowVolumeName();
				CheckBoxNameFormatted.IsChecked = _currentVolume.IsNameFormatted;
				ComboBoxVolumeType.SelectedValue = _currentVolume.fkDocumentTypeID;

				_mode = IndexMode.Save;
				ButtonEditSaveVolume.ToolTip = "Save the volume name.";
				ImageEditSaveVolume.Source = new BitmapImage(new Uri(@"pack://application:,,,/Gijima.BrsMs.Controls;component/Assets/Images/save_32.ico"));
				GridVolumeEditPaper.Visibility = System.Windows.Visibility.Visible;
				ComboBoxVolumeType.Focus();
			}
			else
			{
				SaveVolumeName();
			}
		}

		/// <summary>
		/// Cancel the volume name editing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonVolumeCancel_Click(object sender, RoutedEventArgs e)
		{
			_mode = IndexMode.Edit;
			ButtonEditSaveVolume.ToolTip = "Edit the volume name.";
			ImageEditSaveVolume.Source = new BitmapImage(new Uri(@"pack://application:,,,/Gijima.BrsMs.Controls;component/Assets/Images/edit_32.png"));
			TextBoxVolumeName.Text = _currentVolume.VolumeName;
			GridVolumeEditPaper.Visibility = System.Windows.Visibility.Collapsed;
		}

		/// <summary>
		/// When the user select formatted or un-formatted document name
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CheckBoxNameFormatted_Click(object sender, RoutedEventArgs e)
		{
			if (CheckBoxNameFormatted.IsChecked.Value)
			{
				EnableDisableVolumeIndexControls(true);
				TextBoxVolumeName.IsReadOnly = true;
			}
			else
			{
				InitialiseVolumeIndexControls();
				GridVolumeEditPaper.Visibility = System.Windows.Visibility.Visible;
				ComboBoxVolumeType.IsEnabled = true;
				TextBoxVolumeName.Text = "";
				TextBoxVolumeName.IsReadOnly = false;
			}
		}

		/// <summary>
		/// When the user select a volume type
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBoxVolumeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DocumentType selectedItem = (DocumentType)ComboBoxVolumeType.SelectedItem;

			if (selectedItem != null && selectedItem.DocumentTypeName == "SG")
				CheckBoxNameFormatted.IsChecked = false;
			else
				CheckBoxNameFormatted.IsChecked = true;

			CheckBoxNameFormatted_Click(null, null);
		}


		private void TextBoxVolumePostFix_LostFocus(object sender, RoutedEventArgs e)
		{
			ButtonEditSaveVolume.Focus();
		}

        #endregion

        #region Methods

        /// <summary>
        /// Default Constructor.
        /// </summary>
		public VolumeIndexUX_old()
        {
            InitializeComponent();

			// Dont initialise when designing, we dont have our configuration then
			if (!DesignerProperties.GetIsInDesignMode(this))
			{
				InitialiseControls();
				GetDocumentTypes();
			}
        }

        #region Private Methods

        /// <summary>
        /// Creates and resets the controls in the interface.
        /// </summary>
        private void InitialiseControls()
        {
            PropertyTree deploymentSettings = ConfigurationHelper.Instance.GetDeploymentSettings();
			_currentVolume = null;
			_frontImage = _spineImage = null;
            TextBoxVolumeName.Text = "";
			ButtonEditSaveVolume.IsEnabled = ButtonViewVolumeImage.IsEnabled = false;
			InitialiseVolumeControls();
        }

		/// <summary>
		/// Initialise the volume controls
		/// </summary>
		private void InitialiseVolumeControls()
		{
			ComboBoxVolumeType.IsEnabled = false;
			CheckBoxNameFormatted.IsEnabled = false;
			_mode = IndexMode.Edit;
			ButtonEditSaveVolume.ToolTip = "Edit the volume name.";
			ImageEditSaveVolume.Source = new BitmapImage(new Uri(@"pack://application:,,,/Gijima.BrsMs.Controls;component/Assets/Images/edit_32.png"));
			ComboBoxVolumeType.SelectedIndex = -1;
			GridVolumeEditPaper.Visibility = System.Windows.Visibility.Collapsed;
			InitialiseVolumeIndexControls();
		}

		/// <summary>
		/// Initialise the volume index controls
		/// </summary>
		private void InitialiseVolumeIndexControls()
		{
			TextBoxVolumeStartIndex.Text = TextBoxVolumeEndIndex.Text = TextBoxVolumeYear.Text = TextBoxVolumePostFix.Text = String.Empty;
			GridVolumeEditPaper.Visibility = System.Windows.Visibility.Collapsed;
			EnableDisableVolumeIndexControls(false);
		}

		/// <summary>
		/// Enable/Disable volume name controls
		/// </summary>
		/// <param name="flag"></param>
		private void EnableDisableVolumeIndexControls(bool flag)
		{
			TextBoxVolumeStartIndex.IsEnabled = TextBoxVolumeEndIndex.IsEnabled = TextBoxVolumeYear.IsEnabled = TextBoxVolumePostFix.IsEnabled = flag;
		}

        /// <summary>
        /// Get the document types from the database and populates the combobox.
        /// </summary>
        private void GetDocumentTypes()
        {
            try
            {
				IEnumerable<DocumentType> result = HelperController.GetDocumentTypes();

				ComboBoxVolumeType.DisplayMemberPath = "DocumentTypeName";
				ComboBoxVolumeType.SelectedValuePath = "pkDocumentTypeID";
				ComboBoxVolumeType.ItemsSource = result;
				ComboBoxVolumeType.SelectedItem = 0;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Write(LogStatus.Error, this.GetType(), ex);
                MessageBox.Show(ex.Message, "PostScan Indexing", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

		/// <summary>
		/// Get the volume spine and front images
		/// </summary>
		private async Task GetVolumeImagesAsync()
		{
			try
			{
				if (_frontImage == null || _spineImage == null)
				{
					VolumeImage result = VolumeController.GetVolumeImages(_currentVolume.pkVolumeID);

					if (result != null)
					{
						VolumeImage volumeImages = result;
						MemoryStream ms = null;

						if (IsBusy != null)
							IsBusy(true, null);

						_frontImage = new BitmapImage();
						_frontImage.BeginInit();
						ms = await Task.Run(() => new MemoryStream(new ImageConverter().DecodeFromJpeg2000Amaqele(volumeImages.ImageBack)));
						_frontImage.StreamSource = ms;
						_frontImage.EndInit();
						_spineImage = new BitmapImage();
						_spineImage.BeginInit();
						ms = await Task.Run(() => new MemoryStream(new ImageConverter().DecodeFromJpeg2000Amaqele(volumeImages.ImageSpine)));
						_spineImage.StreamSource = ms;
						_spineImage.EndInit();

						if (IsBusy != null)
							IsBusy(false, null);
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Write(LogStatus.Error, this.GetType(), ex);
				MessageBox.Show(ex.Message, "PostScan Indexing", MessageBoxButton.OK, MessageBoxImage.Error);

				if (IsBusy != null)
					IsBusy(false, null);
			}
		}

		/// <summary>
		/// Show the volume name based on the volume name format. 
		/// </summary>
		private void ShowVolumeName()
		{
			TextBoxVolumeName.Text = _currentVolume.VolumeName;
			TextBoxVolumeStartIndex.Text = _currentVolume.VolumeStartNumber;
			TextBoxVolumeEndIndex.Text = _currentVolume.VolumeEndNumber;
			TextBoxVolumeYear.Text = _currentVolume.VolumeYear;
			TextBoxVolumePostFix.Text = _currentVolume.VolumePostFix;
			TextBoxVolumeName.IsReadOnly = _currentVolume.IsNameFormatted ? true : false;
			CheckBoxNameFormatted.IsEnabled = true;
			ComboBoxVolumeType.IsEnabled = true;
			TextBoxVolumeStartIndex.IsEnabled = _currentVolume.IsNameFormatted ? true : false;
			TextBoxVolumeEndIndex.IsEnabled = _currentVolume.IsNameFormatted ? true : false;
			TextBoxVolumeYear.IsEnabled = _currentVolume.IsNameFormatted ? true : false;
			TextBoxVolumePostFix.IsEnabled = _currentVolume.IsNameFormatted ? true : false;
			ButtonViewVolumeImage.IsEnabled = true;

			// Only enable the edit button if we are allowed to index.
			if (AllowIndexing)
				ButtonEditSaveVolume.IsEnabled = true;
		}

		/// <summary>
		/// Save the volume name
		/// </summary>
		private void SaveVolumeName()
		{
			string validationErrorMessage = ValidateVolumeInput();
			CodeExecutionResult result = new CodeExecutionResult();

			try
			{
				if (validationErrorMessage.Length == 0)
				{
					string newName = "";

					// Only validate the volume name for DEEEDS and not for SG or unformatted name
					// For SG and unformatted name we save the volume name as the user type it without the 
					// volume type prefix and for DEEDS and formatted we include the volume type prefix
					if (ComboBoxVolumeType.Text == "SG" || CheckBoxNameFormatted.IsChecked.Value == false)
					{
						newName = TextBoxVolumeName.Text.ToUpper().Trim();
					}
					else
					{
						newName = ComboBoxVolumeType.Text.ToString().ToUpper().Trim() +
								  TextBoxVolumeStartIndex.Text.ToUpper().Trim() + "-" +
								  TextBoxVolumeEndIndex.Text.ToUpper().Trim() + "-" +
								  TextBoxVolumeYear.Text.ToUpper().Trim() +
								  TextBoxVolumePostFix.Text.ToUpper().Trim();
					}

					// Only update the new name if it was changed
					if (newName != _currentVolume.VolumeName)
					{
						// Check that new Volume Name is still unique for the client before proceeding
						result = SearchController.Search(EntityType.Volume, _currentVolume.fkClientID, newName);

						if (result.DataObject != null)
						{
							if (((List<Volume>)result.DataObject).Count > 0)
							{
								MessageBox.Show(String.Format("Volume name {0} already exists!", newName, "PostScan Indexing", MessageBoxButton.OK, MessageBoxImage.Warning));
								return;
							}
						}

						_currentVolume.fkDocumentTypeID = Int32.Parse(ComboBoxVolumeType.SelectedValue.ToString());
						_currentVolume.VolumeStartNumber = TextBoxVolumeStartIndex.Text.Trim();
						_currentVolume.VolumeEndNumber = TextBoxVolumeEndIndex.Text.Trim();
						_currentVolume.VolumeYear = TextBoxVolumeYear.Text.Trim();
						_currentVolume.VolumePostFix = TextBoxVolumePostFix.Text.ToUpper().Trim();
						_currentVolume.VolumeName = newName;
						_currentVolume.IsNameFormatted = CheckBoxNameFormatted.IsChecked.Value;
						_currentVolume.ModifiedBy = UserADHelper.DomainUsername;
						_currentVolume.CreateDate = DateTime.Now;

						result = VolumeController.SaveVolume(Material, _currentVolume, VolumeExecutionState.Index.IntValue(), "Volume Updated");

						if (result.Result)
						{
							TextBoxVolumeName.Text = newName;
						}
						else
						{
							MessageBox.Show(result.Message, "PostScan Indexing", MessageBoxButton.OK, MessageBoxImage.Error);
						}
					}

					InitialiseVolumeControls();

					// Raise the index completed event with eventargs
					VolumeIndexOldEventArgs e = new VolumeIndexOldEventArgs(_currentVolume, _originalIndex);
					EventHandler<VolumeIndexOldEventArgs> handler = IndexCompleted;

					if (IndexCompleted != null)
						IndexCompleted(null, e);
				}
				else
				{
					MessageBox.Show(String.Format("Note: {0} ", validationErrorMessage), "PostScan Indexing", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Write(LogStatus.Error, this.GetType(), ex);
				MessageBox.Show(ex.Message, "PostScan Indexing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>
		/// Validate the volume name 
		/// </summary>
		/// <returns>Error message</returns>
		private string ValidateVolumeInput()
		{
			try
			{
				if (ComboBoxVolumeType.Text == "SG" || !CheckBoxNameFormatted.IsChecked.Value)
				{
					// Validate volume name.
					if (TextBoxVolumeName.Text.Length == 0)
					{
						TextBoxVolumeName.Focus();
						return "Please enter the volume name.";
					}
				}
				else
				{
					// Validate volume type.
					if (ComboBoxVolumeType.SelectedIndex < 0)
					{
						ComboBoxVolumeType.Focus();
						return "Please select a volume type.";
					}

					// Validate start index.
					if (TextBoxVolumeStartIndex.Text.Equals(""))
					{
						TextBoxVolumeStartIndex.Focus();
						return "Please enter a valid start index.";
					}

					// Validate end index.
					if (TextBoxVolumeEndIndex.Text.Equals(""))
					{
						TextBoxVolumeEndIndex.Focus();
						return "Please enter a valid end index.";
					}

					// Validate year.                
					if (TextBoxVolumeYear.Text.Equals("") || !UIHelper.IsNumeric(TextBoxVolumeYear.Text.Trim()) || TextBoxVolumeYear.Text.Trim().Length != 4)
					{
						TextBoxVolumeYear.Focus();
						return "Please enter a valid year.";
					}

					// Validate Postfix
					if (UIHelper.IsNumeric(TextBoxVolumePostFix.Text))
					{
						TextBoxVolumePostFix.Focus();
						return "Postfix cannot be numeric.";
					}

					if (TextBoxVolumePostFix.Text.Trim().Length > 5)
					{
						TextBoxVolumePostFix.Focus();
						return "Please check postfix - length is too long.";
					}
				}

				return String.Empty;
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

        #endregion

		#endregion
	}

	/// <summary>
	/// Control event args
	/// </summary>
	public class VolumeIndexOldEventArgs : EventArgs
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="volume">The current volume linked to the control</param>
		/// <param name="previousIndex">The previous volume name</param>
		public VolumeIndexOldEventArgs(Volume volume, string previousIndex)
		{
			IndexedVolume = volume;
			PreviousIndex = previousIndex;
			HasChanged = previousIndex == null || previousIndex.Trim().ToUpper() != volume.VolumeName.Trim().ToUpper() ? true : false;
		}

		/// <summary>
		/// Indicate if the volume name changed
		/// </summary>
		public bool HasChanged { get; private set; }

		/// <summary>
		/// The original volume name
		/// </summary>
		public string PreviousIndex { get; private set; }

		/// <summary>
		/// The volume linked to the control
		/// </summary>
		public Volume IndexedVolume { get; private set; }
	}
}
