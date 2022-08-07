namespace Martridge.Models.Configuration.Save {
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
    }
}
