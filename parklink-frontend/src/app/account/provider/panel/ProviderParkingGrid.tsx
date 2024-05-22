import { toast } from 'sonner'
import { styled } from '@mui/material/styles';
import  Chip from '@mui/material/Chip';
import DoneIcon from '@mui/icons-material/Done';
import Switch from '@mui/material/Switch';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { DeleteParking, UpdateParking } from "@/app/server/parking/parking";
import { Parking } from "@/types/parking";
import { useEffect, useState } from "react";
import { GetParkingDataByProviderId } from "@/app/server/parking/parking";
import { Button } from "@/components/ui/button"
import ErrorOutlineOutlinedIcon from '@mui/icons-material/ErrorOutlineOutlined';
import Link from 'next/link';
import { ParkingUpdateRequest } from '@/types/parking';
import { Booking } from '@/types/booking';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { MoreHorizontal } from 'lucide-react';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { Fine } from '@/types/fine';
import { Badge } from '@/components/ui/badge';
import { DeleteFineProvider } from '@/app/server/fine/fine';

// styling the switch component
// this is a custom styled switch component that matches the parklink theme
const ParklinkSwitch = styled(Switch)(({ theme }) => ({
    padding: 8,
    '& .MuiSwitch-track': {
      borderRadius: 22 / 2,
      '&::before, &::after': {
        content: '""',
        position: 'absolute',
        top: '50%',
        transform: 'translateY(-50%)',
        width: 16,
        height: 16,
      },
      '&::before': {
        backgroundImage: `url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" height="16" width="16" viewBox="0 0 24 24"><path fill="${encodeURIComponent(
          theme.palette.getContrastText(theme.palette.primary.main),
        )}" d="M21,7L9,19L3.5,13.5L4.91,12.09L9,16.17L19.59,5.59L21,7Z"/></svg>')`,
        left: 12,
      },
      '&::after': {
        backgroundImage: `url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" height="16" width="16" viewBox="0 0 24 24"><path fill="${encodeURIComponent(
          theme.palette.getContrastText(theme.palette.primary.main),
        )}" d="M19,13H5V11H19V13Z" /></svg>')`,
        right: 12,
      },
    },
    '& .MuiSwitch-thumb': {
      boxShadow: 'none',
      width: 16,
      height: 16,
      margin: 2,
    },
  }));

export default function ParkingGrid({ parkingDataParam, bookingDataParam, userId, fineDataParam }: { parkingDataParam: Parking[], bookingDataParam: Booking[], userId: string, fineDataParam: Fine[] }) {
    const [parkingData, setParkingData] = useState<Parking[]>(parkingDataParam);
    const [bookingData, setBookingData] = useState<Booking[]>(bookingDataParam);
    const [openParkingDeleteDialog, setDeleteDialogOpen] = useState(false);
    const [selectedParking, setSelectedParking] = useState<Parking | undefined>();
    const [openFineDeleteDialog, setFineDeleteDialogOpen] = useState(false);
    const [selectedFine, setSelectedFine] = useState<Fine | undefined>();

    const handleDeleteDialogClickOpen = (parking: Parking | undefined) => {
      setSelectedParking(parking);
      setDeleteDialogOpen(true);
    };

    const handleFineDeleteDialogClickOpen = (fine: Fine | undefined) => {
      setSelectedFine(fine);
      setFineDeleteDialogOpen(true);
    };

    const handleFineDeleteDialogClose = () => {
      setSelectedFine(undefined);
      setFineDeleteDialogOpen(false);
    };
  
    const handleParkingDeleteDialogClose = () => {
      setSelectedParking(undefined);
      setDeleteDialogOpen(false);
    };

    useEffect(() => {
        setParkingData(parkingDataParam);
    }, [parkingDataParam]);

    useEffect(() => {
        setBookingData(bookingDataParam);
    }, [bookingDataParam]);

    async function handleAvailabilityStatus(parking: Parking) {
      // this function is responsible for updating the availability status of a parking spot
      parking.availabilityStatus = !parking.availabilityStatus;

      var parkingUpdateRequest: ParkingUpdateRequest = {
          id: parking.id,
          address: parking.address,
          slotType: parking.slotType,
          slotSize: parking.slotSize,
          availabilityStatus: parking.availabilityStatus,
          parkingRejected: parking.parkingRejected,
          price: parking.price,
          evInfo: parking.evInfo,
          additionalFeatures: parking.additionalFeatures,
          timeLimit: parking.timeLimit,
          timeLimited: parking.timeLimited,
          dayLimit: parking.dayLimit,
          dayLimited: parking.dayLimited,
          slotNotes: parking.slotNotes,
          slotImages: parking.slotImages,
          slotCapacity: parking.slotCapacity,
          longitude: parking.longitude,
          latitude: parking.latitude,
          city: parking.city,
          verificationStatus: parking.verificationStatus
      }

      const res = await UpdateParking(parkingUpdateRequest);
      if (res.status !== 200) {
          toast.error("Failed to update parking spot!");
          return;
      }
      // displaying a success message if the parking spot has been updated successfully
      if (parking.availabilityStatus) {
          toast.success("Parking spot has been made available!");
      }
      else {
          toast.warning("Parking spot has been made unavailable!");
      }
      // fetch the updated data
      GetParkingDataByProviderId(userId).then((data) => {
        setParkingData(data);
      });
    }

    async function HandleDeleteEvent(parkindId: string) {
      DeleteParking(parkindId).then((res) => {
          if (res.status !== 200) {
              toast.error("Failed to delete parking spot!");
              return;
          }
          toast.success("Parking spot has been deleted!");
          GetParkingDataByProviderId(userId).then((data) => {
              setParkingData(data);
          });
      });
      handleParkingDeleteDialogClose();
    }

    async function HandleDeleteFine(fineId: string) {
      DeleteFineProvider(fineId).then((res) => {
          console.log(res)

          if(res != true) {
              toast.error("Failed to delete fine!");
              return;
          }

          toast.success("Fine has been deleted!");
          GetParkingDataByProviderId(userId).then((data) => {
              setParkingData(data);
          });
      });
      handleFineDeleteDialogClose();
    }


    // columns for the data grid component to display the parking data
    const columnsParking: GridColDef[] = [
        { field: 'id', headerName: 'ID', width: 190 },
        { field: 'address', headerName: 'Address', width: 190 },
        { field: 'verificationStatus', headerName: 'Verified', width: 150, renderCell: (params) => {
            // displaying the verification status using a chip component with an icon
            return params.value ? <Chip variant="outlined" icon={<DoneIcon />} size="small" label="Verified" color="success" /> : <Chip variant="outlined" size="small" icon={<ErrorOutlineOutlinedIcon />} label="Unverified" color="warning" />;
        }},
        { field: 'city', headerName: 'City', width: 150 },
        { field: 'availabilityStatus', headerName: 'Availability Status', width: 150, renderCell: (params) => {
            // displaying the availability status using a switch component
            return <div style={{ cursor: 'pointer' }} onClick={() => handleAvailabilityStatus(params.row as Parking)}>
                      <ParklinkSwitch color="success" checked={params.row.availabilityStatus} />
                  </div>
        }},
        { field: 'createdAt', headerName: 'Created At', width: 200, renderCell: (params) => {
            // displaying the date in a human readable format using the toLocaleString method
            return new Date(params.value as string).toLocaleString();
        }},
        {
          field: 'actions',
          headerName: 'Actions',
          width: 120,
          renderCell: (params) => {
            // displaying the verification status using a switch component and a button to view the parking spot details
            return (
                <>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button aria-haspopup="true" size="icon" variant="outline">
                      <MoreHorizontal className="h-4 w-4" />
                      <span className="sr-only">Toggle menu</span>
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuLabel>Actions</DropdownMenuLabel>
                    <Link href={`/account/provider/parking/${params.row.id}`}>
                      <DropdownMenuItem>View</DropdownMenuItem>
                    </Link>
                    <DropdownMenuItem onClick={() => handleDeleteDialogClickOpen(params.row)}>Delete</DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
                </>
            );
        }},
    ];

    // rendering the data grid component with the booking data and columns
    const columnsBooking: GridColDef[] = [
      { field: 'id', headerName: 'ID', width: 190 },
      { field: 'parkingId', headerName: 'Parking Id', width: 150 },
      { field: 'carRegistration', headerName: 'Car Registration', width: 120 },
      { field: 'fineStatus', headerName: 'Fine Status', width: 120, renderCell: (params) => {
          // displaying the verification status using a chip component with an icon
          return params.value ? <Chip variant="outlined" icon={<DoneIcon />} size="small" label="Issued" color="error" /> : <Chip variant="outlined" size="small" icon={<ErrorOutlineOutlinedIcon />} label="Non issued" color="success" />;
      }},
      { field: 'total', headerName: 'Total', width: 80 },
      { field: 'subTotal', headerName: 'Sub Total', width: 80 },
      { field: 'fees', headerName: 'Fees', width: 50 },
      { field: 'startDate', headerName: 'Booking Date', width: 170, renderCell: (params) => {
        // displaying the date in a human readable format using the toLocaleString method
        return new Date(params.value as string).toLocaleString();
      }},
      { field: 'endDate', headerName: 'End Date', width: 170, renderCell: (params) => {
        // displaying the date in a human readable format using the toLocaleString method
        return new Date(params.value as string).toLocaleString();
      }},
      { field: 'bookingConfirmation', headerName: 'Confirmation', width: 130, renderCell: (params) => {
          // displaying the availability status using a switch component
          return params.value ? <Chip variant="outlined" icon={<DoneIcon />} size="small" label="Confirmed" color="success" /> : <Chip variant="outlined" size="small" icon={<ErrorOutlineOutlinedIcon />} label="Cancelled" color="warning" />;
      }},
      { field: 'createdAt', headerName: 'Created Date', width: 170, renderCell: (params) => {
          // displaying the date in a human readable format using the toLocaleString method
          return new Date(params.value as string).toLocaleString();
      }},
      { field: 'email', headerName: 'Email', width: 150 },
      {
        field: 'actions',
        headerName: 'Actions',
        width: 120,
        renderCell: (params) => {
          // displaying the verification status using a switch component and a button to view the parking spot details
          return (
              <>
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button aria-haspopup="true" size="icon" variant="outline">
                    <MoreHorizontal className="h-4 w-4" />
                    <span className="sr-only">Toggle menu</span>
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuLabel>Actions</DropdownMenuLabel>
                  <Link href={`/account/provider/booking/${params.row.id}`}>
                    <DropdownMenuItem>View</DropdownMenuItem>
                  </Link>
                  <Link href={`/account/provider/submit/fine?bookingId=${params.row.id}`}>
                    <DropdownMenuItem>Submit Fine</DropdownMenuItem>
                  </Link>
                </DropdownMenuContent>
              </DropdownMenu>
              </>
          );
      }},
  ];

  const columnsFines: GridColDef[] = [
    { field: 'id', headerName: 'ID', width: 190 },
    { field: 'finePaid', headerName: 'Fine Paid', width: 100, renderCell: (params) => {
        // displaying the verification status using a switch component
        return <Badge variant={params.value == false ? "destructive" : 'secondary'}>{params.value == false ? 'Unpaid' : 'Paid'}</Badge>;
    }
    },
    { field: 'description', headerName: 'Description', width: 160 },
    { field: 'total', headerName: 'Total', width: 50 },
    { field: 'fineStatus', headerName: 'Fine Status', width: 130, renderCell: (params) => {
        // displaying the verification status using a chip component with an icon
        return params.value ? <Chip variant="outlined" icon={<DoneIcon />} size="small" label="Verified" color="success" /> : <Chip variant="outlined" size="small" icon={<ErrorOutlineOutlinedIcon />} label="Unverified" color="warning" />;
    }},
    { field: 'fineIssuerId', headerName: 'Issuer ID', width: 150 },
    { field: 'bookingId', headerName: 'Booking ID', width: 150 },
    { field: 'accountId', headerName: 'Account ID', width: 150 },
    { field: 'createdAt', headerName: 'Created At', width: 200, renderCell: (params) => {
        // displaying the date in a human readable format using the toLocaleString method
        return new Date(params.value as string).toLocaleString();
    }},
    {
      field: 'actions',
      headerName: 'Actions',
      width: 240,
      renderCell: (params) => {
        // displaying the verification status using a switch component and a button to view the parking spot details
        return (
            <>
              <Link href={`/account/provider/fine/${params.row.id}`}>
                  <Button variant="outline" color="primary">View</Button>
              </Link>
              <Button className="ml-2" onClick={() => handleFineDeleteDialogClickOpen(params.row)} variant="destructive">Delete</Button>
            </>
        );
    }},
  ];

  return (
    // rendering the data grid component with the parking data and columns
    <>
    <DataGrid className="mt-5 mb-10"
        rows={parkingData}
        columns={columnsParking}
        disableRowSelectionOnClick
    />
    <div className="flex justify items-left text-xl mb-4 font-bold">
        Booking Data
    </div>
    <DataGrid className="mt-5 mb-10"
        rows={bookingData}
        columns={columnsBooking}
        disableRowSelectionOnClick
    />
    <Dialog
      open={openParkingDeleteDialog}
      onClose={handleParkingDeleteDialogClose}
      aria-labelledby="alert-dialog-title"
      aria-describedby="alert-dialog-description"
    >
      <DialogTitle id="alert-dialog-title">
        {"Are you sure you want to delete this parking space?"}
      </DialogTitle>
        <DialogContent>
          <DialogContentText id="alert-dialog-description">
            This action cannot be undone. Please confirm if you want to delete this parking space.
          </DialogContentText>
        </DialogContent>
        <DialogActions className='mb-2 mr-2'>
          <Button onClick={handleParkingDeleteDialogClose}>Close</Button>
          <Button onClick={() => HandleDeleteEvent(selectedParking!.id)} variant="destructive">Delete</Button>
        </DialogActions>
      </Dialog>
      <div className="flex justify items-left text-xl mb-4 font-bold">
          Fine Data
      </div>
    <DataGrid className="mt-5 mb-10"
        rows={fineDataParam}
        columns={columnsFines}
        disableRowSelectionOnClick
    />

    <Dialog
      open={openFineDeleteDialog}
      onClose={handleFineDeleteDialogClose}
      aria-labelledby="alert-dialog-title"
      aria-describedby="alert-dialog-description"
    >
      <DialogTitle id="alert-dialog-title">
        {"Are you sure you want to delete this fine?"}
      </DialogTitle>
        <DialogContent>
          <DialogContentText id="alert-dialog-description">
            This action cannot be undone. Please confirm if you want to delete this fine.
          </DialogContentText>
        </DialogContent>
        <DialogActions className='mb-2 mr-2'>
          <Button onClick={handleFineDeleteDialogClose}>Close</Button>
          <Button onClick={() => HandleDeleteFine(selectedFine!.id)} variant="destructive">Delete</Button>
        </DialogActions>
      </Dialog>
    </>
  )
}