using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;

namespace Martridge.ViewModels.Localization {
    public class LocalizeExtension : MarkupExtension {
        public LocalizeExtension(string key) {
            this.Key = key;
        }
        public string Key { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            var binding = new ReflectionBindingExtension($"[{this.Key}]") {
                Mode = BindingMode.OneWay,
                Source = Models.Localization.Localizer.Instance,
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
