namespace Parking_Domain.Data
{
    // ------ IMPORTANT ------
    // the class over here is used to build the JSON schema.
    // It is crucial that the names of the entities within this class
    // remain lowercase to make sure they are correct JSON for the frontend.
    // You cannot change them after since they are saved as a string in the database.
    // --- DELETE THE ENTITIES WITHIN THE DB WHEN CHANGING THIS + UPDATE FRONTEND ---
    
    public class S3Image
    {
        // this is usually a Guid that is assigned to the image
        // inside the directory
        public string fileName { get; set; }
        public string fileUri { get; set; }
    }
    
    public class ParkingImageDto
    {
        private Guid Id { get; set; }
        public string directoryPrefix { get; set; }
        public List<S3Image> s3ImageUris { get; set; }
    }
}
