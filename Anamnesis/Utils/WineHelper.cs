// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Utils;

using Serilog;
using System;
using System.Runtime.InteropServices;
internal class WineHelper
	{
	private delegate IntPtr GetWineVersionDelegate();

	public static bool CheckIfWine()
		{
		IntPtr hntdll = GetModuleHandle("ntdll.dll");
		if (hntdll == IntPtr.Zero)
			{
			Log.Information("Not running on NT.");
			return false;
		}

		IntPtr pwine_get_version = GetProcAddress(hntdll, "wine_get_version");
		if (pwine_get_version != IntPtr.Zero)
			{
			var wine_get_version = (GetWineVersionDelegate)Marshal.GetDelegateForFunctionPointer(pwine_get_version, typeof(GetWineVersionDelegate));
			string version = wine_get_version.Invoke().ToString();
			Log.Information("Running on Wine... " + version);
			return true;
		}
		else
		{
			Log.Information("Did not detect Wine, but hntdll existed. " + hntdll);
			return false;
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

	[DllImport("ntdll.dll")]
	[UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
	private static extern IntPtr Wine_get_version();
}
