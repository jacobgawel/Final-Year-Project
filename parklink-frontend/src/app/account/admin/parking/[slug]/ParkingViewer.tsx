import { ParkingHumanized, ParkingImage } from '@/types/parking';
import React, {  } from 'react';
import { Card } from '@mui/material';
// mui icon imports
import VerifiedIcon from '@mui/icons-material/Verified';
import UnpublishedIcon from '@mui/icons-material/Unpublished';
import CircleTwoToneIcon from '@mui/icons-material/CircleTwoTone';
// shadcn imports
import {
    HoverCard,
    HoverCardContent,
    HoverCardTrigger,
  } from "@/components/ui/hover-card"
import { CalendarIcon } from '@mui/x-date-pickers/icons';
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
// swiper imports
import { Swiper, SwiperSlide } from 'swiper/react'
import { Navigation, Pagination, } from 'swiper/modules';
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';
import { CheckCheck } from 'lucide-react';
// framer motion

export default function ParkingViewer({ parking, s3ImageUris }: { parking: any, s3ImageUris: ParkingImage | undefined}) {
    return (
        <>
        <Card variant='outlined' className='p-2'>
            <div className="flex font-bold ml-2 mt-1">
                <h1 className='text-xl mr-2'>Parking</h1>
                    <HoverCard>
                        <HoverCardTrigger asChild>
                            <div>
                                <CircleTwoToneIcon className={'mr-2 transition-transform duration-300 transform hover:scale-150 ' + (parking.availabilityStatus == true ? "text-green-500" : "text-red-500")} />
                            </div>
                        </HoverCardTrigger>
                        <HoverCardContent className="w-80">
                            <div className="flex justify-between space-x-4">
                                <div className="space-y-1">
                                    <p className={"text-sm font-semibold " + (parking.availabilityStatus == true ? "text-green-900" : "text-red-800")}>
                                        { parking.availabilityStatus == true ? "Parking spot is available for booking." : "Parking spot is unavailable for booking." }
                                    </p>
                                    <div className="flex items-center pt-2">
                                        <CalendarIcon className="mr-2 h-4 w-4 opacity-70" />{" "}
                                        <span className="text-xs text-muted-foreground">
                                            Last updated {parking?.humanizedLastEdit}
                                        </span>
                                    </div>
                                </div>
                            </div>
                        </HoverCardContent>
                    </HoverCard>
                    <HoverCard>
                        <HoverCardTrigger asChild>
                            <div className='relative group'>
                                <div className={`transition-transform duration-300 transform ${parking?.verificationStatus ? 'text-green-500' : 'text-yellow-500'} group-hover:scale-150`}>
                                    { parking?.verificationStatus ? <VerifiedIcon className='inline-block' /> : <UnpublishedIcon className='inline-block' /> }
                                </div>
                            </div>
                        </HoverCardTrigger>
                        <HoverCardContent className="w-80">
                            <div className="flex justify-between space-x-4">
                                <div className="space-y-1">
                                    <p className="text-sm font-semibold">
                                        { parking?.verificationStatus == true ? "Parking spot has been verified." : "Parking spot pending verification." }
                                    </p>
                                    <div className="flex items-center pt-2">
                                    <CalendarIcon className="mr-2 h-4 w-4 opacity-70" />{" "}
                                    <span className="text-xs text-muted-foreground">
                                        {
                                            parking?.verificationStatus == true ? parking?.humanizedVerifiedDate.split(".")[0] : "Pending"
                                        }
                                    </span>
                                    </div>
                                </div>
                            </div>
                        </HoverCardContent>
                    </HoverCard>
                    <p className="text-xs font-normal text-gray-400 ml-2 mt-1 mr-2">
                        Edited: {parking?.humanizedLastEditDate.split(".")[0]} / {parking?.humanizedLastEdit}
                    </p>
                    <p className="text-xs font-normal text-gray-400 mt-1 mr-2">
                        Created: {parking?.humanizedCreatedDate.split(".")[0]} / {parking?.humanizedCreatedAt}
                    </p>
                </div>
                <div className='ml-4 mt-2'>
                    <Label htmlFor="address">Address</Label>
                    <p className="text-gray-500 text-xs mb-2">Address of your parking spot. This will be displayed on the parking spot card so make sure that its the most recent and correct location.<br />Changes to the address will have to be re-verified which will stop the parking slot from appearing in the feed.</p>
                    <Input
                        id="address"
                        className={'mb-5'}
                        disabled
                        value={parking.address}
                    />

                    <Label htmlFor="price">Price £ (1 hour)</Label>
                    <p className="text-gray-500 text-xs mb-2">Price per hour of the parking spot. This is the set price that will be applied to the booking.</p>
                    <Input
                        id="price"
                        disabled
                        className={'mb-5 w-[100px]'}
                        value={parking.price}
                    />

                    <Label htmlFor="city">City</Label>
                    <p className="text-gray-500 text-xs mb-2">City of the parking spot location. This is used for some queries which are used to reach customers and optimise results. Make sure this information is correct.</p>
                    <Input
                        id="city"
                        disabled
                        className={'mb-5 w-[250px]'}
                        value={parking.city}
                    />

                    <Label htmlFor="slotType">Slot Type</Label>
                    <p className="text-gray-500 text-xs mb-2">Type of parking slot e.g. Garage, Parking lot...</p>
                    <Input
                        id="slotType"
                        disabled
                        className={'mb-5 w-[250px]'}
                        value={parking.slotType}
                    />

                    <Label htmlFor="slotSize">Slot Size</Label>
                    <p className="text-gray-500 text-xs mb-2">Size of the parking slot.</p>
                    <Input
                        id="slotSize"
                        disabled
                        className={'mb-5 w-[200px]'}
                        value={parking.slotSize}
                    />

                    <div className="flex items-center">
                    <Label htmlFor="timeLimit">Time Limit</Label>
                    {
                        parking.timeLimited == true ? <CheckCheck className={"ml-2"} /> : null
                    }
                    </div>
                    <p className="text-gray-500 text-xs mb-2">Maximum time allowed for parking. Make sure this information is regularly updated if you have a change in circumstanes.</p>
                    <Input
                        id="timeLimit"
                        disabled
                        className={'mb-5 w-[200px]'}
                        value={parking.timeLimit}
                    />

                    <div className="flex items-center">
                    <Label htmlFor="dayLimit">Day Limit</Label>
                    {
                        parking.dayLimited == true ? <CheckCheck className={"ml-2"} /> : null
                    }
                    </div>
                    <p className="text-gray-500 text-xs mb-2">Maximum time allowed for parking. Make sure this information is regularly updated if you have a change in circumstanes.</p>
                    <Input
                        id="dayLimit"
                        disabled
                        className={'mb-5 w-[200px]'}
                        value={parking.dayLimit}
                    />

                    <Label htmlFor="slotCapacity">Slot Capacity</Label>
                    <p className="text-gray-500 text-xs mb-2">Capacity of the parking slot. If you have more slots in your space then list how many to make sure you can make the most of your spaces.</p>
                    <Input
                        id="slotCapacity"
                        disabled
                        className={'mb-5 w-[200px]'}
                        value={parking.slotCapacity}
                    />

                    <Label htmlFor="evInfo">Electrical Vehicle Info</Label>
                    <p className="text-gray-500 text-xs mb-2">Electric Vehicle charging information.</p>
                    <Input
                        id="evInfo"
                        disabled
                        className={'mb-5'}
                        value={parking.evInfo}
                    />

                    <Label htmlFor="additionalFeatures">Additional Features</Label>
                    <p className="text-gray-500 text-xs mb-2">Additional features of the parking spot. Cameras, employees, security etc</p>
                    <Input
                        id="additionalFeatures"
                        disabled
                        className={'mb-5'}
                        value={parking.additionalFeatures}
                    />

                    <Label htmlFor="slotNotes">Slot Notes</Label>
                    <Input
                        id="slotNotes"
                        disabled
                        className={'mb-5'}
                        value={parking.slotNotes}
                    />
                </div>
        </Card>
        <Card variant='outlined' className='p-2 mt-2'>
            <h1 className="font-bold ml-2 mt-1 text-xl">Images</h1>
            
            <div className='ml-4 mt-2 mb-4'>
                {
                    s3ImageUris?.s3ImageUris == null ? <p className="text-gray-500 text-xs mb-2">No images available for this parking spot.</p> : <p className="text-gray-500 text-xs mb-2">Images of the parking spot.</p>
                }
                <Swiper
                    pagination={{ clickable: true }}
                    navigation={true}
                    className="mt-5"
                    spaceBetween={25}
                    slidesPerView={4}
                    modules={[Pagination, Navigation]}
                >
                    {
                        s3ImageUris?.s3ImageUris.map((image: any, index: number) => (
                            <SwiperSlide key={index}>
                                <img src={image.fileUri} className="w-full h-full object-cover" />
                            </SwiperSlide>
                        ))
                    }
                </Swiper>
            </div>
        </Card>
        </>
    )
}