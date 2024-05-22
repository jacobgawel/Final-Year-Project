import { HumanizedBookingDto } from '@/types/booking';
import { toast } from 'sonner'
import React from 'react';
import { motion } from 'framer-motion';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@radix-ui/react-select';
import { Check, Copy, Car, ParkingCircle, CalendarRange, Timer, User, Mail, CreditCard } from 'lucide-react';
import { Button } from '@/components/ui/button';


export default function BookingReceipt({ booking }: { booking: HumanizedBookingDto | undefined }) {
    const [copyClicked, setCopyClicked] = React.useState(false);

    function copyToClipboard(text: string) {
        setCopyClicked(true);
        navigator.clipboard.writeText(text);
        toast.success('Order ID copied to clipboard');
        setTimeout(() => {
            setCopyClicked(false);
        }, 2000);
    }
    
    return (
        <motion.div  
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ duration: 0.5 }}
            >
                <Card className="overflow-hidden">
                    <CardHeader className="flex flex-row items-start bg-muted/50">
                        <div className="grid gap-0.5">
                            <CardTitle className="group flex items-center gap-2 text-lg">
                                Booking ID <div className='bg-slate-200 hover:bg-slate-300 rounded pl-2 pr-2'>{booking?.id}</div>
                                {
                                    copyClicked ? (
                                        <Button
                                            size="icon"
                                            variant="outline"
                                            className="h-6 w-6 opacity-100 transition-opacity"
                                        >
                                            <Check className="h-3 w-3" />
                                            <span className="sr-only">Copied</span>
                                        </Button>
                                    ) : (
                                        <Button
                                            size="icon"
                                            variant="outline"
                                            className="h-6 w-6 opacity-0 transition-opacity group-hover:opacity-100"
                                            onClick={() => copyToClipboard(booking?.id || '')}
                                        >
                                            <Copy className="h-3 w-3" />
                                            <span className="sr-only">Copy Order ID</span>
                                        </Button>
                                    )
                                }
                            </CardTitle>
                            <CardDescription>Date: {booking?.humanizedCreatedAtDate}</CardDescription>
                            <div className='flex gap-2'>
                                {
                                    booking?.refundStatus ? (
                                        <div className='mt-2'>
                                            <CardDescription className='flex font-semibold pl-2 pr-2 pt-1 pb-1 bg-slate-200 rounded'>
                                                {booking?.refundStatus ? "Refunded" : "Not Refunded"}
                                            </CardDescription>
                                        </div>
                                    ) : null
                                }
                                {
                                    booking?.bookingConfirmation == false
                                    ? (
                                        <div className='mt-2'>
                                            <CardDescription className='flex font-semibold pl-2 pr-2 pt-1 pb-1 bg-slate-300 rounded'>
                                                Booking Cancelled
                                            </CardDescription>
                                        </div>
                                    ) : null
                                }
                                {
                                    booking?.fineStatus == true
                                    ? (
                                        <div className='mt-2'>
                                            <CardDescription className='flex font-semibold pl-2 pr-2 pt-1 pb-1 bg-slate-300 rounded'>
                                                Fine Issued
                                            </CardDescription>
                                        </div>
                                    ) : null
                                }
                                {
                                    booking?.bookingConfirmation == true
                                    ? (
                                        <div className='mt-2'>
                                            <CardDescription className='flex font-semibold pl-2 pr-2 pt-1 pb-1 bg-slate-300 rounded'>
                                                Booking Confirmed
                                            </CardDescription>
                                        </div>
                                    ) : null
                                }
                            </div>
                        </div>
                    </CardHeader>
                    <CardContent className="p-6 text-sm">
                    <div className="grid gap-3">
                        <div className="font-semibold">Transaction Details</div>
                        <ul className="grid gap-3">
                            <li className="flex items-center justify-between">
                                <span className="text-muted-foreground">Subtotal</span>
                                <span>{booking?.humanizedSubTotal}</span>
                            </li>
                            <li className="flex items-center justify-between">
                                <span className="text-muted-foreground">Fees</span>
                                <span>{booking?.humanizedFees}</span>
                            </li>
                            <li className="flex items-center justify-between font-semibold">
                                <span className="text-muted-foreground">Total</span>
                                <span>{booking?.humanizedTotal}</span>
                            </li>
                            {
                                booking?.refundStatus ? (
                                    <li className="flex items-center justify-between font-semibold">
                                        <span className="text-muted-foreground">Refunded Amount</span>
                                        <span>{booking?.humanizedRefundAmount}</span>
                                    </li>
                                ) : null
                            }
                        </ul>
                    </div>
                    <Separator className="my-4" />
                    <div className="grid grid-cols-2 gap-4">
                        <div className="grid gap-3">
                        <div className="font-semibold">Parking Information</div>
                            <address className="grid gap-0.5 not-italic text-muted-foreground">
                                <span className='flex items-center gap-1 text-muted-foreground'>
                                    <Car className='h-4 w-4' />
                                    Car Registration - {booking?.carRegistration}
                                </span>
                                <span className='flex items-center gap-1 text-muted-foreground'>
                                    <ParkingCircle className='h-4 w-4' />
                                    Parking ID - {booking?.parkingId}
                                </span>
                                <span className='flex items-center gap-1 text-muted-foreground'>
                                    <CalendarRange className='h-4 w-4' />
                                    Booking - {booking?.humanizedBookingSpan}
                                </span>
                                <span className='flex items-center gap-1 text-muted-foreground'>
                                    <Timer className='h-4 w-4' />
                                    Duration - {booking?.humanizedDuration}
                                </span>
                            </address>
                        </div>
                        <div className="grid auto-rows-max gap-3">
                        <div className="font-semibold">Google Maps Link</div>
                        <div className="text-muted-foreground">
                            <img src={booking?.qrCodeLink} height={100} width={100} alt="Parking QR Code" />
                        </div>
                        </div>
                    </div>
                    <Separator className="my-4" />
                    <div className="grid gap-3">
                        <div className="font-semibold">Customer Information</div>
                        <dl className="grid gap-3">
                        <div className="flex items-center justify-between">
                        <dt className="flex items-center gap-1 text-muted-foreground"><User className='h-4 w-4' /> User ID</dt>
                            <dd>{booking?.accountId}</dd>
                        </div>
                        <div className="flex items-center justify-between">
                            <dt className="flex items-center gap-1 text-muted-foreground"><Mail className='h-4 w-4' /> Email</dt>
                            <dd>
                            <a className='hover:underline' href={"mailto:" + booking?.email}>{booking?.email}</a>
                            </dd>
                        </div>
                        </dl>
                    </div>
                    <Separator className="my-4" />
                    <div className="grid gap-3">
                        <div className="font-semibold">Payment Information</div>
                        <dl className="grid gap-3">
                        <div className="flex items-center justify-between">
                            <dt className="flex items-center gap-1 text-muted-foreground">
                            <CreditCard className="h-4 w-4" />
                            Transaction ID
                            </dt>
                            <dd>{booking?.recordId}</dd>
                        </div>
                        </dl>
                    </div>
                    </CardContent>
                    <CardFooter className="flex flex-row items-center border-t bg-muted/50 px-6 py-3">
                    <div className="text-xs text-muted-foreground">
                        Updated {booking?.humanizedLastUpdated}
                    </div>
                    </CardFooter>
                </Card>
            </motion.div>
    )
}