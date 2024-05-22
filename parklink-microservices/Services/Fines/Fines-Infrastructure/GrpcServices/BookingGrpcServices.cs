using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fines_Infrastructure.GrpcServices;

public class BookingGrpcServices
{
    private readonly BookingProtoService.BookingProtoServiceClient _bookingProtoService;

    public BookingGrpcServices(BookingProtoService.BookingProtoServiceClient bookingProtoService)
    {
        _bookingProtoService = bookingProtoService;
    }

    public async Task<BookingModel> GetBooking(string id)
    {
        var bookingRequest = new GetBookingRequest { BookingId = id };
        return await _bookingProtoService.GetBookingAsync(bookingRequest);
    }

    public async Task<FinePaidResult> FinePaid(string id)
    {
        var fineRequest = new FinePaidRequest { BookingId = id, FinePaid = true };
        return await _bookingProtoService.VerifyPaymentStatusAsync(fineRequest);
    }

    public async Task<BookingUpdateStatus> FineDeleted(string id)
    {
        var bookingRequest = new BookingIdRequest { BookingId = id };
        return await _bookingProtoService.FineDeletedAsync(bookingRequest);
    }

    public async Task<VerificationResult> VerifyFine(string id, bool status)
    {
        var verificationRequest = new VerificationRequest
        {
            BookingId = id,
            FineStatus = status
        };

        return await _bookingProtoService.VerifyFineStatusAsync(verificationRequest);
    }
}