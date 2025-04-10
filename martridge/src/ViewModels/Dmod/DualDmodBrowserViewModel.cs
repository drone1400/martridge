using ReactiveUI;
namespace Martridge.ViewModels.Dmod
{
    public class DualDmodBrowserViewModel : ViewModelAppPage
    {
        public DmodBrowserViewModel? DmodBrowserVm {
            get => this._dmodBrowserVm;
            set => this.RaiseAndSetIfChanged(ref this._dmodBrowserVm, value);
        }
        private DmodBrowserViewModel? _dmodBrowserVm = null;
        
        public OnlineDmodBrowserViewModel? OnlineDmodBrowserVm {
            get => this._onlineDmodBrowserVm;
            set => this.RaiseAndSetIfChanged(ref this._onlineDmodBrowserVm, value);
        }
        private OnlineDmodBrowserViewModel? _onlineDmodBrowserVm = null;
    }
}
