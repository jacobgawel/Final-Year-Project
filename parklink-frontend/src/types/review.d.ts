export interface ReviewCreate {
    reviewRating: number,
    reviewText: string,
    parkingId: string
}

export interface Review {
    id: string,
    reviewRating: number,
    reviewText: string,
    reviewTitle: string,
    parkingId: string,
    accountId: string,
    createdAt: string
}