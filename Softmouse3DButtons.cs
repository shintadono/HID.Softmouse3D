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

		/// <summary>
		/// Button A1 pressed while SL is pressed. (North)
		/// </summary>
		SLA1=0x01000000,

		/// <summary>
		/// Button A2 pressed while SL is pressed. (West)
		/// </summary>
		SLA2=0x02000000,

		/// <summary>
		/// Button A3 pressed while SL is pressed. (South)
		/// </summary>
		SLA3=0x08000000,

		/// <summary>
		/// Button A4 pressed while SL is pressed. (East)
		/// </summary>
		SLA4=0x04000000,

		/// <summary>
		/// Button A1 pressed while SR is pressed. (North)
		/// </summary>
		SRA1=0x00100000,

		/// <summary>
		/// Button A2 pressed while SR is pressed. (West)
		/// </summary>
		SRA2=0x00200000,

		/// <summary>
		/// Button A3 pressed while SR is pressed. (South)
		/// </summary>
		SRA3=0x00800000,

		/// <summary>
		/// Button A4 pressed while SR is pressed. (East)
		/// </summary>
		SRA4=0x00400000,

		/// <summary>
		/// Button A1 pressed while SL and SR is pressed. (North)
		/// </summary>
		SLRA1=0x10000000,

		/// <summary>
		/// Button A2 pressed while SL and SR is pressed. (West)
		/// </summary>
		SLRA2=0x20000000,

		/// <summary>
		/// Button A3 pressed while SL and SR is pressed. (South)
		/// </summary>
		SLRA3=0x80000000,

		/// <summary>
		/// Button A4 pressed while SL and SR is pressed. (East)
		/// </summary>
		SLRA4=0x40000000,
	}
}
