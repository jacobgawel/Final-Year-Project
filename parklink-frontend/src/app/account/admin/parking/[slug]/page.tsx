"use client";

import { GetHumanizedParkingById, GetParkingById } from '@/app/server/parking/parking';
import { Parking, ParkingHumanized, ParkingImage, S3Image } from '@/types/parking';
import React, { useEffect, useState } from 'react';
import { Card } from '@mui/material';
import CircularProgress from '@mui/material/CircularProgress';
import ParkingViewer from './ParkingViewer';

async function getParkingData(id: string) {
    const parking = await GetHumanizedParkingById(id);
    console.log(parking.status)
    if(parking.status === 200) {
        return parking.data;
    }

    if(parking.status === 401) {
        console.log('Unauthorized access to bookings', parking.status);
    }

    return [];
}

// uses the slug from the URL to render with the booking id
export default function Page({ params }: { params: { slug: string } }) {
    const [parking, setParking] = useState<any>();
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getParkingData(params.slug).then((data) => {
            setParking(data);
            setLoading(false);
        });
    }, []);

    var s3ImageObj: ParkingImage | undefined = parking ? JSON.parse(parking.slotImages) : undefined;

    return (
        <>
        {
            loading ? <CircularProgress /> : 
            <Card>
                <ParkingViewer parking={parking} s3ImageUris={s3ImageObj} />
            </Card>
        }
        </>
    )
}