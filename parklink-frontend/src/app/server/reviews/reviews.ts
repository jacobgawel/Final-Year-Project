"use server"

import { getCurrentUser, getTokenClient } from '@/app/actions/authActions';
import { ReviewCreate } from '@/types/review';

export async function GetReviewsForParking(parkingId: string) {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/review/parking/' + parkingId, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store'
    });
    if(!res.ok) {
        return [ res.status, res.statusText ];
    }

    return res.json();
}

export async function CreateReview(review: ReviewCreate) {
    const token = await getTokenClient();
    const user = await getCurrentUser();
    const res = await fetch('http://localhost:8101/review', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store',
        body: JSON.stringify({ ...review, accountId: user?.userId })
    });
    if(!res.ok) {
        return [ res.status, res.statusText ];
    }

    return res.json();
}
