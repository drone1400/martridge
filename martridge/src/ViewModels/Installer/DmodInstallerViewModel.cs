﻿using Avalonia.Controls;
using Avalonia.Metadata;
using Martridge.Models;
using Martridge.Models.Configuration;
using Martridge.Models.Installer;
using Martridge.Models.Localization;
using Martridge.Trace;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Martridge.ViewModels.Installer {
    public class DmodInstallerViewModel : InstallerViewModelBase {

        // ------------------------------------------------------------------------------------------
        //      Source and Destination Selection 
        //

        public string SelectedDmodPacakge {
            get => this._selectedDmodPacakge;
            set => this.RaiseAndSetIfChanged(ref this._selectedDmodPacakge, value);
        }
        private string _selectedDmodPacakge = "";

        public ObservableCollection<DirectoryInfo> InstallableDestinations { get; } = new ObservableCollection<DirectoryInfo>();

        public DirectoryInfo? SelectedInstallableDestination {
            get => this._selectedInstallableDestination;
            set => this.RaiseAndSetIfChanged(ref this._selectedInstallableDestination, value);
        }
        private DirectoryInfo? _selectedInstallableDestination = null;


        // ------------------------------------------------------------------------------------------
        //      Installer logic 
        //
        
        public event EventHandler<DmodInstallerDoneEventArgs>? InstallerDone;
        private DmodInstallerDoneEventArgs _lastInstallerDoneEventArgs = new DmodInstallerDoneEventArgs(DinkInstallerResult.Cancelled);
        
        private ConfigGeneral? _configGeneral = null;
        private DmodInstaller? _installerLogic = null;


        // ------------------------------------------------------------------------------------------
        //      Constructor
        //
        
        public DmodInstallerViewModel() {

            this.PropertyChanged += this.DinkInstallerViewModel_PropertyChanged;
            this.SelectedInstallableDestination = null;
        }

        private void DinkInstallerViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.SelectedInstallableDestination)) {
                
            }
        }

        public void InitializeConfiguration(ConfigGeneral cfg) {
            if (this._configGeneral != null) {
                this._configGeneral.Updated -= this.GeneralUpdated;
            }

            this._configGeneral = cfg;
            if (this._configGeneral != null) {
                this._configGeneral.Updated += this.GeneralUpdated;
                this.InitializeDmodLocations();
            }
        }

        private void GeneralUpdated(object? sender, EventArgs e) {
            this.InitializeDmodLocations();
        }

        private void InitializeDmodLocations() {
            if (this._configGeneral == null) return;
            this.InstallableDestinations.Clear();
            List<DirectoryInfo> dmodPlaces = this._configGeneral.GetRealDmodDirectories();

            foreach (DirectoryInfo dirInfo in dmodPlaces) {
                this.InstallableDestinations.Add(dirInfo);
            }
            
            this.SelectedInstallableDestination = dmodPlaces.First();
        }

        // ------------------------------------------------------------------------------------------
        //      Commands
        //

        #region Commands

        public override void CmdExit(object? parameter = null) {
            if (this.IsInstallerStarted == false) {
                this._lastInstallerDoneEventArgs = new DmodInstallerDoneEventArgs(DinkInstallerResult.Cancelled);
                this.ResetInstallerStateAndFireDoneEvent();
            }
            if (this.IsInstallerFinished) {
                this.ResetInstallerStateAndFireDoneEvent();
            }
        }
        
        [DependsOn(nameof(IsInstallerStarted))]
        [DependsOn(nameof(IsInstallerCancelled))]
        [DependsOn(nameof(IsInstallerFinished))]
        public bool CanCmdExit(object? parameter = null) {
            if (this.IsInstallerCancelled) return false;
            if (this.IsInstallerStarted == false) return true;
            if (this.IsInstallerFinished) return true;
            return false;
        }

        public override void CmdCancel(object? parameter = null) {
            if (this.CanCmdCancel() == false) return;
            
            this.IsInstallerCancelled = true;
            // null check done in CanCmdCancel
            this._installerLogic!.CancelTokenSource.Cancel();
        }
        
        [DependsOn(nameof(IsInstallerStarted))]
        [DependsOn(nameof(IsInstallerCancelled))]
        [DependsOn(nameof(IsInstallerFinished))]
        public bool CanCmdCancel(object? parameter = null) {
            if (this._installerLogic == null) return false;
            // can't cancel if the installer is not started
            if (this.IsInstallerStarted == false) return false;
            // can't cancel if already done
            if (this.IsInstallerFinished) return false;
            // can only cancel once
            return !this.IsInstallerCancelled;
        }
        
        public override void CmdStartInstall(object? parameter = null) {
            if (this.CanCmdStartInstall() == false) return;

            this.StartInstallation();
        }
        
        [DependsOn(nameof(IsInstallerStarted))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(SelectedInstallableDestination))]
        [DependsOn(nameof(SelectedDmodPacakge))]
        public bool CanCmdStartInstall(object? parameter = null) {
            if (this.IsInstallerStarted) return false;
            if (this.ParentWindow == null) return false;
            if (this.SelectedInstallableDestination == null) return false;
            if (string.IsNullOrWhiteSpace(this.SelectedDmodPacakge)) return false;
            if (File.Exists(this.SelectedDmodPacakge) == false) return false;
            if (this._installerLogic != null) return false;
            
            return true;
        }

        public void CmdBrowseDmod(object? parameter = null) {
            this.BrowseDmod_Internal();
        }

        [DependsOn(nameof(IsFileBrowserActive))]
        [DependsOn(nameof(ParentWindow))]
        public bool CanCmdBrowseDmod(object? parameter = null) {
            return !this.IsFileBrowserActive && this.ParentWindow != null;
        }

        private async void BrowseDmod_Internal() {
            if (this.ParentWindow == null || this.IsFileBrowserActive)  return;

            try {
                this.IsFileBrowserActive = true;

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Directory = LocationHelper.AppBaseDirectory;
                ofd.AllowMultiple = false;
                ofd.Filters = new List<FileDialogFilter>() {
                    new FileDialogFilter() {
                        Name = Localizer.Instance[@"Generic/FileTypeDmod"],
                        Extensions = new List<string>() {
                            "dmod",
                        },
                    },
                };

                string[]? result = await ofd.ShowAsync(this.ParentWindow);
                if (result != null) {
                    this.SelectedDmodPacakge = result[0];
                }

            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            } finally {
                this.IsFileBrowserActive = false;
            }
        }
        
        #endregion
        
        
        // ------------------------------------------------------------------------------------------
        //      Installer logic
        //

        private void StartInstallation() {
            if (this.CanCmdStartInstall() == false) return;

            try {
                this.IsInstallerStarted = true;
                
                DirectoryInfo destination = this.SelectedInstallableDestination;
                FileInfo source = new FileInfo(this.SelectedDmodPacakge);

                // create installer trace listener
                this.InstallerTraceListener = new MyTraceListenerGui("Installer Trace Listener");
                this.InstallerTraceListener.ShowLevels = false;
                this.InstallerTraceListener.Levels = MyTraceLevel.Critical | MyTraceLevel.Error | MyTraceLevel.Warning | MyTraceLevel.Information;
                this.InstallerTraceListener.PropertyChanged += ( sender,  args) => {
                    this.RaisePropertyChanged(nameof(this.InstallerProgressLog));
                    this.InstallerProgressLogCaretIndex = int.MaxValue;
                };

                // create installer logic
                this._installerLogic = new DmodInstaller();
                this._installerLogic.CustomTrace.Listeners.Add(this.InstallerTraceListener);
                this._installerLogic.ProgressReport += this.InstallerOnProgressReport;
                this._installerLogic.InstallerDone += this.InstallerOnDone;

                // gui updates
                this.InstallerProgressTitle = source.Name;

                // start installation
                this._installerLogic.StartInstallingDmod(source, destination);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
            
        }

        private void ResetInstallerStateAndFireDoneEvent() {
            this._installerLogic?.CustomTrace.Flush();
            this._installerLogic?.CustomTrace.Close();
            this._installerLogic = null;

            this.InstallerTraceListener?.Close();
            this.InstallerTraceListener = null;
            
            this.IsInstallerStarted = false;
            this.IsInstallerFinished = false;
            this.IsInstallerCancelled = false;

            this.InstallerDone?.Invoke(this, this._lastInstallerDoneEventArgs);
        }

        private void InstallerOnDone(object? sender, DmodInstallerDoneEventArgs args) {
            this.IsInstallerFinished = true;
            this._lastInstallerDoneEventArgs = args;
            if (args.Result == DinkInstallerResult.Cancelled) this.ShowInstallerCancelledMessageBox();
            if (args.Result == DinkInstallerResult.Error) this.ShowInstallerErrorMessageBox(args.Exception);
        }

        private void InstallerOnProgressReport(object? sender, InstallerProgressEventArgs args) {
            try {
                if (args.ProgressLevel == InstallerReportLevel.Primary) {
                    this.InstallerProgressLevel0MainTitle = args.HeadingMain;
                    this.InstallerProgressLevel0SubTitle = args.HeadingSecondary;
                    this.InstallerProgressLevel0Progress = args.ProgressPercent;
                }
                if (args.ProgressLevel == InstallerReportLevel.Secondary) {
                    if (Math.Abs(args.ProgressPercent - 1.0) < 0.00001) {
                        this.InstallerProgressLevel1IsVisibile = false;
                    } else {
                        this.InstallerProgressLevel1IsVisibile = true;
                    }

                    this.InstallerProgressLevel1Indeterminate = false;
                    this.InstallerProgressLevel1MainTitle = args.HeadingMain;
                    this.InstallerProgressLevel1SubTitle = args.HeadingSecondary;
                    this.InstallerProgressLevel1Progress = args.ProgressPercent;
                }
                if (args.ProgressLevel == InstallerReportLevel.Indeterminate) {
                    if (Math.Abs(args.ProgressPercent - 1.0) < 0.00001) {
                        this.InstallerProgressLevel1IsVisibile = false;
                    } else {
                        this.InstallerProgressLevel1IsVisibile = true;
                    }
                    this.InstallerProgressLevel1Indeterminate = true;
                    this.InstallerProgressLevel1MainTitle = args.HeadingMain;
                    this.InstallerProgressLevel1SubTitle = args.HeadingSecondary;
                    this.InstallerProgressLevel1Progress = args.ProgressPercent;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }

        
    }
}

