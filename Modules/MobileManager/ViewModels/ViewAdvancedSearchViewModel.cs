using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewAdvancedSearchViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private IEventAggregator _eventAggregator;
        private string _defaultItem = "-- Please Select --";
        private string _defaultEnum = "None";

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
        public ObservableCollection<EntityDetail> DataEntityCollection
        {
            get { return _dataEntityCollection; }
            set { SetProperty(ref _dataEntityCollection, value); }
        }
        private ObservableCollection<EntityDetail> _dataEntityCollection;


        #region View Lookup Data Collection

        public ObservableCollection<string> EntityColumnCollection
        {
            get { return _entityColumnCollection; }
            set { SetProperty(ref _entityColumnCollection, value); ; }
        }
        private ObservableCollection<string> _entityColumnCollection;

        /// <summary>
        /// The collection of operators types from the OperatorType enum's
        /// </summary>
        public ObservableCollection<string> OperatorTypeCollection
        {
            get { return _operatorTypeCollection; }
            set { SetProperty(ref _operatorTypeCollection, value); }
        }
        private ObservableCollection<string> _operatorTypeCollection;

        /// <summary>
        /// The collection of operators from the string, numeric
        /// date, math and validation OperatorType enum's
        /// </summary>
        public ObservableCollection<string> OperatorCollection
        {
            get { return _operatorCollection; }
            set { SetProperty(ref _operatorCollection, value); }
        }
        private ObservableCollection<string> _operatorCollection;

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected validation data entity
        /// </summary>
        public EntityDetail SelectedDataEntity
        {
            get { return _selectedDataEntity; }
            set
            {
                _selectedDataEntity = value;
                if (SelectedDataEntity != null)
                    EntityColumnCollection = SelectedDataEntity.ReadAllColumnNames();
            }
        }
        private EntityDetail _selectedDataEntity = null;

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
        private string _selectedEntity = EnumHelper.GetDescriptionFromEnum(DataValidationGroupName.None);

        public string SelectedColumn
        {
            get { return _selectedColumn; }
            set
            {
                SetProperty(ref _selectedColumn, value);
                ReadOperatorTypes();
                SelectedOperatorType = "";

                try
                {
                    SelectedOperatorType = SelectedDataEntity.ColumnTypes[EntityColumnCollection.IndexOf(SelectedColumn)];
                }
                catch
                { }
            }
        }
        private string _selectedColumn;

        /// <summary>
        /// The selected operator
        /// </summary>
        public string SelectedOperatorType
        {
            get { return _selectedOperatorType; }
            set
            {
                SetProperty(ref _selectedOperatorType, value);
                //ReadTypeOperators();
            }
        }
        private string _selectedOperatorType;

        /// <summary>
        /// The selected data validation property
        /// </summary>
        public DataValidationProperty SelectedDataProperty
        {
            get { return _selectedDataProperty; }
            set
            {
                SetProperty(ref _selectedDataProperty, value);
                //ReadOperatorTypes();
            }
        }
        private DataValidationProperty _selectedDataProperty = null;

        /// <summary>
        /// The selected operator
        /// </summary>
        public string SelectedOperator
        {
            get { return _selectedOperator; }
            set { SetProperty(ref _selectedOperator, value); }
        }
        private string _selectedOperator;

        /// <summary>
        /// 
        /// </summary>
        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDataEntity
        {
            get { return _validDataEntity; }
            set { SetProperty(ref _validDataEntity, value); }
        }
        private Brush _validDataEntity = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidColumnName
        {
            get { return _validColumnName; }
            set { SetProperty(ref _validColumnName, value); }
        }
        private Brush _validColumnName = Brushes.Red;

        /// <summary>
        /// Input validation properties
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get
            {
                string result = string.Empty;
                switch (columnName)
                {
                    case "SelectedDataEntity":
                        ValidDataEntity = SelectedDataEntity != null && SelectedDataEntity.EntityName != _defaultItem ? Brushes.Silver : Brushes.Red;
                        break;
                    case "SelectedDataProperty":
                        ValidColumnName = SelectedDataProperty != null && SelectedDataProperty.ToString() != _defaultItem ? Brushes.Silver : Brushes.Red;
                        break;
                    case "SelectedColumn":
                        ValidColumnName = SelectedColumn != null && SelectedColumn.ToString() != _defaultItem ? Brushes.Silver : Brushes.Red; 
                        break;
                }
                return result;
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
            InitialiseViewControls();
            ReadDataEntities();
        }

        public void InitialiseViewControls()
        {
            DataEntityCollection = new AdvancedSearchEntities().EntityDetails;
            EntityColumnCollection = new ObservableCollection<string>();

            SelectedDataEntity = DataEntityCollection.Where(x => x.EntityType == null).FirstOrDefault();

            SelectedColumn = null;
        }


        #region Lookup Data Loading

        /// <summary>
        /// Read all the advanced search entity names from the enum class
        /// </summary>
        public void ReadDataEntities()
        {
            IEnumerable<string> enumEntities = Enum.GetValues(typeof(AdvancedDataBaseEntity)).Cast<AdvancedDataBaseEntity>().Select(v => v.ToString()).ToList();
            string[] names = Enum.GetNames(typeof(AdvancedDataBaseEntity)).ToArray<string>();
            ObservableCollection<string> entities = new ObservableCollection<string>();

            foreach (string entity in enumEntities)
            {
                entities.Add(entity);
            }

            //DataEntityCollection = entities;
        }

        /// <summary>
        /// Load all the data entities for the selected group from the database
        /// </summary>
        private void ReadEntityColumnNames()
        {
            try
            {
                DataEntityDisplayName = "Audit Log";
                //DataEntityCollection = new ObservableCollection<string>(new AuditLogModel(_eventAggregator).ReadColumnNames());

                switch (EnumHelper.GetEnumFromDescription<AdvancedDataBaseEntity>(SelectedEntity))
                {
                    case AdvancedDataBaseEntity.AuditLog:
                        DataEntityDisplayName = "Audit Log";
                        //DataEntityCollection = new ObservableCollection<string>(new AuditLogModel(_eventAggregator).ReadColumnNames());
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

        /// <summary>
        /// Load all the operator types from the OperatorType enum
        /// </summary>
        private void ReadOperatorTypes()
        {
            try
            {
                OperatorTypeCollection = new ObservableCollection<string>();

                DataValidationProperty tmp = new Model.Data.DataValidationProperty();
                

                if (SelectedDataProperty != null && SelectedDataProperty.ToString() != _defaultItem)
                {

                    switch ((DataTypeName)SelectedDataProperty.enDataType)
                    {
                        case DataTypeName.Integer:
                        case DataTypeName.Decimal:
                        case DataTypeName.Float:
                        case DataTypeName.Long:
                        case DataTypeName.Short:
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.None));
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.NumericOperator));
                            break;
                        case DataTypeName.DateTime:
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.None));
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.DateOperator));
                            break;
                        case DataTypeName.Bool:
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.None));
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.BooleanOperator));
                            break;
                        default:
                            foreach (OperatorType source in Enum.GetValues(typeof(OperatorType)))
                            {
                                OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
                            }
                            break;
                    }
                }

                SelectedOperatorType = _defaultEnum;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadOperatorTypes",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the data type operators from the string, 
        /// numeric and date OperatorType enum's
        /// </summary>
        private void ReadTypeOperators()
        {
            try
            {
                OperatorCollection = new ObservableCollection<string>();

                if (SelectedOperatorType != null && SelectedOperatorType != _defaultEnum)
                {
                    switch (EnumHelper.GetEnumFromDescription<OperatorType>(SelectedOperatorType))
                    {
                        case OperatorType.StringOperator:
                            foreach (StringOperator source in Enum.GetValues(typeof(StringOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                        case OperatorType.NumericOperator:
                            foreach (NumericOperator source in Enum.GetValues(typeof(NumericOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                        case OperatorType.DateOperator:
                            foreach (DateOperator source in Enum.GetValues(typeof(DateOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                        case OperatorType.BooleanOperator:
                            foreach (BooleanOperator source in Enum.GetValues(typeof(BooleanOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                        case OperatorType.RuleOperator:
                            foreach (RuleOperator source in Enum.GetValues(typeof(RuleOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                    }

                    SelectedOperator = _defaultEnum;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadTypeOperators",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #region Command Execution
        #endregion

        #endregion
    }
}
