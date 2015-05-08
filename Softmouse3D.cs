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

							Softmouse3DButtons buttons=(Softmouse3DButtons)(eventData.ButtonState&0x000f3f00);

							#region Add movement to previously pressed buttons and handle if limit a exceeded
							for(int b=0; b<10; b++)
							{
								if(last.ButtonsStates[b].Clicks==0) continue;

								// sum up movement
								last.ButtonsStates[i].MoveX+=eventData.X;
								last.ButtonsStates[i].MoveY+=eventData.Y;

								if(Math.Abs(last.ButtonsStates[i].MoveX)>MaximumDoubleClickMovement||
									Math.Abs(last.ButtonsStates[i].MoveY)>MaximumDoubleClickMovement||
									(ticks-last.ButtonsStates[i].Down)/10000>MaximumDoubleClickTime)
								{
									last.ButtonsStates[i].Clicks=0;
									last.ButtonsStates[i].Down=0;
									last.ButtonsStates[i].MoveX=0;
									last.ButtonsStates[i].MoveY=0;
								}
							}
							#endregion

							Softmouse3DButtons diffButtons=(buttons^last.Buttons);

							// Order of click and double-click events in WindowsForms (want to provide the same here)
							// 1. Down (Clicks: 1)
							// 2. Click (Clicks: 1)
							// 3. Up (Clicks: 1)
							// ---- end of single click
							// 4. Down (Clicks: 2)
							// 5. DoubleClick (Clicks: 2)
							// 6. Up (Clicks: 1)
							// ? Should we support more Clicks? Is it enough to add up the clicks and raise DoubleClick again, or do we need an extra event?
							// DoubleClick Time is form 1. down to next down. So we can reset the time last.ButtonState[?].Down time value on each button down.

							if(diffButtons!=0) // any buttons changed?
							{
								// First the shift buttons down to support very fast button combination
								if((buttons&Softmouse3DButtons.SL)!=0&&(last.Buttons&Softmouse3DButtons.SL)==0)
								{
								}

								if((buttons&Softmouse3DButtons.SR)!=0&&(last.Buttons&Softmouse3DButtons.SR)==0)
								{
								}

								// A and D Buttons
								for(int b=0; b<8; b++)
								{
									Softmouse3DButtons button=buttonFilter[b];

									if((diffButtons&button)!=0)
									{
										if((buttons&button)!=0) // pressed
										{
											////if(dEvt!=null) dEvt(this, new Softmouse3DButtonsEventArgs(inputHeader.header.hDevice, button, 

											//if(last.ButtonsStates[b].Clicks>0) last.ButtonsStates[i].Clicks++; // Add another click
											//else
											//{
											//	last.ButtonsStates[i].Clicks=1;
											//	last.ButtonsStates[i].Down=ticks;
											//	last.ButtonsStates[i].MoveX=0;
											//	last.ButtonsStates[i].MoveY=0;
											//}
										}
										else // released
										{

										}
									}
								}

								// Last, but not least, the shift buttons releases
								if((buttons&Softmouse3DButtons.SL)==0&&(last.Buttons&Softmouse3DButtons.SL)!=0)
								{
								}

								if((buttons&Softmouse3DButtons.SR)==0&&(last.Buttons&Softmouse3DButtons.SR)!=0)
								{
								}

								// save new last buttons
								last.Buttons=buttons;
							}
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
