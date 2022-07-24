using Avalonia.Controls;
using ReactiveUI;
using System;

namespace Martridge.ViewModels {
    public class ViewModelBase : ReactiveObject {
        public Window? ParentWindow {
            get => this._parentWindow;
            private set => this.RaiseAndSetIfChanged(ref this._parentWindow, value);
        }
        private Window? _parentWindow = null;

        public void AssignParentWindow(Window? w) {
            this.RemoveWindow();
            if (w != null) {
                this.AddWindow(w);
            }
        }
        
        private void OnParentWindow_Closed(object? sender, EventArgs e) {
            this.RemoveWindow();
        }
        private void RemoveWindow() {
            if (this.ParentWindow != null) {
                this.ParentWindow.Closed -= this.OnParentWindow_Closed;
                this.ParentWindow = null;
            }
        }
        private void AddWindow(Window w) {
            this.ParentWindow = w;
            this.ParentWindow.Closed += this.OnParentWindow_Closed;
        }
    }
}
