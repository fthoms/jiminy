using System;

public static class Extensions {
	public static TimeSpan milliseconds(this int v) => TimeSpan.FromMilliseconds(v);
	public static TimeSpan seconds(this int v) => TimeSpan.FromSeconds(v);
	public static TimeSpan minutes(this int v) => TimeSpan.FromMinutes(v);
	public static TimeSpan hours(this int v) => TimeSpan.FromHours(v);
	public static TimeSpan days(this int v) => TimeSpan.FromDays(v);
}
