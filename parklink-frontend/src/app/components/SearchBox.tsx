"use client";

import { useState } from 'react';
import { DatePicker, TimePicker } from '@mui/x-date-pickers';
import dayjs, { Dayjs } from 'dayjs';
import { Button } from '@mui/material';
import Link from 'next/link';
import TextField from '@mui/material/TextField';

export default function SearchBox({ session, accessToken }: { session: any, accessToken: any }) {
    const [searchQuery, setSearchQuery] = useState<any>('')
    const [suggestions, setSuggestions] = useState([])
    const [showSuggestions, setShowSuggestions] = useState(false)
    const [longitude, setLongitude] = useState<any>('')
    const [latitude, setLatitude] = useState<any>('')
    const [date, setDate] = useState<Dayjs | null>(null);
    const [arrival, setArrival] = useState<Dayjs | null>(null);
    const [exit, setExit] = useState<any>(null);

    const today = new Date();

    // fetches the data from the mapbox api
    const fetchData = (value: string) => {
        setShowSuggestions(true)

        if (value == '') {
            setSuggestions([])
            return
        }

        var data = fetch('https://api.mapbox.com/search/searchbox/v1/suggest?q=' + value + '&access_token=' + accessToken + '&country=gb&types=address,street,postcode,district,locality,city,region' + '&session_token=' + session.sessionToken)

        try {
            data.then((res) => {
                // if res is not ok, throw an error
                if(!res.ok) {
                    setSuggestions([])
                    console.log(res.status)
                }

                res.json().then((data) => {
                    setSuggestions(data.suggestions)
                })
            })
        } catch (error) {
            console.log(error)
        }
    }

    // handles the search input changes and updates the state and calls the fetchData function
    const handleChange = (value: any) => {
        // sets the value of the search box to the value of the input
        setSearchQuery(value)
        // calls the fetchData function
        fetchData(value)
    }

    function retrieveSuggestion(mapboxId: string, session_token: any) {
        const data = fetch('https://api.mapbox.com/search/searchbox/v1/retrieve/' + mapboxId + '?access_token=' + process.env.NEXT_PUBLIC_PB_MAPBOX_TOKEN + '&session_token=' + session_token)
    
        data.then((res) => {
            res.json().then((data) => {
                console.log(data.features[0])
                setLongitude(data.features[0].geometry.coordinates[0])
                setLatitude(data.features[0].geometry.coordinates[1])
            })
        }).catch((err) => {
            console.log(err)
        });
    }

    return (
        <>
            <div className="relative">
                <TextField 
                    className='w-full'     // lol wut          
                    value={searchQuery} 
                    onChange={(e) => handleChange(e.target.value)} 
                    label="Search for location"
                    variant='outlined'
                />
            </div>
            <div className='relative mt-2 bg-white p-2'>
                {
                    suggestions &&
                    showSuggestions &&
                    suggestions.map((suggestion: any, index) => {
                        // the event in this block runs a collection of functions onClick
                        // 1) it retrieves the suggestion from the mapbox api
                        // 2) it closes the suggestions box
                        // 3) it sets the selected element as the search query in the search box using the setSearchQuery function (event.currentTarget.textContent)
                        return (
                            <div key={index} className='bg-white p-2 border-b hover:bg-slate-200 cursor-pointer' onClick={ function(event){ retrieveSuggestion(suggestion.mapbox_id, session.sessionToken); setShowSuggestions(false); setSearchQuery(event.currentTarget.textContent); }}>
                                <div className='text-sm text-gray-900'>
                                    { /* customise whats displayed in the suggestions box over here */ }
                                    { suggestion.feature_type === 'postcode' ? suggestion.name + ', ' + (suggestion.context.locality == undefined ? '' : suggestion.context.locality.name + ', ')  + suggestion.context.place.name  +', ' + suggestion.context.district.name : '' }
                                    { suggestion.feature_type === 'place' ? suggestion.name + ', ' + suggestion.context.district.name + ', ' + suggestion.context.country.name : '' }
                                    { suggestion.feature_type == 'street' ? suggestion.name + ', ' + suggestion.context.postcode.name + ', ' + suggestion.context.place.name : ''} 
                                    { suggestion.feature_type == 'district' ? suggestion.name + ', ' + suggestion.context.country.name : '' }
                                    { suggestion.feature_type == 'locality' ? suggestion.name +  ', ' + suggestion.context.place.name + ', ' + suggestion.context.country.name : ''}
                                    { suggestion.feature_type == 'region' ? suggestion.name + ', ' + suggestion.context.country.name : ''}
                                    { suggestion.feature_type == 'address' ? suggestion.name + ', ' + suggestion.context.place.name + ', ' + suggestion.context.district.name : ''}
                                </div>
                            </div>
                        )
                    })
                }
                <div className='text-xs text-gray-500 mt-2'>Powered by <a href='https://www.mapbox.com/' target='_blank' className='text-blue-500'>Mapbox</a></div>
            </div>

            <div className='relative mt-2 bg-white p-2'>
                <div className='flex justify-center'>
                    <div className='grid grid-cols-3 gap-2'>
                        <div>
                            <DatePicker minDate={dayjs(today)} label='Date of arrival' value={date} onChange={(date) => setDate(date)}/>
                        </div>
                        <div>
                            <TimePicker ampm={false} minutesStep={30} label='Time of arrival' value={arrival} onChange={(arrival) => setArrival(arrival)}/>
                        </div>
                        <div>
                            <TimePicker ampm={false} minutesStep={30} label='Time of exit' value={exit} onChange={(exit) => setExit(exit)}/>
                        </div>
                    </div>
                </div>
            </div>
            
            <div className='flex justify-center mt-4'>
                { date != null && arrival != null && exit != null && arrival.isBefore(exit) && searchQuery != '' ? 
                <Link href={'/search?long=' + longitude + '&lat=' + latitude + '&q=' + searchQuery + '&date=' + date?.format('YYYY-MM-DD') + '&arrival=' + arrival?.format('HH:mm') + '&exit=' + exit?.format('HH:mm')}>
                    <Button variant="outlined" color='primary'>Search for deals</Button>
                </Link>
                : <Button variant="outlined" color='primary' disabled>Search for deals</Button> }
            </div>
        </>
    )
}