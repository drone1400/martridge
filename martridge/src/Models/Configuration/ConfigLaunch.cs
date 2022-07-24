using Martridge.Models.Configuration.Save;
using System;

namespace Martridge.Models.Configuration {
    public class ConfigLaunch {

        public event EventHandler? Updated;

        /// <summary>
        /// Launches the game in true color mode
        /// </summary>
        public bool TrueColor { get; private set; } = false;
        
        /// <summary>
        /// Launches the game in windowed mode
        /// </summary>
        public bool Windowed { get; private set; } = true;
        
        /// <summary>
        /// Launches the game with sound
        /// </summary>
        public bool Sound { get; private set; } = true;
        
        /// <summary>
        /// Launches the game with (questionable?) joystick support
        /// </summary>
        public bool Joystick { get; private set; } = true;
        
        /// <summary>
        /// Launches the game in DEBUG mode
        /// </summary>
        public bool Debug { get; private set; } = false;
        
        /// <summary>
        /// Launches the game in V1.07 compatibility mode
        /// </summary>
        public bool V107Mode { get; private set; } = false;
        private void FireUpdatedEvent() {
            this.Updated?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateAll(
            bool trueColor,
            bool windowed,
            bool sound,
            bool joystick,
            bool debug,
            bool v107Mode
            ) {

            bool hasChanges = false;
            
            if (this.TrueColor != trueColor) {
                this.TrueColor = trueColor;
                hasChanges = true;
            }

            if (this.Windowed != windowed) {
                this.Windowed = windowed;
                hasChanges = true;
            }

            if (this.Sound != sound) {
                this.Sound = sound;
                hasChanges = true;
            }

            if (this.Joystick != joystick) {
                this.Joystick = joystick;
                hasChanges = true;
            }

            if (this.Debug != debug) {
                this.Debug = debug;
                hasChanges = true;
            }

            if (this.V107Mode != v107Mode) {
                this.V107Mode = v107Mode;
                hasChanges = true;
            }

            if (hasChanges) {
                this.FireUpdatedEvent();
            }
        }

        public void UpdateFromData(ConfigDataLaunch data) {
            bool hasChanges = false;
            if (data.TrueColor != null && this.TrueColor != data.TrueColor) {
                this.TrueColor = (bool)data.TrueColor;
                hasChanges = true;
            }
            
            if (data.Windowed != null && this.Windowed != data.Windowed) {
                this.Windowed = (bool)data.Windowed;
                hasChanges = true;
            }
            
            if (data.Sound != null && this.Sound != data.Sound) {
                this.Sound = (bool)data.Sound;
                hasChanges = true;
            }
            
            if (data.Joystick != null && this.Joystick != data.Joystick) {
                this.Joystick = (bool)data.Joystick;
                hasChanges = true;
            }
            
            if (data.Debug != null && this.Debug != data.Debug) {
                this.Debug = (bool)data.Debug;
                hasChanges = true;
            }
            
            if (data.V107Mode != null && this.V107Mode != data.V107Mode) {
                this.V107Mode = (bool)data.V107Mode;
                hasChanges = true;
            }

            if (hasChanges) {
                this.FireUpdatedEvent();
            }
        }
        
        public ConfigDataLaunch GetData() {

            return new ConfigDataLaunch()  {
                TrueColor = this.TrueColor,
                Windowed = this.Windowed,
                Sound = this.Sound,
                Joystick = this.Joystick,
                Debug = this.Debug,
                V107Mode = this.V107Mode,
            };
        }
    }
}
