using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpSysManager.Core
{
	public class Thumbprint
	{
		byte[] bytes;
		string bytesStr;

		public unsafe Thumbprint(byte* bytes, int lenght)
			:this(InteropHelper.ToCSharpArray(bytes,lenght))
		{
		}
		public Thumbprint(string bytes)
		{
			if(bytes == null)
				throw new ArgumentNullException("bytes");
			bytesStr = Normalize(bytes);
		}

		private string Normalize(string bytes)
		{
			return String.Join("", bytes.Split(' ', '-'));
		}
		public Thumbprint(byte[] bytes)
		{
			if(bytes == null)
				throw new ArgumentNullException("bytes");
			this.bytes = bytes;
		}

		public byte[] Data
		{
			get
			{
				if(bytes == null)
				{
					bytes = Enumerable.Range(0, bytesStr.Length)
					 .Where(x => x % 2 == 0)
					 .Select(x => Convert.ToByte(bytesStr.Substring(x, 2), 16))
					 .ToArray();
				}
				return bytes;
			}
		}

		public override string ToString()
		{
			if(bytesStr == null)
			{
				bytesStr = Normalize(BitConverter.ToString(bytes));
			}
			return bytesStr;
		}
	}
}
