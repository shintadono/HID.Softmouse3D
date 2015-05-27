using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Win32;
using Win32.IO.HumanInterfaceDevices;
using Win32.IO.RawInput;

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

		#region Events
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
		/// Fired when a button is double-clicked.
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
		#endregion

		#region Limits
		/// <summary>
		/// Gets and sets the maximum movement, that must not exceed while clicking for
		/// two clicks to be considered a double-click. Both directions (x and y) share
		/// this value. Default: 5.
		/// </summary>
		public int MaximumDoubleClickMovement { get; set; }

		/// <summary>
		/// Gets the maximum number of milliseconds allowed between clicks for a
		/// double-click. Default: 250 ms.
		/// </summary>
		public int MaximumDoubleClickTime { get; set; }
		#endregion

		/// <summary>
		/// Creates and initializes an instance of this class.
		/// </summary>
		public Softmouse3D()
		{
			MaximumDoubleClickMovement=5;
			MaximumDoubleClickTime=250;
		}

		#region State
		enum Button
		{
			A1=0,
			A2=1,
			A3=2,
			A4=3,
			D1=4,
			D2=5,
			D3=6,
			D4=7,
			SL=8,
			SR=9,
		}

		static readonly Softmouse3DButtons[] buttonFilter=new Softmouse3DButtons[]
		{
			Softmouse3DButtons.A1, Softmouse3DButtons.A2, Softmouse3DButtons.A3, Softmouse3DButtons.A4,
			Softmouse3DButtons.D1, Softmouse3DButtons.D2, Softmouse3DButtons.D3, Softmouse3DButtons.D4,
			Softmouse3DButtons.SL, Softmouse3DButtons.SR
		};

		struct ButtonState
		{
			public int MoveX, MoveY, Clicks;
			public long Down;
		}

		class State
		{
			public Softmouse3DButtons Buttons;
			public ButtonState[] ButtonsStates=new ButtonState[10];
		}
		#endregion

		Dictionary<IntPtr, State> StatePreDevice=new Dictionary<IntPtr, State>();

		const int RAWHID_without_Data_Size=8;

		/// <summary>
		/// Processes a <see cref="WM.INPUT">WM_INPUT</see> messages and fires the proper events.
		/// </summary>
		/// <param name="lParam">The handle to the raw input dataset.</param>
		/// <param name="size">The size, in bytes, of the raw input dataset.</param>
		public void ProcessInput(IntPtr lParam, uint size)
		{
			// Copy event handler
			EventHandler<Softmouse3DButtonsEventArgs> dEvt=ButtonDown, uEvt=ButtonUp, cEvt=ButtonClick, dcEvt=ButtonDoubleClick;
			EventHandler<Softmouse3DEventArgs> mEvt=MouseMove, wEvt=MouseWheel;

			if(dEvt==null&&uEvt==null&&cEvt==null&&dcEvt==null&&mEvt==null&&wEvt==null) return; // No need to handle if nobody is listing

			long currentTicks=DateTime.Now.Ticks;

			IntPtr buffer=Marshal.AllocHGlobal((int)size);
			uint dwSize=size;

			try
			{
				// Get the raw data set
				uint err=RawInput.GetRawInputData(lParam, RID.INPUT, buffer, ref dwSize, RAWINPUTHEADER.SIZE);
				if(err==uint.MaxValue) throw new Exception(string.Format("Error getting WM_INPUT data. (Error code: 0x{0:X8})", WinKernel.GetLastError()));

				// Filter raw input (header + record number and size)
				RAWINPUT inputHeader=(RAWINPUT)Marshal.PtrToStructure(buffer, typeof(RAWINPUT));
				int inputHeaderSize=(int)RAWINPUTHEADER.SIZE+RAWHID_without_Data_Size;

				if(inputHeader.header.dwSize<inputHeaderSize+Softmouse3DEventData.SIZE||inputHeader.data.hid.dwSizeHid!=Softmouse3DEventData.SIZE) return; // No enough or invalid data to process

				// Fill dictionary if we don't have a device state, yet
				if(!StatePreDevice.ContainsKey(inputHeader.header.hDevice)) StatePreDevice.Add(inputHeader.header.hDevice, new State());

				// Get last device state
				State last=StatePreDevice[inputHeader.header.hDevice];

				// Handle all data records
				for(int i=0; i<inputHeader.data.hid.dwCount; i++)
				{
					// Get a data record from raw data set
					Softmouse3DEventData eventData=(Softmouse3DEventData)Marshal.PtrToStructure(buffer+inputHeaderSize+i*(int)Softmouse3DEventData.SIZE, typeof(Softmouse3DEventData));

					// Get the buttons, filter out other possible stuff and merge SL+Ax, SL+Ax and SL+SR+Ax into Ax buttons. We will handle SL and SR as regular buttons.
					Softmouse3DButtons buttons=(Softmouse3DButtons)(
						(eventData.ButtonState&0x000f3f00)|
						((eventData.ButtonState&0x00f00000)>>4)|
						((eventData.ButtonState&0x0f000000)>>8)|
						((eventData.ButtonState&0xf0000000)>>12));

					#region Handle movement
					// Fire X-Y move event
					if(mEvt!=null&&(eventData.X!=0||eventData.Y!=0)) mEvt(this, new Softmouse3DEventArgs(inputHeader.header.hDevice, eventData.X, eventData.Y, 0, buttons));

					// Fire Z-Wheel move event
					if(wEvt!=null&&eventData.Z!=0) wEvt(this, new Softmouse3DEventArgs(inputHeader.header.hDevice, 0, 0, eventData.Z, buttons));
					#endregion

					#region Add movement to previously pressed buttons and handle if limit a exceeded
					for(int b=0; b<10; b++) // for all buttons
					{
						if(last.ButtonsStates[b].Clicks!=1) continue; // If not clicked yet, or already a double-click => continue

						// Sum up movement
						last.ButtonsStates[b].MoveX+=eventData.X;
						last.ButtonsStates[b].MoveY+=eventData.Y;

						if(Math.Abs(last.ButtonsStates[b].MoveX)>MaximumDoubleClickMovement||
							Math.Abs(last.ButtonsStates[b].MoveY)>MaximumDoubleClickMovement||
							(currentTicks-last.ButtonsStates[b].Down)/10000>MaximumDoubleClickTime)
						{
							last.ButtonsStates[b].Clicks=last.ButtonsStates[b].MoveX=last.ButtonsStates[b].MoveY=0;
							last.ButtonsStates[b].Down=0;
						}
					}
					#endregion

					#region Handle buttons
					Softmouse3DButtons diffButtons=(buttons^last.Buttons);
					if(diffButtons==0) continue; // any buttons changed?

					// Order of click and double-click events in WindowsForms (want to provide the same here)
					// 1. Down (Clicks: 1)
					// 2. Click (Clicks: 1)
					// 3. Up (Clicks: 1)
					// ---- end of single click
					// 4. Down (Clicks: 2)
					// 5. DoubleClick (Clicks: 2)
					// 6. Up (Clicks: 1)
					// Next click starts again at 1. Triple-click & Co. not support (for now)

					// First the shift buttons down to support very fast button combination
					if((buttons&Softmouse3DButtons.SL)!=0&&(last.Buttons&Softmouse3DButtons.SL)==0)
					{
						if(last.ButtonsStates[(int)Button.SL].Clicks==0) // no previous clicks
						{
							last.ButtonsStates[(int)Button.SL].Down=currentTicks;
							last.ButtonsStates[(int)Button.SL].MoveX=last.ButtonsStates[(int)Button.SL].MoveY=0;
						}

						last.ButtonsStates[(int)Button.SL].Clicks++;

						if(dEvt!=null) dEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, Softmouse3DButtons.SL, last.ButtonsStates[(int)Button.SL].Clicks, buttons));
					}

					if((buttons&Softmouse3DButtons.SR)!=0&&(last.Buttons&Softmouse3DButtons.SR)==0)
					{
						if(last.ButtonsStates[(int)Button.SR].Clicks==0) // no previous clicks
						{
							last.ButtonsStates[(int)Button.SR].Down=currentTicks;
							last.ButtonsStates[(int)Button.SR].MoveX=last.ButtonsStates[(int)Button.SR].MoveY=0;
						}

						last.ButtonsStates[(int)Button.SR].Clicks++;

						if(dEvt!=null) dEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, Softmouse3DButtons.SR, last.ButtonsStates[(int)Button.SR].Clicks, buttons));
					}

					// Handle A and D buttons (down and up events)
					for(int b=0; b<8; b++)
					{
						Softmouse3DButtons button=buttonFilter[b];
						if((diffButtons&button)==0) continue; // Nothing new for this button

						if((buttons&button)!=0) // pressed
						{
							if(last.ButtonsStates[b].Clicks==0) // no previous clicks
							{
								last.ButtonsStates[b].Down=currentTicks;
								last.ButtonsStates[b].MoveX=last.ButtonsStates[b].MoveY=0;
							}

							last.ButtonsStates[b].Clicks++;

							if(dEvt!=null) dEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, button, last.ButtonsStates[b].Clicks, buttons));
						}
						else // released
						{
							if(last.ButtonsStates[b].Clicks<2) // Fire Click event
							{
								if(cEvt!=null) cEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, button, 1, buttons));
							}
							else // Fire DoubleClick event
							{
								if(dcEvt!=null) dcEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, button, 2, buttons));
								last.ButtonsStates[b].Clicks=0; // reset clicks
							}

							// Fire Up event
							if(uEvt!=null) uEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, button, 1, buttons)); // clicks 1 (as MS does it)
						}
					}

					// Last, but not least, the shift buttons releases
					if((buttons&Softmouse3DButtons.SL)==0&&(last.Buttons&Softmouse3DButtons.SL)!=0)
					{
						if(last.ButtonsStates[(int)Button.SL].Clicks<2) // Fire Click event
						{
							if(cEvt!=null) cEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, Softmouse3DButtons.SL, 1, buttons));
						}
						else // Fire DoubleClick event
						{
							if(dcEvt!=null) dcEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, Softmouse3DButtons.SL, 2, buttons));
							last.ButtonsStates[(int)Button.SL].Clicks=0; // reset clicks
						}

						// Fire Up event
						if(uEvt!=null) uEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, Softmouse3DButtons.SL, 1, buttons)); // clicks 1 (as MS does it)
					}

					if((buttons&Softmouse3DButtons.SR)==0&&(last.Buttons&Softmouse3DButtons.SR)!=0)
					{
						if(last.ButtonsStates[(int)Button.SR].Clicks<2) // Fire Click event
						{
							if(cEvt!=null) cEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, Softmouse3DButtons.SR, 1, buttons));
						}
						else // Fire DoubleClick event
						{
							if(dcEvt!=null) dcEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, Softmouse3DButtons.SR, 2, buttons));
							last.ButtonsStates[(int)Button.SR].Clicks=0; // reset clicks
						}

						// Fire Up event
						if(uEvt!=null) uEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, Softmouse3DButtons.SR, 1, buttons)); // clicks 1 (as MS does it)
					}

					// Save new last buttons
					last.Buttons=buttons;
					#endregion
				}
			}
			finally
			{
				Marshal.FreeHGlobal(buffer);
			}
		}
	}
}
