using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Ignore;
using Martridge.Models.Localization;
using Martridge.Trace;
using SharpCompress.Common;
using SharpCompress.Compressors.PBZip2;
using SharpCompress.Writers.Tar;

namespace Martridge.Models.DmodPacker {
    public class DmodPacker {
        public event EventHandler<DmodPackerProgressEventArgs>? ProgressReport;
        public event EventHandler? ActivityStarted; 
        public event EventHandler? ActivityEnded;

        public DmodPackerNode? RootNode => this._rootNode;
        public DirectoryInfo? SourceDirectory => this._sourceDirectory;
        public FileInfo? DestinationFile => this._destinationFile;
        public DmodPackerPhase PackPhase => this._packPhase;
        public DinkInstallerResult PackResult => this._packResult;
        public Exception? PackException => this._packException;
        public MyTrace CustomTrace => this._customTrace;

        
        private DmodPackerNode? _rootNode = null;
        private DirectoryInfo? _sourceDirectory = null;
        private FileInfo? _destinationFile = null;
        private DmodPackerPhase _packPhase = DmodPackerPhase.Inactive;
        private DinkInstallerResult _packResult = DinkInstallerResult.Error;
        private Exception? _packException = null;
        private MyTrace _customTrace;
        private FileInfo? _dmodIgnoreFile = null;
        
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly object _syncRoot = new object();
        private readonly DinkTempFileHelper _temp = new DinkTempFileHelper();

        private List<DmodIgnoreRuleMetadata> _ignoreRules = new List<DmodIgnoreRuleMetadata>();
        
        
        private void ReportProgressPrimary() {
            try
            {
                // NOTE: the primary progress is the phase itself
                // there are a total of 5 phases to go through...
                this.ProgressReport?.Invoke(this, new DmodPackerProgressEventArgs(this._packPhase,  this._packResult, (int)this._packPhase / 5.0));
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }
        private void ReportActivityStart() {
            try
            {
                this.ActivityStarted?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }
        
        private void ReportActivityEnd() {
            try
            {
                this.ActivityEnded?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }

        private void LogMessage(string line1)
        {
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, line1);
        }
        private void LogMessage(string line1, string line2)
        {
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, [line1, line2]);
        }
        private void LogMessage(string line1, string line2, string line3)
        {
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, [line1, line2, line3]);
        }

        public DmodPacker()
        {
            this._customTrace = new MyTrace(this.GetType().ToString());
            this._temp.SetLogCallback(this.LogMessage);
        }
        
        public void Cancel()
        {
            this._cancellationTokenSource.Cancel();

            lock (this._syncRoot)
            {
                if (this._packPhase != DmodPackerPhase.Inactive &&
                    this._packPhase != DmodPackerPhase.AwaitingUserInput) return;
                
                // jump to cleanup state to prevent starting initialization or installation...
                this._packPhase = DmodPackerPhase.Cleanup;
            }
            
            this.LogMessage(Localizer.Instance["DmodPacker/Log/CancelledByUser"]);
                
            this.CleanUp();
                
            this.CustomTrace.Flush();
            this.CustomTrace.Close();
                
            lock (this._syncRoot)
            {
                this._packPhase = DmodPackerPhase.Finished;
                this._packResult = DinkInstallerResult.Cancelled;
            }
            this.ReportProgressPrimary();
        }

        public void Initialize(DirectoryInfo sourceDirectory)
        {
            lock (this._syncRoot)
            {
                if (this._packPhase != DmodPackerPhase.Inactive && this._packPhase != DmodPackerPhase.AwaitingUserInput || 
                    this._sourceDirectory != null)
                    return;

                this._packPhase = DmodPackerPhase.Initializing;
                this._sourceDirectory = sourceDirectory;
                this._rootNode = null;
            }

            try
            {
                this.ReportProgressPrimary();
                
                this._sourceDirectory.Refresh();
                if (this._sourceDirectory.Exists == false)
                {
                    this.LogMessage(Localizer.Instance["DmodPacker/Log/DmodDirectoryNotFound"], $"    \"{this._sourceDirectory.FullName}\"");
                    throw new DinkInstallerFileSystemException(this._sourceDirectory.FullName);
                }
                
                this.LogMessage(Localizer.Instance["DmodPacker/Log/Initializing/StartInitializing"], $"    \"{this._sourceDirectory.FullName}\"");
                
                this.ReportActivityStart();
                this.InitializeDmodIgnore();
                bool result = this.ScanDmodDirectory();
                this.ReportActivityEnd();
                
                if (result) {
                    this.LogMessage(Localizer.Instance["DmodPacker/Log/Initializing/Success"]);

                    lock (this._syncRoot)
                    {
                        this._packPhase = DmodPackerPhase.AwaitingUserInput;
                    }
                    this.ReportProgressPrimary();
                }
                else
                {
                    this.LogMessage(Localizer.Instance["DmodPacker/Log/Initializing/Failed"]);
                    throw new Exception();
                }
            } catch (DinkInstallerCancelledByUserException)
            {
                this.LogMessage(Localizer.Instance["DmodPacker/Log/CancelledByUser"]);
                
                this.CleanUp();
                
                this.CustomTrace.Flush();
                this.CustomTrace.Close();
                
                lock (this._syncRoot)
                {
                    this._packPhase = DmodPackerPhase.Finished;
                    this._packResult = DinkInstallerResult.Cancelled;
                }
                this.ReportProgressPrimary();
            }  catch (Exception ex)
            {
                this._packException = ex;
                this.CustomTrace.WriteException(MyTraceCategory.DinkInstaller, this._packException);
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, this._packException);
                
                this.CleanUp();
                
                this.CustomTrace.Flush();
                this.CustomTrace.Close();
                
                lock (this._syncRoot)
                {
                    this._packPhase = DmodPackerPhase.Finished;
                    this._packResult = DinkInstallerResult.Error;
                }
                this.ReportProgressPrimary();
            }
        }


        private void GenerateDefaultIgnore() {
            this._ignoreRules = new List<DmodIgnoreRuleMetadata>();
            try {
                
                for (int lineIndex = 0; lineIndex < DEFAULT_DMOD_IGNORE.Length; lineIndex++) {
                    string line = DEFAULT_DMOD_IGNORE[lineIndex];
                    
                    // ignore empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    // ignore comments
                    if (line.StartsWith("#"))
                        continue;
                    
                    this._ignoreRules.Add(new DmodIgnoreRuleMetadata() {
                        IgnoreRule = new IgnoreRule(line),
                        RuleDefinition = line,
                        RuleIndex = this._ignoreRules.Count,
                        RuleLineIndex = lineIndex-1,
                    });
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }
        
        private static string[] DEFAULT_DMOD_IGNORE = new string[] {
            "# DinkHD debug log",
            "debug.txt",
            "",
            "# WDED metadata and files",
            "sprite_report.txt",
            ".wded/**",
            ".wded_backup/**",
            ".martridge/**",
            "",
            "# the dmod ignore file",
            ".dmodignore",
            "",
            "# GIT files",
            ".git/**",
            ".gitignore",
        };
        
        private void UpdateIsIgnored(DmodPackerNode node)
        {
            foreach (DmodIgnoreRuleMetadata metadata in this._ignoreRules) {
                if (metadata.IgnoreRule.IsMatch(node.RelativePathLower)) {
                    if (metadata.IgnoreRule.Negate) {
                        node.NegateIgnore(metadata.RuleIndex);
                    }
                    else {
                        node.Ignore(metadata.RuleIndex);
                    }
                }
            }
        }

        private void InitializeDmodIgnore() {
            if (this._sourceDirectory == null) {
                return;
            }

            string path = Path.Combine(this._sourceDirectory.FullName, ".dmodignore");
            this._dmodIgnoreFile = new FileInfo(path);
            this._ignoreRules = new List<DmodIgnoreRuleMetadata>();

            if (this._dmodIgnoreFile.Exists == false) {
                this.GenerateDefaultIgnore();
                return;
            }
            
            try {
                using FileStream fs = new FileStream(path, FileMode.Open);
                using StreamReader sr = new StreamReader(fs);

                string? line = null;
                int lineIndex = 0;
                while ((line = sr.ReadLine()) != null) {
                    lineIndex++;
                    
                    // ignore empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    // ignore comments
                    if (line.StartsWith("#"))
                        continue;
                    
                    this._ignoreRules.Add(new DmodIgnoreRuleMetadata() {
                        IgnoreRule = new IgnoreRule(line),
                        RuleDefinition = line,
                        RuleIndex = this._ignoreRules.Count,
                        RuleLineIndex = lineIndex-1,
                    });
                }
            } catch (Exception) {
                this.GenerateDefaultIgnore();
            }
        }

        private bool ScanDmodDirectory()
        {
            this._rootNode = null;
            
            if (this._sourceDirectory == null) 
                return false;
            
            this._rootNode = new DmodPackerNode(this._sourceDirectory, this._sourceDirectory);
            
            Queue<DmodPackerNode> queue = new Queue<DmodPackerNode>();

            queue.Enqueue(this._rootNode);

            int fileCount = 0;

            while (queue.Count > 0)
            {
                DmodPackerNode dirNode = queue.Dequeue();
                
                if (dirNode.NodeType != DmodNodeType.Directory)
                    continue;
                
                // TODO.. maybe index dirFF contents here too?...

                DirectoryInfo dirInfo = new DirectoryInfo(dirNode.FullPath);

                FileInfo[] files = dirInfo.GetFiles();

                foreach (FileInfo file in files)
                {
                    // Ignore Symbolic Links...
                    if (file.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        this.LogMessage(Localizer.Instance["DmodPacker/Log/Initializing/IgnoringSymbolicLink"], file.FullName);
                        continue;
                    }

                    // add to the node's children
                    DmodPackerNode newNode = new DmodPackerNode(file, this._sourceDirectory);
                    this.UpdateIsIgnored(newNode);
                    dirNode.AddChildNode(newNode);

                    fileCount++;
                }
                
                DirectoryInfo[] dirs = dirInfo.GetDirectories();

                foreach (DirectoryInfo dir in dirs)
                {
                    // Ignore Symbolic Links...
                    if (dir.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        this.LogMessage(Localizer.Instance["DmodPacker/Log/Initializing/IgnoringSymbolicLink"], dir.FullName);
                        continue;
                    }

                    // add to the node's children
                    DmodPackerNode newNode = new DmodPackerNode(dir, this._sourceDirectory);
                    this.UpdateIsIgnored(newNode);
                    dirNode.AddChildNode(newNode);
                    // also queue up the directory to process
                    queue.Enqueue(newNode);
                }
            }
            
            this.LogMessage(String.Format(Localizer.Instance["DmodPacker/Log/Initializing/FileCount"], fileCount));

            return true;
        }


        public void PackDmod(FileInfo destinationFile) {
            lock (this._syncRoot)
            {
                if (this._packPhase != DmodPackerPhase.AwaitingUserInput) return;

                this._packPhase = DmodPackerPhase.Packing;
                this._destinationFile = destinationFile;
            }

            bool cancelled = false;
            
            
             try {
                this.ReportProgressPrimary();
                
                if (this._cancellationTokenSource.IsCancellationRequested) {
                    throw new DinkInstallerCancelledByUserException();
                }
                
                // sanity checks for destination
                if (this._destinationFile.Exists) {
                    throw new DinkInstallerFileSystemException(Localizer.Instance["DmodPacker/Log/Packing/DestinationAlreadyExists"] + $" \"{this._destinationFile.FullName}\"");
                }
                if (this._destinationFile.Directory == null) {
                    throw new DinkInstallerFileSystemException(Localizer.Instance["DmodPacker/Log/Packing/DestinationErrorIsRoot"] + $" \"{this._destinationFile.FullName}\"");
                }
                if (Path.IsPathRooted(this._destinationFile.FullName) == false) {
                    throw new DinkInstallerFileSystemException(Localizer.Instance["DmodPacker/Log/Packing/DestinationErrorIsNotRooted"] + $" \"{this._destinationFile.FullName}\"");
                }
                
                // extracting DMOD
                this.ReportActivityStart();
                this.PackDmodToFile();
                this.ReportActivityEnd();
            } catch (DinkInstallerCancelledByUserException) {
                cancelled = true;
                this.LogMessage(Localizer.Instance["DmodPacker/Log/CancelledByUser"]);
            } catch (Exception ex) {
                this._packException = ex;
                this.CustomTrace.WriteException(MyTraceCategory.DinkInstaller, this._packException);
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, this._packException);
            } finally {
                this.ReportActivityEnd();
                
                this.CleanUp();

                this.CustomTrace.Flush();
                this.CustomTrace.Close();
                
                lock (this._syncRoot)
                {
                    this._packPhase = DmodPackerPhase.Finished;
                    if (cancelled == false && this._packException == null) this._packResult = DinkInstallerResult.Success;
                    else if (cancelled) this._packResult = DinkInstallerResult.Cancelled;
                    else this._packResult = DinkInstallerResult.Error;
                }
                this.ReportProgressPrimary();
            }
        }

        private void CleanUp() {
            lock (this._syncRoot)
            {
                this._packPhase = DmodPackerPhase.Cleanup;
            }
            this.ReportProgressPrimary();
            
            this.ReportActivityStart();
            this.LogMessage(Localizer.Instance["DmodPacker/Log/CleaningUpStart"]);
            this._temp.Dispose();
            this.ReportActivityEnd();
            this.LogMessage(Localizer.Instance["DmodPacker/Log/CleaningUpEnd"]);
        }
        

        private void PackDmodToFile()
        {
            if (this._destinationFile == null) return;
            if (this._rootNode == null) return;
            
            if (this._cancellationTokenSource.IsCancellationRequested) {
                throw new DinkInstallerCancelledByUserException();
            }
            
            this.LogMessage(Localizer.Instance["DmodPacker/Log/Packing/Start"], $"    \"{this._rootNode.FullPath}\"", $"    \"{this._destinationFile.FullName}\"");

            DirectoryInfo? tempDir = this._temp.TryCreateTempDirectory();
            if (tempDir == null) return;
            
            this.LogMessage(Localizer.Instance["DmodPacker/Log/Packing/CreatedTempDirectory"],
                $"    \"{tempDir.FullName}\"");
            
            
            string tempTarFile = Path.Combine(tempDir.FullName, this._destinationFile.Name);
            FileInfo tempTarFileInfo = new FileInfo(tempTarFile);
            this._temp.RegisterTempFile(tempTarFileInfo);
            
            this.LogMessage(Localizer.Instance["DmodPacker/Log/Packing/CreatingTempTarFile"],
                $"    \"{tempTarFileInfo.FullName}\"");

            Queue<DmodPackerNode> queue = new Queue<DmodPackerNode>();
            queue.Enqueue(this._rootNode);

            string rootName = this._rootNode.Name;
            string rootPath = this._rootNode.FullPath;

            int fileCount = 0;
            
            using (FileStream fs = new FileStream(tempTarFile, FileMode.Create, FileAccess.Write))
            using (TarWriter writer = new TarWriter(fs, new TarWriterOptions(CompressionType.None, true, TarHeaderWriteFormat.Ustar))) {
                while (queue.Count > 0)
                {
                    if (this._cancellationTokenSource.IsCancellationRequested) {
                        throw new DinkInstallerCancelledByUserException();
                    }
                    
                    DmodPackerNode node = queue.Dequeue();

                    foreach (var kvp in node.Children)
                    {
                        if (kvp.Value.IsIgnored)
                            continue;

                        if (kvp.Value.NodeType == DmodNodeType.Directory) {
                            queue.Enqueue(kvp.Value);
                            continue;
                        }
                        
                        string relativeFileName = Path.Combine(rootName,
                            Path.GetRelativePath(rootPath, kvp.Value.FullPath));
                        
                        using (FileStream fileStream = new FileStream(kvp.Value.FullPath, FileMode.Open, FileAccess.Read)) {
                            writer.Write(relativeFileName, fileStream, kvp.Value.LastModified);
                        }

                        fileCount++;
                    }
                    
                    this.LogMessage(String.Format(Localizer.Instance["DmodPacker/Log/Packing/PackingDmodProgress"], fileCount));
                }
            }
            
            this.LogMessage(String.Format(Localizer.Instance["DmodPacker/Log/Packing//CreatingTarFileDone"], fileCount));
            
            this.LogMessage( Localizer.Instance["DmodPacker/Log/Packing//CompressingDmodFile"]);
            
            int threads = Environment.ProcessorCount;
            using (FileStream fsin = new FileStream(tempTarFile, FileMode.Open, FileAccess.Read))
            using (FileStream fsout = new FileStream(this._destinationFile.FullName, FileMode.Create, FileAccess.Write)) 
            using (BZip2ParallelOutputStream bzip2 = new BZip2ParallelOutputStream(fsout,threads, true, 9)) {
                fsin.CopyTo(bzip2);
                bzip2.Close();
            }
            
            this.LogMessage( Localizer.Instance["DmodPacker/Log/Packing//CompressingDmodFileDone"]);
        }
    }
}
