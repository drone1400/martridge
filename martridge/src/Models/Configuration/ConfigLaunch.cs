using Martridge.Models.Configuration.Save;
using System;
using System.Collections.Generic;

namespace Martridge.Models.Configuration {
    public class ConfigLaunch : IConfigGeneric{
        public event EventHandler<ConfigUpdateEventArgs>? Updated;

        /// <summary>
        /// Additional custom user arguments
        /// </summary>
        public string CustomUserArguments => this._customUserArguments;
        private string _customUserArguments = string.Empty;

        /// <summary>
        /// Use quotation marks in the path (works only for DinkHD after 1.97 and FreeDink versions)
        /// </summary>
        public bool UsePathQuotationMarks => this._usePathQuotationMarks;
        private bool _usePathQuotationMarks = false;

        /// <summary>
        /// Make the DMOD path relative to the chosen Dink Launcher
        /// </summary>
        public bool UsePathRelativeToGame => this._usePathRelativeToGame;
        private bool _usePathRelativeToGame = false;

        /// <summary>
        /// Launches the game in true color mode
        /// </summary>
        public bool TrueColor => this._trueColor;
        private bool _trueColor = false;
        
        /// <summary>
        /// Launches the game in windowed mode
        /// </summary>
        public bool Windowed => this._windowed;
        private bool _windowed = false;
        
        /// <summary>
        /// Launches the game with sound
        /// </summary>
        public bool Sound => this._sound;
        private bool _sound = false;
        
        /// <summary>
        /// Launches the game with (questionable?) joystick support
        /// </summary>
        public bool Joystick => this._joystick;
        private bool _joystick = false;
        
        /// <summary>
        /// Launches the game in DEBUG mode
        /// </summary>
        public bool Debug => this._debug;
        private bool _debug = false;
        
        /// <summary>
        /// Launches the game in V1.07 compatibility mode
        /// </summary>
        public bool V107Mode => this._v107Mode;
        private bool _v107Mode = false;
        
        /// <summary>
        /// Skip update check and stuff in DinkHD
        /// </summary>
        public bool Skip => this._skip;
        private bool _skip = false;
        
        /// <summary>
        /// If true, passes the --refdir argument when launching or editing DMODs
        /// </summary>
        public bool UseRefDir => this._useRefDir; 
        private bool _useRefDir;

        /// <summary>
        /// The RefDir path to use...
        /// </summary>
        public string RefDirPath => this._refDirPath;
        private string _refDirPath = string.Empty;

        
        /// <summary>
        /// If true, Martridge will exit after successfully launching the game
        /// </summary>
        public bool QuitMartridgeOnGameLaunch => this._quitMartridgeOnGameLaunch;
        private bool _quitMartridgeOnGameLaunch = false;
        
        /// <summary>
        /// If true, Martridge will exit after successfully launching the editor
        /// </summary>
        public bool QuitMartridgeOnEditorLaunch => this._quitMartridgeOnEditorLaunch;
        private bool _quitMartridgeOnEditorLaunch = false;

        
        public void UpdateProperties(Dictionary<string, object?> newValues) {
            List<string> updatedProperties = new List<string>();

            void TryUpdateGeneric<T>(KeyValuePair<string, object?> kvp, ref T myValue) {
                if (kvp.Value is T value && myValue!.Equals(value) == false) {
                    myValue = value;
                    updatedProperties.Add(kvp.Key);
                }
            }

            foreach (var kvp in newValues) {
                switch (kvp.Key) {
                    case nameof(this.CustomUserArguments): TryUpdateGeneric(kvp, ref this._customUserArguments); break;
                    case nameof(this.UsePathQuotationMarks): TryUpdateGeneric(kvp, ref this._usePathQuotationMarks); break;
                    case nameof(this.UsePathRelativeToGame): TryUpdateGeneric(kvp, ref this._usePathRelativeToGame); break;
                    case nameof(this.TrueColor): TryUpdateGeneric(kvp, ref this._trueColor); break;
                    case nameof(this.Windowed): TryUpdateGeneric(kvp, ref this._windowed); break; 
                    case nameof(this.Sound): TryUpdateGeneric(kvp, ref this._sound); break; 
                    case nameof(this.Joystick): TryUpdateGeneric(kvp, ref this._joystick); break; 
                    case nameof(this.Debug): TryUpdateGeneric(kvp, ref this._debug); break; 
                    case nameof(this.V107Mode): TryUpdateGeneric(kvp, ref this._v107Mode); break; 
                    case nameof(this.Skip): TryUpdateGeneric(kvp, ref this._skip); break; 
                    case nameof(this.UseRefDir): TryUpdateGeneric(kvp, ref this._useRefDir); break; 
                    case nameof(this.RefDirPath): TryUpdateGeneric(kvp, ref this._refDirPath); break; 
                    case nameof(this.QuitMartridgeOnGameLaunch): TryUpdateGeneric(kvp, ref this._quitMartridgeOnGameLaunch); break; 
                    case nameof(this.QuitMartridgeOnEditorLaunch): TryUpdateGeneric(kvp, ref this._quitMartridgeOnEditorLaunch); break; 
                }
            }
            
            if (updatedProperties.Count > 0) {
                this.FireUpdatedEvent(updatedProperties);
            }
        }
        
        private void FireUpdatedEvent(List<string> updatedProperties) {
            this.Updated?.Invoke(this, new ConfigUpdateEventArgs(updatedProperties));
        }
        
        public ConfigDataLaunch GetData() {

            return new ConfigDataLaunch()  {
                TrueColor = this.TrueColor,
                Windowed = this.Windowed,
                Sound = this.Sound,
                Joystick = this.Joystick,
                Debug = this.Debug,
                V107Mode = this.V107Mode,
                UsePathQuotationMarks = this.UsePathQuotationMarks,
                UsePathRelativeToGame = this.UsePathRelativeToGame,
                CustomUserArguments = this.CustomUserArguments,
                Skip = this.Skip,
                UseRefDir = this.UseRefDir,
                RefDirPath = this.RefDirPath,
                QuitMartridgeOnGameLaunch = this.QuitMartridgeOnGameLaunch,
                QuitMartridgeOnEditorLaunch = this.QuitMartridgeOnEditorLaunch,
            };
        }
    }
}
