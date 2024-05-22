import { getSession } from "@/app/actions/authActions";
import FineViewer from "./FineViewer";
import { GetHumanizedBooking } from "@/app/server/booking/booking"
import { GetFineById } from "@/app/server/fine/fine"

export default async function Page({ params }: { params: { slug: string } }) {
    const session = await getSession();
    const role = session!.user.role;

    var fineId = params.slug;

    function getFineById(fineId: string) {
        return GetFineById(fineId);
    }

    function getHumanizedBooking(bookingId: string) {
        return GetHumanizedBooking(bookingId);
    }

    var fine = await getFineById(fineId);
    var booking = await getHumanizedBooking(fine.bookingId);

    return (
        <>
        {
            role === 'provider' ? (
                <FineViewer fine={fine} booking={booking} />
            ) : (
                <div className="text-2xl text-center m-5 font-semibold">You are not authorized to view this page</div>
            )
        }
        </>
    )
}