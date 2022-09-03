using Avalonia;
using Avalonia.Controls;
using Martridge.Models.Configuration;
using Martridge.ViewModels.DinkyGraphics;
using Martridge.Views.DinkyAlerts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Martridge.ViewModels.DinkyAlerts {
    public static class DinkyAlert {
        

        public static readonly AnimatedDinkGraphicViewModel AnimatedPillbug = new AnimatedDinkGraphicViewModel(
            @"graphics/pillbug/F1-W3-", 
            new List<int>() {3, 3, 3, 3, 3}
        );
            
        public static readonly AnimatedDinkGraphicViewModel AnimatedDinkSword = new AnimatedDinkGraphicViewModel(
            new List<string>() {
                @"graphics/dinksword/D-SI2-01.png",
                @"graphics/dinksword/D-SI2-02.png",
                @"graphics/dinksword/D-SI2-03.png",
                @"graphics/dinksword/D-SI2-04.png",
                @"graphics/dinksword/D-SI2-05.png",
                @"graphics/dinksword/D-SI2-06.png",
                @"graphics/dinksword/D-SI2-05.png",
                @"graphics/dinksword/D-SI2-04.png",
                @"graphics/dinksword/D-SI2-03.png",
                @"graphics/dinksword/D-SI2-02.png",
                },
            new List<int>() {3, 3, 3, 3, 3, 3, 3, 3, 3, 3}
        );
        
        public static readonly AnimatedDinkGraphicViewModel AnimatedDuckWizardRight = new AnimatedDinkGraphicViewModel(
            new List<string>() {
                @"graphics/duckwizard/DK6W-01.png",
                @"graphics/duckwizard/DK6W-02.png",
                @"graphics/duckwizard/DK6W-03.png",
                @"graphics/duckwizard/DK6W-04.png",
            },
            new List<int>() {4, 4, 4, 4,}
        );
        
        public static readonly AnimatedDinkGraphicViewModel AnimatedDuckWizardLeft = new AnimatedDinkGraphicViewModel(
            new List<string>() {
                @"graphics/duckwizard/DK4W-01.png",
                @"graphics/duckwizard/DK4W-02.png",
                @"graphics/duckwizard/DK4W-03.png",
                @"graphics/duckwizard/DK4W-04.png",
            },
            new List<int>() {4, 4, 4, 4,}
        );
        
        public static readonly AnimatedDinkGraphicViewModel AnimatedMartridgeRight = new AnimatedDinkGraphicViewModel(
            new List<string>() {
                @"graphics/martridge/C13W3-01.png",
                @"graphics/martridge/C13W3-02.png",
                @"graphics/martridge/C13W3-03.png",
                @"graphics/martridge/C13W3-04.png",
                @"graphics/martridge/C13W3-05.png",
                @"graphics/martridge/C13W3-06.png",
                @"graphics/martridge/C13W3-07.png",
                @"graphics/martridge/C13W3-08.png",
                @"graphics/martridge/C13W3-09.png",
                @"graphics/martridge/C13W3-10.png",
            },
            new List<int>() {3, 3, 3, 3, 3, 3, 3, 3, 3, 3, }
        );
        
        public static readonly AnimatedDinkGraphicViewModel AnimatedMartridgeLeft = new AnimatedDinkGraphicViewModel(
            new List<string>() {
                @"graphics/martridge/C13W1-01.png",
                @"graphics/martridge/C13W1-02.png",
                @"graphics/martridge/C13W1-03.png",
                @"graphics/martridge/C13W1-04.png",
                @"graphics/martridge/C13W1-05.png",
                @"graphics/martridge/C13W1-06.png",
                @"graphics/martridge/C13W1-07.png",
                @"graphics/martridge/C13W1-08.png",
                @"graphics/martridge/C13W1-09.png",
                @"graphics/martridge/C13W1-10.png",
            },
            new List<int>() {3, 3, 3, 3, 3, 3, 3, 3, 3, 3, }
        );

        public static readonly AnimatedDinkGraphicViewModel AnimatedSpinningDuck = new AnimatedDinkGraphicViewModel(
            new List<string>() {
                "graphics/duck/DK1W-01.png",
                "graphics/duck/DK1W-02.png",
                "graphics/duck/DK1W-03.png",
                "graphics/duck/DK1W-04.png",
                
                "graphics/duck/DK3W-01.png",
                "graphics/duck/DK3W-02.png",
                "graphics/duck/DK3W-03.png",
                "graphics/duck/DK3W-04.png",
                
                "graphics/duck/DK6W-01.png",
                "graphics/duck/DK6W-02.png",
                "graphics/duck/DK6W-03.png",
                "graphics/duck/DK6W-04.png",
                
                "graphics/duck/DK9W-01.png",
                "graphics/duck/DK9W-02.png",
                "graphics/duck/DK9W-03.png",
                "graphics/duck/DK9W-04.png",
                
                "graphics/duck/DK7W-01.png",
                "graphics/duck/DK7W-02.png",
                "graphics/duck/DK7W-03.png",
                "graphics/duck/DK7W-04.png",
                
                "graphics/duck/DK4W-01.png",
                "graphics/duck/DK4W-02.png",
                "graphics/duck/DK4W-03.png",
                "graphics/duck/DK4W-04.png",
            },
            new List<int>() {
                3,3,3,3,
                3,3,3,3,
                3,3,3,3,
                3,3,3,3,
                3,3,3,3,
                3,3,3,3,
            }
        );

        public static void CenterOnParentWindow(Window window, Window parent) {
            Size? frameSize = parent.FrameSize;
            int posX = parent.Position.X;
            int posY = parent.Position.Y;
            if (frameSize != null && 
                double.IsNaN(window.Width) == false &&
                double.IsNaN(window.Height) == false ) {
                
                posX += ((int)(((Size)frameSize).Width / 2 - window.Width / 2));
                posY += ((int)(((Size)frameSize).Height / 2 - window.Height / 2))!;
            } else {
                posX += 100;
                posY += 100;
            }
            window.Position = new PixelPoint(posX, posY);
        }

        public async static Task<AlertResults> GetRememberedResultOrShowDialog(
            ConfigAlertResults config, 
            ConfigAlertResultMap rememberId, 
            string title, 
            string message, 
            AlertResults resultButtons, 
            AlertType type, 
            Window parentWindow, 
            Dictionary<AlertResults, string>? customButtonText = null, 
            string? specialMessage = null) {

            AlertResults result = config.GetResult(rememberId);
            
            // check if result is remembered
            if (result != AlertResults.None) {
                return result;
            }
            
            DinkyAlertWindowViewModel vm = new DinkyAlertWindowViewModel(title, message, resultButtons, type, customButtonText, specialMessage);
            DinkyAlertWindow win = new DinkyAlertWindow { DataContext = vm, };
            
            // cancel closing if result is not set yet
            win.Closing += ( sender,  args) => { if (vm.Result == AlertResults.None) args.Cancel = true; };

            // try to center on parent window
            CenterOnParentWindow(win, parentWindow);

            // show dialog and get result when done
            await win.ShowDialog(parentWindow);

            List<AlertResults> resultPriority = new List<AlertResults>() {
                AlertResults.Cancel,
                AlertResults.No,
                AlertResults.Yes,
                AlertResults.Ok,
            };

            result = vm.Result;

            if (result.HasFlag(AlertResults.RememberResult)) {
                foreach (AlertResults res in resultPriority) {
                    if (result.HasFlag(res)) {
                        config.SaveResult(rememberId, AlertResults.Cancel);
                        return res;
                    }
                }
                return AlertResults.None;
            }
            
            return result;
        }
        

        public async static Task<AlertResults> ShowDialog(string title, string message, AlertResults resultButtons, AlertType type, Window parentWindow, Dictionary<AlertResults, string>? customButtonText = null, string? specialMessage = null) {
            
            DinkyAlertWindowViewModel vm = new DinkyAlertWindowViewModel(title, message, resultButtons, type, customButtonText, specialMessage);
            DinkyAlertWindow win = new DinkyAlertWindow { DataContext = vm, };
            
            // cancel closing if result is not set yet
            win.Closing += ( sender,  args) => { if (vm.Result == AlertResults.None) args.Cancel = true; };
            
            // try to center on parent window
            CenterOnParentWindow(win, parentWindow);
            
            // show dialog and get result when done
            await win.ShowDialog(parentWindow);
            return vm.Result;
        }
    }
}
