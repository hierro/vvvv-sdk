using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;
using vvvv.Utils;
using Un4seen.BassAsio;

namespace vvvv.Nodes
{
    public class BassAsioOutputNode : IPlugin, IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "AudioOut";
                Info.Category = "BassAsio";
                Info.Version = "";
                Info.Help = "Output node for bass asio";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
            }
        }
        #endregion

        private IPluginHost FHost;

        private IStringIn FPinInDevice;
        private IValueIn FPinInChannels;
        private IValueIn FPinInVolumeOutput;
        private IValueIn FPinInActive;
        private IStringOut FPinErrorCode;

        private ASIOPROC myAsioProc;
        private int FDeviceIndex = -1;
        private List<int> FOutputHandled = new List<int>();

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.FHost.Log(TLogType.Message, "Audio Out, Set plugin Host Start");

            BassAsioUtils.LoadPlugins();

            this.FHost.CreateStringInput("Device", TSliceMode.Single, TPinVisibility.True, out this.FPinInDevice);
            this.FPinInDevice.SetSubType("", false);

            this.FHost.CreateValueInput("Is Active", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInActive);
            this.FPinInActive.SetSubType(0.0, 1.0, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Volume", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVolumeOutput);
            this.FPinInVolumeOutput.SetSubType(0, 1, 0.01, 1, false, false, false);

            this.FHost.CreateValueInput("Channels",1,null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInChannels);
            this.FPinInChannels.SetSubType(double.MinValue, double.MaxValue, 1, -1, false, false, true);

            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out this.FPinErrorCode);

            this.myAsioProc = new ASIOPROC(AsioCallback);
            this.FHost.Log(TLogType.Message, "Audio Out, Set plugin Host End");

        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {

        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            try
            {
                #region Channel and device
                if (this.FPinInChannels.PinIsChanged || this.FPinInDevice.PinIsChanged)
                {
                    this.FHost.Log(TLogType.Message, "Evaluate Start");
                    string device;
                    this.FPinInDevice.GetString(0, out device);

                    this.FDeviceIndex = BassAsioUtils.GetDeviceIndex(device);
                    
                    if (this.FDeviceIndex != -1)
                    {
                        bool result = BassAsio.BASS_ASIO_Init(this.FDeviceIndex);
                        if (result)
                        {
                            this.FHost.Log(TLogType.Message, "Msg: Device " + device + " initialized");
                        }
                        else
                        {
                            this.FHost.Log(TLogType.Error, "Error: Device " + device + " initialization failed");
                            this.FHost.Log(TLogType.Error, BassAsio.BASS_ASIO_ErrorGetCode().ToString());
                        }

                        BassAsio.BASS_ASIO_SetDevice(this.FDeviceIndex);

                    }

                    this.FOutputHandled.Clear();
                    BassAsio.BASS_ASIO_Stop();

                    BassAsio.BASS_ASIO_ChannelReset(false, -1, BASSASIOReset.BASS_ASIO_RESET_PAUSE | BASSASIOReset.BASS_ASIO_RESET_JOIN);

                    if (this.FDeviceIndex != -1 && this.FPinInChannels.IsConnected)
                    {
                        int asiooutindex = 0;
                        double dhandle;
                        for (int i = 0; i < this.FPinInChannels.SliceCount; i++)
                        {

                            this.FPinInChannels.GetValue(i, out dhandle);
                            int handle = Convert.ToInt32(dhandle);

                            if (handle != 0 && handle != -1)
                            {

                                //Check if the channel has its own handler
                                if (!BassAsioUtils.InputChannels.ContainsKey(handle))
                                {
                                    BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(handle);

                                    BassAsio.BASS_ASIO_ChannelEnable(false, asiooutindex, myAsioProc, new IntPtr(handle));
                                    if (info.chans == 1)
                                    {
                                        //No need to join on mono channels
                                        asiooutindex++;
                                    }
                                    else
                                    {
                                        for (int chan = 1; chan < info.chans; chan++)
                                        {
                                            bool join = BassAsio.BASS_ASIO_ChannelJoin(false, asiooutindex + 1, asiooutindex);
                                            if (!join)
                                            {
                                                this.FHost.Log(TLogType.Error, "Error: join failed");
                                                this.FHost.Log(TLogType.Error, BassAsio.BASS_ASIO_ErrorGetCode().ToString());
                                            }
                                        }

                                        BassAsio.BASS_ASIO_ChannelSetFormat(false, asiooutindex, BASSASIOFormat.BASS_ASIO_FORMAT_FLOAT);
                                        BassAsio.BASS_ASIO_ChannelSetRate(false, asiooutindex, (double)info.freq);
                                        BassAsio.BASS_ASIO_SetRate((double)info.freq);

                                        asiooutindex += info.chans;
                                    }
                                }
                                else
                                {
                                    BassAsioHandler handler = BassAsioUtils.InputChannels[handle];
                                    handler.SetMirror(asiooutindex);
                                    asiooutindex += 2;

                                    this.FOutputHandled.Add(asiooutindex);
                                    this.FOutputHandled.Add(asiooutindex + 1);
                                }
                            }
                        }

                        bool start = BassAsio.BASS_ASIO_Start(0);
                        if (!start)
                        {
                            this.FPinErrorCode.SetString(0, BassAsio.BASS_ASIO_ErrorGetCode().ToString());
                            this.FHost.Log(TLogType.Error, "Error: Start failed");
                            this.FHost.Log(TLogType.Error, BassAsio.BASS_ASIO_ErrorGetCode().ToString());
                        }
                        else
                        {
                            this.FHost.Log(TLogType.Message, "DEBUG: Start Success");
                            this.FPinErrorCode.SetString(0, "OK");
                        }

                        UpdateChannels();
                    }
                    this.FHost.Log(TLogType.Message, "Evaluate End");
                }
                #endregion

                #region Volume
                if (this.FPinInVolumeOutput.PinIsChanged)
                {
                    if (this.FDeviceIndex != -1 || this.FPinInVolumeOutput.SliceCount > 0)
                    {
                        BASS_ASIO_INFO deviceinfo = BassAsio.BASS_ASIO_GetInfo();

                        int current = 0;
                        for (int i = 0; i < deviceinfo.outputs; i++)
                        {
                            double vol;

                            this.FPinInVolumeOutput.GetValue(current, out vol);
                            BassAsio.BASS_ASIO_ChannelSetVolume(false, i, (float)vol);

                            //Bin for the channels
                            current++;
                            if (current == this.FPinInVolumeOutput.SliceCount)
                            {
                                current = 0;
                            }
                        }
                    }
                }
                #endregion

                #region Is Active Pin
                if (this.FPinInActive.PinIsChanged)
                {
                    UpdateChannels();
                }
                #endregion

            }
            catch (Exception ex)
            {
                this.FHost.Log(TLogType.Error, ex.Message);
                this.FHost.Log(TLogType.Error,ex.StackTrace);
            }
        }
        #endregion

        #region Asio Cal back
        private int AsioCallback(bool input, int channel, IntPtr buffer, int length, IntPtr user)
        {
            try
            {
                //We deal only with outputs here
                //And if the channel has it's own handler, we ignore it
                if (!this.FOutputHandled.Contains(channel) && !input)
                {
                    int _decLength;

                    BASSASIOActive _status = BassAsio.BASS_ASIO_ChannelIsActive(false, channel);
                    //BASSActive _status = Bass.BASS_ChannelIsActive(user.ToInt32());
                    // now we evaluate the status...
                    if (_status != BASSASIOActive.BASS_ASIO_ACTIVE_ENABLED)
                    {
                        //BassAsio.BASS_ASIO_ChannelPause(false, channel);
                        return 0;
                    }
                    else
                    {
                        //BassAsio.BASS_ASIO_ChannelReset(false, channel, BASSASIOReset.BASS_ASIO_RESET_PAUSE);
                        _decLength = Bass.BASS_ChannelGetData(user.ToInt32(), buffer, length);

                        if (_decLength < 0)
                            _decLength = 0;
                        return _decLength;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                this.FHost.Log(TLogType.Error, ex.Message);
                return 0;
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            BassAsio.BASS_ASIO_Stop();
            BassAsio.BASS_ASIO_ChannelReset(false, -1, BASSASIOReset.BASS_ASIO_RESET_PAUSE | BASSASIOReset.BASS_ASIO_RESET_JOIN);
        }
        #endregion

        #region Update Channels
        private void UpdateChannels()
        {
            if (this.FDeviceIndex != -1 || this.FPinInVolumeOutput.SliceCount > 0)
            {
                BASS_ASIO_INFO deviceinfo = BassAsio.BASS_ASIO_GetInfo();

                int current = 0;
                for (int i = 0; i < deviceinfo.outputs; i++)
                {
                    double active;

                    this.FPinInActive.GetValue(current, out active);

                    if (active == 1)
                    {
                        BassAsio.BASS_ASIO_ChannelReset(false, i, BASSASIOReset.BASS_ASIO_RESET_PAUSE);
                    }
                    else
                    {
                        BassAsio.BASS_ASIO_ChannelPause(false, i);
                    }
                    //Bin for the channels
                    current++;
                    if (current == this.FPinInActive.SliceCount)
                    {
                        current = 0;
                    }
                }
            }
        }
        #endregion
    }
}
