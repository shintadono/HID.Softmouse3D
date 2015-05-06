using System;

namespace HID.Softmouse3D
{
	/// <summary>
	/// Arguments provided by the handler for a MouseMove or MoveWheel event.
	/// </summary>
	public class Softmouse3DEventArgs : EventArgs
	{
		/// <summary>
		/// The handle of the device.
		/// </summary>
		public IntPtr hDevice;

		/// <summary>
		/// The change in x direction.
		/// </summary>
		public int DeltaX;

		/// <summary>
		/// The change in y direction.
		/// </summary>
		public int DeltaY;

		/// <summary>
		/// The change in z direction. (Wheel)
		/// </summary>
		public int DeltaZ;

		/// <summary>
		/// The buttons.
		/// </summary>
		public Softmouse3DButtons Buttons;

		/// <summary>
		/// Constructs an empty instance.
		/// </summary>
		public Softmouse3DEventArgs() { }

		/// <summary>
		/// Constructs an instance with parameters set.
		/// </summary>
		/// <param name="hDevice">The handle of the device.</param>
		/// <param name="deltaX">The change in x direction.</param>
		/// <param name="deltaY">The change in y direction.</param>
		/// <param name="deltaZ">The change in z direction. (Wheel)</param>
		/// <param name="buttons">The button states.</param>
		public Softmouse3DEventArgs(IntPtr hDevice, int deltaX, int deltaY, int deltaZ, Softmouse3DButtons buttons)
		{
			this.hDevice=hDevice;
			DeltaX=deltaX;
			DeltaY=deltaY;
			DeltaZ=deltaZ;
			Buttons=buttons;
		}
	}
}
