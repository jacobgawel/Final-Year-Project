"use server";

import { getCurrentUser, getTokenClient } from '@/app/actions/authActions';
import { Parking, ParkingUpdateRequest } from '@/types/parking';

// GetParkingData gets parking data from the server thats been verified by default, but can be set to false to get unverified data (for admin use only)
export async function GetParkingData(verified: boolean = true) {
    const dataType = verified ? "/verified" : "";
    const res = await fetch('http://localhost:8101/parking' + dataType, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
        cache: 'no-store'
    });
    if(!res.ok) {
        return [ res.status, res.statusText ];
    }

    return res.json();
}

export async function GetParkingDataByProviderId(providerId: string) {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/parking/account/' + providerId, {
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

export async function GetParkingDataNavigation(to: any, from: any, profile: string = 'walking')  {
    console.log('https://api.mapbox.com/directions/v5/mapbox/' + profile + "/" + from[0] + "," + from[1] + ";" + to[0] + "," + to[1] + "?geometries=geojson&access_token=" + process.env.NEXT_PUBLIC_PB_MAPBOX_TOKEN)
    const res = await fetch('https://api.mapbox.com/directions/v5/mapbox/' + profile + "/" + from[0] + "," + from[1] + ";" + to[0] + "," + to[1] + "?geometries=geojson&access_token=" + process.env.NEXT_PUBLIC_PB_MAPBOX_TOKEN);
    if(!res.ok) {
        return [ res.status, res.statusText ];
    }

    return res.json();
}

export async function GetParkingById(id: string) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/parking/' + id, {
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

export async function GetHumanizedParkingById(id: string) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/parking/humanized/' + id, {
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

export async function UpdateParking(data: ParkingUpdateRequest) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/parking', {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store',
        body: JSON.stringify(data)
    });

    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText}
    }
    
    return { status: res.status, data: await res.json() }
}

export async function CreateParking(data: FormData) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/parking', {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token?.access_token}`
        },
        cache: 'no-store',
        body: data
    });

    if (!res.ok) {
        // Consider logging or handling the error more specifically here
        console.error("Error submitting form:", res.statusText);
        return { status: res.status, message: res.statusText};
    }

    return { status: res.status, data: await res.json() };
}

export async function DeleteParking(id: string) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/parking/' + id, {
        method: 'DELETE',
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
    
    return { status: res.status, data: res.statusText }
}

export async function GetParkingByDistance(lat: number, lng: number, duration: string) {
    // gets the parking data by distance from the server
    const res = await fetch('http://localhost:8101/parking/distance?' + new URLSearchParams({ latitude: lat.toString(), longitude: lng.toString(), duration: duration }), {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
        cache: 'no-store'
    });
    
    if(!res.ok) {
        return [ res.status, res.statusText ];
    }

    return res.json();
}

export async function UpdateParkingImages(data: FormData) {
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/parking/images', {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token?.access_token}`
        },
        cache: 'no-store',
        body: data
    });

    if (!res.ok) {
        // Consider logging or handling the error more specifically here
        console.error("Error submitting form:", res.statusText);
        return { status: res.status, message: res.statusText};
    }

    return { status: res.status, data: await res.json() };
}
