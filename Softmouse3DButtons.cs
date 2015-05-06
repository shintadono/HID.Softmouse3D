using System;

namespace HID.Softmouse3D
{
	/// <summary>
	/// Defines the buttons of a softmouse 3D device.
	/// </summary>
	/// <remarks>
	/// <code>
	///           ____
	///       __D2    D3__
	///   ___/     A1     \___
	///  /       A2  A4       \
	/// |     SL   A3   SR     |
	/// +-------D1____D4-------+
	/// </code>
	/// </remarks>
	[Flags]
	[CLSCompliant(false)]
	public enum Softmouse3DButtons : uint
	{
		/// <summary>
		/// No button pressed.
		/// </summary>
		NONE=0x00000000,

		/// <summary>
		/// Button D1 pressed. (Bottom-Left)
		/// </summary>
		D1=0x00000400,

		/// <summary>
		/// Button D2 pressed. (Top-Left)
		/// </summary>
		D2=0x00000100,

		/// <summary>
		/// Button D3 pressed. (Top-Right)
		/// </summary>
		D3=0x00000200,

		/// <summary>
		/// Button D4 pressed. (Bottom-Right)
		/// </summary>
		D4=0x00000800,

		/// <summary>
		/// Button Shilf-Left pressed.
		/// </summary>
		SL=0x00002000,

		/// <summary>
		/// Button Shilf-Right pressed.
		/// </summary>
		SR=0x00001000,

		/// <summary>
		/// Button A1 pressed. (North)
		/// </summary>
		A1=0x00010000,

		/// <summary>
		/// Button A2 pressed. (West)
		/// </summary>
		A2=0x00020000,

		/// <summary>
		/// Button A3 pressed. (South)
		/// </summary>
		A3=0x00080000,

		/// <summary>
		/// Button A4 pressed. (East)
		/// </summary>
		A4=0x00040000,
	}
}
