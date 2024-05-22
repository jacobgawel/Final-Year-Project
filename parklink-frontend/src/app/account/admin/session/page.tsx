import React from 'react';
import { getSession, getTokenClient } from '@/app/actions/authActions';
import Heading from '@/app/components/Heading';
import AuthTest from './AuthTest';

export default async function SessionPage() {
    const session = await getSession();
    const token = await getTokenClient();
    return (
        <>
        <Heading title="Session Dashboard" />
        <div className="bg-blue-200 border-2 border-blue-500">
            <h3 className='text-lg'>
                Session Data
            </h3>
            <pre>{JSON.stringify(session, null, 2)}</pre>
        </div>
        <div className='mt-4'>
            <AuthTest />
        </div>
        <div className='bg-green-200 border-2 border-blue-500 mt-4'>
            <h2>Token Data</h2>
            <pre className='overflow-auto'>{JSON.stringify(token, null, 2)}</pre>
        </div>
        </>
    );
}
