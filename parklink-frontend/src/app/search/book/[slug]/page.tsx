"use client";

import Button from '@mui/material/Button';
import { useSearchParams } from 'next/navigation'
import { useEffect, useState } from 'react';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { TimePicker } from '@mui/x-date-pickers/TimePicker';
import { Paper, TextField, styled } from '@mui/material/';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import dayjs from 'dayjs';
import { CreateBookingForUser, CreateTransactionForUser, GetBookingPricingInfo, GetBookingGapsForParkingId } from "@/app/server/booking/booking";
import { BookingPricingInfoRequest, BookingPricingInfoResponse, BookingRequest } from '@/types/booking';
import LoadingButton from '@mui/lab/LoadingButton';
import { toast } from 'sonner'
import { ValidateLicense } from './LicenseCheck';
import Image from 'next/image';
import Link from 'next/link';
import Skeleton from '@mui/material/Skeleton';
import { Check } from 'lucide-react';
import Checkbox from '@mui/material/Checkbox';
import customParseFormat from "dayjs/plugin/customParseFormat";
import { BookingGapsRequestDto } from '@/types/booking';
import { GetFinesForCurrentUser } from '@/app/server/fine/fine';

import { Swiper, SwiperSlide } from 'swiper/react';
import { motion } from "framer-motion"

// Import Swiper styles
import 'swiper/css';
import 'swiper/css/pagination';
import 'swiper/css/navigation';

// import required modules 
import { Pagination, Navigation } from 'swiper/modules';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';


const BookingPaper = styled(Paper)(({ theme }) => ({
    padding: theme.spacing(1),
    color: theme.palette.text.secondary,
    margin: '1rem',
    backgroundColor: theme.palette.background.default,
    boxShadow: '0px 0px 10px 0px rgba(0,0,0,0.1)',
    borderRadius: '0',
    "&:hover": {
        boxShadow: '0px 0px 10px 5px rgba(0,0,0,0.1)',
    },
}));

async function GetFineForUser() {
    const fines = await GetFinesForCurrentUser();
    return fines;
}


export default function MakeBooking({ params }: { params: { slug: string } }) {
    const [arrival, setArrival] = useState<any>('')
    const [exit, setExit] = useState<any>('')
    const [arrivalTime, setArrivalTime] = useState<any>(null)
    const [exitTime, setExitTime] = useState<any>(null)
    const [plate, setPlate] = useState<any>('')
    const [plateValid, setPlateValid] = useState<boolean>(false)
    const searchData = useSearchParams()
    const [loading, setLoading] = useState<boolean>(false)
    const [carMake, setCarMake] = useState<any>('')
    const [carColour, setCarColour] = useState<any>('')
    const [licenseCheckLoading, setLicenseCheckLoading] = useState<boolean>(false)
    const [validatedPlate, setValidatedPlate] = useState<any>('')
    const [licensePlateError, setLicensePlateError] = useState<boolean>(false)
    const [confirmation, setConfirmation] = useState<boolean>(false)
    const [bookingId, setBookingId] = useState<any>('')
    const [recordId, setRecordId] = useState<any>('')
    const [pricingInfoLoading, setPricingInfoLoading] = useState<boolean>(false)
    const [pricingInfo, setPricingInfo] = useState<BookingPricingInfoResponse>()
    const [cardNumber, setCardNumber] = useState<any>('')
    const [cardNumberValid, setCardNumberValid] = useState<boolean>(false)
    const [cardType, setCardType] = useState<any>('')
    const [exitDate, setExitDate] = useState<any>('')
    const [multipleDays, setMultipleDays] = useState<boolean>(false)
    const [alternativeSlots, setAlternativeSlots] = useState<any>([])
    const [showAlternativeSlots, setShowAlternativeSlots] = useState<boolean>(false)
    const [userHasFines, setUserHasFines] = useState<boolean>(false)
    const [finesLoading, setFinesLoading] = useState<boolean>(false)
    const [ccv, setCcv] = useState<any>('')
    const [expiry, setExpiry] = useState<any>('')
    const [ccvValid, setCcvValid] = useState<boolean>(false)
    const [expiryValid, setExpiryValid] = useState<boolean>(false)
    
    var arrivalParam = searchData.get('arrival')
    var exitParam = searchData.get('exit')
    var timeLimit = searchData.get('timelimit');
    var dayLimit = searchData.get('daylimit');
    var dayLimited = searchData.get('daylimited');
    var timeLimited = searchData.get('timelimited');
    var parkingId = params.slug

    useEffect(() => {
        setFinesLoading(true)
        GetFineForUser().then((res) => {
            if (res.length > 0) {
                setUserHasFines(true)
                setFinesLoading(false)
            }
        })
    }, []) 

    dayjs.extend(customParseFormat);

    const maxTimeObject = dayjs(timeLimit, 'HH:mm:ss');

    // check if quries are empty and set them to state if they are not
    if (arrivalParam != null && exitParam != null) {
        // setting the state to the query params
        useEffect(() => {
            setArrival(arrivalParam)
            setExit(exitParam)
            setArrivalTime(dayjs(arrivalParam))
            setExitTime(dayjs(exitParam))
        }, [])
    }

    // change arrival to dayjs object
    var arrivalDate = dayjs(arrival)
    
    function DetectCardType(cardNumber: any) {
        const patterns: any = {
            visa: /^4[0-9]{12}(?:[0-9]{3})?$/,
            mastercard: /^5[1-5][0-9]{14}$/,
            amex: /^3[47][0-9]{13}$/,
            discover: /^6(?:011|5[0-9]{2})[0-9]{12}$/,
        };

        for (const cardType in patterns) {
            if (patterns[cardType].test(cardNumber) && cardNumber.length == 16) {
                setCardType(cardType)
                setCardNumberValid(true)
                break;
            } else if (cardNumber.length < 16) {
                setCardType('')
                setCardNumberValid(false)
            } else if (cardNumber.length == 16 && /^\d+$/.test(cardNumber)){
                setCardType('unknown')
                setCardNumberValid(true)
            } else {
                setCardType('')
                setCardNumberValid(false)
            }
        }
    }

    const handleChange = (plate: any) => {
        setPlate(plate);
    }


    useEffect(() => {
        // Ensure that both arrivalTime and exitTime are not null before fetching pricing info
        if (arrivalTime && exitTime) {
            CheckPricingInfo();
        }
    }, [arrivalTime, exitTime])


    function BookParkingSpot() {
        setLoading(true)

        var booking: BookingRequest;

        if (cardNumber.length < 16) {
            toast.warning('Card number is invalid!')
            setLoading(false)
            return false;
        }

        var ccvRegex = /^[0-9]{3,4}$/;
        var expiryRegex = /^(0[1-9]|1[0-2])\/?([0-9]{4}|[0-9]{2})$/;

        if (!ccvRegex.test(ccv)) {
            toast.warning('CCV is invalid!')
            setLoading(false)
            return false;
        }

        if (!expiryRegex.test(expiry)) {
            toast.warning('Expiry date is invalid!')
            setLoading(false)
            return false;
        }

        if (multipleDays == false) {
            booking = {
                carRegistration: validatedPlate,
                startDate: arrivalDate.format('YYYY-MM-DD') + 'T' + arrivalTime?.format('HH:mm:ss') + 'Z',
                endDate: arrivalDate.format('YYYY-MM-DD') + 'T' + exitTime?.format('HH:mm:ss') + 'Z',
                parkingId: parkingId,
                recordId: recordId
            }
        } else {
            console.log(exitDate)
            if (exitDate == null || exitDate == '' || exitDate == undefined) {
                toast.warning('Please select an exit date!')
                setLoading(false)
                return false;
            }
            booking = {
                carRegistration: validatedPlate,
                startDate: arrivalDate.format('YYYY-MM-DD') + 'T' + arrivalTime?.format('HH:mm:ss') + 'Z',
                endDate: exitDate.format('YYYY-MM-DD') + 'T' + exitTime?.format('HH:mm:ss') + 'Z',
                parkingId: parkingId,
                recordId: recordId
            }
        }

        CreateTransactionForUser({  
            // create a transaction for the user
            // send the card number, total, fees and sub total
            // this will be used to create a record of the transaction
            // when the recordId is created, it will be used to create a booking
            CardNumber: cardNumber, 
            Total: pricingInfo!.total, 
            Fees: pricingInfo!.fees, 
            SubTotal: pricingInfo!.subTotal }).then((res) => {
            console.log(res)
            if (res.status == 201) {
                setRecordId(res.data.id)
                // set the recordId to the booking object
                booking.recordId = res.data.id

                CreateBookingForUser(booking).then((res) => {
                    // delay to show loading button
                    setTimeout(() => {
                        setLoading(false)
                        console.log(res)
                        if (res.status == 201) {
                            setBookingId(res.data.id)
                            toast.success('Booking Successful!')
                            setConfirmation(true)
                            setPlate('')
                            setCarMake('')
                            setCarColour('')
                            setCardNumber('')
                            setCardType('')
                        }
        
                        if (res.status == 401) {
                            toast.error('User unauthorised!')
                        }
        
                        if (res.status == 400) {
                            toast.warning('Bad request! Check the time and date!')
                        }
        
                        if (res.status == 404) {
                            toast.error('Parking spot is unavailable!')
                        }
        
                        if (res.status == 409) {
                            toast.error('Parking spot at capacity!')
                            var bookingGapsRequest: BookingGapsRequestDto = {
                                bookingDate: booking.startDate,
                                bookingExit: booking.endDate
                            }
                            GetBookingGapsForParkingId(parkingId, bookingGapsRequest).then((res) => {
                                setAlternativeSlots(res.data)
                                setShowAlternativeSlots(true)
                            })
                        }
        
                    }, 800);
                })
            } else {
                toast.error('Transaction Failed!')
                setLoading(false)
            }
        })
    }

    function CheckPricingInfo() {
        var request: BookingPricingInfoRequest;

        if (arrivalTime != null || exitTime != null) {
            if (multipleDays == false) {
                var startDate = arrivalDate.format('YYYY-MM-DD') + 'T' + arrivalTime?.format('HH:mm:ss') + 'Z';
                var endDate = arrivalDate.format('YYYY-MM-DD') + 'T' + exitTime?.format('HH:mm:ss') + 'Z';
                setPricingInfoLoading(true)

                request = {
                    parkingId: parkingId,
                    startDate: startDate,
                    endDate: endDate
                }

                GetBookingPricingInfo(request).then((res) => {
                    setPricingInfo(res);
                    setPricingInfoLoading(false);
                })
            } else {
                // these checks must exist to prevent the function from running before the state is set
                // if this crashes we are in big trouble!!
                if ((exitDate != null && exitDate != '' && exitDate != undefined) && (arrivalDate != null && arrivalDate !== undefined)) {
                    var startDate = arrivalDate.format('YYYY-MM-DD') + 'T' + arrivalTime?.format('HH:mm:ss') + 'Z';
                    var endDate = exitDate.format('YYYY-MM-DD') + 'T' + exitTime?.format('HH:mm:ss') + 'Z';
                    setPricingInfoLoading(true)

                    request = {
                        parkingId: parkingId,
                        startDate: startDate,
                        endDate: endDate,
                    }

                    GetBookingPricingInfo(request).then((res) => {
                        setPricingInfo(res);
                        setPricingInfoLoading(false);
                    })
                }
            }
        }
    }

    // quick check to see if the plate is valid, this will run the plate against the dvla api
    function ValidatePlate() {
        setCarMake('')
        setCarColour('')
        setPlateValid(false)
        setLicensePlateError(false)
        setLicenseCheckLoading(true)
        console.log(plate)
        if (plate != '') {
            ValidateLicense(plate).then((res) => {
                console.log(res)
                if (res.data == null) {
                    toast.error('License Plate not found!')
                    setLicenseCheckLoading(false)
                    setPlateValid(false)
                    setLicensePlateError(true)
                    return false;
                }
                setCarMake(res.data.make)
                setCarColour(res.data.colour)
                // delay to show loading button
                setTimeout(() => {
                    setLicenseCheckLoading(false)
                    if (res.status == 200) {
                        toast.success('Valid License Plate!')
                        setPlateValid(true)
                        setValidatedPlate(plate)
                        setLicensePlateError(false)
                    }
                }, 800);
            })
        } else {
            return false;
        }
    }

    return (
        <>
        <LocalizationProvider dateAdapter={AdapterDayjs}>
            <BookingPaper className='m-5'>
                {
                    confirmation == false ? 
                    <>
                <div>
                    {
                        finesLoading == false && userHasFines == true ?
                        <div className='ml-4 mt-4 flex'>
                            <div className='text-xl font-semibold bg-red-200 rounded p-2 mb-2'>
                                You cannot book a parking spot until you have paid your fines
                                <Link className='ml-5 text-blue-500 hover:text-blue-700 font-bold' href='/account/fines'>
                                    Pay Fines
                                </Link>
                            </div>
                        </div> : null
                    }
                    <div className='ml-4 mt-4 flex'>
                        <div className='text-xl font-semibold bg-slate-200 rounded p-2 mb-2'>
                            Your parking session
                        </div>
                    </div>
                    <div className='flex gap-1 mb-2 font-semibold'>
                        <div className='ml-4 mt-2 bg-slate-200 p-2 rounded'>
                            Parking ID {parkingId}
                        </div>
                        {
                            timeLimited == 'true' ? <div className='ml-4 mt-2 bg-slate-200 p-2 rounded'>
                                This spot has a time limit of {timeLimit}
                            </div> : null
                        }
                        {
                            dayLimited == 'true' ? <div className='ml-4 mt-2 bg-slate-200 p-2 rounded'>
                                This spot is limited to {dayLimit} days only
                            </div> : null
                        }
                    </div>
                </div>
                <div className="flex">
                    <div className='m-4'>
                        {
                            arrivalParam != null ? <DatePicker minDate={dayjs(new Date())} label='Date of arrival' value={arrivalDate} onChange={(date) => setArrival(date)} /> : <DatePicker minDate={dayjs(new Date())} label='Date of arrival' onChange={(date) => setArrival(date)} />
                        }
                    </div>
                    <div className='m-4'>
                        <TimePicker minTime={dayjs(arrival).isSame(dayjs(new Date()), 'day') ? dayjs(new Date()) : null} ampm={false} minutesStep={30} label="Time of arrival" value={arrivalTime} onChange={ function(arrival) { setArrivalTime(arrival); CheckPricingInfo() }} />
                    </div>
                    <div className='m-4'>
                        <Checkbox checked={multipleDays} onChange={(e) => setMultipleDays(e.target.checked)} />
                        <DatePicker minDate={arrivalDate.add(1, 'day')} label='Date of exit' onChange={ function(date) { setExitDate(date); } } disabled={!multipleDays} />
                    </div>
                    <div className='m-4'>
                        {
                            multipleDays 
                            ? <TimePicker ampm={false} minutesStep={30} label="Time of exit" value={exitTime} onChange={ function(exit) { setExitTime(exit); CheckPricingInfo() }} /> 
                            : timeLimited == 'true' 
                                ? <TimePicker minTime={dayjs(arrivalTime).add(30, 'minutes')} ampm={false} minutesStep={30} maxTime={dayjs(arrivalTime).add(maxTimeObject.hour(), 'hours')} 
                                    label="Time of exit" value={exitTime} onChange={ function(exit) { setExitTime(exit); CheckPricingInfo() }} />
                                : <TimePicker minTime={dayjs(arrivalTime).add(30, 'minutes')} ampm={false} minutesStep={30} label="Time of exit" value={exitTime} onChange={ function(exit) { setExitTime(exit); CheckPricingInfo() }} />
                        }
                    </div>
                </div>

                <div className='ml-4 mt-2'>
                    Your vehicle
                </div>
                <div className='flex'>
                    <div className='ml-4 mt-2 mb-4'>
                        <TextField error={licensePlateError} label="Registration Plate" value={plate} onChange={(e) => handleChange(e.target.value)} />
                    </div>
                    <LoadingButton disabled={plate == '' ? true : false} loading={licenseCheckLoading} variant='outlined' color='success' className="ml-2 mt-2 mb-4" onClick={() => ValidatePlate()}>
                        Verify Plate
                    </LoadingButton>
                    <div className='ml-4 mt-2 mb-4'>
                        <TextField label="Car Make" value={carMake} disabled />
                    </div>
                    <div className='ml-4 mt-2 mb-4'>
                        <TextField label="Car Colour" value={carColour} disabled />
                    </div>
                </div>
                <div className='ml-4 mt-2'>
                    <div className={"mb-2"}>
                        Payment
                    </div>
                    <Label>Card Number</Label>
                    <Input className='w-[160px]' value={cardNumber} onChange={ function(e) { setCardNumber(e.target.value); DetectCardType(e.target.value) }} />
                    <Label>Expiry</Label>
                    <Input className='w-[160px]' value={expiry} onChange={ function(e) { setExpiry(e.target.value); }} />
                    <Label>CCV</Label>
                    <Input className='w-[160px]' value={ccv} onChange={ function(e) { setCcv(e.target.value); }} />
                </div>
                <div className='flex ml-4 mt-2'>
                    Card Type: {cardType}
                </div>

                { carMake != '' ? <div className='ml-4 mt-2 mb-4'>
                        <Image src={`https://logo.clearbit.com/${carMake}.com`} alt={carMake} width={100} height={100} />
                    </div> : null
                }
                <div className={"flex"}>
                    {
                        pricingInfo != null && pricingInfo != undefined ? pricingInfoLoading ? 
                        <div className='ml-4 mt-2 bg-slate-200 rounded p-4'>
                            <Skeleton width={80} animation="wave" />
                            <Skeleton width={80} animation="wave" />
                            <Skeleton width={100} animation="wave" />
                        </div> : 
                        <div className='ml-4 mt-2 bg-slate-200 rounded p-4'>
                            <div className='text-sm font-bold'>Total</div>
                            <div className='text-sm font-semibold'>Sub Total - {pricingInfo?.humanizedSubTotal}</div>
                            <div className='text-sm font-semibold'>Fees - {pricingInfo?.humanizedFees}</div>
                            <div className='text-xl font-bold'>Total - {pricingInfo?.humanizedTotal}</div>
                        </div> : null
                    }
                </div>
                <div className='mt-3 ml-4 mb-4'>
                    {
                        (parkingId != "" && arrivalDate != null && exitTime != null && plate != "" && plateValid == true && cardNumberValid && userHasFines == false) ?
                        <LoadingButton variant='outlined' color='primary' onClick={() => BookParkingSpot()} loading={loading}>
                            Book
                        </LoadingButton>
                        :
                        <Button variant='outlined' color='primary' disabled>
                            Book
                        </Button>
                    }
                </div>
                {
                   showAlternativeSlots ? 
                   <>
                    <div className={"flex"}>
                    <div className='flex ml-5 mt-5 text-xl font-semibold bg-slate-200 rounded p-2'>
                        Alternative slots
                    </div>
                    </div>
                    <div className="flex justify-center items-center mb-10 ml-5 mr-8">
                        <Swiper 
                            pagination={{ clickable: true }}
                            navigation={true}
                            className="mt-5"
                            spaceBetween={25}
                            slidesPerView={4}
                            modules={[Pagination, Navigation]}
                        >
                            {
                                alternativeSlots.map((slot: any, key: any) => {
                                    return (
                                        <SwiperSlide key={key}>
                                            <motion.div 
                                                whileHover={{ scale: 1.1 }} 
                                                whileTap={{ scale: 0.9 }} 
                                                className='ml-4 mt-4 bg-slate-200 hover:bg-slate-300 p-10 mb-10 rounded font-semibold'
                                            >
                                                {slot.humanizedDateTime}
                                            </motion.div>
                                        </SwiperSlide>
                                    )
                                })
                            }
                        </Swiper>
                    </div>
                    </>
                    : null
                }
                </>
                :
                <>
                    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
                        <div className="w-full max-w-sm">
                            <div className='flex justify-center mb-4'>
                               <Check size={48} className="text-green-400" />
                            </div>
                            <div>
                                <p className='text-slate-600 text-xl font-semibold'>
                                    Thank you for your purchase! Your booking ID is: {bookingId}
                                </p>
                            </div>
                            <div className='mt-4 flex justify-center'>
                                <Link href={'/account/bookings/' + bookingId}>
                                    <Button variant='outlined' color='primary'>View Bookings</Button>
                                </Link>
                            </div>
                        </div>
                    </div>
                </>
                }
            </BookingPaper>
        </LocalizationProvider>
        </>
    )
}