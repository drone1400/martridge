using System.Collections.Generic;
namespace Martridge.Models.Configuration.Launcher.FileData {
    public class ConfigDataLaunch {
        
        /// <summary>
        /// Additional custom user arguments
        /// </summary>
        public string? CustomUserArguments { get ; set; } = "";

        /// <summary>
        /// Use quotation marks in the path (works only for DinkHD after 1.97 and FreeDink versions)
        /// </summary>
        public bool? UsePathQuotationMarks { get; set; } = true;
        
        /// <summary>
        /// Make the DMOD path relative to the chosen Dink Launcher
        /// </summary>
        public bool? UsePathRelativeToGame { get; set; } = true;
        
        /// <summary>
        /// Launches the game in true color mode
        /// </summary>
        public bool? TrueColor { get; set; }
        
        /// <summary>
        /// Launches the game in windowed mode
        /// </summary>
        public bool? Windowed { get; set; }
        
        /// <summary>
        /// Launches the game with sound
        /// </summary>
        public bool? Sound { get; set; }
        
        /// <summary>
        /// Launches the game with (questionable?) joystick support
        /// </summary>
        public bool? Joystick { get; set; }
        
        /// <summary>
        /// Launches the game in DEBUG mode
        /// </summary>
        public bool? Debug { get; set; }
        
        /// <summary>
        /// Launches the game in V1.07 compatibility mode
        /// </summary>
        public bool? V107Mode { get; set; }
        
        /// <summary>
        /// Skip update check and stuff in DinkHD
        /// </summary>
        public bool? Skip { get; set; }

        /// <summary>
        /// If true, passes the --refdir argument when launching or editing DMODs
        /// </summary>
        public bool? UseRefDir { get; set; }

        /// <summary>
        /// The RefDir path to use...
        /// </summary>
        public string? RefDirPath { get; set; }


        /// <summary>
        /// If true, Martridge will exit after successfully launching the game
        /// </summary>
        public bool QuitMartridgeOnGameLaunch { get; set; }

        /// <summary>
        /// If true, Martridge will exit after successfully launching the editor
        /// </summary>
        public bool QuitMartridgeOnEditorLaunch { get; set; }

        public Dictionary<string, object?> GetValues() {
            return new Dictionary<string, object?>() {
                [nameof(ConfigLaunch.CustomUserArguments)] = this.CustomUserArguments,
                [nameof(ConfigLaunch.UsePathQuotationMarks)] = this.UsePathQuotationMarks,
                [nameof(ConfigLaunch.UsePathRelativeToGame)] = this.UsePathRelativeToGame,
                [nameof(ConfigLaunch.TrueColor)] = this.TrueColor,
                [nameof(ConfigLaunch.Windowed)] = this.Windowed,
                [nameof(ConfigLaunch.Sound)] = this.Sound,
                [nameof(ConfigLaunch.Joystick)] = this.Joystick,
                [nameof(ConfigLaunch.Debug)] = this.Debug,
                [nameof(ConfigLaunch.V107Mode)] = this.V107Mode,
                [nameof(ConfigLaunch.Skip)] = this.Skip,
                [nameof(ConfigLaunch.UseRefDir)] = this.UseRefDir,
                [nameof(ConfigLaunch.RefDirPath)] = this.RefDirPath,
                [nameof(ConfigLaunch.QuitMartridgeOnGameLaunch)] = this.QuitMartridgeOnGameLaunch,
                [nameof(ConfigLaunch.QuitMartridgeOnEditorLaunch)] = this.QuitMartridgeOnEditorLaunch,
            };
        }
    }
}
