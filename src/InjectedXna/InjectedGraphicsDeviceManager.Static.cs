using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Extemory;
using Extemory.MemoryEdits;

namespace InjectedXna
{
    public partial class InjectedGraphicsDeviceManager
    {
        private static readonly Direct3DCreate9Delegate Direct3DCreate9Func = Direct3DCreate9Handler;
        private static readonly IDirect3D9CreateDevice IDirect3D9CreateDeviceFunc = IDirect3D9CreateDeviceHandler;
        private static readonly EndSceneDelegate EndSceneFunc = EndSceneHandler;
        private static readonly ResetDelegate ResetFunc = ResetHandler;

        private static bool _isGraphicsCreationHooked;
        private static IntPtr _iDirect3D9;
        private static GraphicsDevice _xnaGraphicsDevice;
        private static Detour<Direct3DCreate9Delegate> _d3DCreateDetour;
        private static Detour<IDirect3D9CreateDevice> _createDeviceDetour;
        private static Detour<EndSceneDelegate> _endSceneDetour;
        private static Detour<ResetDelegate> _resetDetour;

        public static event EventHandler<EndSceneEventArgs> EndScene;
        public static event EventHandler<EventArgs> XnaDeviceCreated;

        /// <summary>
        /// Hooks DirectX creation functions to intercept device creation to get the device pointer.
        /// Needs to be called on Process startup with process started suspended.
        /// </summary>
        public static void HookGraphicsDeviceCreation()
        {
            if (_isGraphicsCreationHooked) return;

            // Load d3d9.dll library to hook the necessary functions
            // NOTE: this is a potential resource leak as there is no matching FreeLibrary
            var hLib = LoadLibrary("d3d9.dll");
            if (hLib == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to load d3d9.dll");

            var pDirect3DCreate9 = GetProcAddress(hLib, "Direct3DCreate9");
            if (pDirect3DCreate9 == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to find proc address for Direct3DCreate9");

            _d3DCreateDetour = pDirect3DCreate9.DetourWith(Direct3DCreate9Func);

            _isGraphicsCreationHooked = true;
        }

        #region Hooking internals

        private static IntPtr Direct3DCreate9Handler(uint sdkVersion)
        {
            var pDirect3D9 = (IntPtr) _d3DCreateDetour.CallOriginal(sdkVersion);
            if (_iDirect3D9 == IntPtr.Zero)
            {
                // store the pointer and detour the CreateDevice function
                _iDirect3D9 = pDirect3D9;

                _createDeviceDetour = pDirect3D9.VTable(IDirect3D9VTable.CreateDevice)
                    .DetourWith(IDirect3D9CreateDeviceFunc);
            }
            return pDirect3D9;
        }

        private static bool _isXnaCreateDeviceCall;
        private static uint _preservedBehaviorFlags;
        private static uint IDirect3D9CreateDeviceHandler(IntPtr thisPtr, uint adapter, uint deviceType, IntPtr hFocusWindow, uint behaviorFlags, IntPtr pPresentationParameters, [Out] IntPtr ppReturnedDeviceInterface)
        {
            uint ret;
            if (!_isXnaCreateDeviceCall)
            {
                try
                {
                    var nativePresentationParameters = 
                        (NativePresentationParameters)Marshal.PtrToStructure(pPresentationParameters, typeof(NativePresentationParameters));
                    var presentationParameters = nativePresentationParameters.ToXnaPresentationParameters(hFocusWindow);

                    _preservedBehaviorFlags = behaviorFlags;
                    _isXnaCreateDeviceCall = true;

                    _xnaGraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.Reach,
                                                            presentationParameters);

                    var pComPtrField = _xnaGraphicsDevice.GetType().GetField("pComPtr", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (pComPtrField == null)
                        throw new Exception("Unable to get pComPtr field from XNA Graphics Device");

                    unsafe
                    {
                        var pComPtr = new IntPtr(Pointer.Unbox(pComPtrField.GetValue(_xnaGraphicsDevice)));
                        Marshal.WriteIntPtr(ppReturnedDeviceInterface, pComPtr);

                        _endSceneDetour = pComPtr.VTable(IDirect3DDevice9VTable.EndScene)
                            .DetourWith(EndSceneFunc);

                        _resetDetour = pComPtr.VTable(IDirect3DDevice9VTable.Reset)
                            .DetourWith(ResetFunc);
                    }
                    // TODO
                    OnCreateDevice();
                    ret = 0;
                }
                catch (Exception)
                {
                    // If we get an exception trying to create the XNA device, just call the original method and pass out the return
                    ret = (uint)_createDeviceDetour.CallOriginal(
                        thisPtr, adapter, deviceType, hFocusWindow, 
                        behaviorFlags, pPresentationParameters, ppReturnedDeviceInterface);
                }
            }
            else
            {
                // Now we're inside the XNA Device's call to CreateDevice - get our cached presentation parameters and add a required flag
                // TODO: check this process / flag
                var pp = (NativePresentationParameters)Marshal.PtrToStructure(pPresentationParameters, typeof(NativePresentationParameters));
                pp.Flags |= 0x1;
                Marshal.StructureToPtr(pp, pPresentationParameters, true);

                ret = (uint) _createDeviceDetour.CallOriginal(
                    thisPtr, adapter, deviceType, hFocusWindow,
                    _preservedBehaviorFlags, pPresentationParameters, ppReturnedDeviceInterface);
            }
            return ret;
        }

        private static void OnCreateDevice()
        {
            var handler = XnaDeviceCreated;
            if (handler != null)
                handler(null, EventArgs.Empty);
        }

        private static IntPtr EndSceneHandler(IntPtr pDevice)
        {
            // TODO
            var endScene = EndScene;
            if (endScene != null)
                endScene(null, new EndSceneEventArgs(pDevice));

            return (IntPtr) _endSceneDetour.CallOriginal(pDevice);
        }

        private static IntPtr ResetHandler(IntPtr pDevice, IntPtr pPresentationParameters)
        {
            // TODO

            return (IntPtr) _resetDetour.CallOriginal(pDevice, pPresentationParameters);
        }

        #endregion

        #region Imports and Delegates

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr Direct3DCreate9Delegate(uint sdkVersion);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate uint IDirect3D9CreateDevice(
            IntPtr thisPtr, uint adapter, uint deviceType, IntPtr hFocusWindow, uint behaviorFlags,
            IntPtr pPresentationParameters, [Out] IntPtr ppReturnedDeviceInterface);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr EndSceneDelegate(IntPtr pDevice);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr ResetDelegate(IntPtr pDevice, [In, Out] IntPtr pPresentationParameters);

        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", SetLastError = true)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName); 
        #endregion

        #region XNA Helpers

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativePresentationParameters
        {
            internal int BackBufferWidth; // 0x00
            internal int BackBufferHeight; // 0x04
            internal int BackBufferFormat; // 0x08
            internal int BackBufferCount; // 0x0C
            internal int MultiSampleType; // 0x10
            internal int MultiSampleQuality; // 0x14
            internal int SwapEffect; // 0x18
            internal IntPtr hDeviceWindow; // 0x1C
            [MarshalAs(UnmanagedType.Bool)]
            internal bool Windowed; // 0x20
            [MarshalAs(UnmanagedType.Bool)]
            internal bool EnableAutoDepthStencil; // 0x24
            internal int AutoDepthStencilFormat; // 0x28
            internal int Flags; // 0x2C
            internal int FullScreen_RefreshRateInHz; // 0x30
            internal int PresentationInterval; // 0x34

            internal PresentationParameters ToXnaPresentationParameters(IntPtr hFocusWindow)
            {
                var pp = new PresentationParameters
                {
                    BackBufferFormat = D3DFMTToSurfaceFormat(BackBufferFormat),
                    BackBufferHeight = BackBufferHeight,
                    BackBufferWidth = BackBufferWidth,
                    DepthStencilFormat = D3DFMTToDepthFormat(AutoDepthStencilFormat),
                    DeviceWindowHandle = hFocusWindow,
                    IsFullScreen = !Windowed,
                    MultiSampleCount = MultiSampleType,
                    PresentationInterval = D3DPresentIntervalToXnaPresentInterval(PresentationInterval),
                    RenderTargetUsage = RenderTargetUsage.PlatformContents
                };

                return pp;
            }

            private static PresentInterval D3DPresentIntervalToXnaPresentInterval(int presentationInterval)
            {
                PresentInterval val;
                if (!Enum.TryParse(presentationInterval.ToString(), out val))
                    return PresentInterval.Default;
                return val;
            }

            // XNA FAILS WITH ENUMS
            private static DepthFormat D3DFMTToDepthFormat(int native)
            {
                switch (native)
                {
                    case 80: // _D16
                        return DepthFormat.Depth16;
                    case 75: // _D24S8
                        return DepthFormat.Depth24Stencil8;
                    case 77: // _D24X8
                        return DepthFormat.Depth24;
                    default:
                        return DepthFormat.None;
                }
            }

            // XNA FAILS WITH ENUMS
            // TODO: add MAKEFOURCC surfaces
            private static SurfaceFormat D3DFMTToSurfaceFormat(int native)
            {
                switch (native)
                {
                    case 0x15:
                        return SurfaceFormat.Color;
                    case 0x17:
                        return SurfaceFormat.Bgr565;
                    case 0x19:
                        return SurfaceFormat.Bgra5551;
                    case 0x1A:
                        return SurfaceFormat.Bgra4444;
                    case 0x31545844:
                        return SurfaceFormat.Dxt1;
                    case 0x33545844:
                        return SurfaceFormat.Dxt3;
                    case 0x35545844:
                        return SurfaceFormat.Dxt5;
                    case 60:
                        return SurfaceFormat.NormalizedByte2;
                    case 0x3f:
                        return SurfaceFormat.NormalizedByte4;
                    case 0x1f:
                        return SurfaceFormat.Rgba1010102;
                    case 0x22:
                        return SurfaceFormat.Rg32;
                    case 0x24:
                        return SurfaceFormat.Rgba64;
                    case 0x1c:
                        return SurfaceFormat.Alpha8;
                    case 0x72:
                        return SurfaceFormat.Single;
                    case 0x73:
                        return SurfaceFormat.Vector2;
                    case 0x74:
                        return SurfaceFormat.Vector4;
                    case 0x6f:
                        return SurfaceFormat.HalfSingle;
                    case 0x70:
                        return SurfaceFormat.HalfVector2;
                    case 0x71:
                        return SurfaceFormat.HalfVector4;
                }
                return 0;
            }
        }

        #endregion

        private static GraphicsDevice GetXnaDevice()
        {
            return _xnaGraphicsDevice;
        }
    }

    public static class IDirect3D9VTable
    {
        public static readonly int CreateDevice = 16;
    }

    public static class IDirect3DDevice9VTable
    {
        public static readonly int Reset = 16;
        public static readonly int EndScene = 42;
    }
}