using System.Runtime.InteropServices;

namespace HID.Softmouse3D
{
	/// <summary>
	/// Contains a dataset of a softmouse 3D raw input event.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public struct Softmouse3DEventData
	{
		/// <summary>
		/// Bitfield containing the buttons state. (also Report ID?)
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public uint ButtonState;

		/// <summary>
		/// The shift in x-direction.
		/// </summary>
		[MarshalAs(UnmanagedType.I1)]
		public sbyte X;

		/// <summary>
		/// The shift in y-direction.
		/// </summary>
		[MarshalAs(UnmanagedType.I1)]
		public sbyte Y;

		/// <summary>
		/// The shift/rotation in z-direction. (Wheel)
		/// </summary>
		[MarshalAs(UnmanagedType.I1)]
		public sbyte Z;

		/// <summary>
		/// Size of <see cref="Softmouse3DEventData"/> in bytes.
		/// </summary>
		public static readonly uint SIZE=(uint)Marshal.SizeOf(typeof(Softmouse3DEventData));
	}
}
