#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Algorithm;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using Leap;

#endregion usings

namespace VVVV.Nodes.Devices.Leap
{

    #region PluginInfo
    [PluginInfo(Name = "Info",
    Category = "Devices Leap",
    Help = "Returns gestures from a Leap Device",
    Tags = "tracking, hand, finger, gestures"
    )]
    #endregion PluginInfo
    public class Info : IPluginEvaluate, IPluginConnections
    {

        [Input("Leap Device")]
        private Pin<Controller> FDeviceIn;

        [Output("FrameRate")]
        ISpread<float> FFrameRateOut;

        [Output("Range")]
        ISpread<float> FRangeOut;

        [Output("HorizontalviewAngle")]
        ISpread<float> FHorizontalviewAngleOut;

        [Output("VerticalviewAngle")]
        ISpread<float> FVerticalviewAngleOut;

     
       
        private Controller controller;

        private bool FInvalidateConnect = false;
        protected bool FInvalidate = true;

      
        public void Evaluate(int SpreadMax)
        {

            if (this.FInvalidateConnect)
            {
                if (controller != null)
                {

                }

                if (this.FDeviceIn.PluginIO.IsConnected)
                {
                   
                    controller = this.FDeviceIn[0];
                  

                    if (controller != null)
                    {
                        //readConfig();
                    }

                }

                this.FInvalidateConnect = false;
            }


            if (this.controller !=  null)
            {
                readInfo();

            }

        }

        private void readInfo() {

            FRangeOut[0] = controller.Devices[0].Range;
            FFrameRateOut[0] = controller.Frame().CurrentFramesPerSecond;
            FHorizontalviewAngleOut[0] = controller.Devices[0].HorizontalViewAngle;
            FVerticalviewAngleOut[0] = controller.Devices[0].VerticalViewAngle;

        }
       

        public void ConnectPin(IPluginIO pin)
        {
            if (pin == this.FDeviceIn.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        public void DisconnectPin(IPluginIO pin)
        {
            if (pin == this.FDeviceIn.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }
    }
}
