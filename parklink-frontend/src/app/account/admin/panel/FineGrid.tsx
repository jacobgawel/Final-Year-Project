import { toast } from 'sonner'
import { styled } from '@mui/material/styles';
import  Chip from '@mui/material/Chip';
import DoneIcon from '@mui/icons-material/Done';
import Switch from '@mui/material/Switch';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { useEffect, useState } from "react";
import { Button } from "@mui/material";
import ErrorOutlineOutlinedIcon from '@mui/icons-material/ErrorOutlineOutlined';
import { VerifyFineAdmin } from '@/app/server/fine/fine';
import Link from 'next/link';
import { Fine } from '@/types/fine';
import { Badge } from "@/components/ui/badge"
import { GetAllFinesForAdmin } from '@/app/server/fine/fine';


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

export default function ParkingGrid({ data }: { data: Fine[]} ) {
    const [fineData, setFineData] = useState<Fine[]>(data);

    useEffect(() => {
      setFineData(data);
    }, [data]);

    
    async function verifyFine(id: string) {
      const res = await VerifyFineAdmin(id);

      if (res.status) {
          toast.error("Failed to verify fine!");
          return;
      }
      
      toast.success("Fine verified successfully!");

      // fetch the updated data
      GetAllFinesForAdmin().then((data) => {
          setFineData(data);
      });
  }


    const columns: GridColDef[] = [
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
                Verify Fine:
                <ParklinkSwitch disabled={params.row.fineStatus} className='mr-2' checked={params.row.fineStatus} onChange={() => verifyFine(params.row.id)} />
                <Link href={`/account/admin/fine/${params.row.id}`}>
                    <Button variant="outlined" color="primary">View</Button>
                </Link>
              </>
          );
      }},
    ];

    return (
        // rendering the data grid component with the parking data and columns
        <DataGrid className="mt-5 mb-10"
            rows={fineData}
            columns={columns}
            disableRowSelectionOnClick
        />
    )
}