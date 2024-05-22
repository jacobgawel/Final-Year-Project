import { useState, useEffect } from 'react';
import mapboxgl from 'mapbox-gl';


const useMapboxMap = (): mapboxgl.Map | null => {
    const [map, setMap] = useState<mapboxgl.Map | null>(null);

    useEffect(() => {
        console.log("useMapboxMap hook called")
        var token = process.env.NEXT_PUBLIC_PB_MAPBOX_TOKEN;
        if (!token) {
            console.log("No token found")
            return;
        }
        mapboxgl.accessToken = token;

        const newMap = new mapboxgl.Map({
            container: 'map', // make sure this container exists in your JSX
            center: [-2.4360, 54.3781],
            style: 'mapbox://styles/mapbox/streets-v12',
            zoom: 1
        });

        newMap.addControl(new mapboxgl.GeolocateControl({
            positionOptions: {
                enableHighAccuracy: true
            },
            trackUserLocation: true,
            showUserHeading: false
        }));

        newMap.addControl(new mapboxgl.NavigationControl(), 'bottom-right');

        newMap.on('load', () => {
            setMap(newMap); // Set the map instance once it's fully loaded
        });

        // Cleanup
        return () => newMap.remove();
    }, []);

    return map;
};

export default useMapboxMap;
