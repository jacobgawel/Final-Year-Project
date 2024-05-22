import { getSession } from "@/app/actions/authActions";
import ProviderPanel from "./ProviderPanel";

export default async function ProviderAdminPanel() {
    const session = await getSession();
    const role = session!.user.role;
    const userId = session!.user.userId;
    return (
        <>
        {
            role === 'provider' ? (
                <ProviderPanel userId={userId} />
            ) : (
                <div className="text-2xl text-center m-5 font-semibold">You are not authorized to view this page</div>
            )
        }
        </>
    )
}