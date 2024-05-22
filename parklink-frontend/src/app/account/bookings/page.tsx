'use client';

import React, { useState, useEffect } from 'react';
import { GetHumanizedBookingForCurrentUser, GetBookingForCurrentUser } from '@/app/server/booking/booking';
import { Booking, HumanizedBookingDto } from '@/types/booking';
import 'mapbox-gl/dist/mapbox-gl.css';
import useMapboxMap from '@/app/components/UseMapBoxMap';
import BookingCard from './BookingCard';
import Skeleton from '@mui/material/Skeleton';
import { Paper, styled } from '@mui/material';

const BookingPaper = styled(Paper)(({ theme }) => ({
    padding: theme.spacing(1),
    color: theme.palette.text.secondary,
    margin: '1rem',
    backgroundColor: theme.palette.background.default,
    boxShadow: '0px 0px 10px 0px rgba(0,0,0,0.1)',
    borderRadius: '0',
}));

async function getBookings() {
    // get the current user from the auth actions
    const bookings = await GetHumanizedBookingForCurrentUser();
    if(bookings.status === 200) {
        // return the bookings data e.g. all bookings for the current user
        return bookings.data;
    }

    if(bookings.status === 401) {
        return bookings.status;
    }
}

// this component is used to display a loading animation
function BookingSkeleton() {
    return (
        <BookingPaper elevation={10}>
            <Skeleton animation='pulse' className='m-2'  variant="rectangular"width={200} height={20} />
            <Skeleton animation='pulse' className='m-2' variant="rectangular" width={360} height={20} />
            <Skeleton animation='pulse' className='m-2' variant="rectangular" width={150} height={40} />
            <Skeleton animation='pulse' className='m-2' variant="rectangular" width={180} height={20} />
            <Skeleton animation='pulse' className='m-2' variant="rectangular" width={150} height={20} />
            <Skeleton animation='pulse' className='m-2' variant="rectangular" width={360} height={40} />
        </BookingPaper>
    )
}

export default function Page() {

    const [bookings, setBookings] = useState<any>([]);
    const [loading, setLoading] = useState(true);

    // fetching the booking details for the current user
    useEffect(() => {
        getBookings().then((bookings) => {
            setBookings(bookings);
            setLoading(false);
        });
    }, []);

    // gets the map object from the useMapBoxMap hook
    // this object is used to manipulate the map
    // the object is passed to the BookingCard component
    // which is used to view the location of the parking using a button click

    const map = useMapboxMap();

    return (
        <>
        <div className='grid grid-cols-2 gap-4'>
            <div className='bg-slate-50 outline outline-slate-200'>
                <div className='overflow-auto h-screen'>
                {
                    // get the index of the last booking in the array
                    loading ? <BookingSkeleton />
                    :

                    bookings == 401 || bookings == undefined ? <h1>User Unauthorized, clear cookies and relogin</h1>
                    :
                    bookings.map((booking: HumanizedBookingDto) => {
                        return (
                            <BookingCard key={booking.id} booking={booking} mapObject={map}/>
                        )
                    })
                }
                </div>
            </div>
            <div>
                <div id='map' className='h-screen'></div>
            </div>
        </div>
        </>
    )
}