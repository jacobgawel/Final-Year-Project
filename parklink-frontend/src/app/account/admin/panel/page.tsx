import { getSession } from "@/app/actions/authActions";
import AdminBoard from "./AdminBoard";

export default async function Dashboard() {
  const session = await getSession();
  const role = session!.user.role;
  return (
      <>
      {
          role === 'admin' ? (
              <AdminBoard />
          ) : (
              <div className="text-2xl text-center m-5 font-semibold">You are not authorized to view this page</div>
          )
      }
      </>
  )
}