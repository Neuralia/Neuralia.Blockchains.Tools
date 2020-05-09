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
	}
}