<Query Kind="Program">
  <Connection>
    <ID>abb72156-1ff6-4572-b046-77c8368fe026</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>localhost</Server>
    <UserName>admin</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAXvA1BrUQZ0+luY1vz1t2yQAAAAACAAAAAAAQZgAAAAEAACAAAAC+UD945o80Kf0RtbmOhM/ivn2xjaWQP/qmi7rTHu3y3wAAAAAOgAAAAAIAACAAAADaxJsjzwhdd1R/LoWFGQYJRG7eHssUa214yArmLqKPnRAAAAD+qfx63mK0F3I9Yn+ZlhbaQAAAAG75L1xigliH9VtgIzAqBA10XZ1qJUiVtYUxmPTiWevjrzKCsOOM5PjWEKpZKIqDxbsagWugx9wQGXS6XIrrvjw=</Password>
    <Database>Booking</Database>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Npgsql.EntityFrameworkCore.PostgreSQL</EFProvider>
      <Port>5432</Port>
    </DriverData>
  </Connection>
  <Reference Relative="..\parklink-microservices\Services\Booking\Booking-Domain\bin\Debug\net7.0\Booking-Domain.dll">G:\University\jg510\parklink-microservices\Services\Booking\Booking-Domain\bin\Debug\net7.0\Booking-Domain.dll</Reference>
</Query>

void Main()
{
	var bookings = Bookings.ToList();
	bookings.Dump();
	//var booking = bookings.FirstOrDefault(b => b.Id == new Guid("64f7cf49-20da-4f3a-9905-ebd889c6c96b"));
	//AddFine(booking);
	//var fines = Fines.ToList();
	//fines.Dump();
	
	// testing fine logic
	foreach (var element in bookings)
	{
		var fineCheck = element.Fines.Any().Dump();
		if (fineCheck) {
			element.Dump();
		}
	}
}

public bool AddFine(Bookings booking) 
{
	// make sure that this is correct

	try
	{
		Guid bookingId = booking.Id;
		Guid attendeeId = Guid.NewGuid();

		Fines fine = new Fines();

		fine.AttendeeId = attendeeId;
		fine.BookingId = bookingId;
		fine.Description = "Prolonged stay at parking spot";
		fine.Total = 12.50;
		booking.FineStatus = true;

		Fines.Add(fine);
		SaveChanges();
	} catch (Exception e) {
		e.Dump();
		return false;
	}
	
	return true;
}

public bool CheckFineStatus(Bookings booking)
{
	if (booking.FineStatus == true) {
		return true;
	}
	
	return false;
}
