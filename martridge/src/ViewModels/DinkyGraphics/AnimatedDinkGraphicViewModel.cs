using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace Martridge.ViewModels.DinkyGraphics {
    public class AnimatedDinkGraphicViewModel : ViewModelBase, IDisposable {

        private static DispatcherTimer _frameTimer;
        
        private List<int> _frameTicks = new List<int>();
        private List<Bitmap> _frames = new List<Bitmap>();

        private Bitmap? _frame = null;
        public Bitmap? Frame {
            get => this._frame;
            private set => this.RaiseAndSetIfChanged(ref this._frame, value);
        }

        private int _frameTick = 0;
        private int _frameIndex = 0;

        static AnimatedDinkGraphicViewModel() {
            _frameTimer = new DispatcherTimer() {
                Interval = new TimeSpan(0, 0, 0, 0, 33),
            };
            _frameTimer.Start();
        }

        public AnimatedDinkGraphicViewModel(string assetImagePrefix, List<int> frameTimes) {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

            assetImagePrefix = @"avares://martridge/Assets/" + assetImagePrefix;
            
            for (int i = 0; i < frameTimes.Count; i++) {
                string fullName = assetImagePrefix + String.Format("{0:00}", i + 1) + ".png";
                this._frameTicks.Add(frameTimes[i]);

                Bitmap bmp = new Bitmap(assets.Open(new Uri(fullName)));
                this._frames.Add(bmp);
            }


            this.Frame = this._frames[0];
            this._frameIndex = 0;
            this._frameTick = 0;
            
            _frameTimer.Tick += this.FrameTimerOnTick;
        }
        
        public AnimatedDinkGraphicViewModel(List<string> imageResoruces, List<int> frameTimes) {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            
            string assetImagePrefix = @"avares://martridge/Assets/";

            if (imageResoruces.Count != frameTimes.Count) {
                throw new ArgumentException("Image count and frame time count must be the same!");
            }
            
            for (int i = 0; i < frameTimes.Count; i++) {
                string fullName = assetImagePrefix + imageResoruces[i];
                this._frameTicks.Add(frameTimes[i]);

                Bitmap bmp = new Bitmap(assets.Open(new Uri(fullName)));
                this._frames.Add(bmp);
            }


            this.Frame = this._frames[0];
            this._frameIndex = 0;
            this._frameTick = 0;
            
            _frameTimer.Tick += this.FrameTimerOnTick;
        }

        public void Dispose() {
            _frameTimer.Tick -= this.FrameTimerOnTick;
        }
        private void FrameTimerOnTick(object? sender, EventArgs e) {
            if (this._frames.Count <= 1) {
                return;
            }
            
            this._frameTick ++;

            if (this._frameTick == this._frameTicks[this._frameIndex]) {
                this._frameTick = 0;
                this._frameIndex ++;
                
                if (this._frameIndex == this._frames.Count) {
                    this._frameIndex = 0;
                }

                this.Frame = this._frames[this._frameIndex];
            }
        }
    }
}
