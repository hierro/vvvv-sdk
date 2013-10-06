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
    [PluginInfo(Name = "Gestures",
    Category = "Devices Leap",
    Help = "Returns gestures from a Leap Device",
    Tags = "tracking, hand, finger, gestures"
    )]
    #endregion PluginInfo
    public class Gestures : IPluginEvaluate, IPluginConnections
    {

        [Input("Leap Device")]
        private Pin<Controller> FDeviceIn;

        [Output("Type")]
        ISpread<String> FTypeOut;

        [Output("State")]
        ISpread<String> FStateOut;

        [Output("ID")]
        ISpread<int> FIdOut;

        [Output("HandsID")]
        ISpread<int> FHandsIDOut;

        [Output("PointablesID")]
        ISpread<int> FPointablesIDOut;

        [Output("Duration")]
        ISpread<float> FDurationOut;

        [Output("DurationSeconds")]
        ISpread<float> FDurationSecondsOut;

        [Output("Progress")]
        ISpread<float> FProgressOut;

      
        private Controller controller;
        private GestureList gestures;

        private bool FInvalidateConnect = false;
        protected bool FInvalidate = true;

        public void Evaluate(int SpreadMax)
        {

            if (this.FInvalidateConnect)
            {
                if (controller != null)
                {
                    enableGestures(false);
                }

                if (this.FDeviceIn.PluginIO.IsConnected)
                {
                 
                    this.controller = this.FDeviceIn[0];

                    if (controller != null)
                    {
                      
                        enableGestures(true);
                    }
                    
                }
                this.FInvalidateConnect = false;
            }


            if (this.controller != null) {

                int handsIndex=0;
                int pointablesIndex = 0;

                gestures = controller.Frame().Gestures();

                SpreadMax = gestures.Count;

                FTypeOut.SliceCount = SpreadMax;
                FStateOut.SliceCount = SpreadMax;
                FDurationOut.SliceCount = SpreadMax;
                FDurationSecondsOut.SliceCount = SpreadMax;
                FIdOut.SliceCount = SpreadMax;
                FProgressOut.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++) {

                    Gesture g=gestures[i];
                   

                    FTypeOut[i] = g.Type.ToString();
                    FStateOut[i] =g.State.ToString();

                    PointableList pointables = g.Pointables;

                    if (pointables.Count > 0)
                    {

                        for (int p = 0; p < pointables.Count; p++)
                        {

                            FPointablesIDOut[pointablesIndex] = pointables[p].Id;
                            pointablesIndex++;

                        }
                    }

                    HandList hands = g.Hands;

                    for (int h = 0; h < hands.Count; h++)
                    {

                        FHandsIDOut[handsIndex] = hands[h].Id;
                        handsIndex++;


                    }


                    FIdOut[i] = g.Id;
                    FDurationOut[i] = g.Duration;
                    FDurationSecondsOut[i] = g.DurationSeconds;

                    
                   
                    switch (g.Type) { 
                    
                        case  Gesture.GestureType.TYPECIRCLE :

                            CircleGesture temp = new CircleGesture(g);
                            FProgressOut[i] = temp.Progress; 

                            break;

                        default :
                            FProgressOut[i] = 0;

                            break;
                    
                    }
                     
                }

            
            }

        }

        private void getGesturesFromFrame() {

            gestures = controller.Frame().Gestures();

           // SpreadMax = gestures.Count;
        
        }

        private void enableGestures(bool mode) {
            if (controller.IsConnected)
            {
                controller.EnableGesture(Gesture.GestureType.TYPECIRCLE, mode);
                controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP, mode);
                controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP, mode);
                controller.EnableGesture(Gesture.GestureType.TYPESWIPE, mode);
            }
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
