using ReactiveUI;
namespace Martridge.ViewModels.Configuration {
    public class DmodDefinitionPlaceholderViewModel : ViewModelBase{
        public string Name {
            get => this._name;
            set => this.RaiseAndSetIfChanged(ref this._name, value);
        }
        private string _name = string.Empty;

        public DmodDefinitionPlaceholderViewModel(string name) {
            this.Name = name;
        }
    }
}
