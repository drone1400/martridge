using System;
using Martridge.Trace;
using Martridge.ViewModels.DinkyAlerts;
using Martridge.ViewModels.DinkyGraphics;
namespace Martridge.ViewModels.Dmod
{
    public class NoDinkyLinuxViewModel : ViewModelBase
    {
        public event EventHandler? ShowConfigurationPageRequested;
        
        
        public AnimatedDinkGraphicViewModel AnimatedDuckWizardLeft {
            get => DinkyAlert.AnimatedDuckWizardLeft;
        }
        
        public AnimatedDinkGraphicViewModel AnimatedDuckWizardRight {
            get => DinkyAlert.AnimatedDuckWizardRight;
        }
        
        public void CmdShowConfigurationPage()
        {
            try
            {
                this.ShowConfigurationPageRequested?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
        }
    }
}
