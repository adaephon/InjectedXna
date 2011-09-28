using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Extemory;
using Extemory.CustomMarshalling;

namespace WowXna
{
    public class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct CLRAssemblyInfo
        {
            [CustomMarshalAs(CustomUnmanagedType.LPWStr)]
            public string AssemblyPath;
            [CustomMarshalAs(CustomUnmanagedType.LPWStr)]
            public string TypeName;
            [CustomMarshalAs(CustomUnmanagedType.LPWStr)]
            public string MethodName;
            [CustomMarshalAs(CustomUnmanagedType.LPWStr)]
            public string Argument;
        }

        public static void Main(string[] args)
        {
            try
            {
                var info = new CLRAssemblyInfo
                               {
                                   Argument = string.Empty,
                                   AssemblyPath = Assembly.GetExecutingAssembly().Location,
                                   TypeName = "WowXna.Program",
                                   MethodName = "DllMain"
                               };

                var proc = new ProcessStartInfo(@"D:\Games\World of Warcraft\wow.exe").CreateProcessSuspended();
                var clrLauncher = proc.InjectLibrary("Meanas.dll");

                var hr = clrLauncher.CallExport("HostCLR");
                if (hr.ToInt32() != 0)
                    throw new Exception(string.Format("HostClr exited with value {0:X8}", hr.ToInt32()));

                hr = clrLauncher.CallExport("ExecuteInHostedCLR", info);
                if (hr.ToInt32() != 0)
                    throw new Exception(string.Format("ExecuteInHostedCLR exited with value {0:X8}", hr.ToInt32()));

                proc.Resume();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        public static int DllMain(string arg)
        {
            try
            {
                Core.Launch();
            }
            catch (Exception)
            {
                return 0xDEAD;
            }
            return 0;
        }
    }
}
