"use server";

import { authOptions } from "../api/auth/[...nextauth]/route";
import { getServerSession } from "next-auth";

export async function getSession() {
    return await getServerSession(authOptions);
}

export async function getCurrentUser() {
    // Gets the current user session e.g. { name, email, image, etc... }
    try {
        const session = await getSession();

        if (!session) {
            return null;
        }

        return session.user;
    } catch (e) {
        console.error(e);
        return null;
    }
}