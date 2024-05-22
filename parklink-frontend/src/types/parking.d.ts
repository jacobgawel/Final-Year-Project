// declaring type for parking

type S3Image = {
    "fileName": string,
    "fileUri": string
}

type ParkingImage = {
    "directoryPrefix": string,
    "s3ImageUris": S3Image[]
}

// this is the format of the dictionary object that is returned from the API
// it represents the parking gap object e.g. start and end time of the gap in the day
type BookingGap = {
    "start": string,
    "end": string
}

export interface BookingGapsRequest {
    "BookingDate": string,
    "Duration": string
}

export interface BookingGapsResponse {
    "parkingId": string,
    "bookingGaps": BookingGap[]
    "humanizedDateTime": string
}

export interface Parking {
    "id": string
    "address": string
    "slotType": string
    "slotSize": string
    "availabilityStatus": boolean
    "price": number
    "evInfo": string
    "city": string
    "additionalFeatures": string
    "timeLimit": string
    "timeLimited": boolean
    "dayLimit": string
    "dayLimited": boolean
    "slotNotes": string
    "slotImages": string
    "slotCapacity": number
    "createdAt": string
    "accountId": string,
    "longitude": number,
    "latitude": number,
    "verificationStatus": boolean
    "parkingRejected": boolean
    "email": string
}

// used to create parking spots
// this object won't be used directly, it will be stringified and appended to the form data object
export interface ParkingCreateRequest {
    address: string,
    slotType: string,
    slotSize: string,
    price: number,
    timeLimit: string,
    timeLimited: boolean,
    dayLimit: number,
    dayLimited: boolean,
    slotCapacity: number,
    evInfo: string,
    additionalFeatures: string,
    slotNotes: string,
    longitude: string,
    latitude: string,
    city: string,
}

export interface ParkingUpdateRequest {
    id: string,
    address: string,
    slotType: string,
    slotSize: string,
    availabilityStatus: boolean,
    price: number,
    evInfo: string,
    additionalFeatures: string,
    timeLimit: string,
    timeLimited: boolean,
    dayLimit: string,
    dayLimited: boolean,
    slotNotes: string,
    slotImages: string,
    slotCapacity: number,
    longitude: number,
    latitude: number,
    city: string,
    verificationStatus: boolean
    parkingRejected: boolean
}

export interface ParkingHumanized extends ParkingUpdateRequest {
    humanizedTimeLimit: string,
    humanizedCreatedDate: string,
    humanizedPricing: string,
    humanizedLastEditDate: string,
    humanizedVerifiedDate: string,
    humanizedLastEdit: string,
    humanizedCreatedAt: string,
}


export interface ParkingDistanceReturn {
    "parking": Parking,
    "distance": number
}