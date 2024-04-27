using Avalonia.Media.Imaging;
using GetText;
using Martridge.Models.Dmod;
using Martridge.Models.Localization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Martridge.ViewModels.Dmod {

    public class DmodDefinition {
        public DmodFileDefinition Files { get; private set; }

        public List<DmodLocalizationDefinition> Localizations { get; private set; } = new List<DmodLocalizationDefinition>();
        public string? Description { get; private set; }
        public string? Name { get; private set; }
        public string? DmodParentDirectory { get; private set; }
        public string? DmodDirectory { get; private set; }
        public Bitmap? Thumbnail { get; private set; }

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
            this.Thumbnail = this.Files.GetThumbnail();

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

        public override string ToString() {
            return $"{this.Name} [{this.Files.DmodRoot.FullName}]";
        }
    }
}
