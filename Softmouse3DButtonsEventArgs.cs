using System;

namespace HID.Softmouse3D
{
	/// <summary>
	/// Arguments provided by the handler for a ButtonDown, ButtonUp, ButtonClick and ButtonDoubleClick event.
	/// </summary>
	public class Softmouse3DButtonsEventArgs : EventArgs
	{
		/// <summary>
		/// The handle of the device.
		/// </summary>
		public IntPtr hDevice;

		/// <summary>
		/// The button.
		/// </summary>
		public Softmouse3DButtons Button;

		/// <summary>
		/// The number of clicks for this button.
		/// </summary>
		public int Clicks;

		/// <summary>
		/// The state of all buttons.
		/// </summary>
		public Softmouse3DButtons ButtonStates;

		/// <summary>
		/// Constructs an empty instance.
		/// </summary>
		public Softmouse3DButtonsEventArgs() { }

		/// <summary>
		/// Constructs an instance with parameters set.
		/// </summary>
		/// <param name="hDevice">The handle of the device.</param>
		/// <param name="button">The button.</param>
		/// <param name="clicks">The number of clicks of the button.</param>
		/// <param name="buttons">The button states.</param>
		public Softmouse3DButtonsEventArgs(IntPtr hDevice, Softmouse3DButtons button, int clicks, Softmouse3DButtons buttons)
		{
			this.hDevice=hDevice;
			Button=button;
			Clicks=clicks;
			ButtonStates=buttons;
		}
	}
}
