using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProcessHacker.Native.Security.AccessControl;
using ProcessHacker.Native;
using ProcessHacker.Native.Api;
using ProcessHacker.Native.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Sockets;
using HttpSysManager.Core;
using HttpSysManager.Native;


namespace HttpSysManager.HttpAPI
{

	public class HttpAPIManager
	{
		public IEnumerable<UrlAcl> GetAclInfo()
		{
			using(new HttpInitializeScope())
			{
				UrlAcl urlAcl;
				int i = 0;
				do
				{
					urlAcl = UrlAcl.Get(i);
					if(urlAcl != null)
						yield return urlAcl;
					i++;
				} while(urlAcl != null);
			}
		}

		public void SetUrlAcl(string url, SecurityDescriptor securityDescriptor)
		{
			using(new HttpInitializeScope())
			{
				if(securityDescriptor == null)
				{
					RemoveUrlAcl(url);
					return;
				}
				var urlAcl = new UrlAcl();
				urlAcl.Prefix = url;
				urlAcl.SecurityDescriptor = securityDescriptor;
				urlAcl.Update();
			}
		}

		public void RemoveUrlAcl(string url)
		{
			using(new HttpInitializeScope())
			{
				UrlAcl urlAcl = new UrlAcl();
				urlAcl.Prefix = url;
				urlAcl.Delete();
			}
		}
		static protected void CreateBuffer(object initObject, out IntPtr ptr, out int lenght)
		{
			lenght = Marshal.SizeOf(initObject);
			ptr = Marshal.AllocHGlobal(lenght);
			Marshal.StructureToPtr(initObject, ptr, false);
		}


		public UrlAcl GetAclInfo(string url)
		{
			using(new HttpInitializeScope())
			{
				return UrlAcl.Get(url);
			}
		}

		public IEnumerable<SSLInfo> GetSSLInfos()
		{
			using(new HttpInitializeScope())
			{
				SSLInfo sslInfo;
				int i = 0;
				do
				{
					sslInfo = SSLInfo.Get(i);
					if(sslInfo != null)
						yield return sslInfo;
					i++;
				} while(sslInfo != null);
			}
		}

		public void SetSSLInfo(IPEndPoint endpoint, SSLInfo info)
		{
			using(new HttpInitializeScope())
			{
				if(info == null)
				{
					RemoveSSLInfo(endpoint);
				}
				else
				{
					info.Endpoint = endpoint;
					info.Update();
				}
			}
		}

		public void RemoveSSLInfo(IPEndPoint endpoint)
		{
			using(new HttpInitializeScope())
			{
				var info = new SSLInfo();
				info.Endpoint = endpoint;
				info.Delete();
			}
		}

		public SSLInfo GetSSLInfo(IPEndPoint endpoint)
		{
			using(new HttpInitializeScope())
			{
				return SSLInfo.Get(endpoint);
			}
		}
	}
}
