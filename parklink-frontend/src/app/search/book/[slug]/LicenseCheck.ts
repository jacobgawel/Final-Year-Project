"use server";

export async function ValidateLicense(license: string) {
    var myHeaders = new Headers();
    // replace that with the env variable later on
    myHeaders.append("x-api-key", "v8W1G3WaVv5kZEGAgGrbv3x8TH9obpPk74VsDC47");
    myHeaders.append("Content-Type", "application/json");

    var raw = JSON.stringify({
        "registrationNumber": license
    });

    var requestOptions = {
        method: 'POST',
        headers: myHeaders,
        body: raw,
    };

    const res = await fetch('https://driver-vehicle-licensing.api.gov.uk/vehicle-enquiry/v1/vehicles', requestOptions);

    // check if the response is ok
    if (!res.ok) {
        return { status: res.status, message: res.statusText }
    }

    return { status: res.status, data: await res.json() }
}