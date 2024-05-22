"use client";

import { GetHumanizedParkingById, UpdateParkingImages } from '@/app/server/parking/parking';
import { ParkingHumanized, ParkingImage, S3Image } from '@/types/parking';
import React, { useEffect, useMemo } from 'react';
import { Card, Skeleton } from '@mui/material';
import { Button } from "@/components/ui/button"
// mui icon imports
import CircularProgress from '@mui/material/CircularProgress';
import ModeEditIcon from '@mui/icons-material/ModeEdit';
import CloseIcon from '@mui/icons-material/Close';
import VerifiedIcon from '@mui/icons-material/Verified';
import SaveAltIcon from '@mui/icons-material/SaveAlt';
import UnpublishedIcon from '@mui/icons-material/Unpublished';
import DeleteIcon from '@mui/icons-material/Delete';
import UploadIcon from '@mui/icons-material/Upload';
import ClearIcon from '@mui/icons-material/Clear';
import { toast } from 'sonner'
import MouseIcon from '@mui/icons-material/Mouse';
import CircleTwoToneIcon from '@mui/icons-material/CircleTwoTone';
// shadcn imports
import {
    HoverCard,
    HoverCardContent,
    HoverCardTrigger,
} from "@/components/ui/hover-card"
// Calendar icon
import { CalendarIcon } from '@mui/x-date-pickers/icons';
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ParkingUpdateRequest } from '@/types/parking';
import { UpdateParking } from "@/app/server/parking/parking";
import {
    Select,
    SelectContent,
    SelectGroup,
    SelectItem,
    SelectLabel,
    SelectTrigger,
    SelectValue,
  } from "@/components/ui/select"
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog"
// swiper imports
import { Swiper, SwiperSlide } from 'swiper/react'
import { Navigation, Pagination, } from 'swiper/modules';
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';
// framer motion
import { motion } from "framer-motion"
import {
    ContextMenu,
    ContextMenuContent,
    ContextMenuItem,
    ContextMenuTrigger,
} from "@/components/ui/context-menu";
import { useDropzone } from 'react-dropzone';
import Checkbox from '@mui/material/Checkbox';


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

async function getParkingData(id: string) {
    const parking = await GetHumanizedParkingById(id);
    if(parking.status === 200) {
        return parking.data;
    }

    if(parking.status === 401) {
        console.log('Unauthorized access to bookings', parking.status);
    }

    return [];
}

export default function Page({ params }: { params: { slug: string } }) {
    // States for the page modes and data transfer
    const [parkingData, setParkingData] = React.useState<ParkingHumanized>();
    const [loading, setLoading] = React.useState(true);
    const [editMode, setEditMode] = React.useState(false);

    // States for the states of the new values
    const [address, setAddress] = React.useState<any>('');
    const [price, setPrice] = React.useState<any>('');
    const [city, setCity] = React.useState<any>('');
    const [slotType, setSlotType] = React.useState<any>('');
    const [slotSize, setSlotSize] = React.useState<any>('');
    const [timeLimit, setTimeLimit] = React.useState<any>('');
    const [slotCapacity, setSlotCapacity] = React.useState<any>('');
    const [evInfo, setEvInfo] = React.useState<any>('');
    const [additionalFeatures, setAdditionalFeatures] = React.useState<any>('');
    const [slotNotes, setSlotNotes] = React.useState<any>('');
    const [availabilityStatus, setAvailabilityStatus] = React.useState<any>('');
    const [deleteImageIds, setDeleteImageIds] = React.useState<string[]>([]);
    const [imageButtonDisabled, setImageButtonDisabled] = React.useState<boolean>(true);
    const [timeLimited, setTimeLimited] = React.useState<any>('');
    const [dayLimit, setDayLimit] = React.useState<any>('');
    const [dayLimited, setDayLimited] = React.useState<any>('');

    var slug = params.slug;

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

    useEffect(() => {
        getParkingData(slug).then((parkingData: ParkingHumanized) => {
            setLoading(true);
            // Set the parking state to the parking object
            // values are assigned to 2 locations to keep the original values
            // and the new values in sync for comparison
            setParkingData(parkingData);
            setAddress(parkingData.address);
            setPrice(parkingData.price);
            setCity(parkingData.city);
            setSlotType(parkingData.slotType);
            setSlotSize(parkingData.slotSize);
            setTimeLimit(parkingData.timeLimit);
            setSlotCapacity(parkingData.slotCapacity);
            setEvInfo(parkingData.evInfo);
            setAdditionalFeatures(parkingData.additionalFeatures);
            setSlotNotes(parkingData.slotNotes);
            setAvailabilityStatus(parkingData.availabilityStatus);
            setTimeLimited(parkingData.timeLimited);
            setDayLimit(parkingData.dayLimit);
            setDayLimited(parkingData.dayLimited);

            setLoading(false);
        });
    }, [slug]);

    var s3ImageObj: ParkingImage | undefined = parkingData ? JSON.parse(parkingData.slotImages) : undefined;

    function setDefaultValues(data: ParkingHumanized) {
        // defaults the values to the original values
        // this is used to discard the changes
        setAddress(data.address);
        setPrice(data.price);
        setCity(data.city);
        setSlotType(data.slotType);
        setSlotSize(data.slotSize);
        setTimeLimit(data.timeLimit);
        setSlotCapacity(data.slotCapacity);
        setEvInfo(data.evInfo);
        setAdditionalFeatures(data.additionalFeatures);
        setSlotNotes(data.slotNotes);
        setAvailabilityStatus(data.availabilityStatus);
        setTimeLimited(data.timeLimited);
        setDayLimit(data.dayLimit);
        setDayLimited(data.dayLimited);
    }

    function handleEditDisabled() {
        setEditMode(!editMode);
        setDefaultValues(parkingData!);
        toast.error('Changes have been discarded');
    }

    function handleEditEnabled() {
        setEditMode(!editMode);
        toast.info('Edit mode enabled');
    }

    function handleAvailabilityChange() {
        setAvailabilityStatus(!availabilityStatus);
        toast.info('Availability status changed');
        setEditMode(true);
    }

    function handleTimeLimited() {
        const newTimeLimited = !timeLimited;
        setTimeLimited(newTimeLimited);
        if (newTimeLimited) {
            setDayLimited(false);
            setDayLimit(0);
        }
    }

    function handleDayLimited() {
        const newDayLimited = !dayLimited;
        setDayLimited(newDayLimited);
        if (newDayLimited) {
            setTimeLimited(false);
            setTimeLimit('00:00:00');
        }
    }

    function handleSave() {
        setEditMode(!editMode);
        publishUpdatedData();
    }

    function publishUpdatedData() {
        var parkingUpdateRequest: ParkingUpdateRequest = {
            // values that have an exclamtion mark are guaranteed are values pulled from the database
            // and not changed by the user so they are guaranteed to be there
            id: parkingData!.id,
            address: address,
            slotType: slotType,
            slotSize: slotSize,
            parkingRejected: parkingData!.parkingRejected,
            availabilityStatus: availabilityStatus,
            price: price,
            evInfo: evInfo,
            additionalFeatures: additionalFeatures,
            timeLimit: timeLimit,
            timeLimited: timeLimited,
            dayLimit: dayLimit,
            dayLimited: dayLimited,
            slotNotes: slotNotes,
            slotImages: parkingData!.slotImages,
            slotCapacity: slotCapacity,
            longitude: parkingData!.longitude,
            latitude: parkingData!.latitude,
            city: city,
            verificationStatus: parkingData!.verificationStatus
        }

        toast.promise(UpdateParking(parkingUpdateRequest), {
            loading: 'Updating parking spot...',
            success: 'Parking spot has been updated!',
            error: 'Failed to update parking spot!'
        });

        getParkingData(slug).then((parkingData) => {
            setLoading(true);
            setParkingData(parkingData);
            setLoading(false);
        });
    }

    function getChanges() {
        var changes = []

        if (dayLimited != parkingData?.dayLimited) {
            changes.push("Day Limited: " + parkingData?.dayLimited + ' -> ' + dayLimited);
        }

        if (dayLimit != parkingData?.dayLimit) {
            changes.push("Day Limit: " + parkingData?.dayLimit + ' -> ' + dayLimit);
        }

        if (timeLimited != parkingData?.timeLimited) {
            changes.push("Time Limited: " + parkingData?.timeLimited + ' -> ' + timeLimited);
        }

        if (timeLimit != parkingData?.timeLimit) {
            changes.push("Time Limit: " + parkingData?.timeLimit + ' -> ' + timeLimit);
        }

        if (address != parkingData?.address) {
            changes.push("Address: " + parkingData?.address + ' -> ' + address);
        }

        if (price != parkingData?.price) {
            changes.push("Price: " + parkingData?.price + ' -> ' + price);
        }

        if (city != parkingData?.city) {
            changes.push("City: " + parkingData?.city + ' -> ' + city);
        }

        if (slotType != parkingData?.slotType) {
            changes.push("Slot Type: " + parkingData?.slotType + ' -> ' + slotType);
        }

        if (slotSize != parkingData?.slotSize) {
            changes.push("Slot Size: " + parkingData?.slotSize + ' -> ' + slotSize);
        }

        if (slotCapacity != parkingData?.slotCapacity) {
            changes.push("Capacity: " + parkingData?.slotCapacity + ' -> ' + slotCapacity);
        }

        if (evInfo != parkingData?.evInfo) {
            changes.push("Electrical Vehicle Info: " + parkingData?.evInfo + ' -> ' + evInfo);
        }

        if (additionalFeatures != parkingData?.additionalFeatures) {
            changes.push("Additional Features: "+ parkingData?.additionalFeatures + ' -> ' + additionalFeatures);
        }

        if (slotNotes != parkingData?.slotNotes) {
            changes.push("Slot notes: " + parkingData?.slotNotes + ' -> ' + slotNotes);
        }

        if (availabilityStatus != parkingData?.availabilityStatus) {
            changes.push("Availability Status: " + parkingData?.availabilityStatus + ' -> ' + availabilityStatus);
        }

        return changes;
    }

    // IMAGE EDIT FUNCTIONALITIES
    function handleImageDelete(imageId: string) {
        if(deleteImageIds.includes(imageId)) {
            return;
        }
        console.log("Pushing image id to delete list: ", imageId)
        setDeleteImageIds([...deleteImageIds, imageId]);
        setImageButtonDisabled(false);
    }

    function handleCancelImageDelete(imageId: string) {
        console.log("Removing image id from delete list: ", imageId)
        setDeleteImageIds(deleteImageIds => deleteImageIds.filter((id) => id !== imageId));
    }

    function clearImageChanges() {
        setDeleteImageIds([]);
        acceptedFiles.splice(0, acceptedFiles.length);
        toast.error('Image changes have been discarded');
        setImageButtonDisabled(true);
    }

    function updateParkingImages() {
        if(acceptedFiles.length == 0 && deleteImageIds.length == 0) {
            toast.error('No changes have been made to the images');
            return;
        }

        var formData = new FormData();

        formData.append('parkingId', parkingData!.id);

        deleteImageIds.forEach((imageId: string) => {
            formData.append('deleteList', imageId);
        });

        acceptedFiles.forEach((file: File) => {
            formData.append('imageList', file);
        });

        toast.promise(UpdateParkingImages(formData), {
            loading: 'Updating parking images...',
            success: 'Parking images have been updated!',
            error: 'Failed to update parking images!'
        });

        acceptedFiles.splice(0, acceptedFiles.length);
        setDeleteImageIds([]);

        getParkingData(slug).then((parkingData) => {
            setLoading(true);
            setParkingData(parkingData);
            setLoading(false);
        });
    }

    return (
        <>
        <Card variant='outlined' className='p-2'>
            <div className="flex font-bold ml-2 mt-1">
                <h1 className='text-xl mr-2'>Parking</h1>
                {
                    loading ? (
                        null
                    ) : (
                        <>
                        <HoverCard>
                            <HoverCardTrigger asChild>
                                <div>
                                    <CircleTwoToneIcon className={'mr-2 transition-transform duration-300 transform hover:scale-150 ' + (availabilityStatus == true ? "text-green-500" : "text-red-500")} />
                                </div>
                            </HoverCardTrigger>
                            <HoverCardContent className="w-80">
                                <div className="flex justify-between space-x-4">
                                    <div className="space-y-1">
                                        <p className={"text-sm font-semibold " + (availabilityStatus == true ? "text-green-900" : "text-red-800")}>
                                            { availabilityStatus == true ? "Parking spot is available for booking." : "Parking spot is unavailable for booking." }
                                        </p>
                                        <div className="flex items-center pt-2">
                                            <CalendarIcon className="mr-2 h-4 w-4 opacity-70" />{" "}
                                            <span className="text-xs text-muted-foreground">
                                                Last updated {parkingData?.humanizedLastEdit}
                                            </span>
                                        </div>
                                        <div className="flex items-center pt-2">
                                            <MouseIcon className="mr-2 h-4 w-4 opacity-70" />{" "}
                                            <motion.span 
                                                whileTap={{ scale: 0.95 }}
                                                className="text-xs text-muted-foreground hover:underline hover:text-gray-800 cursor-pointer" onClick={() => handleAvailabilityChange()}
                                                >
                                                Click here to change the availability status!
                                            </motion.span>
                                        </div>
                                    </div>
                                </div>
                            </HoverCardContent>
                        </HoverCard>
                        <HoverCard>
                            <HoverCardTrigger asChild>
                                <div className='relative group'>
                                    <div className={`transition-transform duration-300 transform ${parkingData?.verificationStatus ? 'text-green-500' : 'text-yellow-500'} group-hover:scale-150`}>
                                        { parkingData?.verificationStatus ? <VerifiedIcon className='inline-block' /> : <UnpublishedIcon className='inline-block' /> }
                                    </div>
                                </div>
                            </HoverCardTrigger>
                            <HoverCardContent className="w-80">
                                <div className="flex justify-between space-x-4">
                                    <div className="space-y-1">
                                        <p className="text-sm font-semibold">
                                            { parkingData?.verificationStatus == true ? "Parking spot has been verified." : "Parking spot pending verification." }
                                        </p>
                                        <div className="flex items-center pt-2">
                                        <CalendarIcon className="mr-2 h-4 w-4 opacity-70" />{" "}
                                        <span className="text-xs text-muted-foreground">
                                            {
                                                parkingData?.verificationStatus == true ? parkingData?.humanizedVerifiedDate.split(".")[0] : "Pending"
                                            }
                                        </span>
                                        </div>
                                    </div>
                                </div>
                            </HoverCardContent>
                        </HoverCard>
                        </>
                    )
                }
                <p className="text-xs font-normal text-gray-400 ml-2 mt-1 mr-2">
                    Edited: {parkingData?.humanizedLastEditDate.split(".")[0]} / {parkingData?.humanizedLastEdit}
                </p>
                <p className="text-xs font-normal text-gray-400 mt-1 mr-2">
                    Created: {parkingData?.humanizedCreatedDate.split(".")[0]} / {parkingData?.humanizedCreatedAt}
                </p>
            </div>
            <div className='ml-4 mt-2'>
                {
                    loading ? (
                        <div className="flex justify-center items-center">
                            <CircularProgress color="info" />
                        </div>
                    ) : (
                        <>
                            <div className="flex justify-end mr-10">
                                {
                                    editMode == false ? (
                                        <Button onClick={() => handleEditEnabled()} variant='default' className='bg-blue-500 hover:bg-blue-600'>
                                            <ModeEditIcon className='text-sm mr-1' /> Edit
                                        </Button>
                                    ) : (
                                        <>
                                        <Button onClick={() => handleEditDisabled()} variant='destructive'>
                                            <CloseIcon className='text-sm mr-1' /> Cancel
                                        </Button>
                                        </>
                                    )
                                }
                            </div>

                            <Label htmlFor="address">Address</Label>
                            <p className="text-gray-500 text-xs mb-2">Address of your parking spot. This will be displayed on the parking spot card so make sure that its the most recent and correct location.<br />Changes to the address will have to be re-verified which will stop the parking slot from appearing in the feed.</p>
                            <Input
                                id="address"
                                disabled={!editMode}
                                className={'mb-5' + (address != parkingData?.address && editMode ? ' border-2 border-orange-300' : '')}
                                // comparison of the new value and the old value to check if the value has been changed or not
                                // this will be used to show the user that the value has been changed using a border
                                onChange={(e) => setAddress(e.target.value)}
                                value={address}
                            />

                            <Label htmlFor="price">Price £ (1 hour)</Label>
                            <p className="text-gray-500 text-xs mb-2">Price per hour of the parking spot. This is the set price that will be applied to the booking.</p>
                            <Input
                                id="price"
                                disabled={!editMode}
                                className={'mb-5 w-[100px]' + (price != parkingData?.price && editMode ? ' border-2 border-orange-300' : '')}
                                onChange={(e) => setPrice(e.target.value)}
                                value={price}
                            />

                            <Label htmlFor="city">City</Label>
                            <p className="text-gray-500 text-xs mb-2">City of the parking spot location. This is used for some queries which are used to reach customers and optimise results. Make sure this information is correct.</p>
                            <Input
                                id="city"
                                disabled={!editMode}
                                className={'mb-5 w-[250px]' + (city != parkingData?.city && editMode ? ' border-2 border-orange-300' : '')}
                                onChange={(e) => setCity(e.target.value)}
                                value={city}
                            />

                            <Label htmlFor="slotType">Slot Type</Label>
                            <p className="text-gray-500 text-xs mb-2">Type of parking slot e.g. Garage, Parking lot...</p>
                            <Select onValueChange={(e) => setSlotType(e)} value={slotType}>
                                <SelectTrigger className="w-[200px] mb-5" disabled={!editMode} >
                                    <SelectValue placeholder="Select Parking Spot Type" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectGroup>
                                        <SelectLabel>Parking Spot Types</SelectLabel>
                                        <SelectItem value="Parking Lot">Parking Lot</SelectItem>
                                        <SelectItem value="Garage">Garage</SelectItem>
                                        <SelectItem value="Valet Parking">Valet Parking</SelectItem>
                                        <SelectItem value="Automated Parking Garage">Automated Parking Garage (mechanical, hydraulic lift etc)</SelectItem>
                                        <SelectItem value="Street Parking Spot">Street Parking Spot</SelectItem>
                                    </SelectGroup>
                                </SelectContent>
                            </Select>

                            <Label htmlFor="slotSize">Slot Size</Label>
                            <p className="text-gray-500 text-xs mb-2">Size of the parking slot.</p>
                            <Input
                                id="slotSize"
                                disabled={!editMode}
                                className={'mb-5 w-[200px]' + (slotSize != parkingData?.slotSize && editMode ? ' border-2 border-orange-300' : '')}
                                onChange={(e) => setSlotSize(e.target.value)}
                                value={slotSize}
                            />

                            <Label htmlFor="timeLimit">Time Limit</Label><Checkbox onChange={handleTimeLimited} disabled={!editMode} checked={timeLimited} className={"ml-2"} />
                            <p className="text-gray-500 text-xs mb-2">Maximum time allowed for parking. Make sure this information is regularly updated if you have a change in circumstanes.</p>
                            <Select onValueChange={(e) => setTimeLimit(e)} value={timeLimit}>
                                <SelectTrigger className="w-[200px] mb-5" disabled={timeLimited !== true || editMode !== true}>
                                    <SelectValue placeholder="Select Parking Spot Type" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectGroup>
                                        <SelectLabel>Parking Spot Types</SelectLabel>
                                        {
                                            Array.from({ length: 24 }, (_, index) => {
                                                const hours = index; // Calculate hours (0 to 23)
                                                const formattedTime = `${hours < 10 ? '0' : ''}${hours}:00:00`; // Format hours as HH:00:00
                                                return (
                                                    <SelectItem key={hours} value={formattedTime}>{formattedTime}</SelectItem>
                                                );
                                            })
                                        }
                                    </SelectGroup>
                                </SelectContent>
                            </Select>

                            <Label htmlFor="dayLimit">Day Limit</Label><Checkbox onChange={handleDayLimited} disabled={!editMode} checked={dayLimited} className={"ml-2"} />
                            <p className="text-gray-500 text-xs mb-2">Maximum time allowed for parking. Make sure this information is regularly updated if you have a change in circumstanes.</p>
                            <Input
                                id="slotCapacity"
                                disabled={dayLimited !== true || editMode !== true}
                                className={'mb-5 w-[200px]' + (dayLimit != parkingData?.dayLimit && editMode ? ' border-2 border-orange-300' : '')}
                                onChange={(e) => setDayLimit(e.target.value)}
                                value={dayLimit}
                            />

                            <Label htmlFor="slotCapacity">Slot Capacity</Label>
                            <p className="text-gray-500 text-xs mb-2">Capacity of the parking slot. If you have more slots in your space then list how many to make sure you can make the most of your spaces.</p>
                            <Input
                                id="slotCapacity"
                                disabled={!editMode}
                                className={'mb-5 w-[200px]' + (slotCapacity != parkingData?.slotCapacity && editMode ? ' border-2 border-orange-300' : '')}
                                onChange={(e) => setSlotCapacity(e.target.value)}
                                value={slotCapacity}
                            />

                            <Label htmlFor="evInfo">Electrical Vehicle Info</Label>
                            <p className="text-gray-500 text-xs mb-2">Electric Vehicle charging information.</p>
                            <Input
                                id="evInfo"
                                disabled={!editMode}
                                className={'mb-5' + (evInfo != parkingData?.evInfo && editMode ? ' border-2 border-orange-300' : '')}
                                onChange={(e) => setEvInfo(e.target.value)}
                                value={evInfo}
                            />

                            <Label htmlFor="additionalFeatures">Additional Features</Label>
                            <p className="text-gray-500 text-xs mb-2">Additional features of the parking spot. Cameras, employees, security etc</p>
                            <Input
                                id="additionalFeatures"
                                disabled={!editMode}
                                className={'mb-5' + (additionalFeatures != parkingData?.additionalFeatures && editMode ? ' border-2 border-orange-300' : '')}
                                onChange={(e) => setAdditionalFeatures(e.target.value)}
                                value={additionalFeatures}
                            />

                            <Label htmlFor="slotNotes">Slot Notes</Label>
                            <Input
                                id="slotNotes"
                                disabled={!editMode}
                                className={'mb-5' + (slotNotes != parkingData?.slotNotes && editMode ? ' border-2 border-orange-300' : '')}
                                onChange={(e) => setSlotNotes(e.target.value)}
                                value={slotNotes}
                            />
                            
                            <div className="flex justify-start mr-10">
                                {
                                    editMode == true ? (
                                        <>
                                        <Dialog>
                                            <DialogTrigger asChild>
                                                <Button className='mt-5 mb-5 text-white bg-blue-600 hover:bg-blue-500'>
                                                    <SaveAltIcon className='text-sm mr-1' /> Save
                                                </Button>
                                            </DialogTrigger>
                                            <DialogContent>
                                                <DialogTitle>Save Changes</DialogTitle>
                                                <DialogDescription>Are you sure you want to save the changes?</DialogDescription>
                                                <div className={"text-sm font-semibold text-gray-600"}>
                                                    <ul>
                                                        {
                                                            getChanges().map((change, index) => (
                                                                <li key={index}>{change}</li>
                                                            ))
                                                        }
                                                    </ul>
                                                </div>
                                                <DialogFooter>
                                                    <Button onClick={() => handleSave()} variant="outline" className="text-white bg-blue-600 hover:bg-blue-500 hover:text-white">Yes</Button>
                                                    <Button onClick={() => handleEditDisabled()} variant="destructive">
                                                        <CloseIcon className='text-sm mr-1' /> Cancel
                                                    </Button>
                                                </DialogFooter>
                                            </DialogContent>
                                        </Dialog>
                                        <Button onClick={() => handleEditDisabled()} variant="destructive" className='mt-5 mb-5 ml-2'>
                                            <CloseIcon className='text-sm mr-1' /> Cancel
                                        </Button>
                                        </>
                                    ) : null
                                }
                            </div>
                        </>
                    )
                }
            </div>
        </Card>
        <Card variant='outlined' className='p-2 mt-2'>
            <h1 className="font-bold ml-2 mt-1 text-xl">Images</h1>
            
            <div className='ml-4 mt-2 mb-4'>
            <Swiper
                pagination={{ clickable: true }}
                navigation={true}
                className="mt-5"
                spaceBetween={25}
                slidesPerView={4}
                modules={[Pagination, Navigation]}
            >
                {loading == false ? (
                    s3ImageObj && s3ImageObj.s3ImageUris.length > 0 ? (
                        s3ImageObj.s3ImageUris.map((image: S3Image, index) => (
                            <SwiperSlide key={index}>
                                <ContextMenu>
                                    <ContextMenuTrigger>
                                    {
                                        // if the image is in the delete list then show the delete icon
                                        // and the image with a black overlay
                                        deleteImageIds.includes(image.fileName) ? (
                                            <div className="relative">
                                                <img src={image.fileUri} className="h-300 w-300" />
                                                <div className="absolute top-0 left-0 w-full h-full bg-black bg-opacity-50 flex justify-center items-center">
                                                    <DeleteIcon style={{ fontSize: 100}} color="info" />
                                                </div>
                                            </div>
                                        ) : (
                                            // if the image is not in the delete list then show the image
                                            <div className="relative">
                                                <img src={image.fileUri} className="h-300 w-300" />
                                                <div className="absolute inset-0 bg-black opacity-0 hover:opacity-50 transition-opacity flex items-center justify-center">
                                                    <p className="text-white">Right click to edit</p>
                                                </div>
                                            </div>
                                        )
                                    }
                                    </ContextMenuTrigger>
                                    {
                                        // if the image is not in the delete list then show the delete option
                                        !deleteImageIds.includes(image.fileName) ? (
                                            <ContextMenuContent>
                                                <ContextMenuItem onClick={() => handleImageDelete(image.fileName)}>
                                                    <DeleteIcon className='text-sm mr-1' /> Delete
                                                </ContextMenuItem>
                                            </ContextMenuContent>
                                        ) : (
                                            // if the image is in the delete list then show the cancel option
                                            <ContextMenuContent>
                                                <ContextMenuItem onClick={() => handleCancelImageDelete(image.fileName)}>
                                                    <ClearIcon className='text-sm mr-1' /> Cancel
                                                </ContextMenuItem>
                                            </ContextMenuContent>
                                        )
                                    }
                                </ContextMenu>
                            </SwiperSlide>
                        ))
                    ) : (
                        <>
                            <SwiperSlide>
                                    <img src="https://placehold.co/400x300" alt="Parking Spot Image" height={300} width={400} />
                            </SwiperSlide>
                            <SwiperSlide>
                                    <img src="https://placehold.co/400x300" alt="Parking Spot Image" height={300} width={400} />
                            </SwiperSlide>
                        </>
                    )
                ) : (
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
                )}
            </Swiper>
            </div>

            <section className="container mt-5">
              <div {...getRootProps({style})}>
                <input {...getInputProps()} />
                <p>Drop and upload your images over here</p>
                <em>(Click on the dropzone to open file explorer)</em>
              </div>
            </section>
            
            <Swiper
              pagination={{ clickable: true }}
              navigation={true}
              className="mt-5"
              spaceBetween={25}
              slidesPerView={4}
              modules={[Pagination, Navigation]}
            >
            {
                acceptedFiles.map((file: File, index) => (
                    <SwiperSlide className='ml-5 mb-5 mt-5' key={index}>
                        <p className="text-start">Image Preview</p>
                        <img src={URL.createObjectURL(file)} alt={file.name} height={300} width={300} />
                    </SwiperSlide>
                ))
            }
            </Swiper>
            {
                acceptedFiles.length > 0 || deleteImageIds.length > 0 ? (
                    <div className="flex justify-start ml-5 mr-10">
                        <Button variant='destructive' className='mt-5 mb-5' onClick={() => clearImageChanges()}>
                            <ClearIcon className='text-sm mr-1' /> Clear Changes
                        </Button>
                        <Button variant='default' className='mt-5 mb-5 ml-2' onClick={() => updateParkingImages()}>
                            <UploadIcon className='text-sm mr-1' /> Upload Changes
                        </Button>
                    </div>
                ) : null
            }
            
        </Card>
        </>
    )
}