using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Models;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewAdvancedSearchViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private IEventAggregator _eventAggregator;

        #region Commands
        #endregion

        #region Properties

        /// <summary>
        /// The data entity display name
        /// </summary>
        public string DataEntityDisplayName
        {
            get { return _dataEntityDisplayName; }
            set { SetProperty(ref _dataEntityDisplayName, value); }
        }
        private string _dataEntityDisplayName = string.Empty;

        /// <summary>
        /// The collection of data entities for the selected group from the database
        /// </summary>
        public ObservableCollection<string> DataEntityCollection
        {
            get { return _dataEntityCollection; }
            set { SetProperty(ref _dataEntityCollection, value); }
        }
        private ObservableCollection<string> _dataEntityCollection;


        #region View Lookup Data Collection
        #endregion

        #region Required Fields

        /// <summary>
        /// The selected validation entity
        /// </summary>
        public string SelectedEntity
        {
            get { return _selectedEntity; }
            set
            {
                SetProperty(ref _selectedEntity, value);
                ReadDataEntities();
            }

        }

        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private string _selectedEntity = EnumHelper.GetDescriptionFromEnum(DataValidationGroupName.None);

        #endregion

        #region Input Validation

        public string this[string columnName]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        public ViewAdvancedSearchViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DataEntityCollection = new ObservableCollection<string>();
            ReadDataEntities();
        }

        #region Lookup Data Loading


        /// <summary>
        /// Read all the advanced search entity names from the enum class
        /// </summary>
        public void ReadDataEntities()
        {
            IEnumerable<string> enumEntities = Enum.GetValues(typeof(AdvancedDataBaseEntity)).Cast<AdvancedDataBaseEntity>().Select(v => v.ToString()).ToList();
            ObservableCollection<string> entities = new ObservableCollection<string>();

            foreach (string entity in enumEntities)
            {
                entities.Add(entity);
            }

            DataEntityCollection = entities;
        }

        /// <summary>
        /// Load all the data entities for the selected group from the database
        /// </summary>
        private void ReadEntityColumnNames()
        {
            try
            {
                DataEntityDisplayName = "Audit Log";
                DataEntityCollection = new ObservableCollection<string>(new AuditLogModel(_eventAggregator).ReadColumnNames());

                switch (EnumHelper.GetEnumFromDescription<AdvancedDataBaseEntity>(SelectedEntity))
                {
                    case AdvancedDataBaseEntity.AuditLog:
                        DataEntityDisplayName = "Audit Log";
                        DataEntityCollection = new ObservableCollection<string>(new AuditLogModel(_eventAggregator).ReadColumnNames());
                        break;
                }

                //SelectedDataEntity = _defaultItem;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataEntitiesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #region Command Execution
        #endregion

        #endregion
    }
}
