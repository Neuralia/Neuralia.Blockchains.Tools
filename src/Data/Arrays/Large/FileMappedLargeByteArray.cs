using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Neuralia.Blockchains.Tools.Data.Arrays.Large {
	
	/// <summary>
	/// this is a very large array mapped to a file on disk. used when RAM is highly limited
	/// </summary>
	public class FileMappedLargeByteArray : IVeryLargeByteArray {

		public long Length { get; } = 0;
		private readonly bool doubleBuffer;
		private readonly string path;
		private string file1;
		private string file2;
		private bool filesCreated = false;
		private Task doubleBufferTask = null;
		
		public FileMappedLargeByteArray(long length, bool doubleBuffer = true) {
			this.Length = length;
			this.doubleBuffer = doubleBuffer;

			this.path = Path.GetTempPath();

			this.file1 = Path.Combine(this.path, Path.GetTempFileName());

			if(doubleBuffer) {
				this.file2 = Path.Combine(this.path, Path.GetTempFileName());
			}
		}

		/// <summary>
		/// these would be too slow. better not use them
		/// </summary>
		/// <param name="i"></param>
		/// <exception cref="NotImplementedException"></exception>
		public byte this[long i] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				throw new NotImplementedException();
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				throw new NotImplementedException();
			}
		}
		
		public async Task Clear() {

			async Task ZeroFile(string file) {
				await using Stream fs = File.OpenWrite(file);

				byte[] buffer = new byte[8192];
					
				long remaining = this.Length;
				while (remaining > 0)
				{
					await fs.WriteAsync(buffer, 0, (int)Math.Min(remaining, buffer.Length)).ConfigureAwait(false);
					remaining -= buffer.Length;
				}
			}
			if(this.filesCreated) {
				if(this.doubleBuffer) {
					this.doubleBufferTask?.Wait();
					// swap the buffers
					var temp = this.file2;
					this.file1 = this.file2;
					this.file2 = temp;

					this.doubleBufferTask = Task.Run(() => ZeroFile(this.file2)).ContinueWith(t => {
						this.doubleBufferTask = null;
					});
				} else {
					await ZeroFile(this.file1).ConfigureAwait(false);
				}
			}
		}

		public async Task CopyTo(ByteArray dest, long srcOffset, int destOffset, int length) {

			if(!this.filesCreated) {
				await this.CreateFiles().ConfigureAwait(false);
			}

			await using(Stream fs = File.OpenRead(this.file1)) {
				await using(BufferedStream bs = new BufferedStream(fs)) {

					fs.Seek(srcOffset, SeekOrigin.Begin);

					long bytesLeft = length;
					int offset = 0;

					while(bytesLeft > 0) {
						// Read may return anything from 0 to numBytesToRead.
						int bytesRead = await bs.ReadAsync(dest.Bytes, dest.Offset + destOffset + offset, (int) Math.Min(bytesLeft, 4096)).ConfigureAwait(false);

						// The end of the file is reached.
						if(bytesRead == 0) {
							break;
						}

						bytesLeft -= bytesRead;
						offset += bytesRead;
					}
				}
			}
		}
		
		public async Task CopyFrom(ByteArray src, int srcOffset, long destOffset, int length) {
			
			if(!this.filesCreated) {
				await this.CreateFiles().ConfigureAwait(false);
			}

			await using(Stream fs = File.OpenWrite(this.file1)) {

				fs.Seek(destOffset, SeekOrigin.Begin);
				await fs.WriteAsync(src.Bytes, src.Offset + srcOffset, length).ConfigureAwait(false);
			}
		}

		private async Task CreateFiles() {

			await this.CreateEmptyFile(this.file1).ConfigureAwait(false);

			if(this.doubleBuffer) {
				this.doubleBufferTask = Task.Run(() => this.CreateEmptyFile(this.file2)).ContinueWith(t => {
					this.doubleBufferTask = null;
				});
			}

			this.filesCreated = true;
		}
		/// <summary>
		/// create an empty file
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private async Task CreateEmptyFile(string fileName) {
			if(File.Exists(fileName)) {
				File.Delete(fileName);
			}
			await using(Stream fs = File.OpenWrite(fileName)) {

				fs.Seek(this.Length - 1, SeekOrigin.Begin);
				await fs.WriteAsync(new byte[] {0}, 0, 1).ConfigureAwait(false);
			}
		}
		
	#region Dispose

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {

				try {
					if(File.Exists(this.file1)) {
						File.Delete(this.file1);
					}
				} catch {
					
				}

				if(this.doubleBuffer) {
					try {
						if(File.Exists(this.file2)) {
							File.Delete(this.file2);
						}
					} catch {
					
					}
				}
			}

			this.IsDisposed = true;
		}

		~FileMappedLargeByteArray() {
			this.Dispose(false);
		}

	#endregion
	}
}