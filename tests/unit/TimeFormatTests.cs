using Xunit;

// TimeFormat: the stopwatch and star-time text shown on the HUD, result
// screen and level select.
public class TimeFormatTests
{
	[Theory]
	[InlineData(0.0f, "00:00.00")]
	[InlineData(27.48f, "00:27.48")]
	[InlineData(65.5f, "01:05.50")]
	[InlineData(107.1f, "01:47.10")]
	[InlineData(300.0f, "05:00.00")]
	public void ClockShowsMinutesSecondsAndHundredths(float seconds, string expected)
	{
		Assert.Equal(expected, TimeFormat.Clock(seconds));
	}

	[Fact]
	public void ClockNeverShowsANegativeTime()
	{
		Assert.Equal("00:00.00", TimeFormat.Clock(-5.0f));
	}

	[Theory]
	[InlineData(30.0f, "0:30")]
	[InlineData(50.0f, "0:50")]
	[InlineData(75.0f, "1:15")]
	[InlineData(120.0f, "2:00")]
	[InlineData(300.0f, "5:00")]
	public void ShortShowsMinutesAndSeconds(float seconds, string expected)
	{
		Assert.Equal(expected, TimeFormat.Short(seconds));
	}
}
