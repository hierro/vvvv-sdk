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
    [PluginInfo(Name = "Fingers",
    Category = "Devices Leap",
    Help = "Hands data drom Leap device",
    Tags = "tracking, hand, finger, gestures"
    )]
    #endregion PluginInfo
    public class Fingers : IPluginEvaluate, IPluginConnections
    {

        [Input("Leap Device")]
        private Pin<Controller> FDeviceIn;

        [Output("Is Tool")]
        ISpread<bool> FFingerIsToolOut;

        [Output("Finger Position")]
        ISpread<Vector3D> FFingerPosOut;

        [Output("Finger Direction")]
        ISpread<Vector3D> FFingerDirOut;

        [Output("Finger Velocity")]
        ISpread<Vector3D> FFingerVelOut;

        [Output("Finger Size")]
        ISpread<Vector2D> FFingerSizeOut;

        [Output("Finger ID")]
        ISpread<int> FFingerIDOut;

        [Output("Hand Slice")]
        ISpread<int> FHandSliceOut;

        private Controller controller;
       

        private bool FInvalidateConnect = false;
        protected bool FInvalidate = true;

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

                    this.controller = this.FDeviceIn[0];

                    if (controller != null)
                    {

                       ////   boh


                    }

                }
                this.FInvalidateConnect = false;
            }


            if (this.controller != null)
            {
                var hands = controller.Frame().Hands;

                SpreadMax = hands.Count;

               

                
                FFingerPosOut.SliceCount = 0;
                FFingerDirOut.SliceCount = 0;
                FFingerVelOut.SliceCount = 0;
                FFingerIsToolOut.SliceCount = 0;
                FFingerSizeOut.SliceCount = 0;
                FFingerIDOut.SliceCount = 0;
                FHandSliceOut.SliceCount = 0;
                

                for (int i = 0; i < SpreadMax; i++)
                {
                    var hand = hands[i];

                    if (hand != null)
                    {
                        /*
                        FHandPosOut[i] = hand.PalmPosition.ToVector3DPos();
                        FHandDirOut[i] = hand.Direction.ToVector3DDir();
                        FHandNormOut[i] = hand.PalmNormal.ToVector3DDir();
                        FHandBallCentOut[i] = hand.SphereCenter.ToVector3DPos();
                        FHandBallRadOut[i] = hand.SphereRadius * 0.001;
                        FHandVelOut[i] = hand.PalmVelocity.ToVector3DPos();
                        FHandIDOut[i] = hand.Id;
                         * */

                    }
                    
                    var pointables = hand.Pointables;

                    for (int j = 0; j < pointables.Count; j++)
                    {
                        var pointable = pointables[j];

                        FFingerPosOut.Add(pointable.TipPosition.ToVector3DPos());
                        FFingerDirOut.Add(pointable.Direction.ToVector3DDir());
                        FFingerVelOut.Add(pointable.TipVelocity.ToVector3DPos());
                        FFingerIsToolOut.Add(pointable.IsTool);
                        FFingerSizeOut.Add(new Vector2D(pointable.Width * 0.001, pointable.Length * 0.001));
                        FFingerIDOut.Add(pointable.Id);
                        FHandSliceOut.Add(i);


                    }
                   
                }


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
