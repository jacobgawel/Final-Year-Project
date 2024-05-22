'use client'

import React from 'react';
import { GetBookingAuthTest } from "@/app/server/booking/booking";
import { Button } from 'flowbite-react';
import { Toaster, toast } from 'sonner'
import LoadingButton from '@mui/lab/LoadingButton';
import KeyIcon from '@mui/icons-material/Key';

export default function AuthTest() {
    const [loading, setLoading] = React.useState(false);
    const [result, setResult] = React.useState<any>();

    function doUpdate() {
        setLoading(true);
        setResult(undefined);
        toast.promise(GetBookingAuthTest(), {
            loading: 'Testing Auth...',
            success: (data) => {
                setResult(data);
                return 'Auth Test Successful!';
            },
            error: (err) => {
                setResult(err);
                return 'Auth Test Failed!';
            }
        })
        // add a delay to add some visual feedback / flair to the loading button
        setTimeout(() => {
            setLoading(false);
        }, 800);
    }

    return (
        <>
        <div className='flex items-center gap-4'>
            <div>
                <LoadingButton
                    loading={loading}
                    loadingPosition="start"
                    startIcon={<KeyIcon />}
                    variant="outlined"
                    onClick={doUpdate}
                >
                    Test Authentication
                </LoadingButton>
            </div>
            <div>
                {JSON.stringify(result, null, 2)}
            </div>
        </div>
        </>
    )
}