<Query Kind="Program">
  <Connection>
    <ID>4592b1df-7a0c-4ad9-8632-78bc88297d9c</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>localhost</Server>
    <UserName>admin</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAfyGOC8b5OEacT/xKXnagIwAAAAACAAAAAAAQZgAAAAEAACAAAADDvw49MsV/gvKprL4m9FzkzMh8QRun5fHQpkJcNTm35gAAAAAOgAAAAAIAACAAAAC1Mn1axrNY7TwURuMJnc1JwiH9x41kQZGDSCFSZdPAHBAAAABid+H0eH2dQTxTAlPI+PfvQAAAAGCfoh9M+XeEPDl7JftJzMHw9caqye9SI6A+ogRZOGVE6YEM3qQzHqCZxis/X0ZrEPXPJztHWdyfwQ0b/ROtSlc=</Password>
    <Database>Booking</Database>
    <DisplayName>Booking Db</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Npgsql.EntityFrameworkCore.PostgreSQL</EFProvider>
    </DriverData>
  </Connection>
</Query>



bool IsSpotAvailable(DateTime newBookingDate, TimeSpan duration, Guid parkingId)
{
	var newStart = newBookingDate.TimeOfDay;
	var newEnd = newStart.Add(duration);
	newEnd.Dump();
	
	// checks the date and booking confirmation to see what slots we need to loop through.
	var sameDayBookings = Bookings.Where(p => p.DateTime.Date.Equals(newBookingDate.Date) && p.BookingConfirmation == true && p.ParkingId == parkingId);
	sameDayBookings.Dump();

	foreach (var element in sameDayBookings)
	{
		var existingStart = element.DateTime.TimeOfDay;
		var existingEnd = element.DateTime.Add(element.Duration).TimeOfDay;
		
		if (
			(newStart >= existingStart && newStart < existingEnd) ||
			(newEnd > existingStart && newEnd <= existingEnd) ||
			(newStart <= existingStart && newEnd >= existingStart)
		)
		{
			// Time overlap with an existing reservation
			Console.WriteLine($"Collision with booking: {element.Id}");
			return false;
		}
	}
	
	return true;
}


void Main()
{
	// declares a new dummy booking
	var newBookingDate = new DateTime(2023, 11, 16, 19, 50, 0);
	var newBookingDuration = new TimeSpan(0, 30, 0);
	newBookingDate.Dump();

	var parking = new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6");
	var duration = new TimeSpan(0, 30, 0);
	
	var bookings = Bookings.Where(p => p.ParkingId == parking);
	
	// change the date of one of the bookings. For testing.
	var booking = bookings.FirstOrDefault(p => p.Id == new Guid("90d7dea2-3c73-4882-8573-70ceab827fa3"));
	booking.DateTime = new DateTime(2023, 11, 10, 20, 0, 0);
	SaveChanges();
	bookings.Dump();
	
	var result = IsSpotAvailable(newBookingDate, newBookingDuration, parking).Dump();
	
	newBookingDate = newBookingDate.AddMinutes(-1);
	newBookingDate.Dump();
}
