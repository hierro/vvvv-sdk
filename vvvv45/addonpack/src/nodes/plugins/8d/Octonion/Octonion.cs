#region licence/info

//////project name
//vvvv plugin template

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{

    //class definition
    public class Multiply : IPlugin, IDisposable
    {
        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //input pin declaration
        private IValueIn FMyValueInput1;
        private IValueIn FMyValueInput2;
        private IValueIn FMyValueInput3;
        private IValueIn FMyValueInput4;

        //output pin declaration
        private IValueOut FMyValueOutput1;
        private IValueOut FMyValueOutput2;

        #endregion field declaration

        #region constructor/destructor

        public Multiply()
        {
            //the nodes constructor
            //nothing to declare for this node
        }

        // Implementing IDisposable's Dispose method.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!FDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                FHost.Log(TLogType.Debug, "PluginTemplate is being deleted");

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~Multiply()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
        #endregion constructor/destructor

        #region node name and infos

        //provide node infos 
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();

                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "Multiply";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "Octonion";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";

                    //the nodes author: your sign
                    FPluginInfo.Author = "fibo";
                    //describe the nodes function
                    FPluginInfo.Help = "Octonion multiplication";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";

                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }
                return FPluginInfo;
            }
        }

        public bool AutoEvaluate
        {
            //return true if this node needs to calculate every frame even if nobody asks for its output
            get { return false; }
        }

        #endregion node name and infos

        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            //create inputs
            FHost.CreateValueInput("Input 1 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput1);
            FMyValueInput1.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, 0, false, false, false);
            
            FHost.CreateValueInput("Input 2 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput2);
            FMyValueInput2.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, 0, false, false, false); 

            FHost.CreateValueInput("Input 3 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput3);
            FMyValueInput3.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, 0, false, false, false);

            FHost.CreateValueInput("Input 4 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput4);
            FMyValueInput4.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, 0, false, false, false);

            //create outputs	    	
            
            FHost.CreateValueOutput("Output 1 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutput1);
            FMyValueOutput1.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, 0, false, false, false);

            FHost.CreateValueOutput("Output 2 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutput2);
            FMyValueOutput2.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, 0, false, false, false);
            
}

        #endregion pin creation

        #region mainloop

        public void Configurate(IPluginConfig Input)
        {
            //nothing to configure in this plugin
            //only used in conjunction with inputs of type cmpdConfigurate
        }

        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {
            //if any of the inputs has changed
            //recompute the outputs
            if (FMyValueInput1.PinIsChanged||FMyValueInput2.PinIsChanged||FMyValueInput3.PinIsChanged||FMyValueInput4.PinIsChanged)
            {
                //first set slicecounts for all outputs
                //the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
                FMyValueOutput1.SliceCount = SpreadMax;
                FMyValueOutput2.SliceCount = SpreadMax;

                //the variables to fill with the input data
                double a1, a2, a3, a4;
                double b1, b2, b3, b4;
                double c1, c2, c3, c4;
                double d1, d2, d3, d4;

                Vector4D a, b, c, d,out1,out2;
                //loop for all slices
                for (int i = 0; i < SpreadMax; i++)
                {
                    //read data from inputs
                    FMyValueInput1.GetValue4D(i, out a1, out a2, out a3, out a4);
                    FMyValueInput2.GetValue4D(i, out b1, out b2, out b3, out b4);
                    FMyValueInput3.GetValue4D(i, out c1, out c2, out c3, out c4);
                    FMyValueInput4.GetValue4D(i, out d1, out d2, out d3, out d4);

                    a = new Vector4D(a1, a2, a3, a4);
                    b = new Vector4D(b1, b2, b3, b4);
                    c = new Vector4D(c1, c2, c3, c4);
                    d = new Vector4D(d1, d2, d3, d4);


                    // http://en.wikipedia.org/wiki/Octonion
                    // following the Cayley-Dickinson construction
                    // (a,b)(c,d) = (ac - db * ,da + bc * )
                    // where x * is the conjugate of x

                    out1 = new Vector4D(b1,-b2,-b3,-b4);
                    out2 = new Vector4D(c1,-c2,-c3,-c4);

                    out1 = a * c - d * out1;
                    out2 = d * a + b * out2;
                    
                    //write data to outputs

                    FMyValueOutput1.SetValue4D(i, out1.x, out1.y, out1.z, out1.w);
                    FMyValueOutput2.SetValue4D(i, out2.x, out2.y, out2.z, out2.w);
                    
                }
            }

        #endregion mainloop
        }
    }

    //class definition
    public class Divide : IPlugin, IDisposable
    {
        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //input pin declaration
        private IValueIn FMyValueInput1;
        private IValueIn FMyValueInput2;
        private IValueIn FMyValueInput3;
        private IValueIn FMyValueInput4;

        //output pin declaration
        private IValueOut FMyValueOutput1;
        private IValueOut FMyValueOutput2;

        #endregion field declaration

        #region constructor/destructor

        public Divide()
        {
            //the nodes constructor
            //nothing to declare for this node
        }

        // Implementing IDisposable's Dispose method.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!FDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                FHost.Log(TLogType.Debug, "PluginTemplate is being deleted");

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~Divide()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
        #endregion constructor/destructor

        #region node name and infos

        //provide node infos 
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();

                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "Divide";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "Octonion";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";

                    //the nodes author: your sign
                    FPluginInfo.Author = "fibo";
                    //describe the nodes function
                    FPluginInfo.Help = "Octonion division";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";

                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }
                return FPluginInfo;
            }
        }

        public bool AutoEvaluate
        {
            //return true if this node needs to calculate every frame even if nobody asks for its output
            get { return false; }
        }

        #endregion node name and infos

        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            //create inputs
            FHost.CreateValueInput("Input 1 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput1);
            FMyValueInput1.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 1, 0, 0, 0, false, false, false);

            FHost.CreateValueInput("Input 2 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput2);
            FMyValueInput2.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, 0, false, false, false);

            FHost.CreateValueInput("Input 3 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput3);
            FMyValueInput3.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 1, 0, 0, 0, false, false, false);

            FHost.CreateValueInput("Input 4 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput4);
            FMyValueInput4.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, 0, false, false, false);

            //create outputs	    	

            FHost.CreateValueOutput("Output 1 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutput1);
            FMyValueOutput1.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, 0, false, false, false);

            FHost.CreateValueOutput("Output 2 ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutput2);
            FMyValueOutput2.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, 0, false, false, false);

        }

        #endregion pin creation

        #region mainloop

        public void Configurate(IPluginConfig Input)
        {
            //nothing to configure in this plugin
            //only used in conjunction with inputs of type cmpdConfigurate
        }

        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {
            //if any of the inputs has changed
            //recompute the outputs
            if (FMyValueInput1.PinIsChanged || FMyValueInput2.PinIsChanged || FMyValueInput3.PinIsChanged || FMyValueInput4.PinIsChanged)
            {
                //first set slicecounts for all outputs
                //the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
                FMyValueOutput1.SliceCount = SpreadMax;
                FMyValueOutput2.SliceCount = SpreadMax;

                //the variables to fill with the input data
                double a1, a2, a3, a4;
                double b1, b2, b3, b4;
                double c1, c2, c3, c4;
                double d1, d2, d3, d4;

                Vector4D a, b, c, d, out1, out2;
                //loop for all slices
                for (int i = 0; i < SpreadMax; i++)
                {
                    //read data from inputs
                    FMyValueInput1.GetValue4D(i, out a1, out a2, out a3, out a4);
                    FMyValueInput2.GetValue4D(i, out b1, out b2, out b3, out b4);
                    FMyValueInput3.GetValue4D(i, out c1, out c2, out c3, out c4);
                    FMyValueInput4.GetValue4D(i, out d1, out d2, out d3, out d4);

                    a = new Vector4D(a1, a2, a3, a4);
                    b = new Vector4D(b1, b2, b3, b4);
                    // create c and d already inverted, then just use the same algorithm as multiplication
                    c = new Vector4D(c1, -c2, -c3, -c4) / (c1 * c1 + c2 * c2 + c3 * c3 + c4 * c4 + d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4);
                    d = new Vector4D(-d1, -d2, -d3, -d4) / (c1 * c1 + c2 * c2 + c3 * c3 + c4 * c4 + d1 * d1 + d2 * d2 + d3 * d3 + d4 * d4);

                    out1 = new Vector4D(b.x, -b.y, -b.z, -b.w);
                    out2 = new Vector4D(c.x, -c.y, -c.z, -c.w);

                    out1 = a * c - d * out1;
                    out2 = d * a + b * out2;

                    //write data to outputs

                    FMyValueOutput1.SetValue4D(i, out1.x, out1.y, out1.z, out1.w);
                    FMyValueOutput2.SetValue4D(i, out2.x, out2.y, out2.z, out2.w);

                }
            }

        #endregion mainloop
        }
    }
}

