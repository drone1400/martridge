using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
namespace Martridge.Controls {
    
    [PseudoClasses(":selected")]
    public class SelectableButton : Button {
        /// <summary>
        /// Defines the <see cref="IsSelected"/> property.
        /// </summary>
        public static readonly StyledProperty<bool?> IsSelectedProperty =
            AvaloniaProperty.Register<SelectableButton, bool?>(nameof(IsSelected), false,
                defaultBindingMode: BindingMode.TwoWay);
        
        
        /// <summary>
        /// Gets or sets whether the <see cref="SelectableButton"/> is selected.
        /// </summary>
        public bool? IsSelected
        {
            get => this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        static SelectableButton() {
        }

        public SelectableButton() {
            this.UpdatePseudoClasses(this.IsSelected);
        }
        
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsSelectedProperty)
            {
                var newValue = change.GetNewValue<bool?>();

                this.UpdatePseudoClasses(newValue);
            }
        }

        private void UpdatePseudoClasses(bool? isSelected)
        {
            this.PseudoClasses.Set(":selected", isSelected == true);
        }
    }
}
