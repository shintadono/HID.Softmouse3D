using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Win32;
using Win32.HumanInterfaceDevices;
using Win32.RawInput;

namespace HID.Softmouse3D
{
	/// <summary>
	/// Class for handling <see cref="WM.INPUT">WM_INPUT</see> messages for softmouse 3D devices.
	/// </summary>
	/// <remarks>For a stateless version of this class see <see cref="Softmouse3DStateless"/>.</remarks>
	public class Softmouse3D
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
		/// Fired when a button is pressed down.
		/// </summary>
		public event EventHandler<Softmouse3DButtonsEventArgs> ButtonDown;

		/// <summary>
		/// Fired when a button is released.
		/// </summary>
		public event EventHandler<Softmouse3DButtonsEventArgs> ButtonUp;

		/// <summary>
		/// Fired when a button is clicked.
		/// </summary>
		public event EventHandler<Softmouse3DButtonsEventArgs> ButtonClick;

		/// <summary>
		/// Fired when a button is double clicked.
		/// </summary>
		public event EventHandler<Softmouse3DButtonsEventArgs> ButtonDoubleClick;

		/// <summary>
		/// Fired when the mouse is moved.
		/// </summary>
		public event EventHandler<Softmouse3DEventArgs> MouseMove;

		/// <summary>
		/// Fired when the mouse wheel is moved.
		/// </summary>
		public event EventHandler<Softmouse3DEventArgs> MouseWheel;

		struct ButtonState
		{
			public int MoveX, MoveY, Clicks;
			public long Down;
		}

		class State
		{
			public Softmouse3DButtons Buttons;
			public ButtonState D1, D2, D3, D4;
			public ButtonState A1, A2, A3, A4;
			public ButtonState SL, SR;
		}

		Dictionary<IntPtr, State> StatePreDevice=new Dictionary<IntPtr, State>();

		const int RAWHID_without_Data_Size=8;

		/// <summary>
		/// Processes a <see cref="WM.INPUT">WM_INPUT</see> messages and raises the proper events.
		/// </summary>
		/// <param name="lParam">The handle to the raw input dataset.</param>
		/// <param name="size">The size, in bytes, of the raw input dataset.</param>
		public void ProcessInput(IntPtr lParam, uint size)
		{
			// copy event handler
			EventHandler<Softmouse3DButtonsEventArgs> dEvt=ButtonDown;
			EventHandler<Softmouse3DButtonsEventArgs> uEvt=ButtonUp;
			EventHandler<Softmouse3DButtonsEventArgs> cEvt=ButtonClick;
			EventHandler<Softmouse3DButtonsEventArgs> dcEvt=ButtonDoubleClick;
			EventHandler<Softmouse3DEventArgs> mEvt=MouseMove;
			EventHandler<Softmouse3DEventArgs> wEvt=MouseWheel;

			if(dEvt!=null||uEvt!=null||cEvt!=null||dcEvt!=null||mEvt!=null||wEvt!=null) // no need to handle if nobody is listing
			{
				long ticks=DateTime.Now.Ticks;

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
						// Fill dictionary if missed
						if(!StatePreDevice.ContainsKey(inputHeader.header.hDevice))
							StatePreDevice.Add(inputHeader.header.hDevice, new State());

						State last=StatePreDevice[inputHeader.header.hDevice];

						for(int i=0; i<inputHeader.data.hid.dwCount; i++)
						{
							Softmouse3DEventData eventData=(Softmouse3DEventData)Marshal.PtrToStructure(buffer+inputHeaderSize+i*(int)Softmouse3DEventData.SIZE, typeof(Softmouse3DEventData));

							// TODO

							//last.D1.Down=ticks;
							//last.D1.MoveX=0;
							//last.D1.MoveY=0;
							//last.D1.Clicks=0;

							//evt(this, new Softmouse3DEventArgs(inputHeader.header.hDevice, eventData.X, eventData.Y, eventData.Z, (Softmouse3DButtons)eventData.ButtonState));
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
