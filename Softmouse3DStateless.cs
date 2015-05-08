using System;
using System.Runtime.InteropServices;
using Win32;
using Win32.IO.HumanInterfaceDevices;
using Win32.IO.RawInput;

namespace HID.Softmouse3D
{
	/// <summary>
	/// Class for handling <see cref="WM.INPUT">WM_INPUT</see> messages for softmouse 3D devices.
	/// </summary>
	/// <remarks>
	/// This is the stateless version of <see cref="Softmouse3D"/> and has only one event
	/// (<see cref="Softmouse3DStateless.Input"/>) which is raised for every data set from a softmouse 3D device.
	/// </remarks>
	public class Softmouse3DStateless
	{
		/// <summary>
		/// Returns whether a device is a compatible devices, or not.
		/// </summary>
		/// <param name="info">The <see cref="RID_DEVICE_INFO"/> describing the device.</param>
		/// <returns><b>true</b> if the device is compatible with the class; otherwise, <b>false</b>.</returns>
		public static bool IsSoftmouse3D(RID_DEVICE_INFO info)
		{
			return info.dwType==RIM_TYPE.HID&&info.hid.usUsage==HID_USAGE_GENERIC_DESKTOP.JOYSTICK&&
				info.hid.usUsagePage==HID_USAGE_PAGE.GENERIC_DESKTOP&&
				info.hid.dwVendorId==VendorID.GGS&&info.hid.dwProductId==ProductID.Softmouse3D;
		}

		/// <summary>
		/// Input event.
		/// </summary>
		public event EventHandler<Softmouse3DEventArgs> Input;

		const int RAWHID_without_Data_Size=8;

		/// <summary>
		/// Processes a <see cref="WM.INPUT">WM_INPUT</see> messages and raises the proper events.
		/// </summary>
		/// <param name="lParam">The handle to the raw input dataset.</param>
		/// <param name="size">The size, in bytes, of the raw input dataset.</param>
		public void ProcessInput(IntPtr lParam, uint size)
		{
			EventHandler<Softmouse3DEventArgs> evt=Input;
			if(evt!=null) // no need to handle if nobody is listing
			{
				IntPtr buffer=Marshal.AllocHGlobal((int)size);
				uint dwSize=size;

				try
				{
					uint err=RawInput.GetRawInputData(lParam, RID.INPUT, buffer, ref dwSize, RAWINPUTHEADER.SIZE);
					if(err==uint.MaxValue) throw new Exception(string.Format("Error getting WM_INPUT data. (Error code: 0x{0:X8})", WinKernel.GetLastError()));

					RAWINPUT inputHeader=(RAWINPUT)Marshal.PtrToStructure(buffer, typeof(RAWINPUT));
					int inputHeaderSize=(int)RAWINPUTHEADER.SIZE+RAWHID_without_Data_Size;

					if(inputHeader.header.dwSize>=inputHeaderSize+Softmouse3DEventData.SIZE&&
						inputHeader.data.hid.dwSizeHid==Softmouse3DEventData.SIZE)
					{
						for(int i=0; i<inputHeader.data.hid.dwCount; i++)
						{
							Softmouse3DEventData eventData=(Softmouse3DEventData)Marshal.PtrToStructure(buffer+inputHeaderSize+i*(int)Softmouse3DEventData.SIZE, typeof(Softmouse3DEventData));
							evt(this, new Softmouse3DEventArgs(inputHeader.header.hDevice, eventData.X, eventData.Y, eventData.Z, (Softmouse3DButtons)eventData.ButtonState));
						}
					}
				}
				finally
				{
					Marshal.FreeHGlobal(buffer);
				}
			}
		}
	}
}
