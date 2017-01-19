using Gijima.BrsMs.Common;
using Gijima.BrsMs.Common.Helper;
using Gijima.BrsMs.Controller.Entity;
using Gijima.BrsMs.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gijima.BrsMs.Controls
{
	/// <summary>
	/// Interaction logic for VolumeNextControl.xaml
	/// </summary>
	public partial class VolumeNextControl : UserControl
	{
		public VolumeNextControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// This will check in, first the documents, then the volume itself.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonCheckOutIn_Click(object sender, EventArgs e)
		{
			if (((Button)sender).Content.ToString() == "Check In")
			{
				if (!_lineDetectionCompleted)
				{
					MessageBox.Show("Line detection is still in progress.", "PostScanIndexing", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (ValidateDocuments())
				{
					if (Convert.ToInt32(TextBlockFailCountValue.Text) > 0)
					{
						PopupWindow popup = popup = new PopupWindow(PopupWindow.PopupContentType.ExceptionAuthorisation, "Exception Authorisation");
						popup.onPopupResult -= Popup_onPopupResult;
						popup.onPopupResult += Popup_onPopupResult;
						popup.Owner = Window.GetWindow(this);
						popup.ShowDialog();
					}
					else
						CheckInVolume();
				}
				else
				{
					MessageBox.Show("Please Complete all documents.", "PostScanIndexing", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
			}
			else
			{
				LoadNextVolume();
			}
		}

		/// <summary>
		/// When the user edit or save the volume name
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonEditSaveVolume_Click(object sender, RoutedEventArgs e)
		{
			if (((Button)sender).Content.ToString() == "Edit")
			{
				ShowVolumeName();
				checkNameFormatted.IsChecked = _currentVolume.IsNameFormatted;
				comboVolumeType.SelectedValue = _currentVolume.fkDocumentTypeID;
				buttonEditSaveVolume.Content = "Save";
				buttonEditSaveVolume.ToolTip = "Save the volume name.";
				gridVolumeEdit.Visibility = System.Windows.Visibility.Visible;
				comboVolumeType.Focus();
			}
			else
			{
				UpdateVolumeName();
			}
		}


		private void buttonViewVolumeImage_Click(object sender, RoutedEventArgs e)
		{

		}

		private void checkNameFormatted_Click(object sender, RoutedEventArgs e)
		{

		}

		private void buttonVolumeCancel_Click(object sender, RoutedEventArgs e)
		{

		}

		private void comboVolumeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		#region Private methods

		private void InitialiseControls()
		{
			textVolumeBarcode.Text = "";
			textVolumeName.Text = "";
			buttonCheckOutIn.Content = "Check Out";
			buttonCheckOutIn.ToolTip = "Check-out the the next available volume.";
			buttonCheckOutIn.Focus();
			buttonCheckOutIn.IsEnabled = true;
		}

		/// <summary>
		/// This method will load the current volume that user is busy with.
		/// </summary>
		private void GetCurrentWorkItem()
		{
			try
			{
				_isZoomed = false;
				_currentVolume = VolumeController.GetCurrentCheckedOutVolumeForUser(VolumeExecutionState.Index.IntValue(), UserADHelper.DomainUsername, _application.SelectedClientID);

				// If there is nothing checked out to this person, load a new one.
				if (_currentVolume != null)
				{
					textVolumeBarcode.Text = _currentVolume.Barcode;
					textVolumeName.Text = _currentVolume.VolumeName;
					buttonCheckOutIn.Content = "Check In";
					buttonCheckOutIn.ToolTip = "Check-in the volume.";
					comboFailReason.IsEnabled = true;
					LoadDocuments();
				}
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Write(LogStatus.Error, this.GetType(), ex);
				MessageBox.Show(ex.Message, "PostScan Indexing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#endregion

		#region VolumeNextControl events and raisers

		public event EventHandler<VolumeEventArgs> VolumeLoaded;

		private void RaiseVolumeLoaded(Volume volume)
		{
		}

		#endregion

		#region VolumeNextControl attributes and properties

		private Volume _currentVolume;

		#endregion
	}

	public class VolumeEventArgs : EventArgs
	{
	}
}
