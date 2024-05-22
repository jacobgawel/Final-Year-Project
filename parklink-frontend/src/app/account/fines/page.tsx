import { GetFinesForUser } from "@/app/server/fine/fine"
import { getCurrentUser } from "@/app/actions/authActions"
import FineCard from "./FineCard"

export default async function Page() {
    const user = await getCurrentUser()
    const fines = await GetFinesForUser(user!.userId)
    return (
        <div>
            <h1 className="mb-5 font-semibold">Fines</h1>
            {
                fines.map((fine: any, index: number) => (
                    <FineCard fine={fine} key={index} />
                ))
            }
        </div>
    )
}