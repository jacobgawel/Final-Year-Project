import { Booking } from "@/types/booking";
import { GetBookingData } from "@/app/server/booking/booking";
import  Chip from '@mui/material/Chip';
import DoneIcon from '@mui/icons-material/Done';
import { useEffect, useState } from "react";
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import WarningIcon from '@mui/icons-material/Warning';
import ErrorOutlineOutlinedIcon from '@mui/icons-material/ErrorOutlineOutlined';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';
import Link from "next/link";

export default function BookingGrid({ data }: { data: Booking[] }) {
    const [bookingData, setBookingData] = useState<Booking[]>(data);
    const [bookingDialog, setBookingDialog] = useState(false);

    useEffect(() => {
        setBookingData(data);
    }, [data]);

    useEffect(() => {
        GetBookingData().then((data) => {
            setBookingData(data);
        });
    }, []);

    // dialog functions
    const handleBookingDialog = () => {
        setBookingDialog(true);
    };

    const handleBookingDialogClose = () => {
        setBookingDialog(false);
    };

    const columns: GridColDef[] = [
        { field: 'id', headerName: 'ID', width: 150 },
        { field: 'carRegistration', headerName: 'Car Registration', width: 130 },
        { field: 'accountId', headerName: 'Account ID', width: 130 },
        { field: 'fineStatus', headerName: 'Fine Status', width: 100, renderCell: (params) => {
            return (
                <div>
                    {params.value ? <Chip variant="outlined" size="small" label="Yes" icon={<WarningIcon />} color="error" /> : <Chip variant="outlined" size="small" label="No" icon={<DoneIcon />} color="success" />}
                </div>
            )
        }},
        { field: 'total', headerName: 'Price', width: 60, type: 'number', renderCell: (params) => {
            return (
                <div>
                    £{params.value}
                </div>
            )
        }},
        { field: 'bookingConfirmation', headerName: 'Booking Confirmation', width: 130, renderCell: (params) => {
            return (
                <div>
                    {params.value ? <Chip variant="outlined" size="small" label="Yes" icon={<DoneIcon />} color="success" /> : <Chip variant="outlined" size="small" label="No" icon={<ErrorOutlineOutlinedIcon />} color="default" />}
                </div>
            )
        }},
        { field: 'parkingId', headerName: 'Parking ID', width: 150 },
        { field: 'createdAt', headerName: 'Created At', width: 180, renderCell: (params) => {
            return (
                <div>
                    {new Date(params.value).toLocaleString()}
                </div>
            )
        }},
        { field: 'email', headerName: 'Email', width: 200 },
        {
            field: 'actions',
            headerName: 'Actions',
            width: 150,
            renderCell: (params) => {
                return (
                    <>
                        <Link href={`/account/admin/booking/${params.row.id}`}>
                            <Button className="m-2" variant="outlined" color="primary" size="small">View</Button>
                        </Link>
                    </>
                )
            }
        }
    ];

    return (
        <>
            <div style={{ height: 400, width: '100%' }}>
                <DataGrid className="mt-5" rows={bookingData} columns={columns} disableRowSelectionOnClick />
            </div>
        </>
    )
}