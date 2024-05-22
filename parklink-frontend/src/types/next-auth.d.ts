import { DefaultSession } from "next-auth";

// these types are used in the jwt callback and session callback
// to add the username to the token and session

declare module 'next-auth' {
    // add additional details to the session by defining the details stored in the session
    interface Session {
        user: {
            username: string,
            role: string
            userId: string
        } & DefaultSession['user'],
    }

    interface Profile {
        username: string
        role: string
    }
}

declare module 'next-auth/jwt' {
    interface JWT {
        user: {
            username: string
            access_token?: string
        }
    }
}