using Gijima.IOBM.Infrastructure.Structs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Web;
using System.Windows;
using System.Windows.Controls;

namespace Gijima.Controls.WPF
{
    /// <summary>
    /// Interaction logic for ADUserSearchUX.xaml
    /// </summary>
    public partial class ADUserSearchUX : UserControl
	{
        #region Declarations

        #region Attributes

        public const string ADLogin = "cn";
        public const string ADAccountName = "SAMAccountName";
        public const string ADConnection = "LDAP_Connection";
        public const string ADFirstName = "givenname";
        public const string ADSurname = "sn";
        public const string ADDepartment = "department";
        public const string ADContactNumber = "telephonenumber";
        public const string ADMobileNumber = "mobile";
        public const string ADSearchFilter = "(&(objectClass=user)(objectCategory=Person)(&(givenname={0}*)(sn={1}*)))";

        #endregion

        #region Properties

        /// <summary>
        /// Property to hold the selected user name
        /// </summary>
        private ADUser SelectedADUser { get; set; }

        /// <summary>
        /// Gets User's AD account e.g. "domain\\100001"
        /// </summary>
        public static string DomainUsername
        {
            get
            {
                if (HttpContext.Current != null)
                    return HttpContext.Current.User.Identity.Name;
                else
                    return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }
        }

        /// <summary>
        /// Property to hold the domain name 
        /// </summary>
        private static string DomainName
		{
			get { return "GIJIMA"; }
		}

		#endregion

		#region Event

		public event EventHandler<UserSearchEventArgs> onADUserSelected;
		
		#endregion

		#endregion

		#region Event handlers

		private void ButtonSeach_Click(object sender, RoutedEventArgs e)
		{
			Search();
		}

		/// <summary>
		/// When the user get selected from the search results
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DataGridADUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DataGridADUsers.SelectedItem != null)
			{
				if (onADUserSelected != null)
					onADUserSelected(sender, new UserSearchEventArgs((ADUser)DataGridADUsers.SelectedItem));
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="imageWidth">The control width.</param>
		public ADUserSearchUX(int imageWidth = 450)
		{
			InitializeComponent();
			this.Height = Double.NaN; 
			this.Width = imageWidth;
		}

		/// <summary>
		/// Initialize all search controls
		/// </summary>
		private void InitializeSearchControls()
		{
			TextBoxUserName.Text = String.Empty;
			TextBoxSurname.Text = String.Empty;
			DataGridADUsers.ItemsSource = null;
			SelectedADUser = null;
			TextBoxUserName.Focus();
		}

		/// <summary>
		/// Calls the AD Search functionality and binds results to the grid
		/// </summary>
		private void Search()
		{
			try
			{
				List<ADUser> adUserList = new List<ADUser>();
				string firstname = TextBoxUserName.Text;
				string surname = TextBoxSurname.Text;

				SearchResultCollection searchResult = SearchForADUser(firstname, surname);

				if (searchResult.Count > 0)
				{
					foreach (SearchResult result in searchResult)
					{
						ADUser user = new ADUser();
						user.Firstname = result.Properties[ADFirstName].Count > 0 ? (string)result.Properties[ADFirstName][0] : String.Empty;
						user.Surname = result.Properties[ADSurname].Count > 0 ? (string)result.Properties[ADSurname][0] : String.Empty;
						user.Username = result.Properties[ADAccountName].Count > 0 ? DomainName + @"\" + (string)result.Properties[ADAccountName][0] : String.Empty;
						adUserList.Add(user);
					}
				}
				else
				{
					MessageBox.Show("No application users found.", "Security", MessageBoxButton.OK, MessageBoxImage.Information);
				}

				DataGridADUsers.ItemsSource = adUserList;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

        /// <summary>
        /// Method to search the Active Directory to return users according to specified search criteria
        /// </summary>
        /// <param name="searchCriteria">Firstname and/or Surname</param>
        /// <returns>SearchResultCollection</returns>
        public static SearchResultCollection SearchForADUser(string Firstname, string Surname)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MobileManagerDomain"].ToString();

            // "LDAP://192.168.1.9/OU=AG,DC=astjhb,DC=local"
            DirectoryEntry dirEntry = new DirectoryEntry(connectionString);
            dirEntry.Username = @"gijima\10012434";
            dirEntry.Password = "P@ssw0rd0117";

            dirEntry.AuthenticationType = AuthenticationTypes.ServerBind;

            DirectorySearcher search = new DirectorySearcher(dirEntry);
            search.PropertiesToLoad.Add(ADLogin);
            search.PropertiesToLoad.Add(ADAccountName);
            search.PropertiesToLoad.Add(ADFirstName);
            search.PropertiesToLoad.Add(ADSurname);
            search.PropertiesToLoad.Add(ADDepartment);
            search.PropertiesToLoad.Add(ADContactNumber);
            search.PropertiesToLoad.Add(ADMobileNumber);

            // Filter data
            // Please see - http://msdn.microsoft.com/en-us/library/ms675768(VS.85).aspx
            search.Filter = String.Format(ADSearchFilter, Firstname, Surname);

            // SortOption
            search.Sort = new SortOption(ADSurname, SortDirection.Ascending);

            // Limit result
            search.SizeLimit = 50;
            search.SearchScope = SearchScope.Subtree;

            try
            {
                return search.FindAll();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error! {0}, {1}.",
                                    ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty));
            }
        }

        #endregion
    }

	/// <summary>
	/// Class to allow a username to be passed as an event argument
	/// </summary>
	public class UserSearchEventArgs : EventArgs
	{
		ADUser _adUser;

		public UserSearchEventArgs(ADUser adUser)
		{
			_adUser = adUser;
		}

		public ADUser ADUser
		{
			get { return _adUser; }
		}
	}
}
