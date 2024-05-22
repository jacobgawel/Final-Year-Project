"use client";

import BookingReceipt from "@/app/components/BookingReceipt";
import { GetHumanizedBooking } from "@/app/server/booking/booking";
import { HumanizedBookingDto } from "@/types/booking";
import { motion } from "framer-motion";
import { Database } from "lucide-react";
import { useSearchParams } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { useDropzone } from 'react-dropzone';
import { Swiper, SwiperSlide } from 'swiper/react';
import { Pagination, Navigation } from 'swiper/modules';
import { Button } from "@/components/ui/button"
import { CreateFineForBooking } from "@/app/server/fine/fine";

// Import Swiper styles
import 'swiper/css';
import 'swiper/css/pagination';
import 'swiper/css/navigation';
import { toast } from "sonner";

// Dropzone styling
const baseStyle = {
  flex: 1,
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  padding: '20px',
  borderWidth: 2,
  borderRadius: 2,
  borderColor: '#eeeeee',
  borderStyle: 'dashed',
  backgroundColor: '#fafafa',
  color: '#bdbdbd',
  outline: 'none',
  transition: 'border .24s ease-in-out'
};

const focusedStyle = {
  borderColor: '#2196f3'
};

const acceptStyle = {
  borderColor: '#95bd97'
};

const rejectStyle = {
  borderColor: '#d53e29'
};


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


export default function FinePage() {
    const [booking, setBooking] = useState<HumanizedBookingDto>();
    const [loading, setLoading] = useState(true);
    const [fineReason, setFineReason] = useState('' as string);
    const [success, setSuccess] = useState(false);
    
    // dropzone init, only accept images
    const {getRootProps, getInputProps, isFocused, isDragAccept, isDragReject, acceptedFiles} = useDropzone(
        { noKeyboard: true, accept: { 'image/*': ['.jpg', '.jpeg', '.png'] }}
        );
    const style = useMemo<any>(() => ({
        ...baseStyle,
        ...(isFocused ? focusedStyle : {}),
        ...(isDragAccept ? acceptStyle : {}),
        ...(isDragReject ? rejectStyle : {})
    }), [
        isFocused,
        isDragAccept,
        isDragReject
    ]);

    const searchData = useSearchParams()

    var bookingId: any = searchData.get('bookingId')
    
    useEffect(() => {
        GetBooking(bookingId).then((booking: HumanizedBookingDto) => {
            setBooking(booking);
            setLoading(false);
        });
    }, [bookingId]);

    async function handleSubmit(event: any) {
        event.preventDefault();
        const formData = new FormData();
        formData.append('bookingId', bookingId);
        formData.append('description', fineReason);
        formData.append('file', acceptedFiles[0]);

        const response = await CreateFineForBooking(formData);

        console.log(response)

        if (response[0] === 409) {
            toast.error('You have already submitted a fine for this booking.');
        } else {
            toast.success('Fine submitted successfully.');
            setSuccess(true);
        }
    }

    return (
        <div>
            <h1 className="text-2xl font-semibold mt-2 mb-4">Submit a fine</h1>
            <p className="text-sm text-gray-500 mb-4">Submit a fine for the booking below.</p>
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

            {
                success === false ? (
                    <div className="mt-8">
                        <h2 className="text-lg font-semibold">Fine details</h2>
                        <p className="text-sm text-gray-500 mb-4">Enter the details of the fine below. Requires at least 20 chars.</p>
                        <div className="grid grid-cols-1 gap-6">
                            <div>
                                <label className="block text-sm font-medium text-gray-700">Fine reason</label>
                                <textarea onChange={(e) => setFineReason(e.target.value)} className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-primary focus:border-primary sm:text-sm" />
                                { fineReason.length <= 20 && fineReason.length != 0 && <p className="text-red-500 text-sm mt-1">Fine reason must be at least 20 characters long.</p>}
                            </div>
                            <div>
                                <p className="block text-sm font-medium text-gray-700">Fine evidence</p>
                                <p className="text-sm text-gray-500">Upload an image of the vehicle as evidence in the parking space.</p>
                            </div>
                            <section>
                                <div {...getRootProps({style})}>
                                    <input {...getInputProps()} />
                                    <p>Drop and upload your images over here</p>
                                    <em>(Click on the dropzone to open file explorer)</em>
                                </div>
                            </section>
                        </div>
                        <div className="mt-4">
                                {acceptedFiles.length >= 2 && <p className="text-red-500 text-xl font-semibold">You can only upload 2 images. Simply drag 1 image into the box to reset the files.</p>}
                            </div>
                        <div className="mt-8">
                            <p className="text-sm text-gray-500 mb-4">Image that will be used as evidence. You may only submit 1 image.</p>
                            <Swiper
                                modules={[Pagination, Navigation]}
                                spaceBetween={50}
                                slidesPerView={4}
                                pagination={{ clickable: true }}
                                navigation
                            >
                                {acceptedFiles.map((file: any) => (
                                    <SwiperSlide key={file.path}>
                                        <img src={URL.createObjectURL(file)} />
                                    </SwiperSlide>
                                ))}
                            </Swiper>
                        </div>
                        <div className="mt-8">
                            <Button onClick={handleSubmit} disabled={fineReason.length <= 20 || acceptedFiles.length < 1 || acceptedFiles.length >= 2} className="w-full">Submit fine</Button>
                        </div>
                    </div>

                ) : (
                    <div className="mt-8">
                        <h2 className="text-lg font-semibold">Fine submitted</h2>
                        <p className="text-sm text-gray-500 mb-4">Your fine has been submitted successfully. Please wait while we verify your request.</p>
                    </div>
                )
            }
        </div>
    );
}