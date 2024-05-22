import { getSession } from "@/app/actions/authActions";
import Link from "next/link";
import { CircleUser, Menu, Package2, Search } from "lucide-react"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Input } from "@/components/ui/input"


export default async function Page() {
    const session = await getSession();
    const role = session!.user.role;
    return (
        <>
        {
            role === 'provider' ? (
                <>
                <div className="flex min-h-screen w-full flex-col">
                    <main className="flex flex-1 flex-col gap-4 p-4 md:gap-8 md:p-10">
                        <div className="mx-auto grid w-full max-w-6xl gap-2">
                        <h1 className="text-3xl font-semibold">Provider Options</h1>
                        </div>
                        <div className="mx-auto grid w-full max-w-6xl items-start gap-6 md:grid-cols-[180px_1fr] lg:grid-cols-[250px_1fr]">
                        <nav
                            className="grid gap-4 text-sm text-muted-foreground" x-chunk="dashboard-04-chunk-0"
                        >
                            <Link className="hover:text-slate-600 hover:underline" href="/account/provider/panel">Provider Panel</Link>
                            <Link className="hover:text-slate-600 hover:underline" href="/account/admin/session">Session Variables (Dev)</Link>
                        </nav>
                        </div>
                    </main>
                </div>
                </>
            ) : (
                <div className="text-2xl text-center m-5 font-semibold">You are not authorized to view this page</div>
            )
        }
        </>
    )
}