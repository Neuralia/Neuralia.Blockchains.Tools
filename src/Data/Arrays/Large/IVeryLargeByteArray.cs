using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Neuralia.Blockchains.Tools.Data.Arrays.Large {
	public interface IVeryLargeByteArray : IDisposableExtended {
		long Length { get; }

		byte this[long i] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}

		Task Clear();
		Task CopyTo(ByteArray dest, long srcOffset, int destOffset, int length);
		Task CopyFrom(ByteArray src, int srcOffset, long destOffset, int length);
	}
}