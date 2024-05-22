"use client";

import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
  } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import Image from "next/image"
import { useState, useEffect } from "react";
import { UserPayFine } from "@/app/server/fine/fine";

  
export default function FineCard({ fine }: { fine: any }) {
    const [cardNumber, setCardNumber] = useState<string>("")
    const [expiryDate, setExpiryDate] = useState<string>("")
    const [cvv, setCvv] = useState<string>("")
    const [isPaid, setIsPaid] = useState<boolean>(false)
    const [isCardNumberError, setIsCardNumberError] = useState<boolean>(false)
    const [isExpiryDateError, setIsExpiryDateError] = useState<boolean>(false)
    const [isCvvError, setIsCvvError] = useState<boolean>(false)

    function handleCardNumberChange(e: any) {
        setCardNumber(e.target.value)
    }

    function handleExpiryDateChange(e: any) {
        setExpiryDate(e.target.value)
    }

    function handleCvvChange(e: any) {
        setCvv(e.target.value)
    }

    function validateExpiryDate(expiryDate: string) {
        return expiryDate.length === 4
    }

    function validateCvv(cvv: string) {
        return cvv.length === 3
    }

    function validateCardNumber(cardNumber: string) {
        return cardNumber.length === 16
    }

    useEffect(() => {
        console.log(cardNumber)
    }, [cardNumber])

    useEffect(() => {
        console.log(expiryDate)
    }, [expiryDate])

    useEffect(() => {
        console.log(cvv)
    }, [cvv])

    function payFine(fine: any) {
        if (!validateCardNumber(cardNumber)) {
            console.log("Invalid card number")
            setIsCardNumberError(true)
            return
        } else {
            setIsCardNumberError(false)
        }
        if (!validateExpiryDate(expiryDate)) {
            console.log("Invalid expiry date")
            setIsExpiryDateError(true)
            return
        } else {
            setIsExpiryDateError(false)
        }
        if (!validateCvv(cvv)) {
            console.log("Invalid cvv")
            setIsCvvError(true)
            return
        } else {
            setIsCvvError(false)
        }
        UserPayFine(fine.id).then((res) => {
            setIsPaid(true)
            console.log(res)
        })
    }

    return (
        <>
        {
            isPaid ? 
            <Card className="w-[350px] p-2">
                <div>
                    Fine has been paid successfully!
                </div>
            </Card>
            :
            <Card className="w-[350px]">
                <CardHeader>
                    <CardTitle>Fine</CardTitle>
                    <CardDescription>Reason: {fine.description}</CardDescription>
                </CardHeader>
                <CardContent>
                    <div className="grid w-full items-center gap-4">
                        <Label>Amount (£)</Label>
                        <Input value={fine.total} disabled />
                    </div>
                    <div className="grid w-full items-center gap-4">
                        <Label>Date</Label>
                        <Input value={fine.createdAt} disabled />
                    </div>
                    <div className={"mt-5"}>
                        <Image src={fine.imageUri} alt="Fine Image" width={200} height={200} priority />
                    </div>
                </CardContent>
                <CardContent>
                    <CardDescription>Booking ID: {fine.bookingId}</CardDescription>
                </CardContent>
                <CardContent>
                    <CardDescription>Card Number:</CardDescription>
                    <Input onChange={handleCardNumberChange} />
                    { isCardNumberError && <div className="text-red-500">Invalid card number</div> }
                </CardContent>
                <CardContent>
                    <CardDescription>Expiry Date:</CardDescription>
                    <Input onChange={handleExpiryDateChange} />
                    { isExpiryDateError && <div className="text-red-500">Invalid expiry date</div> }
                </CardContent>
                <CardContent>
                    <CardDescription>CVV:</CardDescription>
                    <Input onChange={handleCvvChange} />
                    { isCvvError && <div className="text-red-500">Invalid cvv</div> }
                </CardContent>
                <CardFooter className="flex justify-between">
                    <Button onClick={() => payFine(fine)} variant="outline">PAY NOW</Button>
                </CardFooter>
            </Card>
        }
        </>
    );
}