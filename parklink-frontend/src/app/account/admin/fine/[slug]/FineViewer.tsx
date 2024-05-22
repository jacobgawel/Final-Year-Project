"use client";

import BookingReceipt from "@/app/components/BookingReceipt";
import FineReceipt from "@/app/components/FineReceipt";

export default function FineViewer({ fine, booking }: { fine: any, booking: any }) {

    return (
        <div>
            <BookingReceipt booking={booking.data} />
            <FineReceipt fine={fine} />
        </div>
    )
}