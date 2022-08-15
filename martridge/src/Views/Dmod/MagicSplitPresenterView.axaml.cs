using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.Dmod {
    public partial class MagicSplitPresenterView : UserControl {
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // One Page Mode
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        public static readonly AvaloniaProperty<double> OnePageSwitchThresholdProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, double>(nameof(OnePageSwitchThreshold), 1000);
        public double OnePageSwitchThreshold {
            get => (double)(this.GetValue(OnePageSwitchThresholdProperty) ?? double.NaN);
            set => this.SetValue(OnePageSwitchThresholdProperty,value);
        }
        
        public static readonly AvaloniaProperty<double> OnePageSwitchHysteresisProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, double>(nameof(OnePageSwitchHysteresis), 10);
        public double OnePageSwitchHysteresis {
            get => (double)(this.GetValue(OnePageSwitchHysteresisProperty) ?? double.NaN);
            set => this.SetValue(OnePageSwitchHysteresisProperty,value);
        }

        public static readonly AvaloniaProperty<bool> IsViewOnePageModeProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, bool>(nameof(IsViewOnePageMode), false);
        public bool IsViewOnePageMode {
            get => (bool)(this.GetValue(IsViewOnePageModeProperty) ?? false);
            private set => this.SetValue(IsViewOnePageModeProperty,value);
        }
        
        public static readonly AvaloniaProperty<bool> IsOnePageShowingRightContentProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, bool>(nameof(IsOnePageShowingRightContent), false);
        public bool IsOnePageShowingRightContent {
            get => (bool)(this.GetValue(IsOnePageShowingRightContentProperty) ?? false);
            set => this.SetValue(IsOnePageShowingRightContentProperty,value);
        }
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Left Panel
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        public static readonly AvaloniaProperty<object?> LeftPanelContentProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, object?>(nameof(LeftPanelContent), null);
        public object? LeftPanelContent {
            get => this.GetValue(LeftPanelContentProperty);
            set => this.SetValue(LeftPanelContentProperty,value);
        }
        
        public static readonly AvaloniaProperty<double> LeftPanelMinWidthProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, double>(nameof(LeftPanelMinWidth), 480);
        public double LeftPanelMinWidth {
            get => (double)(this.GetValue(LeftPanelMinWidthProperty) ?? double.NaN);
            set => this.SetValue(LeftPanelMinWidthProperty,value);
        }
        
        public static readonly AvaloniaProperty<int> LeftPanelColumnProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, int>(nameof(LeftPanelColumn), 0);
        public int LeftPanelColumn {
            get => (int)(this.GetValue(LeftPanelColumnProperty) ?? 0);
            private set => this.SetValue(LeftPanelColumnProperty,value);
        }
        
        public static readonly AvaloniaProperty<int> LeftPanelColumnSpanProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, int>(nameof(LeftPanelColumnSpan), 1);
        public int LeftPanelColumnSpan {
            get => (int)(this.GetValue(LeftPanelColumnSpanProperty) ?? 0);
            private set => this.SetValue(LeftPanelColumnSpanProperty,value);
        }
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Right Panel
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        public static readonly AvaloniaProperty<object?> RightPanelContentProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, object?>(nameof(RightPanelContent), null);
        public object? RightPanelContent {
            get => this.GetValue(RightPanelContentProperty);
            set => this.SetValue(RightPanelContentProperty,value);
        }
        
        public static readonly AvaloniaProperty<double> RightPanelMinWidthProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, double>(nameof(RightPanelMinWidth), 480);
        public double RightPanelMinWidth {
            get => (double)(this.GetValue(RightPanelMinWidthProperty) ?? double.NaN);
            set => this.SetValue(RightPanelMinWidthProperty,value);
        }
        
        public static readonly AvaloniaProperty<int> RightPanelColumnProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, int>(nameof(RightPanelColumn), 2);
        public int RightPanelColumn {
            get => (int)(this.GetValue(RightPanelColumnProperty) ?? 0);
            private set => this.SetValue(RightPanelColumnProperty,value);
        }
        
        public static readonly AvaloniaProperty<int> RightPanelColumnSpanProperty =
            AvaloniaProperty.Register<MagicSplitPresenterView, int>(nameof(RightPanelColumnSpan), 1);
        public int RightPanelColumnSpan {
            get => (int)(this.GetValue(RightPanelColumnSpanProperty) ?? 0);
            private set => this.SetValue(RightPanelColumnSpanProperty,value);
        }
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Constructor and logic
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        private readonly Grid? _theGrid;

        public MagicSplitPresenterView() {
            this.InitializeComponent();

            this._theGrid = this.Content as Grid;

            this.PropertyChanged += ( sender,  args) => {
                if (args.Property.Name == nameof(this.Bounds)) {
                    this.ResizeMagic();
                }

                if (args.Property.Name == nameof(this.LeftPanelMinWidth) ||
                    args.Property.Name == nameof(this.RightPanelMinWidth)) {
                    this.RefreshMagicProperties();
                }
            };
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void RefreshMagicProperties() {
            if (this.IsViewOnePageMode == false) {
                this.LeftPanelColumn = 0;
                this.LeftPanelColumnSpan = 1;
                this.RightPanelColumn = 2;
                this.RightPanelColumnSpan = 1;
                if (this._theGrid != null) {
                    this._theGrid.ColumnDefinitions[0].MinWidth = this.LeftPanelMinWidth;
                    this._theGrid.ColumnDefinitions[2].MinWidth = this.RightPanelMinWidth;
                }
            } else {
                this.LeftPanelColumn = 0;
                this.LeftPanelColumnSpan = 3;
                this.RightPanelColumn = 0;
                this.RightPanelColumnSpan = 3;
                if (this._theGrid != null) {
                    this._theGrid.ColumnDefinitions[0].MinWidth = 0;
                    this._theGrid.ColumnDefinitions[2].MinWidth = 0;
                }
            }
        }

        /// <summary>
        /// Switches the view from side by side mode to single page mode depending on the control bounds width
        /// </summary>
        private void ResizeMagic() {
            if (this.IsViewOnePageMode && 
                this.Bounds.Width > this.OnePageSwitchThreshold + this.OnePageSwitchHysteresis) {
                this.IsViewOnePageMode = false;
                this.RefreshMagicProperties();
            } else if (this.IsViewOnePageMode == false && 
                       this.Bounds.Width < this.OnePageSwitchThreshold - this.OnePageSwitchHysteresis) {
                this.IsViewOnePageMode = true;
                this.RefreshMagicProperties();
            }
        }
    }
}

