using Avalonia.Input;
namespace Martridge.ViewModels {
    public abstract class ViewModelAppPage : ViewModelBase {

        public virtual bool ProcessKeyDown(Key key, KeyModifiers modifiers) {
            return false;
        } 
    }
}
