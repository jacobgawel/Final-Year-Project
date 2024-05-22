import NextAuth, { NextAuthOptions } from "next-auth";
import DuendeIDS6Provider from "next-auth/providers/duende-identity-server6"

export const authOptions: NextAuthOptions = {
    session: {
        strategy: "jwt",
    },
    providers: [
      DuendeIDS6Provider({
        // the id below is the id of the provider, we need to use 
        // this for the buttons in the frontend e.g. signin('id-server')
        id: "id-server",
        clientId: "nextApp",
        clientSecret: "NotASecret",
        issuer: "http://localhost:8100",
        authorization: {params: {scope: "openid profile parkingApp"}}
      })
    ],
    callbacks: {
      // like said in the documentation, this is the only way to 
      // add the username to the token and session
      async jwt({token, profile, account}) {
        // use the profile to add additional details to the token
        // console.log(profile);
        if(profile) {
          // transform the token to the contain addtional details here
          token.username = profile.username as string; // fix: cast profile.username to string
          token.role = profile.role as string;
          token.userId = profile.sub as string;
        }

        if(account) {
          token.access_token = account.access_token;
        }

        return token;
      },
      async session({session, token}) {
        if(token) {
          // transform the session to the contain addtional details here
          session.user.username = token.username as string;
          session.user.role = token.role as string;
          session.user.userId = token.userId as string;
        }
        return session;
      }
    }
}

const handler = NextAuth(authOptions);
export { handler as GET, handler as POST }