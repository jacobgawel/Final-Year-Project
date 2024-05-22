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
</Query>

void Main()
{
	var bookingTransactioAnalytics = BookingRecords.Where(br => br.Verified == true);
	
	bookingTransactioAnalytics.Dump();

	List<Record> record = InitializeRecordsForAllMonths();

	foreach (var booking in bookingTransactioAnalytics)
	{
		int month = booking.TransactionDate.Month;
		Record rec = record.Find(r => r.Month == month.ToString());

		if (rec != null)
		{
			rec.Fees += booking.Fees;
		}
	}

	record.Dump();
	
}

public List<Record> InitializeRecordsForAllMonths()
{
	List<Record> records = new List<Record>();

	for (int i = 1; i <= 12; i++)
	{
		Record rec = new Record();
		rec.Month = i.ToString();
		records.Add(rec);
	}

	return records;
}


public class Record {
	public string Month {get;set;}
	public decimal Fees {get;set;}
}


// You can define other methods, fields, classes and namespaces here
