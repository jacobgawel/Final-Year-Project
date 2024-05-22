'use server'
import { getCurrentUser, getTokenClient } from '@/app/actions/authActions';
import { Booking, BookingRefundDto, BookingPricingInfoRequest, BookingRecordDto, BookingRequest, BookingUpdateDto, BookingGapsRequestDto } from '@/types/booking';

export async function GetBookingAuthTest() {
    const user = await getCurrentUser();
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking/account/' + user?.userId, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store'
    });
    // check if the response is ok
    if (!res.ok) {
        return [ res.status, res.statusText ];
    }
    return [ res.status, res.statusText ];
}

export async function GetBookingByProviderId(providerId: string) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking/provider/' + providerId, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store'
    });
    // check if the response is ok
    if (!res.ok) {
        return [ res.status, res.statusText ];
    }
    return res.json();
}

export async function GetHumanizedBooking(bookingId: string) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking/humanized/' + bookingId, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store'
    });
    // check if the response is ok
    if (!res.ok) {
        return [ res.status, res.statusText ];
    }
    return { status: res.status, data: await res.json() }
}

export async function GetBookingPricingInfo(bookingInfo: BookingPricingInfoRequest) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking/price', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        body: JSON.stringify(bookingInfo)
    });
    // check if the response is ok
    if (!res.ok) {
        return [ res.status, res.statusText ];
    }
    
    return res.json();
}

export async function GetBookingData() {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking', {
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


export async function GetHumanizedBookingForCurrentUser() {
    const user = await getCurrentUser();
    const token = await getTokenClient();

    // dont cache this request
    const res = await fetch('http://localhost:8101/booking/account/card/' + user?.userId, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store'
    });
    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText}
    }
    return { status: res.status, data: await res.json() }
}

export async function GetBookingForCurrentUser() {
    const user = await getCurrentUser();
    const token = await getTokenClient();

    // dont cache this request
    const res = await fetch('http://localhost:8101/booking/account/' + user?.userId, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store'
    });
    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText}
    }
    return { status: res.status, data: await res.json() }
}

export async function GetBookingForUserById(id: string) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking/' + id, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store'
    });
    
    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText}
    }
    return { status: res.status, data: await res.json() }
}

export async function CreateTransactionForUser(transaction: BookingRecordDto){
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking/transaction', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        body: JSON.stringify(transaction)
    });
    
    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText}
    }
    return { status: res.status, data: await res.json() }

}

export async function CreateBookingForUser(booking: BookingRequest) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        body: JSON.stringify(booking)
    });
    
    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText}
    }
    return { status: res.status, data: await res.json() }
}

export async function UpdateBookingForUser(booking: BookingUpdateDto) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking', {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        body: JSON.stringify(booking)
    });
    
    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText}
    }
    return { status: res.status, data: await res.json() }
}

export async function GetBookingAnalyticsForProvider(providerId: string) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking/analytics/provider/' + providerId, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store'
    });
    
    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText}
    }
    return { status: res.status, data: await res.json() }
}

export async function GetBookingGapsForParkingId(parkingId: string, bookingGapsRequest: BookingGapsRequestDto) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking/gaps/' + parkingId, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        body: JSON.stringify(bookingGapsRequest),
        cache: 'no-store'
    });
    
    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText}
    }
    return { status: res.status, data: await res.json() }
}

export async function GetBookingAnalyticsForAdmin() {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking/analytics/admin' , {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store'
    });
    
    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText}
    }
    return { status: res.status, data: await res.json() }
}

export async function GetBookingRefundDetails(bookingId: string) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/booking/refund/' + bookingId, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store'
    });
    
    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText}
    }

    return { status: res.status, data: await res.json() }
}