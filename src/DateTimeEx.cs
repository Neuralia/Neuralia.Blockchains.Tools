using System;

namespace Neuralia.Blockchains.Tools {
	public static class DateTimeEx {
		
		private static DateTime savedNetworkDateTime = DateTime.UtcNow;
		private static TimeSpan timeDelta = TimeSpan.Zero;
		
		/// <summary>
		///     this gives us the real adjusted time from time servers
		/// </summary>
		public static DateTime CurrentTime => DateTime.UtcNow.Add(timeDelta);
		
		public static void SetTime(DateTime networkDateTime) {
			savedNetworkDateTime = networkDateTime.ToUniversalTime();
			timeDelta = savedNetworkDateTime.Subtract(DateTime.UtcNow);
		}

		/// <summary>
		/// in neuralium, we only use DateTimeKind.Utc, so here we have the min and max set as such
		/// </summary>
		public static DateTime MinValue => DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
		public static DateTime MaxValue => DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
		public static DateTime Today => DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
	}
}