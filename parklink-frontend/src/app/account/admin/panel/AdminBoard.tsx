"use client";

import { use, useEffect, useState } from "react";
import { GetParkingData } from "@/app/server/parking/parking";
import { Parking } from "@/types/parking";
import LinearProgress from '@mui/material/LinearProgress';
import ParkingGrid from "./ParkingGrid";
import { GetAllFinesForAdmin } from '@/app/server/fine/fine';
import { GetAdminFineAnalytics } from "@/app/server/fine/fine";
import { GetBookingAnalyticsForAdmin, GetBookingData } from "@/app/server/booking/booking";
import BookingGrid from "./BookingGrid";
import { LoadingButton } from "@mui/lab";
import RefreshIcon from '@mui/icons-material/Refresh';
import { toast } from "sonner";
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
import FineGrid from "./FineGrid";



export default function AdminBoard() {
    const [parkingData, setParkingData] = useState<Parking[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [bookingData, setBookingData] = useState<any>([]);
    const [loadingAdminRefresh, setLoadingAdminRefresh] = useState<boolean>(false);
    const [bookingAnalytics, setBookingAnalytics] = useState<BookingAnalyticsDto>();
    const [bookingAnalyticsLoading, setBookingAnalyticsLoading] = useState<boolean>(true);
    const [fineAnalytics, setFineAnalytics] = useState<any>([]);
    const [fineData, setFineData] = useState<any>([]);

    useEffect(() => {
        GetBookingAnalyticsForAdmin().then((data) => {
            setBookingAnalytics(data.data);
            setBookingAnalyticsLoading(false);
        });
    }, []);

    useEffect(() => {
        GetAllFinesForAdmin().then((data) => {
            setFineData(data);
        });
    }, []);

    useEffect(() => {
        GetAdminFineAnalytics().then((data) => {
            setFineAnalytics(data);
            setLoading(false);
        });
    }, []);

    useEffect(() => {
        GetParkingData(false).then((data) => {
            setParkingData(data);
            setLoading(false);
        });
    }, []);

    useEffect(() => {
        GetBookingData().then((data) => {
            setBookingData(data);
        });
    }, []);

    async function refreshData() {
        setLoadingAdminRefresh(true);
        await GetParkingData(false).then((data) => {
            setParkingData(data);
        });
        await GetBookingData().then((data) => {
            setBookingData(data);
        });
        await GetAllFinesForAdmin().then((data) => {
            setFineData(data);
        });
        await GetAdminFineAnalytics().then((data) => {
            setFineAnalytics(data);
        });
        await GetBookingAnalyticsForAdmin().then((data) => {
            setBookingAnalytics(data.data);
        });
        setTimeout(() => {
            setLoadingAdminRefresh(false);
        }, 500);
        toast.info("Data refreshed");
    }

    if (loading) {
        return (
            <div className="m-5">
                <LinearProgress />
            </div>
        )
    }

    return (
        <>
        {
            bookingData[0] == "401" || fineData[0] == "401" ? 
            <h1 className="m-5 text-2xl font-semibold">Error fetching data, please relogin</h1> :
            <div className="m-5">
                <div className="flex justify items-left text-xl mb-4 font-bold">
                    Admin Panel
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
                            !fineAnalytics ? (
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
                <div className="flex justify items-left mb-4">
                    <LoadingButton loading={loadingAdminRefresh} startIcon={<RefreshIcon />} variant="outlined" onClick={refreshData}>Refresh Data</LoadingButton>
                </div>
                <h2 className="text-xl font-semibold">Parking Spots</h2>
                <ParkingGrid data={parkingData} />
                <h2 className="text-xl font-semibold">Bookings</h2>
                <BookingGrid data={bookingData} />
                <h2 className="text-xl font-semibold mt-5">Fines</h2>
                <FineGrid data={fineData} />
            </div>
        }
        </>
    )
}