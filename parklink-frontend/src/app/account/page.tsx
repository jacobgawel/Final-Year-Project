import { getCurrentUser } from "../actions/authActions"

export default async function Profile() {
    var userDetails = await getCurrentUser();
    console.log(userDetails);
    return (
        <>
        <div className="flex items-center justify-center">
            <div className="bg-white rounded-lg shadow-lg p-8">
                <h1 className="text-4xl font-bold mb-2">Profile</h1>
                <p className="text-gray-500 mb-2">View your profile details</p>
                <div className="border-b border-gray-300 mb-4"></div>
                <p className="text-gray-500">Welcome back, {userDetails?.email}</p>
                <p className="text-gray-500">Name: {userDetails?.name}</p>
                <p className="text-gray-500">Role: {userDetails?.role}</p>
            </div>
        </div>
        </>
    )
}