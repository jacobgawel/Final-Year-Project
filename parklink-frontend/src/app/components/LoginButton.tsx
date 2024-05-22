"use client";

import { signIn } from "next-auth/react";
import { useRouter, usePathname } from "next/navigation";
import { Button } from "@mui/material";

export default function UserActions() {
    const pathName = usePathname()
    return (
        <Button variant="outlined" onClick={() => signIn("id-server", {callbackUrl: pathName})}>
            Login
        </Button>
    )
}