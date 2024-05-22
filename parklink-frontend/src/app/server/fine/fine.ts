"use server";

import { getCurrentUser, getTokenClient } from '@/app/actions/authActions';

export async function GetFinesByUserId(userId: string) {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/fine/account/' + userId, {
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

export async function CreateFineForBooking(formRequest: FormData) {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/fine', {
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + token?.access_token
        },
        body: formRequest
    });

    if(!res.ok) {
        return [ res.status, res.statusText ];
    }

    return res.json();
}

export async function GetFinesForUser(userId: string) {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/fine/user/' + userId, {
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

export async function VerifyFineAdmin(fineId: string) {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/fine/verify/' + fineId, {
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

export async function UserPayFine(fineId: string) {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/fine/pay/' + fineId, {
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

export async function GetFineById(fineId: string) {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/fine/' + fineId, {
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

export async function GetProviderAnalytics(providerId: string) {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/fine/provider/analytics/' + providerId, {
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

export async function GetAdminFineAnalytics() {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/fine/admin/analytics', {
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

export async function DeleteFineProvider(fineId: string) {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/fine/' + fineId, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        cache: 'no-store'
    });

    if(!res.ok) {
        return [ res.status, res.statusText ];
    }

    return true;
}

export async function GetAllFinesForAdmin() {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/fine', {
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

export async function GetAllFinesForProvider(bookingId: string) {
    const token = await getTokenClient();
    const res = await fetch('http://localhost:8101/fine/provider/' + bookingId, {
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

export async function GetFinesForCurrentUser() {
    const user = await getCurrentUser();
    const token = await getTokenClient();

    const res = await fetch('http://localhost:8101/fine/user/' + user?.userId, {
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

