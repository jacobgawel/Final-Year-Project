"use client";

import ProviderParkingGrid from "./ProviderParkingGrid";
import { GetParkingDataByProviderId } from "@/app/server/parking/parking";
import { GetProviderAnalytics as GetFineProviderAnalytics } from "@/app/server/fine/fine";
import { GetBookingByProviderId, GetBookingAnalyticsForProvider } from "@/app/server/booking/booking";
import { useState, useEffect, use } from "react";
import LinearProgress from '@mui/material/LinearProgress';
import NewParkingDialog from "./NewParkingDialog";
import { LoadingButton } from "@mui/lab";
import RefreshIcon from '@mui/icons-material/Refresh';
import { toast } from 'sonner'
import { Parking } from "@/types/parking";
import { GetAllFinesForProvider } from "@/app/server/fine/fine";
import {
    Card,
    CardContent,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { BookingAnalyticsDto } from "@/types/booking";
import {
    HoverCard,
    HoverCardContent,
    HoverCardTrigger,
} from "@/components/ui/hover-card"
  


export default function ProviderPanel({ userId }: { userId: string }) {
    const [parkingData, setParkingData] = useState<any>([]);
    const [fineAnalytics, setFineAnalytics] = useState<any>([]);
    const [bookingData, setBookingData] = useState<any>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [loadingParkingRefresh, setLoadingParkingRefresh] = useState<boolean>(false);
    const [bookingAnalytics, setBookingAnalytics] = useState<BookingAnalyticsDto>();
    const [bookingAnalyticsLoading, setBookingAnalyticsLoading] = useState<boolean>(true);
    const [fineData, setFineData] = useState<any>([]);

    useEffect(() => {
        GetFineProviderAnalytics(userId).then((data) => {
            setFineAnalytics(data);
            setLoading(false);
        });
    }, []);

    useEffect(() => {
        GetAllFinesForProvider(userId).then((data) => {
            setFineData(data);
            setLoading(false);
        });
    }, []);

    useEffect(() => {
        GetParkingDataByProviderId(userId).then((data) => {
            setParkingData(data);
            setLoading(false);
        });
    }, []);

    useEffect(() => {
        GetBookingByProviderId(userId).then((data) => {
            setBookingData(data);
            setLoading(false);
        });
    }, []);

    useEffect(() => {
        GetBookingAnalyticsForProvider(userId).then((data) => {
            setBookingAnalytics(data.data);
            setBookingAnalyticsLoading(false);
        });
    }, []);

    if (loading) {
        return (
            <div className="m-5">
                <LinearProgress />
            </div>
        )
    }

    async function refreshData() {
        setLoadingParkingRefresh(true);
        console.log("Refreshing data")
        await GetParkingDataByProviderId(userId).then((data: Parking[]) => {
            setParkingData(data);
        });
        await GetBookingByProviderId(userId).then((data) => {
            setBookingData(data);
        });
        await GetBookingAnalyticsForProvider(userId).then((data) => {
            setBookingAnalytics(data.data);
        });
        await GetFineProviderAnalytics(userId).then((data) => {
            setFineAnalytics(data);
        });
        await GetAllFinesForProvider(userId).then((data) => {
            setFineData(data);
        });
        setTimeout(() => {
            setLoadingParkingRefresh(false);
        }, 500);
        toast.info("Data refreshed");
    }


    return (
        <>
        {
            // check if the user is authorized
            // this is the reason why parkingData is "any" type
            parkingData[0] != 401 ? (
                <>
                <div className="flex justify items-left text-xl mb-4 font-bold">
                    Dashboard (Provider)
                </div>
                <div className="grid grid-flow-row grid-cols-5 gap-4 mb-5 mt-5">
                    <Card className="hover:bg-slate-50">
                        {
                            bookingAnalyticsLoading ? (
                                <CardContent className="p-2">
                                    <LinearProgress className="mt-2 mb-2" />
                                </CardContent>
                            ) : (
                                <>
                                <CardHeader className="pb-2">
                                    <Label>Today's Bookings</Label>
                                    <CardTitle className="text-4xl font-bold">{bookingAnalytics?.bookingToday}</CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <div className="text-xs text-muted-foreground">{bookingAnalytics?.comparisonBookingYesterday}</div>
                                    <div className="text-xs text-muted-foreground mt-2">Yesterday: {bookingAnalytics?.bookingYesterday}</div>
                                </CardContent>
                                </>
                            )
                        }
                    </Card>
                    <Card className="hover:bg-slate-50">
                        {
                            bookingAnalyticsLoading ? (
                                <CardContent className="p-2">
                                    <LinearProgress className="mt-2 mb-2" />
                                </CardContent>
                            ) : (
                                <>
                                <CardHeader className="pb-2">
                                    <Label>Booking's This Week</Label>
                                    <CardTitle className="text-4xl font-bold">{bookingAnalytics?.totalBookingThisWeek}</CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <div className="text-xs text-muted-foreground">{bookingAnalytics?.comparisonBookingLastWeek}</div>
                                    <div className="text-xs text-muted-foreground mt-2">Last week: {bookingAnalytics?.totalBookingLastWeek}</div>
                                </CardContent>
                                </>
                            )
                        }
                        
                    </Card>
                    <Card className="hover:bg-slate-50">
                        {
                            bookingAnalyticsLoading ? (
                                <CardContent className="p-2">
                                    <LinearProgress className="mt-2 mb-2" />
                                </CardContent>
                            ) : (
                                <>
                                <HoverCard>
                                    <HoverCardTrigger>
                                        <CardHeader className="pb-2">
                                            <Label>Active Fines</Label>
                                            <CardTitle className="text-4xl font-bold">{fineAnalytics?.activeFines}</CardTitle>
                                        </CardHeader>
                                        <CardContent>
                                            <div className="text-xs text-muted-foreground">Pending Fines: {fineAnalytics?.pendingFines}</div>
                                            <div className="text-xs text-muted-foreground mt-6">Total Fines: {fineAnalytics?.totalFines}</div>
                                        </CardContent>
                                    </HoverCardTrigger>
                                    <HoverCardContent>
                                        <Label>Fine Revenue</Label>
                                        <div className="text-4xl font-bold">{fineAnalytics?.fineRevenue}</div>
                                        <div className="text-xs text-muted-foreground mt-2">Paid Fines: {fineAnalytics?.paidFines}</div>
                                    </HoverCardContent>
                                </HoverCard>
                                </>
                            )
                        }
                    </Card>
                    <Card className="hover:bg-slate-50">
                        {
                            bookingAnalyticsLoading ? (
                                <CardContent className="p-2">
                                    <LinearProgress className="mt-2 mb-2" />
                                </CardContent>
                            ) : (
                                <>
                                <HoverCard>
                                    <HoverCardTrigger>
                                        <CardHeader className="pb-2">
                                            <Label>Total Booked This Month</Label>
                                            <CardTitle className="text-4xl font-bold">{bookingAnalytics?.totalBookingThisMonth}</CardTitle>
                                        </CardHeader>
                                        <CardContent>
                                            <div className="text-xs text-muted-foreground">{bookingAnalytics?.comparisonBookingLastMonth}</div>
                                            <div className="text-xs text-muted-foreground mt-2">Last month: {bookingAnalytics?.totalBookingLastMonth}</div>
                                        </CardContent>
                                    </HoverCardTrigger>
                                    <HoverCardContent>
                                        <Label>Revenue This Month</Label>
                                        <div className="text-4xl font-bold font-bold">{bookingAnalytics?.bookingRevenueThisMonth}</div>
                                        <div className="text-sm font-bold mt-2">Last Month: {bookingAnalytics?.bookingRevenueLastMonth}</div>
                                        <div className="text-xs text-muted-foreground mt-2">Comparison: {bookingAnalytics?.comparisonRevenueLastMonth}</div>
                                    </HoverCardContent>
                                </HoverCard>
                                </>
                            )
                        }
                    </Card>
                    <Card className="hover:bg-slate-50">
                        {
                            bookingAnalyticsLoading ? (
                                <CardContent className="p-2">
                                    <LinearProgress className="mt-2 mb-2" />
                                </CardContent>
                            ) : (
                                <>
                                <CardHeader className="pb-2">
                                    <Label>This Week Revenue</Label>
                                    <CardTitle className="text-4xl font-bold">{bookingAnalytics?.bookingRevenueCurrentWeek}</CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <div className="text-xs text-muted-foreground">{bookingAnalytics?.comparisonRevenueLastWeek}</div>
                                    <div className="text-xs text-muted-foreground mt-2">Last week: {bookingAnalytics?.bookingRevenueLastWeek}</div>
                                </CardContent>
                                </>
                            )
                        }
                    </Card>
                </div>
                <div className="flex justify items-left text-xl mb-4 font-bold">
                    Parking Spaces
                </div>
                <div className="flex justify items-left">
                    <NewParkingDialog />
                    <LoadingButton loading={loadingParkingRefresh} startIcon={<RefreshIcon />} className="ml-2" variant="outlined" onClick={refreshData}>Refresh</LoadingButton>
                </div>
                <ProviderParkingGrid parkingDataParam={parkingData} bookingDataParam={bookingData} userId={userId} fineDataParam={fineData} />
                </>
            )
            : (
                <div>User Unauthorized, try relogin</div>
            )
        }
        </>
    )
}
