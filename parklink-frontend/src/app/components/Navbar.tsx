"use server";

import { FaParking } from "react-icons/fa";
import { getCurrentUser } from "../actions/authActions";
import UserActions from "./UserActions";
import LoginButton from "./LoginButton";
import Link from "next/link";

export default async function NavigationBar() {
    const user = await getCurrentUser();

    let navItems: string[] = ["Home", "Search" ]

    const role = user?.role;

    return (
        <header className='sticky top-0 z-50 flex justify-between bg-white p-5 items-center text-grey-800 shadow-md'>
            <Link href={"/"}>
                <div className="flex items-center gap-2 text-3xl font-semibold text-sky-600">
                    <FaParking size={34} />Parklink
                </div>
            </Link>
            <div className='flex items-center gap-4 font-semibold'>
                {navItems.map((item, index) => (
                    // If the item is "Home", then link to the root page. Otherwise, link to the page with the same name as the item.
                    // This is a really bad way of doing this but I don't know how to do it better.
                    item == "Home" ? (
                        <Link href={"/"} key={index}>
                            <div className="cursor-pointer hover:text-blue-500">{item}</div>
                        </Link>
                    ) : (
                    <Link href={"/" + item.toLowerCase().replace(" ", "-")} key={index}>
                        <div className="cursor-pointer hover:text-blue-500">{item}</div>
                    </Link>
                    )
                ))}
                <div>
                    { user ? ( <UserActions user={user} role={role}/> ) : ( <LoginButton /> ) }
                </div>
            </div>
        </header>
    );
}