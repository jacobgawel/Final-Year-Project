import React, { useState, useEffect } from 'react';
import { Parking } from '@/types/parking';
import { Booking, BookingRefundDto, BookingUpdateDto, HumanizedBookingDto } from '@/types/booking';
import Link from 'next/link';
import { GetParkingById } from '@/app/server/parking/parking';
import mapboxgl from 'mapbox-gl';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import Chip from '@mui/material/Chip';
import { Paper, styled } from '@mui/material';
import IconButton from '@mui/material/IconButton';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import DeleteRoundedIcon from '@mui/icons-material/DeleteRounded';
import { UpdateBookingForUser, GetBookingRefundDetails } from '@/app/server/booking/booking';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { toast } from 'sonner'


async function getParkingById(id: string) {
    // get the current user from the auth actions
    const parking = await GetParkingById(id);
    
    if(parking.status === 200) {
        // return the parking data e.g. all the details for the parking
        return parking.data;
    }

    if(parking.status === 401) {
        console.log('Unauthorized access to parking', parking.status);
    }

    return [];
}

async function getParkingRefundDetails(id: string) {
    // get the current user from the auth actions
    const refundDetails = await GetBookingRefundDetails(id);
    
    if(refundDetails.status === 200) {
        // return the parking data e.g. all the details for the parking
        return refundDetails.data;
    }

    if(refundDetails.status === 401) {
        console.log('Unauthorized access to parking', refundDetails.status);
    }

    return [];

}



const BookingPaper = styled(Paper)(({ theme }) => ({
    padding: theme.spacing(1),
    color: theme.palette.text.secondary,
    margin: '1rem',
    backgroundColor: theme.palette.background.default,
    boxShadow: 'none',
    borderRadius: '0',
    '&:hover': {
        boxShadow: '0px 0px 10px 0px rgba(0,0,0,0.1)',
        backgroundColor: "#f9f9f9"
    },
}));

export default function BookingCard({ booking, mapObject }: { booking: HumanizedBookingDto, mapObject: mapboxgl.Map | null }) {
    // get the date and time from the booking
    var date = booking.startDate.split('T')[0];
    var time = booking.startDate.split('T')[1];

    const [parkingDetails, setParkingDetails] = useState<Parking | null>(null);
    const [refundDetails, setRefundDetails] = useState<BookingRefundDto | null>(null);

    var bookingUpdate: BookingUpdateDto = {
        id: booking.id,
        carRegistration: booking.carRegistration,
        startDate: booking.startDate,
        endDate: booking.endDate,
        fineStatus: booking.fineStatus,
        bookingConfirmation: booking.bookingConfirmation,
        duration: booking.duration,
        subTotal: booking.subTotal,
        total: booking.total,
        fees: booking.fees,
        qrCodeLink: booking.qrCodeLink
    }

    // this function is called when the view location button is clicked
    // it will fly to the location of the parking on the map
    const viewLocation = (lat: any, long: any) => {
        console.log("View Location clicked");
        if (long != null && lat != null) {
            console.log("Long:", long, "Lat:", lat)
            try {
                if (mapObject) {
                    console.log("Map instance is available");
            
                    if (mapObject.loaded()) {
                        console.log("Map is loaded, executing flyTo");
                        
                        mapObject.flyTo({
                            center: [long, lat], // Replace with desired coordinates
                            zoom: 15,
                            speed: 1.5, // Increase the speed
                            curve: 1.42, // Adjust the curve for a more direct path
                            maxDuration: 6000, // Maximum duration of the flight in milliseconds
                            essential: true // This animation is considered essential with respect to prefers-reduced-motion
                        });

                        // Adding error event listener for debugging
                        mapObject.on('error', (e) => {
                            console.error("Map error:", e.error);
                        });
                    } else {
                        console.log("Map is not fully loaded");
                    }
                } else {
                    console.log("Map instance is not available");
                }
            } catch (error) {
                console.error("Error viewing location:", error);
            }
        } else {
            console.error("Long or Lat is null");
        }
    };

    // get the parking id from the booking
    const parkingId = booking.parkingId;

    // get the parking data
    useEffect(() => {
        getParkingById(parkingId).then((parkingDetails: Parking) => {
            setParkingDetails(parkingDetails);
        });
    }, []);


    if (parkingDetails != null && mapObject) {
        if (parkingDetails.longitude && parkingDetails.latitude) {
            try {
                new mapboxgl.Marker()
                .setLngLat([parkingDetails.longitude, parkingDetails.latitude])
                .addTo(mapObject);
            } catch (error) { // this error is caught if the long and lat are not available
                // usually this may happen if the parking spot has been deleted
                console.error("Error adding marker to map:", error);
            }
        }
    }

    const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
    const openDropdown = Boolean(anchorEl);
    const handleClickDropdown = (event: React.MouseEvent<HTMLElement>) => {
      setAnchorEl(event.currentTarget);
    };
    const handleCloseDropdown = () => {
      setAnchorEl(null);
    };

    const [openDialog, setOpenDialog] = React.useState(false);

    const handleClickOpenDialog = () => {
        // TODO: Logic to calculate the refund amount should be added here
        // get the refund details
        getParkingRefundDetails(booking.id).then((refundDetails: BookingRefundDto) => {
            setRefundDetails(refundDetails);
        });
        
        setAnchorEl(null);
        console.log('Open dialog');
        setOpenDialog(true);
    };

    const handleCloseDialog = () => {
        console.log('Close dialog');
        setOpenDialog(false);
    };

    async function UpdateBooking(bookingUpdate: BookingUpdateDto) {
        // get the current user from the auth actions
        booking.bookingConfirmation = false;
        bookingUpdate.bookingConfirmation = false;
        // function must be here due to the dialog needing to close before the toast is shown
        handleCloseDialog();
        const updatedBooking = await UpdateBookingForUser(bookingUpdate);
        
        if(updatedBooking.status === 200) {
            // return the parking data e.g. all the details for the parking
            console.log(updatedBooking.data)
            toast.success('Booking cancelled successfully')
        }
    
        if(updatedBooking.status === 401) {
            booking.bookingConfirmation = true;
            console.log('Unauthorized access to parking', updatedBooking.status);
            toast.error('Error cancelling booking')
        }
    
        if(updatedBooking.status === 400) {
            booking.bookingConfirmation = true;
            console.log('Error cancelling booking', updatedBooking.status);
            toast.error('Error cancelling booking')
        }
    }

    // check to see if the booking is ongoing
    var bookingStartDateTime = new Date(booking.startDate);
    var bookingEndDateTime = new Date(booking.endDate);
    
    // checks to see if the booking is ongoing, finished or future
    var bookingOngoing = bookingStartDateTime < new Date() && bookingEndDateTime > new Date() && booking.bookingConfirmation;
    var bookingFinished = bookingEndDateTime < new Date() && booking.bookingConfirmation;
    var bookingFuture = bookingStartDateTime > new Date() && booking.bookingConfirmation;

    var checkResult = ""
    var pillColor: any = "" // this is the color of the pill that will be displayed on the booking card
    if (bookingOngoing) {
       checkResult = 'Ongoing';
       pillColor = 'success';
    } else if (bookingFinished) {
        checkResult = 'Finished';
        pillColor = 'warning';
    } else if (bookingFuture) {
        checkResult = 'Confirmed';
        pillColor = 'primary';
    }

    return (
        <>
        <BookingPaper>
            <div className='flex mt-2 ml-2 justify-right'>
                { booking.fineStatus && <Chip variant='outlined' label={"Fine Issued"} color={"error"} size="small" className='m-1' /> }
                {
                    !booking.bookingConfirmation ? <Chip variant='outlined' label={"Cancelled"} color={"error"} size="small" className='m-1' /> : null
                }
                {
                    booking.bookingConfirmation ? <Chip variant='outlined' label={checkResult} color={pillColor} size="small" className='m-1' /> : null
                }
            </div>
            <p className='m-2 text-xs italic text-slate-400'>id: {booking.id}</p>
            <h5 className="m-2 text-2xl font-bold tracking-tight text-gray-900 dark:text-white">
                {booking.humanizedTotal}
            </h5>
            <p className="m-2 font-normal text-gray-700 dark:text-gray-400">
                {booking.humanizedDate}
            </p>
            <p className="m-2 font-normal text-gray-700 dark:text-gray-400">
                {booking.humanizedDuration}
            </p>
            <p className='m-2 font-normal text-xs text-gray-700 dark:text-gray-400'>
                Purchased: {booking.humanizedCreatedAt}
            </p>
            <div className='flex justify-center m-4'>
                <Stack spacing={2} direction="row">
                    <Link href={`/account/bookings/${booking.id}`}>
                        <Button variant='outlined' color='primary'>View Booking</Button>
                    </Link>
                    <Button variant='outlined' color='secondary' onClick={() => parkingDetails && viewLocation(parkingDetails.latitude, parkingDetails.longitude)}>View Location</Button>
                    <IconButton
                        aria-label="more"
                        id="long-button"
                        aria-controls={openDropdown ? 'long-menu' : undefined}
                        aria-expanded={openDropdown ? 'true' : undefined}
                        aria-haspopup="true"
                        onClick={handleClickDropdown}
                    >
                        <MoreVertIcon />
                    </IconButton>
                    <Menu
                        id="long-menu"
                        MenuListProps={{
                        'aria-labelledby': 'long-button',
                        }}
                        anchorEl={anchorEl}
                        open={openDropdown}
                        onClose={handleCloseDropdown}
                    >
                        <MenuItem onClick={handleClickOpenDialog} disabled={!booking.bookingConfirmation || bookingEndDateTime < new Date()}>
                            <DeleteRoundedIcon /> Cancel Booking
                        </MenuItem>
                    </Menu>
                </Stack>
            </div>
        </BookingPaper>
                <Dialog
            open={openDialog}
            onClose={handleCloseDialog}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title">
            {"Are you sure you want to cancel this booking?"}
            </DialogTitle>
            <DialogContent>
            <DialogContentText id="alert-dialog-description" className="text-gray-800">
                {refundDetails  ? (
                    <div className="border border-gray-300 rounded p-4">
                    <div className="flex justify-between mb-2">
                        <p className="font-semibold">Refund Details</p>
                        {
                            // if the booking is not refundable, display a red pill
                            refundDetails.noRefund == true ? <div className="bg-red-200 px-2 rounded">No Refund</div>
                            : <div className="bg-gray-200 px-2 rounded">{refundDetails.fullRefund ? "Full Refund" : "Partial Refund"}</div>
                        }
                    </div>
                    {
                        // if the booking is not refundable, display an alternate message
                        // otherwise display the refund details
                        // bookings that are due in 4 hours or less are not refundable
                        refundDetails.noRefund == true ? <p className="italic">This booking is not refundable</p> :
                        <div>
                        <p className="mb-2">Percentage <span className="bg-gray-200 px-2 rounded">{refundDetails.refundPercentage}</span></p>
                        <p>Total <span className="bg-gray-200 px-2 rounded">{refundDetails.refundAmount}</span></p>
                        </div>
                    }
                    </div>
                ) : (
                    <p className="italic">Calculating refund details...</p>
                )}
                </DialogContentText>
            </DialogContent>
            <DialogActions>
            <Button color={"info"} onClick={handleCloseDialog}>NO</Button>
            <Button color={"error"} onClick={() => UpdateBooking(bookingUpdate)} autoFocus>
                YES
            </Button>
            </DialogActions>
        </Dialog>
        </>
    )
}