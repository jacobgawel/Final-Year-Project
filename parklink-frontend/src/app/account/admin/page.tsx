import { getSession } from "@/app/actions/authActions";
import Link from "next/link";

export default async function Page() {
    const session = await getSession();
    const role = session!.user.role;
    return (
        <>
        {
            role === 'admin' ? (
                <>
                <ul>
                    <li>
                        <Link href="/account/admin/panel">
                            Admin Panel
                        </Link>
                    </li>
                    <li>
                        <Link href="/account/admin/session">
                            Session Variables (Dev only)
                        </Link>
                    </li>
                </ul>
                
                </>
            ) : (
                <div className="text-2xl text-center m-5 font-semibold">You are not authorized to view this page</div>
            )
        }
        </>
    )
}