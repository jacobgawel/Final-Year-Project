"use client";

import Image from 'next/image'
import SearchBox from './components/SearchBox';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import { SearchSession, SearchBoxCore } from '@mapbox/search-js-core'

export default function Home() {
  // should fix the outrageous usage quota **hopefully**
  // try to find a way to cache this token maybe?
  const search = new SearchBoxCore({ accessToken: process.env.NEXT_PUBLIC_PB_MAPBOX_TOKEN });
  const session = new SearchSession(search);
  var accessToken = process.env.NEXT_PUBLIC_PB_MAPBOX_TOKEN;

  return (
    <>
    <LocalizationProvider dateAdapter={AdapterDayjs}>
    <div className='grid grid-cols-2 gap-10'>
      <div className='bg-slate-50'>
        <div className='m-4'>
          <h1 className='text-4xl font-bold'>Find and book a parking spot</h1>
          <h2 className='text-3xl'>in seconds<span className='text-blue-500 font-bold'>.</span></h2>

          <div className='m-2 mt-8'>
            <SearchBox session={session} accessToken={accessToken}/>
          </div>

        </div>
      </div>
      <div>
        <div className='mb-2'>
          <Image src='https://s3.eu-west-2.amazonaws.com/prlnk.cdn/site/hero-home.png' priority={true} width={800} height={800} alt={'hero image for the front page'}/>
        </div>
        <div className='mb-2'>
          <Image src='https://s3.eu-west-2.amazonaws.com/prlnk.cdn/site/pug-on-laptop.png' width={800} height={800} alt={'hero image for the front page'}/>
        </div>
      </div>
    </div>
    </LocalizationProvider>
    </>
  );
}