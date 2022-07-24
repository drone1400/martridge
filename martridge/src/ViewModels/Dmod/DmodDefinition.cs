using Avalonia.Media.Imaging;
using GetText;
using Martridge.Models.Dmod;
using Martridge.Models.Localization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Martridge.ViewModels.Dmod {

    public class DmodDefinition {
        public DmodFileDefinition Files { get; private set; }

        public List<DmodLocalizationDefinition> Localizations { get; private set; }
        public string? Description { get; private set; }
        public string? Name { get; private set; }
        public string? Path { get; private set; }
        public Bitmap? Thumbnail { get; private set; }

        public DmodDefinition(DmodFileDefinition fileDef) {
            this.Files = fileDef;

            this.Description = this.Files.GetDescription();
            this.Name = this.Files.GetName();
            this.Path = this.Files.DmodRoot?.FullName;
            this.Thumbnail = this.Files.GetThumbnail();

            this.InitializeLocalizations();
        }

        private void InitializeLocalizations() {
            this.Localizations = new List<DmodLocalizationDefinition>();

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
            return $"{this.Name} [{this.Files.DmodRoot?.FullName}]";
        }

        public void CmdOpenLocation(object? parameter) {
            ProcessStartInfo pinfo = new ProcessStartInfo(this.Path) {
                UseShellExecute = true,
                Verb = "open",
            };
            Process.Start(pinfo);
        }

        public bool CanCmdOpenLocation(object? parameter) {
            if (Directory.Exists(this.Path)) return true;
            return false;
        }

    }
}
