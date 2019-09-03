using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neuralia.Blockchains.Tools.General {
	public class BitSequence {

		private int currentBitSize;

		private ulong data;
		private bool entriesChanged;

		public BitSequence(byte buffer, IEnumerable<Entry> entries) : this(buffer, sizeof(uint), entries) {
			this.data |= buffer;
			this.TotalByteSize = 1;
		}

		public BitSequence(short buffer, IEnumerable<Entry> entries) : this((ulong) buffer, sizeof(uint), entries) {
			this.data |= (ushort) buffer;
			this.TotalByteSize = sizeof(short);
		}

		public BitSequence(ushort buffer, IEnumerable<Entry> entries) : this(buffer, sizeof(uint), entries) {
			this.data |= buffer;
			this.TotalByteSize = sizeof(ushort);
		}

		public BitSequence(int buffer, IEnumerable<Entry> entries) : this((ulong) buffer, sizeof(uint), entries) {
			this.data |= (uint) buffer;
			this.TotalByteSize = sizeof(int);
		}

		public BitSequence(uint buffer, IEnumerable<Entry> entries) : this(buffer, sizeof(uint), entries) {
		}

		public BitSequence(ulong buffer, IEnumerable<Entry> entries) : this(buffer, sizeof(ulong), entries) {

		}

		public BitSequence(ulong buffer, int byteSize, IEnumerable<Entry> entries) {
			this.data = buffer;
			this.TotalByteSize = byteSize;
			this.Entries.AddRange(entries);
			this.entriesChanged = true;
		}

		public int TotalByteBitSize => this.TotalByteSize * 8;

		public List<Entry> Entries { get; } = new List<Entry>();

		public int TotalByteSize { get; }

		public (ulong buffer, int byteSize) GetBuffer() {
			return (this.data, this.TotalByteSize);
		}

		public List<Entry> GetEntries() {
			return this.Entries.ToList();
		}

		private void EnsureInputDataSize(int byteSize) {
			if(byteSize > this.TotalByteSize) {
				throw new ApplicationException("Data size is bigger than available buffer");
			}
		}

		private void EnsureOutputDataSize(int byteSize) {
			if(byteSize < this.TotalByteSize) {
				throw new ApplicationException("Data size is bigger than available buffer");
			}
		}

		public void SetBuffer(byte value) {
			this.EnsureInputDataSize(sizeof(byte));

			this.data = value;
		}

		public void SetBuffer(short value) {
			this.EnsureInputDataSize(sizeof(short));

			this.data = (ushort) value;
		}

		public void SetBuffer(ushort value) {
			this.EnsureInputDataSize(sizeof(ushort));

			this.data = value;
		}

		public void SetBuffer(uint value) {
			this.EnsureInputDataSize(sizeof(uint));

			this.data = value;
		}

		public void SetBuffer(int value) {
			this.EnsureInputDataSize(sizeof(int));

			this.data = (uint) value;
		}

		public void SetBuffer(ulong value) {
			this.EnsureInputDataSize(sizeof(ulong));

			this.data = value;
		}

		public void GetBuffer(out byte buffer) {
			this.EnsureOutputDataSize(sizeof(short));

			buffer = (byte) this.data;
		}

		public void GetBuffer(out ushort buffer) {
			this.EnsureOutputDataSize(sizeof(ushort));

			buffer = (ushort) this.data;
		}

		public void GetBuffer(out uint buffer) {
			this.EnsureOutputDataSize(sizeof(uint));

			buffer = (uint) this.data;
		}

		public void GetBuffer(out ulong buffer) {
			this.EnsureOutputDataSize(sizeof(ulong));

			buffer = this.data;
		}

		private void AddEntry(int bitSize, string name) {
			int currentBitSize = this.GetCurrentBitSize();

			if((currentBitSize + bitSize) > this.TotalByteBitSize) {
				throw new InvalidDataException("Total bitsize exceeds data size");
			}

			this.Entries.Add(new Entry {bitSize = bitSize, name = name, offset = currentBitSize});
			this.entriesChanged = true;
		}

		public int GetCurrentBitSize() {
			if(this.entriesChanged) {
				this.currentBitSize = this.Entries.Sum(e => e.bitSize);
			}

			return this.currentBitSize;
		}

		public ulong MaskEntryValue(int index, ulong value) {
			if(index >= this.Entries.Count) {
				throw new ApplicationException("index higher than entry counts");
			}

			Entry entry = this.Entries[index];

			return this.MaskEntryValue(entry, value);
		}

		public ulong MaskEntryValue(string name, ulong value) {
			Entry? entry = this.Entries.SingleOrDefault(e => e.name == name);

			if(!entry.HasValue) {
				throw new ApplicationException("No entry found for the given name");
			}

			return this.MaskEntryValue(entry.Value, value);
		}

		private ulong MaskEntryValue(Entry entry, ulong value) {

			ulong mask = BuildNoOffsetMask(entry);

			return value & mask;
		}

		public void SetEntryValue(int index, ulong value) {
			if(index >= this.Entries.Count) {
				throw new ApplicationException("index higher than entry counts");
			}

			Entry entry = this.Entries[index];

			this.SetEntryValue(entry, value);
		}

		public void SetEntryValue(string name, ulong value) {
			Entry? entry = this.Entries.SingleOrDefault(e => e.name == name);

			if(!entry.HasValue) {
				throw new ApplicationException("No entry found for the given name");
			}

			this.SetEntryValue(entry.Value, value);
		}

		private void SetEntryValue(Entry entry, ulong value) {

			int valueBitSize = BitUtilities.GetValueBitSize(value);

			if(valueBitSize > entry.bitSize) {
				throw new ApplicationException("value is too big for the allocated space");
			}

			ulong mask = BuildMask(entry);
			this.data = this.data & ~mask;
			ulong offsetValue = value << entry.offset;
			this.data |= offsetValue;
		}

		public ulong GetEntryValue(int index) {
			if(index >= this.Entries.Count) {
				throw new ApplicationException("index higher than entry counts");
			}

			Entry entry = this.Entries[index];

			return this.GetEntryValue(entry);
		}

		public ulong GetEntryValue(string name) {
			Entry? entry = this.Entries.SingleOrDefault(e => e.name == name);

			if(!entry.HasValue) {
				throw new ApplicationException("No entry found for the given name");
			}

			return this.GetEntryValue(entry.Value);
		}

		public ulong GetEntryValue(Entry entry) {

			ulong mask = BuildMask(entry);
			ulong value = this.data & mask;
			value = value >> entry.offset;

			return value;
		}

		public static ulong BuildNoOffsetMask(Entry entry) {

			return BuildNoOffsetMask(entry.bitSize, entry.offset);
		}

		public static ulong BuildNoOffsetMask(int bitSize, int offset) {

			ulong mask = 0;

			for(int i = 0; i < bitSize; i++) {
				mask |= (ulong) (1L << i);
			}

			return mask;
		}

		public static ulong BuildMask(Entry entry) {

			return BuildMask(entry.bitSize, entry.offset);
		}

		public static ulong BuildMask(int bitSize, int offset) {

			ulong mask = BuildNoOffsetMask(bitSize, offset);

			return mask << offset;
		}

		public struct Entry {
			public int bitSize;
			public string name;
			public int offset;
		}
	}
}