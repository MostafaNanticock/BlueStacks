using BlueStacks.hyperDroid.Common;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace BlueStacks.hyperDroid.VideoCapture
{
	public class DeviceEnumerator : IDisposable
	{
		private IMoniker m_Moniker;

		private string m_FriendlyName;

		public string FriendlyName
		{
			get
			{
				return this.m_FriendlyName;
			}
		}

		public IMoniker Moniker
		{
			get
			{
				return this.m_Moniker;
			}
		}

		public Guid ClassGUID
		{
			get
			{
				Guid result = default(Guid);
				this.m_Moniker.GetClassID(out result);
				return result;
			}
		}

		public string GetDisplayName
		{
			get
			{
				string result = null;
				try
				{
					this.m_Moniker.GetDisplayName((IBindCtx)null, (IMoniker)null, out result);
					return result;
				}
				catch (Exception ex)
				{
					Logger.Error(ex.ToString());
					return result;
				}
			}
		}

		public static List<DeviceEnumerator> ListDevices(Guid filterType)
		{
			ICreateDevEnum createDevEnum = null;
			List<DeviceEnumerator> list = new List<DeviceEnumerator>();
			DeviceEnumerator deviceEnumerator = null;
			createDevEnum = (ICreateDevEnum)new CreateDevEnum();
			IEnumMoniker enumMoniker = default(IEnumMoniker);
			ErrorHandler errorHandler = (ErrorHandler)createDevEnum.CreateClassEnumerator(filterType, out enumMoniker, 0);
			if (enumMoniker != null)
			{
				try
				{
					IMoniker[] array = null;
					try
					{
						while (true)
						{
							array = new IMoniker[1];
							if (enumMoniker.Next(1, array, IntPtr.Zero) != 0)
							{
								break;
							}
							deviceEnumerator = new DeviceEnumerator();
							deviceEnumerator.m_Moniker = array[0];
							deviceEnumerator.m_FriendlyName = deviceEnumerator.getProperty("FriendlyName");
							string property = deviceEnumerator.getProperty("DevicePath");
							if (property.Contains("\\usb#vid"))
							{
								list.Add(deviceEnumerator);
								Logger.Info("Camera device {0}", deviceEnumerator.m_FriendlyName);
							}
						}
						return list;
					}
					catch (Exception ex)
					{
						if (array != null)
						{
							Marshal.ReleaseComObject(array[0]);
						}
						list = null;
						Logger.Error("Failed to enumerate Video input devices: {0}", ex.ToString());
						throw;
					}
				}
				finally
				{
					Marshal.ReleaseComObject(enumMoniker);
				}
			}
			return null;
		}

		public void Dispose()
		{
			if (this.m_Moniker != null)
			{
				Marshal.ReleaseComObject(this.m_Moniker);
				this.m_Moniker = null;
			}
			this.m_FriendlyName = null;
		}

		public string getProperty(string sProperty)
		{
			object obj = null;
			IPropertyBag propertyBag = null;
			string result = null;
			Guid gUID = typeof(IPropertyBag).GUID;
			try
			{
				this.m_Moniker.BindToStorage((IBindCtx)null, (IMoniker)null, ref gUID, out obj);
				propertyBag = (IPropertyBag)obj;
				object obj2 = default(object);
				ErrorHandler errorHandler = (ErrorHandler)propertyBag.Read(sProperty, out obj2, (IErrorLog)null);
				result = (string)obj2;
				return result;
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to fetch Property: {0}", sProperty);
				Logger.Error(ex.ToString());
				return result;
			}
			finally
			{
				obj = null;
				if (propertyBag != null)
				{
					Marshal.ReleaseComObject(propertyBag);
					propertyBag = null;
				}
			}
		}
	}
}
