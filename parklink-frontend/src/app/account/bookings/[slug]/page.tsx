"use client";

import { HumanizedBookingDto } from '@/types/booking';
import React from 'react';
import { GetHumanizedBooking } from '@/app/server/booking/booking';
import {
    Database,
  } from "lucide-react"
  
import { toast } from 'sonner';
import { motion } from 'framer-motion';
import BookingReceipt from '@/app/components/BookingReceipt';


async function GetBooking(id: string) {
    const bookings = await GetHumanizedBooking(id);

    if (Array.isArray(bookings)) {
        return [];
    }

    if (bookings.status === 200) {
        return bookings.data;
    }

    if (bookings.status === 401) {
        console.log('Unauthorized access to bookings', bookings.status);
    }

    return [];
}

// uses the slug from the URL to render with the booking id
export default function Page({ params }: { params: { slug: string } }) {
    const [booking, setBooking] = React.useState<HumanizedBookingDto>();
    const [loading, setLoading] = React.useState(true);
    
    React.useEffect(() => {
        GetBooking(params.slug).then((booking: HumanizedBookingDto) => {
            setBooking(booking);
            setLoading(false);
        });
    }, [params.slug]);

    return (
        <>
            <div>
                {
                    loading ? (
                        <div className="flex items-center justify-center h-32">
                            <motion.div
                                animate={{ rotate: 360 }}
                                transition={{ repeat: Infinity, duration: 1 }}
                                className="h-8 w-8 text-primary"
                            >
                                <Database />
                            </motion.div>
                        </div>
                    ) : (
                        <BookingReceipt booking={booking} />
                    )
                }
            </div>
        </>
    )
}