using System;
using System.Collections.ObjectModel;
using System.IO;
using ReactiveUI;
using Avalonia.Metadata;
using Martridge.Models;
using Martridge.Models.Configuration;
using Martridge.Models.Installer;
using Martridge.Models.Localization;
using Martridge.Trace;
using Martridge.ViewModels.DinkyAlerts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace Martridge.ViewModels.Installer
{
    public class DinkInstallerViewModel : ViewModelAppPage
    {
        // ------------------------------------------------------------------------------------------
        //      Progress reporting 
        //

        /// <summary>
        /// A nice title to show at the top of the UI
        /// </summary>
        public string DinkInstallerTitle {
            get => this._dinkInstallerTitle;
            private set => this.RaiseAndSetIfChanged(ref this._dinkInstallerTitle, value);
        }
        private string _dinkInstallerTitle = "";

        /// <summary>
        /// Heading for primary progress bar
        /// </summary>
        public string ProgressPrimaryHeading {
            get => this._progressPrimaryHeading;
            private set => this.RaiseAndSetIfChanged(ref this._progressPrimaryHeading, value);
        }
        private string _progressPrimaryHeading = "";

        /// <summary>
        /// Subheading for primary progress bar
        /// </summary>
        public string ProgressPrimarySubheading {
            get => this._progressPrimarySubheading;
            private set => this.RaiseAndSetIfChanged(ref this._progressPrimarySubheading, value);
        }
        private string _progressPrimarySubheading = "";

        /// <summary>
        /// Primary progress bar percent
        /// </summary>
        public double ProgressPrimaryPercent {
            get => this._progressPrimaryPercent;
            private set => this.RaiseAndSetIfChanged(ref this._progressPrimaryPercent, value);
        }
        private double _progressPrimaryPercent = 0.0;

        /// <summary>
        /// If true, should show the secondary progress bar
        /// </summary>
        public bool ProgressSecondaryIsVisible {
            get => this._progressSecondaryIsVisible;
            private set => this.RaiseAndSetIfChanged(ref this._progressSecondaryIsVisible, value);
        }
        private bool _progressSecondaryIsVisible = false;

        /// <summary>
        /// Heading for secondary progress bar
        /// </summary>
        public string ProgressSecondaryHeading {
            get => this._progressSecondaryHeading;
            private set => this.RaiseAndSetIfChanged(ref this._progressSecondaryHeading, value);
        }
        private string _progressSecondaryHeading = "";

        /// <summary>
        /// Subheading for secondary progress bar
        /// </summary>
        public string ProgressSecondarySubtitle {
            get => this._progressSecondarySubtitle;
            private set => this.RaiseAndSetIfChanged(ref this._progressSecondarySubtitle, value);
        }
        private string _progressSecondarySubtitle = "";

        /// <summary>
        /// Secondary progress bar percent
        /// </summary>
        public double ProgressSecondaryPercent {
            get => this._progressSecondaryPercent;
            private set => this.RaiseAndSetIfChanged(ref this._progressSecondaryPercent, value);
        }
        private double _progressSecondaryPercent = 0.0;

        /// <summary>
        /// If true, secondary progress is indeterminate
        /// </summary>
        public bool ProgressSecondaryIsIndeterminate {
            get => this._progressSecondaryIsIndeterminate;
            private set => this.RaiseAndSetIfChanged(ref this._progressSecondaryIsIndeterminate, value);
        }
        private bool _progressSecondaryIsIndeterminate = false;


        /// <summary>
        /// Installer log text...
        /// </summary>
        public string InstallerLogText {
            get => this._installerTraceListener?.Text ?? "";
        }
        private MyTraceListenerGui? _installerTraceListener = null;

        /// <summary>
        /// Installer log caret index...
        /// </summary>
        public int InstallerLogCaretIndex {
            get => this._installerLogCaretIndex;
            private set => this.RaiseAndSetIfChanged(ref this._installerLogCaretIndex, value);
        }
        private int _installerLogCaretIndex = 0;



        // ------------------------------------------------------------------------------------------
        //      Installer State 
        //

        public bool IsInstallerStarted {
            get => this._isInstallerStarted;
            private set => this.RaiseAndSetIfChanged(ref this._isInstallerStarted, value);
        }
        private bool _isInstallerStarted = false;

        public bool IsInstallerFinished {
            get => this._isInstallerFinished;
            private set => this.RaiseAndSetIfChanged(ref this._isInstallerFinished, value);
        }
        private bool _isInstallerFinished = false;

        public bool IsInstallerCancelled {
            get => this._isInstallerCancelled;
            private set => this.RaiseAndSetIfChanged(ref this._isInstallerCancelled, value);
        }
        private bool _isInstallerCancelled = false;

        public bool IsFileBrowserActive {
            get => this._isFileBrowserActive;
            private set => this.RaiseAndSetIfChanged(ref this._isFileBrowserActive, value);
        }
        private bool _isFileBrowserActive = false;


        // ------------------------------------------------------------------------------------------
        //      Installable Selection 
        //

        public ObservableCollection<DinkInstallableCategory> InstallableCategories { get; } = new ObservableCollection<DinkInstallableCategory>();

        public DinkInstallableCategory? SelectedInstallableCategory {
            get => this._selectedInstallableCategory;
            set => this.RaiseAndSetIfChanged(ref this._selectedInstallableCategory, value);
        }
        private DinkInstallableCategory? _selectedInstallableCategory = null;

        public DinkInstallableEntry? SelectedInstallable {
            get => this._selectedInstallable;
            set => this.RaiseAndSetIfChanged(ref this._selectedInstallable, value);
        }
        private DinkInstallableEntry? _selectedInstallable = null;



        // ------------------------------------------------------------------------------------------
        //      Installer logic
        //

        public event EventHandler<DinkInstallerDoneEventArgs>? InstallerDone;
        private DinkInstallerDoneEventArgs _lastInstallerDoneEventArgs = new DinkInstallerDoneEventArgs(DinkInstallerResult.Cancelled);

        private DinkInstaller? _installerLogic = null;
        private DinkInstallerOnlineListHelper? _installerOnlineHelper = null;

#if PLATF_WINDOWS
        public bool DinkInstallerNotSupported {
            get => false;
        }
#else
        public bool DinkInstallerNotSupported  {
            get => true;
        }
#endif



        // ------------------------------------------------------------------------------------------
        //      Pre-Install Info... 
        //

        public bool IsInstallableInitialized {
            get => this._isInstallableInitialized;
            private set => this.RaiseAndSetIfChanged(ref this._isInstallableInitialized, value);
        }
        private bool _isInstallableInitialized = false;

        public bool IsInstallableInitializing {
            get => this._isInstallableInitializing;
            private set => this.RaiseAndSetIfChanged(ref this._isInstallableInitializing, value);
        }
        private bool _isInstallableInitializing = false;

        public string InstallerDestination {
            get => this._installerDestination;
            private set =>
                // TODO... validate path...
                this.RaiseAndSetIfChanged(ref this._installerDestination, value);
        }
        private string _installerDestination = "";
        private string _installerDestinationAuto = "";
        private string _installerDestinationAutoPreviousName = "";

        // ------------------------------------------------------------------------------------------
        //      Constructor 
        //
        public DinkInstallerViewModel()
        {
            //this.SelectedInstallableVersionIndex = 0;
            this.PropertyChanged += this.DinkInstallerViewModel_PropertyChanged;
        }

        private void DinkInstallerViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.SelectedInstallable))
            {
                if (this.SelectedInstallable != null)
                {
                    string name = !string.IsNullOrWhiteSpace(this.SelectedInstallable.InstallerData.DestinationName)
                        ?   this.SelectedInstallable.InstallerData.DestinationName
                        :   this.SelectedInstallable.InstallerData.Name;

                    if (string.IsNullOrWhiteSpace(this._installerDestinationAutoPreviousName) == false &&
                        this._installerDestinationAuto.EndsWith(this._installerDestinationAutoPreviousName))
                    {
                        string baseStr = this._installerDestinationAuto.Substring(0, this._installerDestinationAuto.Length - this._installerDestinationAutoPreviousName.Length);
                        // maintain same base destination
                        this._installerDestinationAuto = Path.Combine(baseStr, name);
                    }
                    else
                    {
                        this._installerDestinationAuto = Path.Combine(LocationHelper.AppBaseDirectory, name);
                    }


                    this._installerDestinationAutoPreviousName = name;
                    this.InstallerDestination = this._installerDestinationAuto;
                    this.DinkInstallerTitle = this.SelectedInstallable.DisplayName;
                }
                else
                {
                    this.InstallerDestination = "";
                }
            }
            else if (e.PropertyName == nameof(this.SelectedInstallableCategory))
            {
                // NOTE: it seems that automatically selecting an installable like this sometimes leads to the GUI TextBoxes not being properly updated...
                // need to investigate later, but for now just let the user manually select instead
                // I guess this has something to do with the ItemSource and SelectedItem for the ListBox changing?... 
                //this.SelectedInstallable = this.SelectedInstallableCategory?.InstallerEntries.First();
            }
        }

        public async void InitializeInstallerList(bool updateDefaultsIfFileExists)
        {
            try
            {
                if (this._installerOnlineHelper != null) return;
                this.IsInstallableInitializing = true;
                this.IsInstallableInitialized = false;

                this.SelectedInstallable = null;
                this.SelectedInstallableCategory = null;
                this.InstallableCategories.Clear();

                this._installerOnlineHelper = new DinkInstallerOnlineListHelper();

                ConfigInstallerList? cfgInst = await this._installerOnlineHelper.GetConfigInstallerList();

                if (cfgInst == null) return;

                this.SelectedInstallable = null;
                this.SelectedInstallableCategory = null;
                this.InstallableCategories.Clear();

                Dictionary<string, DinkInstallableCategory> tempDict = new Dictionary<string, DinkInstallableCategory>();
                foreach (var kvp in cfgInst.Installables)
                {
                    foreach (var kvp2 in kvp.Value)
                    {
                        if (tempDict.ContainsKey(kvp2.Value.Category) == false)
                        {
                            DinkInstallableCategory cat = new DinkInstallableCategory(kvp2.Value.Category);
                            tempDict.Add(kvp2.Value.Category, cat);
                            this.InstallableCategories.Add(cat);
                        }
                        tempDict[kvp2.Value.Category].AddDinkInstallerdata(kvp2.Value);
                    }
                }

                this.SelectedInstallableCategory = this.InstallableCategories.First();
                //this.SelectedInstallable = this.SelectedInstallableCategory?.InstallerEntries.First();

                this.IsInstallableInitialized = true;
            } catch (Exception ex)
            {
                MyTrace.Global.WriteMessage(MyTraceCategory.DinkInstaller, $"Error initializing installables from configInstallerList.json");
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
            finally
            {
                this.IsInstallableInitializing = false;
                this._installerOnlineHelper = null;
            }
        }

        // ------------------------------------------------------------------------------------------
        //      Commands
        //

        #region Commands

        public void CmdExit(object? parameter = null)
        {
            this._installerOnlineHelper?.CancelTokenSource.Cancel();

            if (this.IsInstallerStarted == false || this.DinkInstallerNotSupported)
            {
                this._lastInstallerDoneEventArgs = new DinkInstallerDoneEventArgs(DinkInstallerResult.Cancelled);
                this.ResetInstallerStateAndFireDoneEvent();
            }
            if (this.IsInstallerFinished)
            {
                this.ResetInstallerStateAndFireDoneEvent();
            }
        }

        [DependsOn(nameof(IsInstallerStarted))]
        [DependsOn(nameof(IsInstallerCancelled))]
        [DependsOn(nameof(IsInstallerFinished))]
        public bool CanCmdExit(object? parameter = null)
        {
            if (this.IsInstallerCancelled) return false;
            if (this.IsInstallerStarted) return this.IsInstallerFinished;

            return true;
        }

        public void CmdCancel(object? parameter = null)
        {
            if (this.CanCmdCancel() == false) return;

            this.IsInstallerCancelled = true;
            // null check done in CanCmdCancel
            this._installerLogic!.Cancel();
        }
        [DependsOn(nameof(IsInstallerStarted))]
        [DependsOn(nameof(IsInstallerCancelled))]
        [DependsOn(nameof(IsInstallerFinished))]
        public bool CanCmdCancel(object? parameter = null)
        {
            if (this._installerLogic == null) return false;
            // can't cancel if the installer is not started
            if (this.IsInstallerStarted == false) return false;
            // can't cancel if already done
            if (this.IsInstallerFinished) return false;
            // can only cancel once
            return !this.IsInstallerCancelled;
        }

        public async void CmdStartInstall(object? parameter = null)
        {
            if (this.CanCmdStartInstall() == false) return;

            await this.StartInstallation();
        }

        [DependsOn(nameof(DinkInstallerNotSupported))]
        [DependsOn(nameof(IsInstallerStarted))]
        [DependsOn(nameof(SelectedInstallable))]
        public bool CanCmdStartInstall(object? parameter = null)
        {
            if (this.DinkInstallerNotSupported) return false;
            if (this.IsInstallerStarted) return false;
            if (this.SelectedInstallable == null) return false;
            if (this._installerLogic != null) return false;

            return true;
        }

        public async void CmdBrowseDestination(object? parameter = null)
        {
            await this.BrowseDestination();
        }

        [DependsOn(nameof(IsFileBrowserActive))]
        [DependsOn(nameof(SelectedInstallable))]
        public bool CanCmdBrowseDestination(object? parameter = null)
        {
            return !this.IsFileBrowserActive && this.SelectedInstallable != null;
        }

        private Task BrowseDestination()
        {
            if (this.SelectedInstallable == null || this.IsFileBrowserActive)
                return Task.CompletedTask;
            
            this.IsFileBrowserActive = true;

            return Task.Run( () => {

                try
                {
                    string baseDirectory = LocationHelper.AppBaseDirectory;
                    if (this._installerDestinationAuto == this._installerDestination)
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(this._installerDestination);
                        if (dirInfo.Parent?.Exists == true)
                        {
                            baseDirectory = dirInfo.Parent.FullName;
                        }
                    }
                    else
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(this._installerDestination);
                        if (dirInfo.Exists)
                        {
                            baseDirectory = dirInfo.FullName;
                        }
                    }
                    
                    IStorageFolder? storageFolder = LocationHelper.BrowseFolderPicker(
                        Localizer.Instance["DinkInstaller/BrowseDinkDestination"],
                        baseDirectory);
                    
                    if (storageFolder != null)
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(storageFolder.Path.LocalPath);

                        if (dirInfo.Exists && dirInfo.Parent != null)
                        {
                            string name = !string.IsNullOrWhiteSpace(this.SelectedInstallable.InstallerData.DestinationName)
                                ?   this.SelectedInstallable.InstallerData.DestinationName
                                :   this.SelectedInstallable.InstallerData.Name;

                            if (
                                // name sameness check depends on platform...
                        #if PLATF_WINDOWS
                                dirInfo.Name.ToLowerInvariant() == name.ToLowerInvariant()
                        #else
                                dirInfo.Name == name
                        #endif
                            )
                            {
                                string basePath = dirInfo.FullName.Substring(0, dirInfo.FullName.Length - name.Length);
                                this._installerDestinationAuto = Path.Combine(basePath, name);
                            }
                            else
                            {
                                this._installerDestinationAuto = Path.Combine(dirInfo.FullName, name);
                            }

                            this._installerDestinationAutoPreviousName = name;
                            this.InstallerDestination = this._installerDestinationAuto;
                        }
                    }
                } catch (Exception ex)
                {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
                finally
                {
                    this.IsFileBrowserActive = false;
                }
            });
        }

        #endregion

        // ------------------------------------------------------------------------------------------
        //      Installer logic
        //

        private Task StartInstallation()
        {
            if (this.CanCmdStartInstall() == false) return Task.CompletedTask;

            return Task.Run(() => {
                try
                {
                    this.IsInstallerStarted = true;

                    DirectoryInfo destination = new DirectoryInfo(this.InstallerDestination);
                    bool removeOldFiles = false;

                    // create installer trace listener
                    this._installerTraceListener = new MyTraceListenerGui("Installer Trace Listener");
                    this._installerTraceListener.ShowLevels = false;
                    this._installerTraceListener.Levels = MyTraceLevel.Critical | MyTraceLevel.Error | MyTraceLevel.Warning | MyTraceLevel.Information;
                    this._installerTraceListener.PropertyChanged += ( sender,  args) => {
                        this.RaisePropertyChanged(nameof(this.InstallerLogText));
                        this.InstallerLogCaretIndex = int.MaxValue;
                    };

                    // Check if destination already exists
                    if (destination.Exists && (destination.GetDirectories().Length > 0 ||
                            destination.GetFiles().Length > 0))
                    {

                        string title = Localizer.Instance[@"DinkInstallerView/MessageBox_CleanInstallConfirm_Title"];
                        string body = Localizer.Instance[@"DinkInstallerView/MessageBox_CleanInstallConfirm_Body"];



                        // make sure the correct new line characters are used
                        body = body.Replace("\n\r", Environment.NewLine);

                        Task<AlertResults> taskDialog = DinkyAlert.ShowDialog(title, body, AlertResults.Yes | AlertResults.No | AlertResults.Cancel, AlertType.Warning);
                        taskDialog.Wait();
                        
                        if (taskDialog.Result == AlertResults.Cancel)
                        {
                            this.InstallerOnDone(this, new DinkInstallerDoneEventArgs(DinkInstallerResult.Cancelled));
                            return;
                        }
                        if (taskDialog.Result == AlertResults.Yes)
                        {
                            title = Localizer.Instance[@"DinkInstallerView/MessageBox_CleanInstallConfirm_DeleteDoubleConfirm_Title"];
                            body = Localizer.Instance[@"DinkInstallerView/MessageBox_CleanInstallConfirm_DeleteDoubleConfirm_Body"];
                            body += Environment.NewLine;
                            body += destination.FullName;

                            Task<AlertResults> taskDialog2 = DinkyAlert.ShowDialog(title, body, AlertResults.Yes | AlertResults.Cancel, AlertType.Warning);
                            taskDialog2.Wait();
                            
                            if (taskDialog2.Result == AlertResults.Yes)
                            {
                                removeOldFiles = true;
                            }
                            else
                            {
                                this.InstallerOnDone(this, new DinkInstallerDoneEventArgs(DinkInstallerResult.Cancelled));
                                return;
                            }
                        }
                    }

                    // create installer logic
                    this._installerLogic = new DinkInstaller();
                    this._installerLogic.CustomTrace.Listeners.Add(this._installerTraceListener);
                    this._installerLogic.ProgressReport += this.InstallerOnPrimaryProgressReport;
                    this._installerLogic.SecondaryProgressReport += this.InstallerOnSecondaryProgressReport;
                    this._installerLogic.InstallerDone += this.InstallerOnDone;

                    // start installation
                    this._installerLogic.InstallDink(destination, removeOldFiles,  this.SelectedInstallable!.InstallerData);
                } catch (Exception ex)
                {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }
        private void ResetInstallerStateAndFireDoneEvent()
        {
            this._installerLogic?.CustomTrace.Flush();
            this._installerLogic?.CustomTrace.Close();
            this._installerLogic = null;

            this._installerTraceListener?.Close();
            this._installerTraceListener = null;

            this.IsInstallerStarted = false;
            this.IsInstallerFinished = false;
            this.IsInstallerCancelled = false;

            this.InstallerDone?.Invoke(this, this._lastInstallerDoneEventArgs);
        }

        private void InstallerOnDone(object? sender, DinkInstallerDoneEventArgs args)
        {
            this.IsInstallerFinished = true;
            this._lastInstallerDoneEventArgs = args;
            if (args.Result == DinkInstallerResult.Cancelled) this.ShowInstallerCancelledMessageBox();
            if (args.Result == DinkInstallerResult.Error) this.ShowInstallerErrorMessageBox(args.Exception);
        }
        private void InstallerOnPrimaryProgressReport(object? sender, DinkInstallerProgressEventArgs args)
        {
            try
            {
                this.ProgressPrimaryHeading = args.HeadingMain;
                this.ProgressPrimarySubheading = args.HeadingSecondary;
                this.ProgressPrimaryPercent = args.ProgressPercent;
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }

        private void InstallerOnSecondaryProgressReport(object? sender, DinkInstallerProgressEventArgs args)
        {
            try
            {
                this.ProgressSecondaryHeading = args.HeadingMain;
                this.ProgressSecondarySubtitle = args.HeadingSecondary;
                this.ProgressSecondaryPercent = args.ProgressPercent;

                if (double.IsNaN(args.ProgressPercent))
                {
                    this.ProgressSecondaryIsIndeterminate = true;
                    this.ProgressSecondaryIsVisible = true;
                }
                else
                {
                    this.ProgressSecondaryIsIndeterminate = false;
                    this.ProgressSecondaryIsVisible = !(Math.Abs(args.ProgressPercent - 1.0) < 0.00001);
                }
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }

        // ------------------------------------------------------------------------------------------
        //      Message box stuff
        //

        private void ShowInstallerCancelledMessageBox()
        {
            Dispatcher.UIThread.InvokeAsync(async () => {
                try
                {
                    string title = Localizer.Instance[@"DinkInstallerView/MessageBox_Cancel_Title"];
                    string body = Localizer.Instance[@"DinkInstallerView/MessageBox_Cancel_Body"];
                    await DinkyAlert.ShowDialog(title, body, AlertResults.Ok, AlertType.Info);
                } catch (Exception ex)
                {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }

        private void ShowInstallerErrorMessageBox(Exception? exception)
        {
            Dispatcher.UIThread.InvokeAsync(async () => {
                try
                {
                    string title = Localizer.Instance[@"DinkInstallerView/MessageBox_Error_Title"];
                    string body = Localizer.Instance[@"DinkInstallerView/MessageBox_Error_Body"] + Environment.NewLine + MyTrace.GetExceptionMessages(exception);
                    await DinkyAlert.ShowDialog(title, body, AlertResults.Ok, AlertType.Error);
                } catch (Exception ex)
                {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }
    }
}
