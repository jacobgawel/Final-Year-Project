import { toast } from 'sonner'
import { styled } from '@mui/material/styles';
import  Chip from '@mui/material/Chip';
import DoneIcon from '@mui/icons-material/Done';
import Switch from '@mui/material/Switch';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { UpdateParking } from "@/app/server/parking/parking";
import { Parking } from "@/types/parking";
import { useEffect, useState } from "react";
import { GetParkingData } from "@/app/server/parking/parking";
import { Button } from "@mui/material";
import ErrorOutlineOutlinedIcon from '@mui/icons-material/ErrorOutlineOutlined';
import Link from 'next/link';

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

export default function ParkingGrid({ data }: { data: Parking[]} ) {
    const [parkingData, setParkingData] = useState<Parking[]>(data);

    useEffect(() => {
      setParkingData(data);
    }, [data]);

    async function handleVerify(parking: Parking) {
        parking.parkingRejected = false;
        parking.verificationStatus = !parking.verificationStatus;
        const res = await UpdateParking(parking);
        if (res.status !== 200) {
            toast.error("Failed to update parking spot!");
            return;
        }
        toast.success("Parking spot has been " + (parking.verificationStatus ? "verified" : "unverified") + " successfully!");

        // fetch the updated data
        GetParkingData(false).then((data) => {
            setParkingData(data);
        });
    }

    async function handleReject(parking: Parking) {
        parking.parkingRejected = true;
        const res = await UpdateParking(parking);
        if (res.status !== 200) {
            toast.error("Failed to reject parking spot!");
            return;
        }
        toast.success("Parking spot has been rejected successfully!");

        // fetch the updated data
        GetParkingData(false).then((data) => {
            setParkingData(data);
        });
    }

    const columns: GridColDef[] = [
        { field: 'id', headerName: 'ID', width: 190 },
        { field: 'accountId', headerName: 'Account ID', width: 190 },
        { field: 'parkingRejected', headerName: 'Parking Rejected', width: 150, renderCell: (params) => {
            // displaying the parking rejected status using a chip component with an icon
            return params.value ? <Chip variant="outlined" size="small" label="Rejected" color="error" /> : <Chip variant="outlined" size="small" label="Not Rejected" color="success" />;
        }},
        { field: 'verificationStatus', headerName: 'Verified', width: 150, renderCell: (params) => {
            // displaying the verification status using a chip component with an icon
            return params.value ? <Chip variant="outlined" icon={<DoneIcon />} size="small" label="Verified" color="success" /> : <Chip variant="outlined" size="small" icon={<ErrorOutlineOutlinedIcon />} label="Unverified" color="warning" />;
        }},
        { field: 'availabilityStatus', headerName: 'Availability Status', width: 150, renderCell: (params) => {
            // displaying the availability status using a switch component
            return <Chip variant="outlined" size="small" label={params.value ? "Available" : "Unavailable"} color={params.value ? "success" : "default"} />;
        }},
        { field: 'createdAt', headerName: 'Created At', width: 200, renderCell: (params) => {
            // displaying the date in a human readable format using the toLocaleString method
            return new Date(params.value as string).toLocaleString();
        }},
        {
          field: 'actions',
          headerName: 'Actions',
          width: 400,
          renderCell: (params) => {
            // displaying the verification status using a switch component and a button to view the parking spot details
            return (
                <>
                Verification Status:
                <div style={{ cursor: 'pointer' }} onClick={() => handleVerify(params.row as Parking)}>
                    <ParklinkSwitch color="success" checked={params.row.verificationStatus} />
                </div>
                <Link href={`/account/admin/parking/${params.row.id}`}>
                  <Button className="m-2" variant="outlined" color="primary" size="small">View</Button>
                </Link>
                <Button className="m-2" variant="outlined" color="error" size="small" onClick={() => handleReject(params.row as Parking)}>Reject</Button>
                </>
            );
        }},
    ];

    return (
        // rendering the data grid component with the parking data and columns
        <DataGrid className="mt-5 mb-10"
            rows={parkingData}
            columns={columns}
            disableRowSelectionOnClick
        />
    )
}