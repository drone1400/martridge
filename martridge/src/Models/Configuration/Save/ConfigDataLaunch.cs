namespace Martridge.Models.Configuration.Save {
    public class ConfigDataLaunch {
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
