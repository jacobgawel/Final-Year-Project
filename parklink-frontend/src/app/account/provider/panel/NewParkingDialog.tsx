import * as React from 'react';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { LoadingButton } from '@mui/lab';
import { Swiper, SwiperSlide } from 'swiper/react';
import AddBoxTwoToneIcon from '@mui/icons-material/AddBoxTwoTone';
import { SearchBoxCore, SearchSession } from '@mapbox/search-js-core';
import cityData from "@/app/data/output.json";
import Autocomplete from '@mui/material/Autocomplete';
import { useDropzone } from 'react-dropzone';
import { toast } from 'sonner'
import Checkbox from '@mui/material/Checkbox';
import Select, { SelectChangeEvent } from '@mui/material/Select';

// Import Swiper styles
import 'swiper/css';
import 'swiper/css/pagination';
import 'swiper/css/navigation';

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

// import required modules
import { Pagination, Navigation } from 'swiper/modules';
import { CreateParking } from '@/app/server/parking/parking';
import { ParkingCreateRequest } from '@/types/parking';
import { useMemo, useState } from 'react';
import { LocalizationProvider, TimePicker } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { InputLabel, MenuItem } from '@mui/material';


export default function NewParkingDialog() {
  // manual values from the form
  const [open, setOpen] = React.useState(false);
  const [slotType, setSlotType] = React.useState('');
  const [slotSize, setSlotSize] = React.useState('');
  const [price, setPrice] = React.useState('');
  const [evChargingInfo, setEvChargingInfo] = React.useState('');
  const [additionalFeatures, setAdditionalFeatures] = React.useState('');
  const [slotNotes, setSlotNotes] = React.useState('');
  const [slotCapacity, setSlotCapacity] = React.useState('');
  const [loading, setLoading] = React.useState(false);
  const [priceError, setPriceError] = React.useState(false);
  const [slotCapacityError, setSlotCapacityError] = React.useState(false);
  const [slotTypeError, setSlotTypeError] = React.useState(false);
  const [slotSizeError, setSlotSizeError] = React.useState(false);
  const [fileError, setFileError] = React.useState(false);
  const [city, setCity] = React.useState<any>('');
  const [searchQueryError, setSearchQueryError] = React.useState(false);
  const [citySelectError, setCitySelectError] = React.useState(false);

  const[dayLimit, setDayLimit] = React.useState('');
  const[dayLimited, setDayLimited] = React.useState(false);
  const [dayLimitError, setDayLimitError] = React.useState(false);

  const[timeLimited, setTimeLimited] = React.useState(false);
  const [timeLimit, setTimeLimit] = React.useState<any>();
  const [timeLimitError, setTimeLimitError] = React.useState(false);


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

  // search box values that are populated by the mapbox api
  const [searchQuery, setSearchQuery] = useState<any>('')
  const [suggestions, setSuggestions] = useState([])
  const [showSuggestions, setShowSuggestions] = useState(false)
  const [longitude, setLongitude] = useState<any>('')
  const [latitude, setLatitude] = useState<any>('')

  const search = new SearchBoxCore({ accessToken: process.env.NEXT_PUBLIC_PB_MAPBOX_TOKEN });
  const session = new SearchSession(search);
  var accessToken = process.env.NEXT_PUBLIC_PB_MAPBOX_TOKEN;

  // fetch data from the mapbox api
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
  // this functions handles the search input changes and updates the state and calls the fetchData function
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

  function validateForm() {
    const priceRegex = /^\d+(\.\d{1,2})?$/;
    const slotCapacityRegex = /^\d+$/;
    var errors = false;

    var totalSize = 0;
    acceptedFiles.forEach(element => {
      totalSize += element.size;
    });

    if(dayLimited && dayLimit === '') {
      setDayLimitError(true);
      errors = true;
    } else {
      setDayLimitError(false);
    }

    if(timeLimited && timeLimit === '') {
      setTimeLimitError(true);
      errors = true;
    } else {
      setTimeLimitError(false);
    }

    if(timeLimited && dayLimited) {
      setDayLimitError(true);
      setTimeLimitError(true);
      errors = true;
    }

    if (timeLimited === false && dayLimited === false) {
      setDayLimitError(false);
      setTimeLimitError(false);
      errors = true;
    }

    if (totalSize > 30000000) {
      setFileError(true);
      errors = true;
    } else {
      setFileError(false);
    }

    if (searchQuery === '') {
      errors = true;
      setSearchQueryError(true);
    } else {
      setSearchQueryError(false);
    }

    if (city === '') {
      setCitySelectError(true);
      errors = true;
    } else {
      setCitySelectError(false);
    }

    if (slotType === '') {
      setSlotTypeError(true);
      errors = true;
    } else {
      setSlotTypeError(false);
    }

    if (slotSize === '') {
      setSlotSizeError(true);
      errors = true;
    } else {
      setSlotSizeError(false);
    }

    if (!priceRegex.test(price)) {
      setPriceError(true);
      errors = true;
    } else {
      setPriceError(false);
    }

    if (!slotCapacityRegex.test(slotCapacity)) {
      setSlotCapacityError(true);
      errors = true;
    } else {
      setSlotCapacityError(false);
    }

    if (errors) {
      return true;
    }
    
    return false;
  }

  function handleSlotTypeChange(event: any) {
    setSlotType(event.target.value);
  }

  function handleSlotSizeChange(event: any) {
    setSlotSize(event.target.value);
  }

  function handlePriceChange(event: any) {
    setPrice(event.target.value);
  }

  function handleDayLimitChange(event: any) {
    setDayLimit(event.target.value);
  }

  function handleTimeLimitChange(newValue: any) {
    setTimeLimit(newValue.format('HH:mm') + ':00');
    console.log(newValue.format('HH:mm') + ':00')
  }

  function handleEvChargingInfoChange(event: any) {
    setEvChargingInfo(event.target.value);
  }

  function handleAdditionalFeaturesChange(event: any) {
    setAdditionalFeatures(event.target.value);
  }

  function handleSlotNotesChange(event: any) {
    setSlotNotes(event.target.value);
  }

  function handleSlotCapacityChange(event: any) {
    setSlotCapacity(event.target.value);
  }

  const handleClickOpen = () => {
    setOpen(true);
  };

  const handleClose = () => {
    acceptedFiles.splice(0, acceptedFiles.length);
    setOpen(false);
    setLoading(false);
    setFileError(false);
    setSearchQuery('');
    setLatitude('');
    setLongitude('');
    setCity('');
    setSlotType('');
    setSlotSize('');
    setPrice('');
    setTimeLimit('');
    setEvChargingInfo('');
    setAdditionalFeatures('');
    setSlotNotes('');
    setSlotCapacity('');
    setPriceError(false);
    setTimeLimitError(false); 
    setSlotCapacityError(false);
    setSlotTypeError(false);
    setSlotSizeError(false);
    setSearchQueryError(false);
    setCitySelectError(false);
    setTimeLimited(false);
    setDayLimit('');
    setDayLimited(false);
    setDayLimitError(false);
  };

  // Handle form submission
  async function handleSubmit(event: any) {
    setLoading(true);
    const errors = validateForm();
    if (errors) {
      setLoading(false);
      return;
    }

    // event is the form submission event
    // prevent the default action of the form submission
    event.preventDefault();

    // create a new parking object
    var parking: ParkingCreateRequest = {
      address: searchQuery,
      slotType: slotType,
      slotSize: slotSize,
      price: parseFloat(price),
      dayLimit: dayLimited ? parseInt(dayLimit) : 0,
      dayLimited: dayLimited,
      timeLimited: timeLimited,
      timeLimit: timeLimited ? timeLimit : '00:00:00',
      slotCapacity: parseInt(slotCapacity),
      evInfo: evChargingInfo,
      additionalFeatures: additionalFeatures,
      slotNotes: slotNotes,
      longitude: longitude,
      latitude: latitude,
      city: city.label,
    }

    // create a new form data object
    const formData = new FormData();

    // stringify the parking object and append it to the form data
    formData.append('parking', JSON.stringify(parking));

    // append all the files to the form data
    // acceptedFiles is an array of files
    acceptedFiles.forEach((file: File) => {
      formData.append('files', file);
    });

    const res = await CreateParking(formData);

    if (res.status !== 201) {
      console.error("Failed to create parking spot!");
      setLoading(false);
      return;
    }

    setLoading(false);
    handleClose();
    toast.success("Parking spot submitted for verification");

  }
  

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
    <React.Fragment>
      <Button variant="outlined" startIcon={<AddBoxTwoToneIcon />} onClick={handleClickOpen}>
        Add New Parking Spot
      </Button>
      <Dialog
        open={open}
        onClose={handleClose}
      >
        <DialogTitle>Submit New Parking Spot</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Please enter all the required information to submit a new parking spot. All fields marked with an asterisk (*) are required.
            All information will be verified by our team before it is published to the public and made available for booking.
          </DialogContentText>

          <div className="relative mt-4">
                <TextField 
                    className='w-full'     // lol cant believe this worked                 
                    value={searchQuery} 
                    onChange={(e) => handleChange(e.target.value)} 
                    label="Search for location *"
                    variant='outlined'
                    error={searchQueryError}
                    helperText={searchQueryError ? 'Please enter a valid location' : ''}
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
            </div>
            <div className='relative bg-white p-2'>
                <div className='text-sm text-gray-900'>
                    Dev Info<br/>
                    coords: { longitude + ' ' + latitude }<br/>
                    query: { searchQuery } <br/>
                    city { city != null ? city.label : ''}
                </div>
            </div>

            <Autocomplete
              value={city}
              onChange={(event: any, newValue: any | null) => {
                setCity(newValue);
              }}
              id="controllable-states-"
              options={cityData}
              sx={{ width: 300 }}
              renderInput={(params) => <TextField {...params} error={citySelectError} label="City *" />}
            />
            
            <InputLabel className='mt-2' id="select-slot-type">Select Slot Type</InputLabel>
            <Select
              className='mt-2'
              labelId="select-slot-type"
              value={slotType}
              onChange={handleSlotTypeChange}
              error={slotTypeError}
              fullWidth
            >
              <MenuItem value="Parking Lot">Parking Lot</MenuItem>
              <MenuItem value="Garage">Garage</MenuItem>
              <MenuItem value="Valet Parking">Valet Parking</MenuItem>
              <MenuItem value="Automated Parking Garage">Automated Parking Garage (mechanical, hydraulic lift etc)</MenuItem>
              <MenuItem value="Street Parking Spot">Street Parking Spot</MenuItem>
            </Select>
            
            <TextField
              required
              error={slotSizeError}
              helperText={slotSizeError ? 'Please enter the size of the parking spot e.g. Compact, Midsize, Large, etc.' : ''}
              label="Slot Size"
              type="text"
              fullWidth
              variant="outlined"
              margin="dense"
              placeholder='e.g. Compact, Midsize, Large, etc.'
              onChange={handleSlotSizeChange}
            />

            <TextField
              error={priceError}
              helperText={priceError ? 'Please enter a valid price' : ''}
              required
              label="Price"
              type="number"
              variant="outlined"
              margin="dense"
              placeholder='e.g. 10.00/hr'
              onChange={handlePriceChange}
            />

            <br />
            <Checkbox onChange={(e) => setDayLimited(e.target.checked)} />
            <TextField
              error={dayLimitError}
              helperText={dayLimitError ? 'Please enter a valid price' : ''}
              disabled={!dayLimited}
              required
              label="Day Limit"
              type="number"
              variant="outlined"
              margin="dense"
              placeholder='e.g. 1, 2, 3, 4'
              onChange={handleDayLimitChange}
            />

            <br />
            <Checkbox onChange={(e) => setTimeLimited(e.target.checked)} />
            <TimePicker 
            className='mt-2 mb-2'
              label="Time Limit"
              ampm={false} 
              minutesStep={30}
              value={timeLimited ? timeLimit : null}
              onChange={(newValue) => handleTimeLimitChange(newValue)}
              disabled={!timeLimited}
            />

            <br />
            <TextField
              error={slotCapacityError}
              helperText={slotCapacityError ? 'Please enter a valid slot capacity' : ''}
              required
              label="Slot Capacity"
              type="number"
              variant="outlined"
              margin="dense"
              placeholder='e.g. 1, 2, 3, etc.'
              onChange={handleSlotCapacityChange}
            />
            <TextField
              label="Electric Vehicle information (optional)"
              type="text"
              variant="outlined"
              margin="dense"
              placeholder='e.g. Does this spot have an EV charger?'
              onChange={handleEvChargingInfoChange}
              fullWidth
            />
            <TextField
              label="Additional Features (optional)"
              type="text"
              variant="outlined"
              margin="dense"
              placeholder='e.g. Covered, Gated, Security, etc.'
              onChange={handleAdditionalFeaturesChange}
              fullWidth
            />
            <TextField
              label="Slot Notes (optional)"
              type="text"
              variant="outlined"
              margin="dense"
              placeholder='e.g. Please park in the back, etc.'
              onChange={handleSlotNotesChange}
              fullWidth
            />
            <br />

            <section className="container mt-5">
              <div {...getRootProps({style})}>
                <input {...getInputProps()} />
                <p>Drop and upload your images over here</p>
                <em>(Click on the dropzone to open file explorer)</em>
              </div>
            </section>

            <br />
            {
              fileError ? <p className="text-red-500">Please make sure that your upload size is less than 30MB</p> : null
            }

            <Swiper
              pagination={{ clickable: true }}
              navigation={true}
              className="mt-5"
              spaceBetween={25}
              slidesPerView={2}
              modules={[Pagination, Navigation]}
            >
              {
              acceptedFiles.map((file: File, index) => (
                <SwiperSlide key={index}>
                  <img src={URL.createObjectURL(file)} alt={file.name} height={300} width={300} />
                </SwiperSlide>
              ))
            }
            </Swiper>
        </DialogContent>
        <DialogActions>
          <Button color="error" onClick={handleClose}>Cancel</Button>
          <LoadingButton loading={loading} variant="outlined" onClick={handleSubmit} >Request Verification</LoadingButton>
        </DialogActions>
      </Dialog>
    </React.Fragment>
    </LocalizationProvider>
  );
}