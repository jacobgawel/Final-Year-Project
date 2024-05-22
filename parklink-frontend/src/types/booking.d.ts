export interface Booking {
    "id": string
    "carRegistration": string
    "accountId": string
    "startDate": string
    "endDate": string
    "fineStatus": boolean
    "bookingConfirmation": boolean
    "duration": string
    "parkingId": string
    "providerId": string
    "createdAt": string
    "email": string
    "subTotal": number
    "total": number
    "fees": number
    "qrCodeLink": string
    "recordId": string

    // refund fields
    "refundAmount": number
    "refundStatus": string
}

export interface BookingPricingInfoResponse {
    "fees": number
    "subTotal": number
    "total": number
    "parkingId": string

    // humanized fields
    "humanizedFees": string
    "humanizedSubTotal": string
    "humanizedTotal": string
}

export interface BookingPricingInfoRequest {
    "parkingId": string
    "startDate": string
    "endDate": string
}

export interface BookingUpdateDto {
    "id": string
    "carRegistration": string
    "startDate": string
    "endDate": string
    "fineStatus": boolean
    "bookingConfirmation": boolean
    "duration": string
    "subTotal": number
    "total": number
    "fees": number
    "qrCodeLink": string
}

export interface BookingInfo {
    "arrival": dayjs,
    "exit": dayjs,
    "duration": string,
}

export interface BookingRecordDto {
    "CardNumber": string
    "Total": number
    "Fees": number
    "SubTotal": number
}

export interface BookingRecord {
    "id": string,
    "accountId": string,
    "cardNumber": string,
    "transactionDate": string,
    "total": number,
    "fees": number,
    "subTotal": number,
}

export interface BookingRequest {
    "carRegistration": string
    "startDate": string
    "endDate": any
    "parkingId": string
    "recordId": string
}

export interface HumanizedBookingDto extends BookingUpdateDto {
    "parkingId": string
    "providerId": string
    "email": string
    "recordId": string
    "accountId": string

    // refund fields
    "refundAmount": number
    "refundStatus": string

    // humanized fields
    "humanizedDate": string
    "humanizedDuration": string
    "humanizedTotal": string
    "humanizedSubTotal": string
    "humanizedFees": string
    "humanizedCreatedAt": string
    "humanizedBookingSpan": string
    "humanizedCreatedAtDate": string
    "humanizedLastUpdated": string
    "humanizedRefundAmount": string
}

export interface BookingAnalyticsDto {
    // Number of bookings today
    "bookingToday": number
    // Number of bookings yesterday
    "bookingYesterday": number
    // Current active fines
    "activeFines": number
    // Total number of bookings this month
    "totalBookingThisMonth": number
    // Total number of bookings last month
    "totalBookingLastMonth": number
    // The overall total of bookings
    "totalBooking": number
    // Number of bookings this week
    "totalBookingThisWeek": number
    // Number of bookings last week
    "totalBookingLastWeek": number
    // Revenue last week
    "bookingRevenueLastWeek": string
    // Revenue current week
    "bookingRevenueCurrentWeek": string
    // Revenue last month
    "bookingRevenueLastMonth": string
    // Revenue this month
    "bookingRevenueThisMonth": string
    // Comparison revenue last week
    "comparisonRevenueLastWeek": string
    // Comparison booking yesterday
    "comparisonBookingYesterday": string
    // Comparison booking last week
    "comparisonBookingLastWeek": string
    // Comparison booking last month
    "comparisonBookingLastMonth": string
    // Comparison revenue last month
    "comparisonRevenueLastMonth": string
}

export interface BookingRefundDto {
    "refundAmount": number
    "refundPercentage": string
    "bookingId": string
    "fullRefund": boolean
    "noRefund": boolean
    // humanized fields
    "humanizedDate": string
    "humanizedDuration": string
    "humanizedTotal": string
    "humanizedSubTotal": string
    "humanizedFees": string
    "humanizedCreatedAt": string
    "humanizedBookingSpan": string
    "humanizedCreatedAtDate": string
    "humanizedLastUpdated": string
}

export interface BookingGapsRequestDto {
    "bookingDate": string
    "bookingExit": string // the date the booking will exit
}