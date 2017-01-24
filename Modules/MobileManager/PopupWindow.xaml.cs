using System;
using System.Windows;

namespace Gijima.IOBM.MobileManager
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        #region Declarations

        public enum PopupButtonType
        {
            OK,
            OKCancel,
            YesNo,
            SubmitCancel,
            Close
        }

        #region Public Events

        public event EventHandler PopupResult;

        #endregion

        #endregion

        #region Event Handlers

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonSubmit_Click(object sender, RoutedEventArgs e)
        {
            PopupResult?.Invoke(sender, e);
            Close();
        }

        #endregion


        public PopupWindow(object contentControl, string popupTitle, PopupButtonType buttonType)
        {
            InitializeComponent();
            TextBlockControlHeader.Text = popupTitle;
            ContentPlaceHolder.Children.Add((UIElement)contentControl);
            SetControlButtons(buttonType);
        }

        private void SetControlButtons(PopupButtonType buttonType)
        {
            ButtonCancel.Visibility = ButtonSubmit.Visibility = System.Windows.Visibility.Collapsed;

            switch (buttonType)
            {
                case PopupButtonType.OK:
                    ButtonSubmit.Visibility = System.Windows.Visibility.Visible;
                    ButtonSubmit.Content = "OK";
                    break;
                case PopupButtonType.OKCancel:
                    ButtonCancel.Visibility = ButtonSubmit.Visibility = System.Windows.Visibility.Visible;
                    ButtonSubmit.Content = "OK";
                    ButtonCancel.Content = "Cancel";
                    break;
                case PopupButtonType.YesNo:
                    ButtonCancel.Visibility = ButtonSubmit.Visibility = System.Windows.Visibility.Visible;
                    ButtonSubmit.Content = "Yes";
                    ButtonCancel.Content = "No";
                    break;
                case PopupButtonType.SubmitCancel:
                    ButtonCancel.Visibility = ButtonSubmit.Visibility = System.Windows.Visibility.Visible;
                    ButtonSubmit.Content = "Submit";
                    ButtonCancel.Content = "Cancel";
                    break;
                default:
                    ButtonCancel.Visibility = System.Windows.Visibility.Visible;
                    ButtonCancel.Content = "Close";
                    break;
            }
        }
    }
}
