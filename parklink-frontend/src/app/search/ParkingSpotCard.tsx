import * as React from 'react';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import { Button, CardActionArea, CardActions } from '@mui/material';
import { Parking, ParkingImage, S3Image } from '@/types/parking';
import Chip from '@mui/material/Chip';
import Stack from '@mui/material/Stack';
import Rating from '@mui/material/Rating';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Drawer from '@mui/material/Drawer';
import { BookingInfo } from '@/types/booking';
import Link from 'next/link';
import { Swiper, SwiperSlide } from 'swiper/react';
import { motion } from "framer-motion"
import { GetReviewsForParking } from '../server/reviews/reviews';
import { CreateReview } from '../server/reviews/reviews';

// Import Swiper styles
import 'swiper/css';
import 'swiper/css/pagination';
import 'swiper/css/navigation';

// import required modules 
import { Pagination, Navigation } from 'swiper/modules';
import ReviewCard from './ReviewCard';
import { Input } from '@/components/ui/input';


export default function ParkingSpotCard({ parking, bookingInfo, mapObject, distance, distanceObj }: { parking: Parking, bookingInfo: BookingInfo | undefined, mapObject: mapboxgl.Map | null, distance: number, distanceObj: boolean }) {
    const [parkingDialog, setParkingDialog] = React.useState(false);
    const [reviewDrawerState, setReviewDrawerState] = React.useState(false);
    const [reviews, setReviews] = React.useState([]);
    const [reviewLoading, setReviewLoading] = React.useState(true);
    const [reviewRating, setReviewRating] = React.useState(0);
    const [userReviewText, setUserReviewText] = React.useState('');
    const [userReviewRating, setUserReviewRating] = React.useState<any>(0);

    var availability = parking.availabilityStatus ? 'Available' : 'Unavailable';
    var color: any = parking.availabilityStatus ? 'success' : 'default'; // added any type to fix error with color

    // dialog functions
    const handleParkingDialog = () => {
        setParkingDialog(true);
    };

    const handleParkingDialogClose = () => {
        setParkingDialog(false);
    };

    function toggleReviewDrawer() {
        setReviewDrawerState(!reviewDrawerState);
        setParkingDialog(false); // close the dialog when opening the drawer to avoid errors with the dialog
    }

    // get reviews for parking spot
    React.useEffect(() => {
        GetReviewsForParking(parking.id).then((res) => {
            setReviews(res.reviews);
            setReviewRating(res.parkingRating);
            setReviewLoading(false);
        });
    }, []);

    // create review
    function submitReview() {
        CreateReview({
            reviewRating: userReviewRating,
            reviewText: userReviewText,
            parkingId: parking.id
        }).then((res) => {
            setUserReviewRating(0);
            setUserReviewText('');
            GetReviewsForParking(parking.id).then((res) => {
                setReviews(res.reviews);
                setReviewRating(res.parkingRating);
                setReviewLoading(false);
            });
        });
    }

    // max is the initial query paramater that is assigned
    // it contains the first query paramater
    var bookingBody = `?timelimit=${parking.timeLimit}&daylimit=${parking.dayLimit}&daylimited=${parking.dayLimited}&timelimited=${parking.timeLimited}`;

    if (bookingInfo) {
        // if bookingInfo exists, the rest of the params are appended to the query
        bookingBody += `&arrival=${bookingInfo.arrival}&exit=${bookingInfo.exit}`
    }

    var s3ImageObj: ParkingImage = JSON.parse(parking.slotImages);

    // view location functionality
    const viewLocation = (lat: any, long: any) => {
        console.log("View Location clicked");
        console.log("Long:", long, "Lat:", lat)
    
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
    };

    return (
        <>
        <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.5 }}
            >
            <Card sx={{ maxWidth: 345 }} variant='elevation'>
                <CardActionArea onClick={handleParkingDialog}>
                <CardContent> 
                    <div 
                        className='flex mt-2'>
                        <Stack direction="row" spacing={1}>
                            <Chip color={color} variant='outlined' label={availability} size='small' />
                        </Stack>
                        <Rating
                            className='ml-2 mt-1'
                            size='small'
                            name="parking-rating"
                            value={reviewRating ? reviewRating : 0}
                            readOnly
                        />
                        <div className="text-xs mt-1.5 ml-2">
                            {reviewRating}
                        </div>
                        {
                            distanceObj == true ? (
                                <div className='text-xs bg-slate-300 rounded pl-1 pr-1 font-semibold ml-3 mt-1.5'>
                                        {distance.toFixed(2)} miles
                                </div>
                            ) : (
                                null
                            )
                        }
                    </div>
                    <div 
                        className='mt-2'>
                        {parking.address}
                    </div>
                    <div className='font-medium text-xs mt-2'>{parking.id}</div>
                    <div
                        className='mt-2 text-md'>
                        £{parking.price.toFixed(2)}
                    </div>
                    <div className='mt-2 text-sm flex'>
                        {/* {parking.slotType} - {parking.slotSize} - Max time: {parking.timeLimit} */}
                        <div className={"flex gap-2"}>
                            <div className={"text-xs bg-slate-200 p-1 rounded"}>
                                {parking.slotType}
                            </div>
                            <div className={"text-xs bg-slate-200 p-1 rounded"}>
                                {parking.slotSize}
                            </div>
                            {
                                parking.timeLimited ? 
                                <div className={"text-xs bg-slate-200 p-1 rounded"}>
                                    {parking.timeLimit} limit
                                </div> : null
                            }
                            {
                                parking.dayLimited ? 
                                <div className={"text-xs bg-slate-200 p-1 rounded"}>
                                    {parking.dayLimit} day limit
                                </div> : null
                            }
                        </div>
                    </div>
                </CardContent>
                </CardActionArea>
                <CardActions className='ml-2'>
                    <Button size="small" color='secondary'
                    onClick={() => parking && viewLocation(parking.latitude, parking.longitude)}>
                        View on Map
                    </Button>
                    {
                        parking.availabilityStatus ? (
                            <Link href={`/search/book/${parking.id}` + bookingBody}>
                                <Button variant='outlined' size="small" color="primary">
                                    Book now
                                </Button>
                            </Link>
                        ) : (
                            <Button variant='outlined' size="small" color="primary" disabled>
                                Book now
                            </Button>
                        )
                    }
                </CardActions>
            </Card>
        </motion.div>
        <Dialog
            open={parkingDialog}
            onClose={handleParkingDialogClose}
            >

            <DialogTitle id="alert-dialog-title">
                <div className='font-bold text-xl'>
                    Parking Spot Details
                </div>
            </DialogTitle>
            <DialogContent>
                <div className='flex'>
                    <Rating
                        className='mt-1 mr-3'
                        size='small'
                        name="parking-rating"
                        value={reviewRating}
                        readOnly
                    />
                    <Chip color="default" variant='outlined' label="Reviews" size='small' clickable onClick={toggleReviewDrawer} />
                </div>
                <div className={"flex mt-4 mb-4 gap-3"}>
                    <div className='mt-2'>
                        <div className='font-semibold text-xs'>Parking Type:</div>
                        <div className='text-xs'>{parking.slotType}</div>
                    </div>
                    <div className='mt-2'>
                        <div className='font-semibold text-xs'>Parking Size:</div>
                        <div className='text-xs'>{parking.slotSize}</div>
                    </div>
                    <div className='mt-2'>
                        <div className='font-semibold text-xs'>Availability:</div>
                        <div className='text-xs'>{parking.availabilityStatus ? 'Available' : 'Unavailable'}</div>
                    </div>
                    <div className='mt-2'>
                        <div className='font-semibold text-xs'>Capacity:</div>
                        <div className='text-xs'>{parking.slotCapacity}</div>
                    </div>
                </div>
                <div className='mt-2 bg-slate-200 p-2 rounded'>
                        <div className='font-semibold text-xs'>Additional features:</div>
                        <div className='text-xs'>{parking.additionalFeatures}</div>
                    </div>
                <div className='mt-2 bg-slate-200 rounded p-1'>
                    <div className={"text-xs"}>
                        {
                            parking.timeLimited ? 
                            <div className={"text-xs p-1 rounded"}>
                                {parking.timeLimit} limit
                            </div> : null
                        }
                        {
                            parking.dayLimited ?
                            <div className={"text-xs p-1 rounded"}>
                                {parking.dayLimit} day limit
                            </div> : null
                        }
                    </div>
                </div>
                
                <div className='mt-2 bg-slate-200 p-2 rounded'>
                    <div className='font-semibold text-xs'>Location:</div>
                    <div className='text-xs'>{parking.address}</div>
                </div>
                <div className='mt-2'>
                    <div className='bg-slate-200 rounded p-3'>
                        <div className={"font-bold text-md"}>
                            £{parking.price.toFixed(2)} per hour
                        </div>
                    </div>
                </div>
                <div className='mt-8 mb-2'>
                <Swiper modules={[Pagination, Navigation]} spaceBetween={10} slidesPerView={2} pagination={{ clickable: true }} navigation={true}>
                    {
                        parking.slotImages == null || s3ImageObj.s3ImageUris.length == 0 ?
                            <>
                            <SwiperSlide>
                                    <img src="https://placehold.co/400x300" alt="Parking Spot Image" height={300} width={400} />
                            </SwiperSlide>
                            <SwiperSlide>
                                    <img src="https://placehold.co/400x300" alt="Parking Spot Image" height={300} width={400} />
                            </SwiperSlide>
                            </>
                        : 
                        s3ImageObj.s3ImageUris.map((image: S3Image, index) => {
                            return (
                                <SwiperSlide key={index}>
                                    <img src={image.fileUri} alt="Parking Spot Image" height={300} width={300} />
                                </SwiperSlide>
                            )
                        })
                    }
                </Swiper>
                </div>
            </DialogContent>
            <DialogActions>
                <Button variant='outlined' onClick={handleParkingDialogClose}>
                    Close
                </Button>
            </DialogActions>
        </Dialog>

        <Drawer
            anchor="left"
            open={reviewDrawerState}
            onClose={toggleReviewDrawer}
        >
            <div className='mb-3'>
                <div className='ml-5 mt-10 mr-10 font-semibold'>
                    Reviews - {parking.address}
                </div>
                <div className='mt-1 ml-5 flex'>
                    <Rating
                        className='mt-1'
                        size='small'
                        name="parking-rating"
                        value={reviewRating}
                        readOnly
                    />
                    <div className="text-xs mt-1.5 ml-2">
                        {reviewRating}
                    </div>
                </div>
            </div>
            <div className='ml-5 mt-5'>
                <div className='font-semibold'>
                    Write a review
                </div>
            </div>
            <div className='ml-5 mt-5'>
                Leave a rating: 
                <Rating
                    className='ml-3 mt-1'
                    size='small'
                    name="review-rating"
                    value={userReviewRating}
                    onChange={(event, newValue) => {
                        setUserReviewRating(newValue);
                    }}
                />
            </div>
            <div className='ml-5 mt-5 mr-5'>
                <Input placeholder="Write a review..." onChange={(event) => setUserReviewText(event.target.value)} />
                <Button variant='outlined' className='mt-2' onClick={submitReview}>
                    Submit
                </Button>
            </div>
            {
                reviewLoading ? (
                    <div className='ml-5 mt-5'>
                        Loading reviews...
                    </div>
                ) : (
                    reviews && reviews.map((review: any, index) => {
                        return (
                            <ReviewCard key={index} review={review} />
                        )
                    })
                )
            }
        </Drawer>
        </>
      );
}

