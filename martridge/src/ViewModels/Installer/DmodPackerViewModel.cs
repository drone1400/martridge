using Avalonia.Metadata;
using Martridge.Models.Installer;
using Martridge.Models.Localization;
using Martridge.Trace;
using Martridge.ViewModels.DinkyAlerts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Martridge.Models;
using Martridge.Models.Configuration;
using Martridge.Models.Dmod;
using Martridge.Models.DmodInstaller;
using Martridge.Models.DmodPacker;
using ReactiveUI.Validation.Extensions;

namespace Martridge.ViewModels.Installer {
    public class DmodPackerViewModel : ViewModelBase {

        /// <summary>
        /// A nice title to show at the top of the UI
        /// </summary>
        public string DmodPackerTitle {
            get => this._dmodPackerTitle;
            private set => this.RaiseAndSetIfChanged(ref this._dmodPackerTitle, value);
        }
        private string _dmodPackerTitle = "";

        /// <summary>
        /// Indicates overall packing phase
        /// </summary>
        public double DmodPackerPhaseProgressPercent {
            get => this._dmodPackerPhaseProgressPercent;
            private set => this.RaiseAndSetIfChanged(ref this._dmodPackerPhaseProgressPercent, value);
        }
        private double _dmodPackerPhaseProgressPercent = 0;

        /// <summary>
        /// Shows various DMOD packer log info
        /// </summary>
        public string DmodPackerProgressLog {
            get => this._packerTraceListener?.Text ?? "";
        }
        private MyTraceListenerGui? _packerTraceListener = null;

        /// <summary>
        /// For keeping the DMOD packer log textbox scrolled down
        /// </summary>
        public int DmodPackerProgressLogCaretIndex {
            get => this._dmodPackerProgressLogCaretIndex;
            private set => this.RaiseAndSetIfChanged(ref this._dmodPackerProgressLogCaretIndex, value);
        }
        private int _dmodPackerProgressLogCaretIndex = 0;

        /// <summary>
        /// If true, the DMOD is actively being packed or scanned
        /// </summary>
        public bool DmodPackingInProgress {
            get => this._dmodPackingInProgress;
            private set => this.RaiseAndSetIfChanged(ref this._dmodPackingInProgress, value);
        }
        private bool _dmodPackingInProgress = false;

        /// <summary>
        /// Temporary DMOD destination value holder
        /// </summary>
        public string TemporaryDmodDestination {
            get => this._temporaryDmodDestination;
            private set => this.RaiseAndSetIfChanged(ref this._temporaryDmodDestination, value);
        }
        private string _temporaryDmodDestination = "";
        
        /// <summary>
        /// Final DMOD destination, this is what is actually used to pack the DMOD
        /// </summary>
        public string FinalDmodDestination {
            get => this._finalDmodDestination;
            private set => this.RaiseAndSetIfChanged(ref this._finalDmodDestination, value);
        }
        private string _finalDmodDestination = "";
        
        /// <summary>
        /// Temporary DMOD source value holder
        /// </summary>
        public string TemporaryDmodSourceDirectory {
            get => this._temporaryDmodSourceDirectory;
            set => this.RaiseAndSetIfChanged( ref this._temporaryDmodSourceDirectory, value);
        }
        private string _temporaryDmodSourceDirectory = "";

        /// <summary>
        /// Final DMOD source directory
        /// </summary>
        public string FinalDmodSourceDirectory {
            get => this._finalDmodSourceDirectory;
            private set => this.RaiseAndSetIfChanged(ref this._finalDmodSourceDirectory, value);
        }
        private string _finalDmodSourceDirectory = "";

        /// <summary>
        /// Current DMOD install phase...
        /// </summary>
        public DmodPackerPhase PackerPhase {
            get => this._packerPhase;
            private set => this.RaiseAndSetIfChanged( ref this._packerPhase, value);
        }
        private DmodPackerPhase _packerPhase = DmodPackerPhase.Inactive;

        /// <summary>
        /// Indicates if the file browser was opened and not yet closed
        /// </summary>
        public bool IsFileBrowserActive {
            get => this._isFileBrowserActive;
            private set => this.RaiseAndSetIfChanged(ref this._isFileBrowserActive, value);
        }
        private bool _isFileBrowserActive = false;

        // ------------------------------------------------------------------------------------------
        //      Installer logic 
        //
        
        public event EventHandler<DmodPackerDoneEventArgs>? PackerDone;
        
        private DmodPackerDoneEventArgs _packerDoneEventArgs = new DmodPackerDoneEventArgs(DinkInstallerResult.Cancelled);
        
        private ConfigGeneral? _configGeneral = null;
        private DmodPacker? _packerLogic = null;


        // ------------------------------------------------------------------------------------------
        //      Constructor
        //
        
        public DmodPackerViewModel() {
            this.ResetPackerState();
            
            // validate source DMOD directory
            this.ValidationRule(x => x.TemporaryDmodSourceDirectory,
                dmodSource => string.IsNullOrWhiteSpace(dmodSource) == false,
                Localizer.Instance["DmodPacker/ViewModel/Validation/SourceDmodEmpty"]);
            this.ValidationRule(x => x.TemporaryDmodSourceDirectory,
                dmodSource => {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(dmodSource)) return true; // validated elsewhere...
                        DmodFileDefinition dfd = new DmodFileDefinition(new DirectoryInfo(dmodSource));
                        return dfd.IsCorrectlyDefined;
                    } catch (Exception)
                    {
                        return false;
                    }
                },
                Localizer.Instance["DmodPacker/ViewModel/Validation/NotAValidDmodDirectory"]);
            
            
            // validate final destination DMOD file...
            this.ValidationRule(x => x.TemporaryDmodDestination,
                finalDestination => string.IsNullOrWhiteSpace(finalDestination) == false,
                Localizer.Instance["DmodPacker/ViewModel/Validation/DestinationDmodEmpty"]);
            this.ValidationRule(x => x.TemporaryDmodDestination,
                dmodDestination => {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(dmodDestination)) return true; // validated elsewhere...
                        FileInfo fileInfo = new FileInfo(dmodDestination);
                        return fileInfo.Exists == false;
                    } catch (Exception)
                    {
                        return false;
                    }
                },
                Localizer.Instance["DmodPacker/ViewModel/Validation/DestinationDmodAlreadyExists"]);
        }

        public void InitializeConfiguration(ConfigGeneral cfg) {
            if (this._configGeneral != null) {
                this._configGeneral.Updated -= this.GeneralUpdated;
            }

            this._configGeneral = cfg;
            if (this._configGeneral != null) {
                this._configGeneral.Updated += this.GeneralUpdated;
            }
        }

        private void GeneralUpdated(object? sender, EventArgs e) {
            // TODO?...
        }

        // ------------------------------------------------------------------------------------------
        //      Commands
        //

        #region Commands

        public async void CmdInitializeDmod(object? parameter = null)
        {
            if (this.PackerPhase != DmodPackerPhase.Inactive) return;
            if (parameter is not string dmodPath) return;

            try
            {
                DmodFileDefinition dfd = new DmodFileDefinition(dmodPath);
                if (dfd.IsCorrectlyDefined == false) return;
            } catch (Exception)
            {
                return;
            }

            this.FinalDmodSourceDirectory = dmodPath;
            await this.StartInitializingDmod();
        }
        [DependsOn(nameof(PackerPhase))]
        public bool CanCmdInitializeDmod(object? parameter = null)
        {
            if (this.PackerPhase != DmodPackerPhase.Inactive) return false;
            if (parameter is not string dmodPath) return false;

            try
            {
                DmodFileDefinition dfd = new DmodFileDefinition(dmodPath);
                if (dfd.IsCorrectlyDefined == false) return false;
            } catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void CmdFinish(object? parameter = null)
        {
            if (this.PackerPhase != DmodPackerPhase.Finished)
                return;
            
            this.ResetPackerState();
            this.FirePackerDone();
        }
        [DependsOn(nameof(PackerPhase))]
        public bool CanCmdFinish(object? parameter = null)
        {
            return this.PackerPhase == DmodPackerPhase.Finished;
        }

        public void CmdCancel(object? parameter = null) {
            if (this.CanCmdCancel() == false) return;

            if (this._packerLogic == null)
            {
                this._packerDoneEventArgs = new DmodPackerDoneEventArgs(DinkInstallerResult.Cancelled);
                this.ResetPackerState();
                this.FirePackerDone();
            }
            else
            {
                this._packerLogic.Cancel();
            }
        }
        [DependsOn(nameof(PackerPhase))]
        public bool CanCmdCancel(object? parameter = null)
        {
            if (this.PackerPhase == DmodPackerPhase.Inactive) return true;
            if (this.PackerPhase == DmodPackerPhase.Initializing) return true;
            if (this.PackerPhase == DmodPackerPhase.AwaitingUserInput) return true;
            if (this.PackerPhase == DmodPackerPhase.Packing) return true;

            return false;
        }
        
        public async void CmdStartPacking(object? parameter = null)
        {
            if (this.CanCmdStartPacking(parameter) == false) return;
            this.FinalDmodDestination = (parameter as string)!; // already validated above...
            await this.StartPacking();
        }
        [DependsOn(nameof(PackerPhase))]
        public bool CanCmdStartPacking(object? parameter = null)
        {
            if (parameter is not string path) return false;
            if (string.IsNullOrWhiteSpace(path)) return false;
            if (File.Exists(path)) return false;
            if (this.PackerPhase != DmodPackerPhase.AwaitingUserInput) return false;
            return true;
        }

        public async void CmdBrowseDmodSource(object? parameter = null) {
            await this.BrowseDmodSource();
        }

        [DependsOn(nameof(IsFileBrowserActive))]
        public bool CanCmdBrowseDmodSource(object? parameter = null) {
            return !this.IsFileBrowserActive;
        }
        
        public async void CmdBrowseDmodDestination(object? parameter = null) {
            await this.BrowseDmodDestination();
        }

        [DependsOn(nameof(IsFileBrowserActive))]
        public bool CanCmdBrowseDmodDestination(object? parameter = null) {
            return !this.IsFileBrowserActive;
        }
        
        #endregion
        
        
        // ------------------------------------------------------------------------------------------
        //      Command logic
        //

        private Task BrowseDmodSource() {
            if (this.IsFileBrowserActive)
                return Task.CompletedTask;
            
            this.IsFileBrowserActive = true;

            return Task.Run(() => {
                try
                {
                    IStorageFolder? storageFolder = LocationHelper.BrowseFolderPicker(
                        Localizer.Instance["DmodPacker/ViewModel/BrowseDmodSource"]);

                    if (storageFolder != null)
                    {
                        this.TemporaryDmodSourceDirectory = storageFolder.Path.LocalPath;
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
        
        private Task BrowseDmodDestination() {
            if (this.IsFileBrowserActive)
                return Task.CompletedTask;
            
            this.IsFileBrowserActive = true;

            return Task.Run(() => {
                try
                {
                    IStorageFile? storageFile = LocationHelper.BrowseFileSave(
                        Localizer.Instance["DmodPacker/ViewModel/BrowseDmodDestination"],
                        new [] {
                            new FilePickerFileType("DMOD") {
                                Patterns = new [] { "*.dmod" },
                            },
                        });

                    if (storageFile != null)
                    {
                        this.TemporaryDmodDestination = storageFile.Path.LocalPath;
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

        private Task StartInitializingDmod()
        {
            return Task.Run(() => {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(this.FinalDmodSourceDirectory);
                    if (dirInfo.Exists == false) return;
                    
                    // preemptively update phase...
                    this.PackerPhase = DmodPackerPhase.Initializing;
                    
                    // create installer trace listener
                    this._packerTraceListener = new MyTraceListenerGui("Installer Trace Listener");
                    this._packerTraceListener.ShowLevels = false;
                    this._packerTraceListener.Levels = MyTraceLevel.Critical | MyTraceLevel.Error | MyTraceLevel.Warning | MyTraceLevel.Information;
                    this._packerTraceListener.PropertyChanged += ( sender,  args) => {
                        this.RaisePropertyChanged(nameof(this.DmodPackerProgressLog));
                        this.DmodPackerProgressLogCaretIndex = int.MaxValue;
                    };
                    this.RaisePropertyChanged(nameof(this.DmodPackerProgressLog));

                    // create installer logic
                    this._packerLogic = new DmodPacker();
                    this._packerLogic.CustomTrace.Listeners.Add(this._packerTraceListener);
                    this._packerLogic.ProgressReport += this.PackerOnProgressReport;
                    this._packerLogic.ActivityStarted += this.PackerOnActivityStarted;
                    this._packerLogic.ActivityEnded += this.PackerOnActivityEnded;
                    this._packerLogic.Initialize(dirInfo);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }

        private Task StartPacking() {
            return Task.Run(() => {
                try
                {
                    if (this._packerLogic == null) return;
                    if (string.IsNullOrWhiteSpace(this.FinalDmodDestination)) return;
                    
                    FileInfo fileInfo = new FileInfo(this.FinalDmodDestination);
                    if (fileInfo.Exists) return;
                    
                    this._packerLogic.PackDmod(fileInfo);
                    
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });

        }

        private void ResetPackerState() {
            if (this._packerLogic != null)
            {
                this._packerLogic.CustomTrace.Flush();
                this._packerLogic.CustomTrace.Close();
                this._packerLogic.ProgressReport -= this.PackerOnProgressReport;
                this._packerLogic = null;
            }

            this._packerTraceListener?.Close();
            this._packerTraceListener = null;
            
            this.RaisePropertyChanged(nameof(this.DmodPackerProgressLog));
            this.RaisePropertyChanged(nameof(this.DmodPackerProgressLogCaretIndex));
            
            this.PackerPhase = DmodPackerPhase.Inactive;
            
            this.FinalDmodSourceDirectory = "";
            this.FinalDmodDestination = "";

            this.DmodPackerTitle = Localizer.Instance["DmodPacker/ViewModel/Title"];
            this.DmodPackerPhaseProgressPercent = 0.0;
            this.DmodPackingInProgress = false;
        }

        private void FirePackerDone()
        {
            try
            {
                this.PackerDone?.Invoke(this, this._packerDoneEventArgs);
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }
        
        private void PackerOnActivityEnded(object? sender, EventArgs e)
        {
            this.DmodPackingInProgress = false;
        }
        private void PackerOnActivityStarted(object? sender, EventArgs e)
        {
            this.DmodPackingInProgress = true;
        }

        private void PackerOnProgressReport(object? sender, DmodPackerProgressEventArgs args) {
            try
            {
                if (sender is not DmodPacker packer) return;
                
                // set current install phase
                this.PackerPhase = args.Phase;
                
                string title = Localizer.Instance["DmodPacker/ViewModel/Title"];
                if (string.IsNullOrWhiteSpace(packer.SourceDirectory?.Name) == false)
                {
                    title += " - " + packer.SourceDirectory.Name;
                }
                this.DmodPackerTitle = title;
                
                this.DmodPackerPhaseProgressPercent = args.ProgressPercent;
                
                // just in case this wasn't cleared?...
                if (args.Phase == DmodPackerPhase.Finished)
                {
                    this.DmodPackingInProgress = false;
                }

                switch (args.Phase)
                {
                    case DmodPackerPhase.AwaitingUserInput:
                    {
                        // just finished Initializing...
                        // TODO... populate directory trees...
                        break;
                    }
                    case DmodPackerPhase.Finished:
                    {
                        // just finished overall...
                        this._packerDoneEventArgs = new DmodPackerDoneEventArgs(args.Result, packer.DestinationFile, packer.SourceDirectory);
                        if (args.Result == DinkInstallerResult.Cancelled) this.ShowInstallerCancelledMessageBox();
                        if (args.Result == DinkInstallerResult.Error) this.ShowInstallerErrorMessageBox(packer.PackException);
                        break;
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }

        // ------------------------------------------------------------------------------------------
        //      Message box stuff
        //

        private void ShowInstallerCancelledMessageBox() {
            Dispatcher.UIThread.InvokeAsync(async () => {
                try {
                    string title = Localizer.Instance["DmodPacker/ViewModel/MessageBox_Cancel_Title"];
                    string body = Localizer.Instance["DmodPacker/ViewModel/MessageBox_Cancel_Body"];
                    await DinkyAlert.ShowDialog(title, body, AlertResults.Ok, AlertType.Info);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }

        private void ShowInstallerErrorMessageBox(Exception? exception) {
            Dispatcher.UIThread.InvokeAsync(async () => {
                try {
                    string title = Localizer.Instance["DmodPacker/ViewModel/MessageBox_Error_Title"];
                    string body = Localizer.Instance["DmodPacker/ViewModel/MessageBox_Error_Body"] + Environment.NewLine + MyTrace.GetExceptionMessages(exception);
                    await DinkyAlert.ShowDialog(title, body, AlertResults.Ok, AlertType.Error);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }
    }
}

