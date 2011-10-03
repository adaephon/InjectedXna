using System;

namespace InjectedXna
{
    public class EndSceneEventArgs : EventArgs
    {
        public EndSceneEventArgs(IntPtr pDevice)
        {
            DevicePointer = pDevice;
        }

        protected IntPtr DevicePointer { get; private set; }
    }
}