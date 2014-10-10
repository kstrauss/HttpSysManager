using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ProcessHacker.Native;

namespace HttpSysManager.Core
{
	public class InteropHelper
	{
		public static unsafe byte[] ToCSharpArray(byte* bytes, int lenght)
		{
			byte[] output = new byte[lenght];
			for(int i = 0 ; i < lenght ; i++)
				output[i] = (byte)bytes[i];
			return output;
		}

		public static unsafe LocalMemoryAlloc ToCArray(byte[] array)
		{
			var ptr = (byte*)Marshal.AllocHGlobal(array.Length).ToPointer();
			for(int i = 0 ; i < array.Length ; i++)
				ptr[i] = array[i];
			return new LocalMemoryAlloc(new IntPtr(ptr));
		}
	}
}
