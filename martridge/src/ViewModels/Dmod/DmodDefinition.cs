using Avalonia.Media.Imaging;
using GetText;
using Martridge.Models.Dmod;
using Martridge.Models.Localization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ReactiveUI;

namespace Martridge.ViewModels.Dmod {

    public class DmodDefinition : ReactiveObject{
        public DmodFileDefinition Files { get; }

        public List<DmodLocalizationDefinition> Localizations { get; } = new List<DmodLocalizationDefinition>();
        public string? Description { get; }
        public string? Name { get; }
        public string? DmodParentDirectory { get; }
        public string? DmodDirectory { get; }
        public Bitmap? Thumbnail {
            get => this._thumbnail; 
            private set => this.RaiseAndSetIfChanged(ref this._thumbnail, value);
        }
        private Bitmap? _thumbnail = null;

        public DmodDefinition(DmodFileDefinition fileDef) {
            this.Files = fileDef;

            this.Description = this.Files.GetDescription();
            this.Name = this.Files.GetName();
            this.DmodDirectory = this.Files.DmodRoot.FullName;
            if (this.Files.DmodRoot.Parent != null) {
                this.DmodParentDirectory = this.Files.DmodRoot.Parent.FullName;
            } else {
                string dirPath = this.Files.DmodRoot.FullName;
                string dirName = this.Files.DmodRoot.Name;
                this.DmodParentDirectory = dirPath.Substring(0, dirPath.Length - dirName.Length);
            }

            this.InitializeLocalizations();
        }

        private void InitializeLocalizations() {
            string defaultLoc = Localizer.Instance[@"Generic/DoNotChangeDmodLocalization"];
            
            this.Localizations.Add(new DmodLocalizationDefinition() {
                Header = defaultLoc,
                CultureInfo = null,
            });

            foreach (FileInfo fmo in this.Files.LocalizationFiles) {
                using (FileStream fs = new FileStream(fmo.FullName, FileMode.Open, FileAccess.Read)) {
                    Catalog cat = new Catalog(fs);
                    this.Localizations.Add(new DmodLocalizationDefinition() {
                        Header = cat.CultureInfo.NativeName,
                        CultureInfo = cat.CultureInfo,
                    });
                }
            }
        }

        public void LoadThumbnail() {
            this.Thumbnail?.Dispose();
            this.Thumbnail = this.Files.GetThumbnail();
        }

        public void UnloadThumbnail() {
            this.Thumbnail?.Dispose();
            this.Thumbnail = null;
        }

        public override string ToString() {
            return $"{this.Name} [{this.Files.DmodRoot.FullName}]";
        }
    }
}
