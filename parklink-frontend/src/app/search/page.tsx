"use client"

import React from 'react'
import { GetParkingByDistance, GetParkingData } from '@/app/server/parking/parking'
import { useState, useEffect } from 'react'
import { Parking, ParkingDistanceReturn } from '@/types/parking'
import { useSearchParams } from 'next/navigation'
import useMapboxMap from '@/app/components/UseMapBoxMap'
import 'mapbox-gl/dist/mapbox-gl.css';
import mapboxgl from 'mapbox-gl';
import dayjs from 'dayjs'
import ParkingSpotCard from './ParkingSpotCard'
import Skeleton from '@mui/material/Skeleton';
import { BookingInfo } from '@/types/booking'
import Image from 'next/image'
import Grid from '@mui/material/Grid';

// swiper imports
import { Swiper, SwiperSlide } from 'swiper/react'
import { Navigation, Pagination, } from 'swiper/modules';
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';

function formatDuration(durationInSeconds: number) {
    let seconds = Math.floor(durationInSeconds % 60);
    let minutes = Math.floor((durationInSeconds / 60) % 60);
    let hours = Math.floor(durationInSeconds / 3600);

    // Pad the minutes and seconds with leading zeros if they are less than 10
    let paddedHours = hours.toString().padStart(2, '0');
    let paddedMinutes = minutes.toString().padStart(2, '0');
    let paddedSeconds = seconds.toString().padStart(2, '0');

    return `${paddedHours}:${paddedMinutes}:${paddedSeconds}`;
}

function formatMinutes(minutes: any): string {
    if (minutes == null) {
        return '00:00:00'
    }
    
    let hours = Math.floor(minutes / 60);
    let remainingMinutes = minutes % 60;

    // Padding the hours and minutes with '0' if they are less than 10
    let formattedHours = hours.toString().padStart(2, '0');
    let formattedMinutes = remainingMinutes.toString().padStart(2, '0');

    return `${formattedHours}:${formattedMinutes}:00`;
}

export default function MakeBooking() {

    const [normalParking, setNormalParking] = useState<any>([])
    const searchData = useSearchParams()
    const [duration, setDuration] = useState<any>('')
    const [loading, setLoading] = useState<boolean>(true)
    const [bookingInfo, setBookingInfo] = useState<BookingInfo>()
    const [distanceParkingData, setDistanceParkingData] = useState<any>([])

    var q = searchData.get('q')
    var lat = searchData.get('lat')
    var long = searchData.get('long')
    var arrival = searchData.get('arrival') // HH:MM
    var date = searchData.get('date') // YYYY-MM-DD
    var exit = searchData.get('exit') // HH:MM

    // check if quries are empty
    if (q == null) {
        q = ''
    }

    if (lat == null) {
        lat = ''
    }

    if (long == null) {
        long = ''
    }

    if (arrival == null) {
        arrival = ''
    }

    if (date == null) {
        date = ''
    }

    if (exit == null) {
        exit = ''
    }

    useEffect(() => {
        // check if arrival, date and exit are not empty, if they are not empty, calculate the duration of the stay
        if (arrival != '' && date != '' && exit != '') {
            // Combine date and time for arrival and exit
            var arrivalDateTime = dayjs(`${date}T${arrival}`);
            var exitDateTime = dayjs(`${date}T${exit}`);

            // calculate the duration of the stay
            var tempDuration = exitDateTime.diff(arrivalDateTime, 'seconds');
            setDuration(formatDuration(tempDuration))

            // set booking info
            setBookingInfo({
                arrival: arrivalDateTime.format('YYYY-MM-DDTHH:mm:ss'),
                exit: exitDateTime.format('YYYY-MM-DDTHH:mm:ss'),
                duration: formatDuration(tempDuration)
            })

            // format the duration to be a TimeSpan format string
            var duration: string = formatMinutes(exitDateTime?.diff(arrivalDateTime, 'minute'));
            console.log("Duration: " + duration);
            
            GetParkingByDistance(Number(lat), Number(long), duration).then((data) => {
                setDistanceParkingData(data);
                setLoading(false);
            }).catch((error) => {
                console.error('Error fetching data:', error);
                // Handle error here if needed
                setLoading(false);
            });
        } else {
            // Fetch data only if the parameters are empty
            GetParkingData().then((data) => {
                setNormalParking(data);
                setLoading(false);
            }).catch((error) => {
                console.error('Error fetching data:', error);
                // Handle error here if needed
                setLoading(false);
            });
        }
    }, [arrival, date, exit, lat, long])
    
    var map = useMapboxMap()

    if (map && map.loaded()) {
        if (lat && long) {
            // cast lat and long to number
            map.flyTo({
                center: [Number(long), Number(lat)],
                zoom: 15,
                speed: 1.5, // Increase the speed
                curve: 1.42, // Adjust the curve for a more direct path
                maxDuration: 6000, // Maximum duration of the flight in milliseconds
                essential: true // This animation is considered essential with respect to prefers-reduced-motion
            })
            
            // if there is an error, log it
            map.on('error', (e) => {
                console.error("Map error:", e.error);
            });

            // add marker
            var marker = new mapboxgl.Marker({ color: '#FF0000'})
                .setLngLat([Number(long), Number(lat)])
                .addTo(map);
            
            // add popup with the name destination and make it red
            var popup = new mapboxgl.Popup({ offset: 25 }).setText(
                'Destination'
            );

            marker.setPopup(popup);

        }
    }

    var parkingLocation = true;

    if (normalParking.length > 0) {
        normalParking.forEach((parking: Parking) => {
            if (map) {
                // add marker
                var marker = new mapboxgl.Marker()
                    .setLngLat([Number(parking.longitude), Number(parking.latitude)])
                    .addTo(map);
                
                // add popup
                var popup = new mapboxgl.Popup({ offset: 25 }).setText(
                    `Parking Spot: ${parking.address}`
                );

                marker.setPopup(popup);
            }
        })
    }

    if (distanceParkingData.length > 0) {
        distanceParkingData.forEach((parking: ParkingDistanceReturn) => {
            if (map) {
                // add marker
                var marker = new mapboxgl.Marker()
                    .setLngLat([Number(parking.parking.longitude), Number(parking.parking.latitude)])
                    .addTo(map);
                
                // add popup
                var popup = new mapboxgl.Popup({ offset: 25 }).setText(
                    `Parking Spot: ${parking.parking.address}`
                );

                marker.setPopup(popup);
            }
        })
    }

    if ((normalParking.length == 0 && distanceParkingData.length == 0) && !loading) {
        parkingLocation = false;
    }

    return (
        <>
            <div className='font-semibold'>Find and book a parking spot in seconds</div>
            <div className='mt-2 mb-2'>
                <Swiper 
                    slidesPerView={4} 
                    spaceBetween={1} 
                    navigation={true} 
                    modules={[Navigation, Pagination]} 
                    pagination={{
                        type: 'progressbar',
                }}>
                {
                    loading && parkingLocation != false ? (
                        <>
                            <SwiperSlide className='m-2 mt-4'>
                                <Skeleton sx={{ height: 190, width: 345 }} variant="rectangular" />
                            </SwiperSlide>
                            <SwiperSlide className='m-2 mt-4'>
                                <Skeleton sx={{ height: 190, width: 345 }} variant="rectangular" />
                            </SwiperSlide> 
                            <SwiperSlide className='m-2 mt-4'>
                                <Skeleton sx={{ height: 190, width: 345 }} variant="rectangular" />
                            </SwiperSlide>
                            <SwiperSlide className='m-2 mt-4'>
                                <Skeleton sx={{ height: 190, width: 345 }} variant="rectangular" />
                            </SwiperSlide>
                        </>
                    ) :
                    normalParking.length > 0 ? normalParking.map((parking: Parking) => (
                        <SwiperSlide key={parking.id} className='m-2 mt-4'>
                            <ParkingSpotCard parking={parking} bookingInfo={bookingInfo} mapObject={map} distance={0} distanceObj={false} />
                        </SwiperSlide>
                    )) : distanceParkingData.length > 0 ? distanceParkingData.map((parking: ParkingDistanceReturn) => (
                        <SwiperSlide key={parking.parking.id} className='m-2 mt-4'>
                            <ParkingSpotCard parking={parking.parking} bookingInfo={bookingInfo} mapObject={map} distance={parking.distance} distanceObj={true} />
                        </SwiperSlide>
                    )) : null
                }
                </Swiper>
                {
                    parkingLocation == false ? (
                        <div className='text-center text-red-500 font-semibold mt-4'>
                            No parking spots found
                        </div>
                    ) : null
                }
            </div>
            
            <div id="map" className="w-full h-96"></div>
            <div className='mt-4 flex'>
                <Grid container spacing={2}>
                    <Grid item xs={12} sm={6}>
                        <Image src='https://s3.eu-west-2.amazonaws.com/prlnk.cdn/site/pug-searching.png' priority={true} width={800} height={800} alt={'hero image for the front page'}/>
                    </Grid>
                    <Grid item xs={12} sm={6}>
                        <Image src='https://s3.eu-west-2.amazonaws.com/prlnk.cdn/site/pug-worried.png' priority={true} width={800} height={800} alt={'hero image for the front page'}/>
                    </Grid>
                </Grid>
            </div>
        </>
    )
}
