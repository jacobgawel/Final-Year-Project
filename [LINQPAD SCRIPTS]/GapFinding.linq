<Query Kind="Program">
  <Connection>
    <ID>e20ac7da-2ec3-4f19-a634-10ff3128a26b</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <Server>localhost</Server>
    <SqlSecurity>true</SqlSecurity>
    <UserName>sa</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAYOe7iX0I/E6h306NIAuFswAAAAACAAAAAAAQZgAAAAEAACAAAABOaZqoZF31b/gFpQTYa2pVc9i8+sTSgHmQ2JCluNZ0qwAAAAAOgAAAAAIAACAAAACoeuwmvGadXUe5DevBBZ2mI5b9NbJpTStMO7NJKXnqMxAAAADOzkGyFbxghlRMfVTwPuUFQAAAAPqcoGVwh748704rqI5WUmcRQjUlF8aEcFefOtTu1fkPYnzzoCxJxKgnECOa/3cSQCFesfZhHxX3+5oDTfXr3y0=</Password>
    <Database>Booking</Database>
    <DisplayName>Booking</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
  <NuGetReference>NodaTime</NuGetReference>
  <NuGetReference>TimePeriodLibrary.NET</NuGetReference>
  <Namespace>Itenso.TimePeriod</Namespace>
  <Namespace>NodaTime</Namespace>
  <Namespace>NodaTime.Calendars</Namespace>
  <Namespace>NodaTime.Extensions</Namespace>
  <Namespace>NodaTime.Text</Namespace>
  <Namespace>NodaTime.TimeZones</Namespace>
  <Namespace>NodaTime.TimeZones.Cldr</Namespace>
  <Namespace>NodaTime.Utility</Namespace>
  <Namespace>NodaTime.Xml</Namespace>
</Query>

void Main()
{
    // Sample existing bookings for the day
    var bookings = Bookings.Where(selections => selections.StartDate.Date == new DateTime(2024, 03, 11).Date).ToList();
	
	
	var existingBookings = new List<TimeRange>();
	bookings.ForEach(booking => {
		existingBookings.Add(new TimeRange() {
			Duration = booking.Duration.ToTimeSpan(),
			End = booking.EndDate,
			Start = booking.StartDate
		});
	});
	
	
	TimeRange booking = new TimeRange() {
		Start = new DateTime(2024, 03, 11, 13, 0, 0),
		End = new DateTime(2024, 03, 11, 16, 0,0),
		Duration = TimeSpan.FromHours(2)
	};

	TimeRange booking2 = new TimeRange()
	{
		Start = new DateTime(2024, 03, 11, 11, 0, 0),
		End = new DateTime(2024, 03, 11, 16, 0, 0),
		Duration = TimeSpan.FromHours(1)
	};

	existingBookings.Add(booking);
	existingBookings.Add(booking2);

	existingBookings.Dump();

	// Desired booking parameters
    DateTime desiredDate = new DateTime(2024, 3, 11);
    TimeSpan desiredDuration = TimeSpan.FromHours(1);
	TimeSpan desiredTime = new TimeSpan(13, 0, 0); // Desired time (e.g., 1:00 PM)

	// Find available gaps
	var availableGaps = FindAvailableGaps(existingBookings, desiredDate, desiredDuration);

	availableGaps = availableGaps.OrderBy(gap => Math.Abs(((decimal)(gap.Start.TimeOfDay.TotalMinutes + gap.End.TimeOfDay.TotalMinutes) / 2) - (decimal)desiredTime.TotalMinutes)).ToList();
	
	// Display the available gaps
	foreach (var gap in availableGaps)
	{
		gap.Dump();
	}
}

List<TimeRange> FindAvailableGaps(List<TimeRange> existingBookings, DateTime desiredDate, TimeSpan desiredDuration)
{
    // Define the full day range
    var fullDayRange = new TimeRange(desiredDate, desiredDate.AddDays(1));

    // Get the gaps between existing bookings
    var bookedPeriods = existingBookings.OrderBy(b => b.Start).ToList();
    var availableGaps = new List<TimeRange>();

    if (bookedPeriods.Count > 0 && bookedPeriods[0].Start > fullDayRange.Start)
    {
        availableGaps.Add(new TimeRange(fullDayRange.Start, bookedPeriods[0].Start));
    }

    for (int i = 0; i < bookedPeriods.Count - 1; i++)
    {
        var gap = new TimeRange(bookedPeriods[i].End, bookedPeriods[i + 1].Start);
        if (gap.Duration >= desiredDuration)
        {
            availableGaps.Add(gap);
        }
    }

	if (bookedPeriods.Count > 0 && bookedPeriods.Last().End < fullDayRange.End)
	{
		availableGaps.Add(new TimeRange(bookedPeriods.Last().End, fullDayRange.End));
	}

	return availableGaps;
}
