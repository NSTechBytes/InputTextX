using InputTextX;
using Rainmeter;
using static InputTextX.InputOverlay;
using System.Runtime.InteropServices;
using System;

public static class Plugin
{
    private static IntPtr lastStringPtr = IntPtr.Zero;
    [DllExport]
    public static void Initialize(ref IntPtr data, IntPtr rm)
    {
        Measure measure = new Measure();
        Rainmeter.API api = new Rainmeter.API(rm);
        measure.skinWindow = api.GetSkinWindow();
        data = GCHandle.ToIntPtr(GCHandle.Alloc(measure));
        if (api.ReadInt("Logging", 0) == 1)
            api.Log(API.LogType.Notice, "Plugin initialized.");
    }
    [DllExport]
    public static void Finalize(IntPtr data)
    {
        Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
        measure.Unload();
        GCHandle.FromIntPtr(data).Free();
        if (measure.skinWindow != IntPtr.Zero)
        {
            NativeMethods.EnableWindow(measure.skinWindow, true);
        }
        if (lastStringPtr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(lastStringPtr);
            lastStringPtr = IntPtr.Zero;
        }
    }
    [DllExport]
    public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
    {
        Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
        measure.Reload(new Rainmeter.API(rm), ref maxValue);
    }
    [DllExport]
    public static double Update(IntPtr data)
    {
        Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
        return measure.Update();
    }
    [DllExport]
    public static IntPtr GetString(IntPtr data)
    {
        Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
        string s = measure.GetString();
        if (lastStringPtr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(lastStringPtr);
            lastStringPtr = IntPtr.Zero;
        }
        lastStringPtr = Marshal.StringToHGlobalUni(s);
        return lastStringPtr;
    }
    [DllExport]
    public static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)] string args)
    {
        Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
        measure.ExecuteCommand(args);
    }
}