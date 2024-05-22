<Query Kind="Program">
  <NuGetReference>SendGrid</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Bson</Namespace>
  <Namespace>Newtonsoft.Json.Converters</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>Newtonsoft.Json.Schema</Namespace>
  <Namespace>Newtonsoft.Json.Serialization</Namespace>
  <Namespace>SendGrid</Namespace>
  <Namespace>SendGrid.Helpers.Errors</Namespace>
  <Namespace>SendGrid.Helpers.Errors.Model</Namespace>
  <Namespace>SendGrid.Helpers.EventWebhook</Namespace>
  <Namespace>SendGrid.Helpers.Mail</Namespace>
  <Namespace>SendGrid.Helpers.Mail.Model</Namespace>
  <Namespace>SendGrid.Helpers.Reliability</Namespace>
  <Namespace>SendGrid.Permissions</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
	Execute().Wait();
}

public class BookingConfirmationDto
{
	public string Email { get; set; }
	public string Arrival { get; set; }
	public string ParkingLocation { get; set; }
	public string BookingId { get; set; }
	public decimal SubTotal { get; set; }
	public decimal Fees { get; set; }
	public decimal Total { get; set; }
}

static async Task Execute()
{
	var apiKey = "API_KEY_HERE";
	var client = new SendGridClient(apiKey);
	var from = new EmailAddress("jakubgawel@cloudcore.cc", "Jakub Gawel");
	var subject = "Hello, from Parklink";
	var to = new EmailAddress("jakegawel1310@gmail.com", "Jakub Gawel");

	var dynamicTemplateData = new Dictionary<string, string> {
		{"email", "jakubgawel@icloud.com"},
		{"arrival", "Tuesday, May 30, 09:00"},
		{"exit", "Tuesday, May 30, 09:00"},
	    { "parkingLocation", "6 Hazel Drive M225LY, Greater Manchester"},
	    { "bookingId", "1231-23312-3123"},
	    { "subTotal", "12"},
	    { "fees", "23"},
	    { "total", "35"}
	};
	
	var msg = MailHelper.CreateSingleTemplateEmail(from, to, "d-bd7b46ea448d437aafceee291970852f", dynamicTemplateData);
	var response = await client.SendEmailAsync(msg);
	response.StatusCode.Dump();
}
