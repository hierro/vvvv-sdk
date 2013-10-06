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
	[PluginInfo(Name = "Leap",Category = "Devices Leap", Help = "Returns the tracking data of the Leap device",Tags = "tracking, hand, finger")]
	#endregion PluginInfo
	public class LeapNode : IPluginEvaluate, IDisposable
	{
		#region fields & pins
		#pragma warning disable 0649

        [Input("Device")]
        ISpread<int> FDeviceIn;

        [Output("Controller")]
        ISpread<Controller> FDeviceOut;

		#pragma warning restore
		
		Controller FLeapController = new Controller();

        private Controller controller;
        private bool inited = false;
        private double factor= 0.001;
		
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
            if (!inited) {

                getControllers();
                inited = true;
            
            }
            
			if(controller.IsConnected && controller != null)
			{

                FDeviceOut.SliceCount = 1;
                FDeviceOut[0] = controller;

 
			}
                 
			
		}


        private void getControllers() {

            DeviceList deviceList = new DeviceList();
            controller = new Controller();

        }

		public void Dispose()
		{
			controller.Dispose();
		}
		
	}
	
	public static class LeapExtensions
	{
		public static Vector3D ToVector3DPos(this Vector v)
		{
            return new Vector3D(v.x * 0.001, v.y * 0.001, v.z * -0.001);
		}
		
		public static Vector3D ToVector3DDir(this Vector v)
		{
			return new Vector3D(v.x, v.y, -v.z);
		}
	}
}




