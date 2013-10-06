#region usings
using System;
using System.Collections.Generic;
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
    [PluginInfo(Name = "Configuration",
    Category = "Devices Leap",
    Help = "Returns gestures from a Leap Device",
    Tags = "tracking, hand, finger, gestures"
    )]
    #endregion PluginInfo
    public class Configuration : IPluginEvaluate, IPluginConnections
    {


        [Input("Leap Device")]
        private Pin<Controller> FDeviceIn;

        [Input("CircleMinArc", IsSingle = true, DefaultValue=4.712389)]
        IDiffSpread<float> FCircleMinArcIn;

        [Input("CircleMinRadius", IsSingle = true, DefaultValue = 5)]
        IDiffSpread<float> FCircleMinRadiusIn;

        [Input("SwipeMinLength", IsSingle = true, DefaultValue = 150)]
        IDiffSpread<float> FSwipeMinLengthIn;

        [Input("SwipeMinVelocity", IsSingle = true, DefaultValue = 1000)]
        IDiffSpread<float> FSwipeMinVelocityIn;

        [Input("KeyTapMinDownVelocity", IsSingle = true, DefaultValue = 50)]
        IDiffSpread<float> FKeyTapMinDownVelocityIn;

        [Input("KeyTapHistorySeconds", IsSingle = true, DefaultValue = 0.1)]
        IDiffSpread<float> FKeyTapHistorySecondsIn;

        [Input("KeyTapMinDistance", IsSingle = true, DefaultValue = 3.0)]
        IDiffSpread<float> FKeyTapMinDistanceIn;

        [Input("ScreenTapMinForwardVelocity", IsSingle = true, DefaultValue = 50)]
        IDiffSpread<float> FScreenTapMinForwardVelocityIn;

        [Input("ScreenTapHistorySeconds", IsSingle = true, DefaultValue = 0.1)]
        IDiffSpread<float> FScreenTapHistorySecondsIn;

        [Input("ScreenTapMinDistance", IsSingle = true, DefaultValue = 5.0)]
        IDiffSpread<float> FScreenTapMinDistanceIn;

        [Output("Configuration", IsSingle = true)]
        ISpread<string> FConfigurationOut;


        private bool FInvalidateConnect = false;
        protected bool FInvalidate = true;
        private Controller controller=null;
        private Config conf=null;
        

        public void Evaluate(int SpreadMax)
        {

            if (this.FInvalidateConnect)
            {
                if (controller != null)
                {
                    //enableGestures(false);
                }

                if (this.FDeviceIn.PluginIO.IsConnected)
                {

                    controller = this.FDeviceIn[0];
                    

                    if (controller != null)
                    {
                        conf = controller.Config;
                        //enableGestures(true);

                        FConfigurationOut[0] = "Is controller valid:" +controller.Devices[0].IsValid.ToString();
                    }

                }
                else { 
                
                    
                }
                this.FInvalidateConnect = false;
            }

            

            if (controller != null && controller.IsConnected && conf !=null)
            {
                if (FCircleMinArcIn.IsChanged || FCircleMinRadiusIn.IsChanged || FSwipeMinLengthIn.IsChanged || FSwipeMinVelocityIn.IsChanged
                    || FKeyTapHistorySecondsIn.IsChanged || FKeyTapMinDistanceIn.IsChanged || FKeyTapMinDownVelocityIn.IsChanged
                    || FScreenTapHistorySecondsIn.IsChanged || FScreenTapMinDistanceIn.IsChanged || FScreenTapMinForwardVelocityIn.IsChanged
                    )
                {

                   // FConfigurationOut[0] = conf.GetFloat("Gesture.Circle.MinArc").ToString();

                    updateConfiguration();
                   
                }
            }
           

        }

        private void updateConfiguration() {

           conf.SetFloat("Gesture.Circle.MinArc",FCircleMinArcIn[0]);
           conf.SetFloat("Gesture.Circle.MinRadius", FCircleMinRadiusIn[0]);
           conf.SetFloat("Gesture.Swipe.MinLength", FSwipeMinLengthIn[0]);
           conf.SetFloat("Gesture.Swipe.MinVelocity", FSwipeMinVelocityIn[0]);
           conf.SetFloat("Gesture.KeyTap.MinDownVelocity", FKeyTapMinDownVelocityIn[0]);
           conf.SetFloat("Gesture.KeyTap.HistorySeconds", FKeyTapHistorySecondsIn[0]);
           conf.SetFloat("Gesture.KeyTap.MinDistance", FKeyTapMinDistanceIn[0]);
           conf.SetFloat("Gesture.ScreenTap.MinForwardVelocity", FScreenTapMinForwardVelocityIn[0]);
           conf.SetFloat("Gesture.ScreenTap.HistorySeconds", FScreenTapHistorySecondsIn[0]);
           conf.SetFloat("Gesture.ScreenTap.MinDistance", FScreenTapMinDistanceIn[0]);

           conf.Save();

           FConfigurationOut[0]= readConfig();
           

        }

        private string readConfig()
        {

            string s = "";

            s += "Circle.MinArc: "+ conf.GetFloat("Gesture.Circle.MinArc").ToString() + "\r\n";
            s += "Circle.MinRadius: " + conf.GetFloat("Gesture.Circle.MinRadius").ToString() + "\r\n";
            s += "Swipe.MinLength: " + conf.GetFloat("Gesture.Swipe.MinLength").ToString() + "\r\n";
            s += "Swipe.MinVelocity: " + conf.GetFloat("Gesture.Swipe.MinVelocity").ToString() + "\r\n";
            s += "KeyTap.MinDownVelocity: " + conf.GetFloat("Gesture.KeyTap.MinDownVelocity").ToString() + "\r\n";
            s += "KeyTap.HistorySeconds: " + conf.GetFloat("Gesture.KeyTap.HistorySeconds").ToString() + "\r\n";
            s += "KeyTap.MinDistance: " + conf.GetFloat("Gesture.KeyTap.MinDistance").ToString() + "\r\n";
            s += "ScreenTap.MinForwardVelocity: " + conf.GetFloat("Gesture.ScreenTap.MinForwardVelocity").ToString() + "\r\n";
            s += "ScreenTap.HistorySeconds: " + conf.GetFloat("Gesture.ScreenTap.HistorySeconds").ToString() + "\r\n";
            s += "ScreenTap.MinDistance: " + conf.GetFloat("Gesture.ScreenTap.MinDistance").ToString() ;

            return s;


        }
        

        private void saveConfig() {
            if (conf != null) conf.Save();
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
