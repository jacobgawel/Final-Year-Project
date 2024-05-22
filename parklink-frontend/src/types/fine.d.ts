export interface Fine {
    "id": string
    "description": string
    "total": number
    "createdAt": string
    "fineIssuerId": string
    "bookingId": string
    "accountId": string
    "fineStatus": boolean
    "finePaid": boolean
    "imageUri": string
}

export interface FineAnalyticsDto {
    "totalFines": number
    "paidFines": number
    "pendingFines": number
    "fineRevenue": string
    "activeFines": number
}