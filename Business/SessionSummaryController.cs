#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using CrystalDecisions.CrystalReports.Engine;
using GameTech.Elite.Base;
using GameTech.Elite.Client.Modules.SessionSummary.Properties;
using GameTech.Elite.Client.Modules.SessionSummary.UI;
using GameTech.Elite.Client.Modules.SessionSummary.ViewModel;
using GameTech.Elite.Reports;
using GameTech.Elite.UI;
using GameTech.Elite.Base.System;
using GameTech.Elite.Client.Modules.SessionSummary.Messages;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// Represents the Session Summary application.
    /// </summary>
    internal sealed class SessionSummaryController : Notifier, IEliteModuleController, ISessionSummaryController
    {
        #region Member Variables
        private bool m_isInitialized;
        private int m_instanceId = -1;
        private bool m_isBusy;

        private SessionSummarySettings m_settings;
        private SessionSummaryReportWindow m_reportWindow;

        private LoadingForm m_loadingForm;
        private Window m_mainWindow;
        private Window m_currentWindow;

        private List<Staff> m_staffMembers;
        #endregion

        #region Member Methods
        /// <summary>
        /// Performs any processing or data setup needed before the module is
        /// started.
        /// </summary>
        /// <param name="showLoadingWindow">Whether the module should display a
        /// loading or splash screen while performing initialization.</param>
        /// <param name="isTouchScreen">Whether this module is running on a
        /// touchscreen-based device.</param>
        /// <param name="instanceId">The unique identifier of the running
        /// instance of this module.</param>
        /// <returns>true if the initialization was successful; otherwise,
        /// false.</returns>
        public bool Initialize(bool showLoadingWindow, bool isTouchScreen, int instanceId)
        {
            // Check to see if we are already initialized.
            if(IsInitialized)
                return IsInitialized;

            InstanceId = instanceId;

            // Create a settings object with the default values.
            Settings = new SessionSummarySettings();

            // Start COM interface to EliteMCP.
            EliteModuleComm modComm = null;
            int machineId = 0;
            int staffId = 0;

            try
            {
                modComm = new EliteModuleComm();
                machineId = modComm.GetMachineId();
                OperatorId = modComm.GetOperatorId();
                staffId = modComm.GetStaffId();
            }
            catch(Exception ex)
            {
                MessageBoxOptions options = 0;

                if(CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
                    options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;

                MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Resources.GetDeviceInfoFailed, ex.Message),
                                Resources.SessionSummaryName, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, options);

                return IsInitialized;
            }

            // Session Summary always runs in windowed mode.
            Settings.DisplayMode = DisplayMode.Windowed;

            // Create and show the loading form.
            LoadingForm = new LoadingForm(Settings.DisplayMode);
            LoadingForm.ApplicationName = Resources.SessionSummaryName;
            LoadingForm.Version = EliteModule.GetVersion(Assembly.GetExecutingAssembly());
            LoadingForm.Copyright = EliteModule.GetCopyright(Assembly.GetExecutingAssembly());
            LoadingForm.Cursor = System.Windows.Forms.Cursors.WaitCursor;

            if(showLoadingWindow)
                LoadingForm.Show();

            LoadingForm.Status = Resources.LoadingMachineInfo;

            CreateApplication();

            if(!LoadSettings(OperatorId, machineId))
                return IsInitialized;

            if (!LoadOperatorData(OperatorId))
                return IsInitialized;

            if (!LoadStaff(staffId))
                return IsInitialized;

            if(!CreateLogger())
                return IsInitialized;

            // Check to see if we only want to display in English.
            if(Settings.ForceEnglish)
            {
                EliteModule.ForceEnglish();
                Logger.Log("Forcing English.", LoggerLevel.Configuration);
            }

            if (!LoadCurrencies())
                return IsInitialized;

            if (!CreateControllers())
                return IsInitialized;

            if (!CreateReport())
                return IsInitialized;

            LoadingForm.Status = Resources.Starting;

            if(!CreateWindows())
                return IsInitialized;

            // Have the controller notified when the main window opens.
            MainWindow.Loaded += MainWindowLoaded;

            IsInitialized = true;

            Logger.Log("Session Summary initialized!", LoggerLevel.Debug);

            return IsInitialized;
        }

        /// <summary>
        /// Creates the WPF application and loads resource dictionaries.
        /// </summary>
        private void CreateApplication()
        {
            LoadingForm.Status = Resources.LoadingResources;

            new Application();

            try
            {
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Application.ResourceAssembly = Assembly.GetExecutingAssembly();

                Application.Current.Resources.MergedDictionaries.Add(ThemeLoader.LoadTheme(Settings.DisplayMode));
            }
            catch(Exception ex)
            {
                MessageWindow.Show(string.Format(CultureInfo.CurrentCulture, Resources.LoadResourceDictionariesFailed, ex.Message), Resources.SessionSummaryName, MessageWindowType.Close);
            }
        }

        /// <summary>
        /// Loads the SessionSummary's settings from the server.
        /// </summary>
        /// <param name="operatorId">The id of the operator to use in the
        /// settings messages.</param>
        /// <param name="machineId">The id of the machine to use in the
        /// settings messages.</param>
        /// <returns>true if success; otherwise false.</returns>
        private bool LoadSettings(int operatorId, int machineId)
        {
            try
            {
                EliteModule.GetModuleSettings(Settings, machineId, operatorId, SettingCategory.GlobalSystemSettings);
            }
            catch(Exception ex)
            {
                MessageWindow.Show(string.Format(CultureInfo.CurrentCulture, Resources.GetSettingsFailed, ex.Message), Resources.SessionSummaryName, MessageWindowType.Close);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads the operator data for the specified operator
        /// </summary>
        /// <param name="OperatorId"></param>
        /// <returns></returns>
        private bool LoadOperatorData(int operatorId)
        {
            LoadingForm.Status = Resources.LoadingOperatorData;

            var getOpDataMsg = new GetCompleteOperatorDataMessage(operatorId);
            getOpDataMsg.Send();

            if (getOpDataMsg.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(String.Format(CultureInfo.CurrentCulture, Resources.GetOperatorDataFailed, ServerErrorTranslator.GetReturnCodeMessage(getOpDataMsg.ReturnCode)));

            if (getOpDataMsg.Operators != null)
            {
                foreach (var item in getOpDataMsg.Operators)
                {
                    if (item.Id == operatorId)
                    {
                        CashMethod = item.CashMethod;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Loads the current staff member and a list of all staff.
        /// </summary>
        /// <param name="staffId">The Id of the current staff member logged in.</param>
        /// <returns>True if successful; otherwise false.</returns>
        private bool LoadStaff(int staffId)
        {
            LoadingForm.Status = Resources.LoadingStaff;

            try
            {
                List<Staff> staff = EliteModule.GetStaff(staffId, false, true);

                if (staff == null || staff.Count == 0)
                    throw new SessionSummaryException(Resources.LoadingStaffFailedCurrentNotFound);
                else
                    StaffMember = staff[0];

                staff = EliteModule.GetStaff(0, true, false);

                if (staff == null || staff.Count == 0)
                    throw new SessionSummaryException(Resources.LoadingStaffFailedNoStaffFound);
                else
                {
                    // The default is current user (if they are in the list).
                    List<Staff> tempStaffList = (from staffMember in staff
                                                 where staffMember.Id != Staff.AutomatedSale && staffMember.Id != Staff.GameTechEmployee
                                                 select staffMember).ToList();
                    staff = tempStaffList;
                    staff.Sort();
                    m_staffMembers = staff;
                    RaisePropertyChanged("StaffMembers");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Get staff failed: " + ex.Message, LoggerLevel.Severe);
                MessageWindow.Show(string.Format(CultureInfo.CurrentCulture, Resources.LoadingStaffFailed, ex.Message), Resources.SessionSummaryName, MessageWindowType.Close);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates Session Summary's logger.
        /// </summary>
        /// <returns>true if success; otherwise false.</returns>
        private bool CreateLogger()
        {
            try
            {
                Logger = Logger.Create(Resources.SessionSummaryName, Settings);
                RaisePropertyChanged("Logger");
            }
            catch(Exception ex)
            {
                MessageWindow.Show(string.Format(CultureInfo.CurrentCulture, Resources.LogFailed, ex.Message), Resources.SessionSummaryName, MessageWindowType.Close);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create sub controllers used in the application.
        /// </summary>
        /// <returns>true if success; otherwise false.</returns>
        private bool CreateControllers()
        {
            LoadingForm.Status = Resources.LoadingControllers;
            try
            {
                SummaryController = new SummaryController(this);
                SessionCostsController = new SessionCostsController(this);
            }
            catch (Exception ex)
            {
                Logger.Log("Create controllers failed: " + ex.Message, LoggerLevel.Severe);
                MessageWindow.Show(string.Format(CultureInfo.CurrentCulture, Resources.LoadingControllersFailed, ex.Message), Resources.SessionSummaryName, MessageWindowType.Close);
                return false;
            }

            return true;
        }
       
        /// <summary>
        /// Creates the report used in the application.
        /// </summary>
        /// <returns>true if succces; otherwise false.</returns>
        private bool CreateReport()
        {
            LoadingForm.Status = Resources.LoadingReport;
            try
            {
                SessionSummaryReport = Report.GetReport(ReportId.SessionSummary, CultureInfo.CurrentCulture.Name, Settings).Document;
                SessionSummaryReport.SetParameterValue("@OperatorId", OperatorId);
                SessionSummaryReport.SetParameterValue("@IncludeMerchandise", 1);
                SessionSummaryReport.SetParameterValue("@IncludeConcessions", 1);
                SessionSummaryReport.SetParameterValue("@IncludePullTabs", 1);
            }
            catch (Exception ex)
            {
                Logger.Log("Create Report failed: " + ex.Message, LoggerLevel.Severe);
                MessageWindow.Show(string.Format(CultureInfo.CurrentCulture, Resources.LoadingReportFailed, ex.Message), Resources.SessionSummaryName, MessageWindowType.Close);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the windows used in the application.
        /// </summary>
        private bool CreateWindows()
        {
            try
            {
                MainWindow = new MainWindow(this, Settings.UseAcceleration);
                MainWindow.DataContext = new MainViewModel(this);
                CurrentView = MainWindow;
            }
            catch(Exception ex)
            {
                Logger.Log("Create UI failed: " + ex.Message, LoggerLevel.Severe);
                MessageWindow.Show(string.Format(CultureInfo.CurrentCulture, Resources.CreateUIFailed, ex.Message), Resources.SessionSummaryName, MessageWindowType.Close);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts the module.
        /// </summary>
        public void Run()
        {
            if(IsInitialized && MainWindow != null)
            {
                Logger.Log("Starting Session Summary.", LoggerLevel.Information);

                Application.Current.Run(MainWindow);
            }
        }

        /// <summary>
        /// Handles when something wants the module to stop.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs object that contains the event
        /// data.</param>
        public void OnStop(object sender, EventArgs e)
        {
            Stop(true);
        }

        /// <summary>
        /// Shuts down the application and, optionally, closes the main window.
        /// </summary>
        /// <param name="closeMainWindow">true if the main window should be
        /// closed; otherwise false.</param>
        public void Stop(bool closeMainWindow)
        {
            if(IsInitialized && MainWindow != null && closeMainWindow)
                MainWindow.Dispatcher.Invoke(new Action(MainWindow.Close), null);

            if(IsInitialized && Application.Current != null)
                Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles when something wants the module to come to the top of the
        /// screen.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs object that contains the event
        /// data.</param>
        public void OnComeToFront(object sender, EventArgs e)
        {
            if(IsInitialized && MainWindow != null)
                MainWindow.Dispatcher.Invoke(new Action(ActivateMainWindow), null);
        }

        /// <summary>
        /// Activates the main window and sets its window state to Normal.
        /// </summary>
        private void ActivateMainWindow()
        {
            if(MainWindow != null)
            {
                MainWindow.WindowState = WindowState.Normal;
                MainWindow.Activate();
            }
        }

        /// <summary>
        /// Handles when a server initiated message is received for the module.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A MessageReceivedEventArgs object that contains the
        /// event data.</param>
        public void OnServerMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            // TOOD: Handle any server initiated messages.
        }

        /// <summary>
        /// Handles when the MainWindow is loaded.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A RoutedEventArgs object that contains the
        /// event data.</param>
        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            MainWindow.Loaded -= MainWindowLoaded;
            DisposeLoadingForm();
        }

        /// <summary>
        /// Displays a window that tells the user the application is closing
        /// because of a server comm. failure.
        /// </summary>
        public void ServerCommFailure()
        {
            Window window = WindowHelper.GetActiveWindow();

            if(window != null)
                MessageWindow.Show(window, Resources.ServerCommFailed, TextAlignment.Center, string.Empty, MessageWindowType.Pause, EliteModule.ServerCommWaitTime);

            Logger.Log("Server communications failed. Shutting down.", LoggerLevel.Severe);
            OnStop(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the loading message (if the module is still loading)
        /// </summary>
        /// <param name="msg">the load message to display</param>
        public void LoadingMessage(string msg)
        {
            if (LoadingForm != null)
                LoadingForm.Status = msg;
        }

        /// <summary>
        /// Closes and disposes of the loading form.
        /// </summary>
        private void DisposeLoadingForm()
        {
            if(LoadingForm != null)
            {
                LoadingForm.CloseForm();
                LoadingForm.Dispose();
                LoadingForm = null;
            }
        }

        /// <summary>
        /// Starts the process of exiting the application.
        /// </summary>
        public void StartExit()
        {
            if(MainWindow != null)
                MainWindow.Close();
        }

        /// <summary>
        /// Exits the application (if the program isn't busy).
        /// </summary>
        /// <returns>true if the application is exiting; otherwise
        /// false.</returns>
        public bool Exit()
        {
            if(!IsBusy || (IsBusy && MessageWindow.Show(MainWindow, Resources.ConfirmExit, string.Empty, MessageWindowType.YesNo) == MessageBoxResult.Yes))
            {
                Stop(false);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Closes the current view.
        /// </summary>
        public void CloseCurrentView()
        {
            if(CurrentView != null && CurrentView != MainWindow)
                CurrentView.Close();
        }

        /// <summary>
        /// Displays the specified view.
        /// </summary>
        /// <param name="view">The view to display.</param>
        /// <exception cref="System.ArgumentException">view is
        /// invalid.</exception>
        public void NavigateTo(SessionSummaryView view, NavigateToArgs e)
        {
            switch(view)
            {
                case SessionSummaryView.About:
                    AboutWindow aboutWin = new AboutWindow(Settings.UseAcceleration, Resources.SessionSummaryName,
                                                           EliteModule.GetVersion(Assembly.GetExecutingAssembly()),
                                                           EliteModule.GetCopyright(Assembly.GetExecutingAssembly()),
                                                           Resources.ModuleDescription);
                    aboutWin.Owner = MainWindow;
                    CurrentView = aboutWin;
                    aboutWin.ShowDialog();

                    CurrentView = MainWindow;
                    break;

                case SessionSummaryView.SessionCosts:
                    {
                        SessionCostsWindow sessionCostWin = new SessionCostsWindow(Settings.UseAcceleration);
                        sessionCostWin.Owner = MainWindow;
                        sessionCostWin.DataContext = new SessionCostsViewModel(SessionCostsController);
                        CurrentView = sessionCostWin;
                        sessionCostWin.ShowDialog();

                        ((IDisposable)sessionCostWin.DataContext).Dispose();
                        CurrentView = MainWindow;
                    }
                    break;

                case SessionSummaryView.SelectSessionCosts:
                    {
                        SelectSessionCostsWindow sessionCostWin = new SelectSessionCostsWindow(Settings.UseAcceleration);
                        sessionCostWin.Owner = MainWindow;
                        sessionCostWin.DataContext = new SelectSessionCostsViewModel(SessionCostsController, (Summary)e.Argument);
                        CurrentView = sessionCostWin;
                        sessionCostWin.ShowDialog();

                        ((IDisposable)sessionCostWin.DataContext).Dispose();
                        CurrentView = MainWindow;
                    }
                    break;

                case SessionSummaryView.Report:
                    {
                        if (m_reportWindow == null)
                        {
                            m_reportWindow = new SessionSummaryReportWindow();
                            m_reportWindow.Owner = MainWindow;
                            m_reportWindow.Closed += ReportWindowClosed;
                            m_reportWindow.Show();
                        }
                        m_reportWindow.Focus();

                        ArrayList args = (ArrayList)e.Argument;
                        DateTime date = (DateTime)args[0];
                        short session = (short)args[1];

                        RefreshSessionSummaryReport(date, session);
                        m_reportWindow.DisplayReport(SessionSummaryReport);
                    }
                    break;

                case SessionSummaryView.ActualCashCalculator:
                    {
                        var calcWin = new ActualCashCalculatorWindow(Settings.UseAcceleration);
                        calcWin.Owner = MainWindow;
                        calcWin.DataContext = new ActualCashCalculatorViewModel(this, (Summary)e.Argument);
                        CurrentView = calcWin;
                        calcWin.ShowDialog();

                        var disposable = calcWin.DataContext as IDisposable;
                        if (disposable != null) disposable.Dispose();

                        CurrentView = MainWindow;
                    }
                    break;

                case SessionSummaryView.SessionBankBreakdown:
                    {
                        var calcWin = new SessionBanksWindow(Settings.UseAcceleration);
                        calcWin.Owner = MainWindow;
                        calcWin.DataContext = new SessionBanksViewModel(this, (List<BasicBankInfo>)e.Argument);
                        CurrentView = calcWin;
                        calcWin.ShowDialog();

                        var disposable = calcWin.DataContext as IDisposable;
                        if (disposable != null) disposable.Dispose();

                        CurrentView = MainWindow;
                    }
                    break;

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Handles when the report window has been closed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The EventArgs of the event.</param>
        void ReportWindowClosed(object sender, EventArgs e)
        {
            if (m_reportWindow != null)
            {
                m_reportWindow.Closed -= ReportWindowClosed;
                m_reportWindow = null;
            }
        }

        /// <summary>
        /// Refreshes the session summary report document
        /// </summary>
        /// <param name="date">The date of the session.</param>
        /// <param name="session">The number of the session.</param>
        /// <returns>The newly refreshed session summary report document.</returns>
        private ReportDocument RefreshSessionSummaryReport(DateTime date, short session)
        {
            // Clone the report so that if refreshes properly
            SessionSummaryReport = (ReportDocument)SessionSummaryReport.Clone();
            // Set the login info
            Report.SetDataSourceConnections(SessionSummaryReport, Settings);
            // Set the parameters again
            SessionSummaryReport.SetParameterValue("@OperatorId", OperatorId);
            SessionSummaryReport.SetParameterValue("@GamingDate", date);
            SessionSummaryReport.SetParameterValue("@Session", session);
            SessionSummaryReport.SetParameterValue("@IncludeMerchandise", 1);
            SessionSummaryReport.SetParameterValue("@IncludeConcessions", 1);
            SessionSummaryReport.SetParameterValue("@IncludePullTabs", 1);

            return SessionSummaryReport;
        }

        /// <summary>
        /// Performs any process or data clean up needed before the module is
        /// unloaded.
        /// </summary>
        public void Shutdown()
        {
            Logger.Log("Session Summary shutting down.", LoggerLevel.Debug);

            IsInitialized = false;

            CurrentView = null;

            if(MainWindow != null)
            {
                MainWindow.Close();

                if(MainWindow.DataContext != null && !((ViewModelBase)MainWindow.DataContext).IsDisposed)
                    ((ViewModelBase)MainWindow.DataContext).Dispose();

                MainWindow = null;

                // Dispose of the report
                SessionSummaryReport.Dispose();
                SessionSummaryReport = null;
            }

            InstanceId = -1;

            Logger.Log("Session Summary shutdown complete.", LoggerLevel.Information);
        }

        /// <summary>
        /// Loads the currencies in the system.
        /// </summary>
        /// <returns></returns>
        private bool LoadCurrencies()
        {
            LoadingForm.Status = Resources.LoadingCurrencyInformation;

            GetCurrencyDefinitionListMessage getCurrencyMsg = new GetCurrencyDefinitionListMessage(String.Empty, false);
            getCurrencyMsg.Send();

            if (getCurrencyMsg.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(String.Format(CultureInfo.CurrentCulture, Resources.GetCurrenciesFailed, ServerErrorTranslator.GetReturnCodeMessage(getCurrencyMsg.ReturnCode)));

            Currencies = new List<Currency>(getCurrencyMsg.Currencies);

            // Get the exchange rates.
            GetDailyExchangeRatesMessage getRatesMsg = new GetDailyExchangeRatesMessage();
            getRatesMsg.Send();

            if (getRatesMsg.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(String.Format(CultureInfo.CurrentCulture, Resources.GetExchangeRatesFailed, ServerErrorTranslator.GetReturnCodeMessage(getRatesMsg.ReturnCode)));

            foreach (var rate in getRatesMsg.Rates)
            {
                foreach (var currency in Currencies)
                {
                    if (currency.ISOCode == rate.Key)
                    {
                        currency.ExchangeRate = rate.Value;
                        break;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the unique identifier of the running instance of this
        /// module.
        /// </summary>
        private int InstanceId
        {
            get
            {
                return m_instanceId;
            }
            set
            {
                if(m_instanceId != value)
                {
                    m_instanceId = value;
                    RaisePropertyChanged("InstanceId");
                }
            }
        }

        /// <summary>
        /// Gets whether the call to the module's Initialize method was
        /// successful.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return m_isInitialized;
            }
            private set
            {
                m_isInitialized = value;
                RaisePropertyChanged("IsInitialized");
            }
        }

        /// <summary>
        /// Gets the the name of this module.
        /// </summary>
        public string Name
        {
            get
            {
                return Resources.SessionSummaryName;
            }
        }

        /// <summary>
        /// Gets or sets the loading form.
        /// </summary>
        private LoadingForm LoadingForm
        {
            get
            {
                return m_loadingForm;
            }
            set
            {
                m_loadingForm = value;
                RaisePropertyChanged("LoadingForm");
            }
        }

        /// <summary>
        /// Gets the application's main window.
        /// </summary>
        public Window MainWindow
        {
            get
            {
                return m_mainWindow;
            }
            private set
            {
                m_mainWindow = value;
                RaisePropertyChanged("MainWindow");
            }
        }

        /// <summary>
        /// Gets the current, active view.
        /// </summary>
        public Window CurrentView
        {
            get
            {
                return m_currentWindow;
            }
            private set
            {
                if(m_currentWindow != value)
                {
                    m_currentWindow = value;
                    RaisePropertyChanged("CurrentView");
                }
            }
        }

        /// <summary>
        /// Gets Session Summary's settings.
        /// </summary>
        public SessionSummarySettings Settings
        {
            get
            {
                return m_settings;
            }
            private set
            {
                m_settings = value;
                RaisePropertyChanged("Settings");
            }
        }

        /// <summary>
        /// Gets whether the controller is performing a long running operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return m_isBusy;
            }
            private set
            {
                m_isBusy = value;
                RaisePropertyChanged("IsBusy");
            }
        }

        /// <summary>
        /// Gets the operator id of this module.
        /// </summary>
        public int OperatorId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a list of all staff in the system.
        /// </summary>
        public IEnumerable<Staff> StaffMembers
        {
            get
            {
                return m_staffMembers;
            }
        }

        /// <summary>
        /// Gets the current staff member logged in.
        /// </summary>
        public Staff StaffMember
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the session summary report.
        /// </summary>
        public ReportDocument SessionSummaryReport
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the controller that manages session summaries
        /// </summary>
        public ISummaryController SummaryController
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the controller that manages session costs.
        /// </summary>
        public ISessionCostsController SessionCostsController
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or Sets the list of supported currencies in the system.
        /// </summary>
        public IEnumerable<Currency> Currencies 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// The current active cash method for this user.
        /// </summary>
        public CashMethod CashMethod { get; private set; }

        #endregion

        #region Static Properties
        /// <summary>
        /// Gets the Session Summary's logger.
        /// </summary>
        public static Logger Logger
        {
            get;
            private set;
        }
        #endregion
    }   
}