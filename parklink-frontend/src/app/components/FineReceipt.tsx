import { Card } from "@mui/material"
import Image from "next/image"

export default function FineReceipt({ fine }: { fine: any } ) {
    return (
        <Card className="mt-4 border border-gray-300 rounded-md">
            <div className="bg-gray-100 p-4 flex items-center justify-between">
                <div className="font-semibold text-xl">Fine Receipt</div>
                <div className="italic font-semibold bg-gray-200 px-2 py-1 rounded-md">{fine.id}</div>
            </div>
            
            <div className="p-4 grid grid-cols-2 gap-4">
                <div>
                    <div className="font-semibold">Fine Description</div>
                    <div className="text-gray-700">{fine.description}</div>
                </div>
                <div>
                    <div className="font-semibold">Fine Total</div>
                    <div className="text-gray-700">£{fine.total}</div>
                </div>
                <div className="col-span-2">
                    <Image alt={"fine image"} src={fine.imageUri} width={200} height={200} className="object-cover rounded-md" />
                </div>
            </div>
        </Card>
    )
}