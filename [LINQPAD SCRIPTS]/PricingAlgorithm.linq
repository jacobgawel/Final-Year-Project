<Query Kind="Statements" />

var transactionFeePercentage = 5M; // 5% transaction fee (adjust as needed)
var processingFeePercentage = 25M; // 25% processing fee (adjust as needed)

var parkingBooking = new DateTime(12, 6, 8, 15, 0, 0);
var duration = TimeSpan.FromHours(2);

var bookingTotalMinutes = duration.TotalMinutes;
var bookingInterval = bookingTotalMinutes / 30;
var pricePerHour = 1.5M;

var subTotal = 0M;
parkingBooking.DayOfWeek.Dump();
parkingBooking.TimeOfDay.Dump();

for (int i = 0; i < bookingInterval; i++)
{
    DateTime currentIntervalStart = parkingBooking.AddMinutes(i * 30);

	decimal intervalPrice = (decimal)30 / 60 * pricePerHour; // Calculate price for the current interval

	if (currentIntervalStart.Hour >= 7 && currentIntervalStart.Hour < 14)
	{
		subTotal += intervalPrice; // Normal rate
	}
	else if ((currentIntervalStart.DayOfWeek == DayOfWeek.Friday || currentIntervalStart.DayOfWeek == DayOfWeek.Saturday)
		&& currentIntervalStart.Hour >= 17 && currentIntervalStart.Hour < 21)
	{
		subTotal += intervalPrice * 1.35M;
	}
	else if (currentIntervalStart.DayOfWeek == DayOfWeek.Sunday && currentIntervalStart.Hour >= 17 && currentIntervalStart.Hour < 21)
	{
		subTotal += intervalPrice * 1.15M;
	}
	else if (currentIntervalStart.Hour >= 17 && currentIntervalStart.Hour < 21)
	{
		subTotal += intervalPrice * 1.25M;
	}
	else if (currentIntervalStart.Hour >= 14 && currentIntervalStart.Hour < 17)
	{
		subTotal += intervalPrice * 1.75M; // Rush hour rate (adjust as needed)
	}
	else
	{
		subTotal += intervalPrice * 0.5M; // Quiet period rate (adjust as needed)
	}
}

var totalCost = subTotal;
var transactionFee = totalCost * (transactionFeePercentage / 100);
var processingFee = totalCost * (processingFeePercentage / 100);

totalCost += transactionFee + processingFee;

Console.WriteLine("Fees: " + (transactionFee + processingFee));
Console.WriteLine("Sub Total: " + subTotal);
Console.WriteLine("Total Cost: " + totalCost);

Console.WriteLine("Parklink cut: " + (processingFee + transactionFee));

