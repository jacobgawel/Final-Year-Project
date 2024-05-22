export { default } from "next-auth/middleware"

// add routes that need to be protected by authentication
// over here

export const config = { 
    matcher: [
        "/session",
        "/account/:path*",
        "/search/book/:path*",
    ],
    pages: {
        signIn: "/api/auth/signin"
    }
}