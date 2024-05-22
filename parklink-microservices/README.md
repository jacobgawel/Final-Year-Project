# Parklink Guide

### Links to the Parklink microservices
##### the links below are for the swagger documentation of the microservices that are hosted on the docker containers
### Docker Configurations
| Service | Link | Description |
| --- | --- | --- |
| Booking API | http://localhost:8000/api/v1/booking | Booking service |
| Parking API | http://localhost:8001/api/v1/parking | Parking service |
| Reviews API | http://localhost:8002/api/v1/review | Reviews service |
| Fine API | http://localhost:8003/api/v1/fine | Fine service |
| Fine gRPC | http://localhost:8005 | gRPC service |
| Booking gRPC | http://localhost:8004 | gRPC service |
| IdentityServer | http://localhost:8100 | Duende IdentityServer6 |
| YARP Proxy - Gateway Service | http://localhost:8101 | YARP Proxy |

### Localhost Configurations
| Service | Link | Description |
| --- | --- | --- |
| Booking API | http://localhost:5000/api/v1/booking | Booking service |
| Parking API | http://localhost:5001/api/v1/parking | Parking service |
| Reviews API | http://localhost:5001/api/v1/review | Reviews service |
| Fine API | http://localhost:5003/api/v1/fine | Fine service |
| Booking gRPC | http://localhost:5004 | gRPC service for booking |
| Fine gRPC | http://localhost:5005 | gRPC service for parking |
| IdentityServer | http://localhost:5100 | Duende IdentityServer6 |
| YARP Proxy - Gateway Service | http://localhost:6001 | YARP Proxy |
### How to run the Parklink microservices

##### Prerequisites
- Docker
- Docker Compose

##### Steps

1. Clone the repository

2. Open a terminal and navigate to the root directory of the "parklink-microservices" folder

3. Run the following command to build and run the docker images (-d detached mode)

```bash
docker-compose up --build -d
```

The docker-compose file will build and dockerise all the microservices including the frontend and the database.

#### Proxy Service

The proxy service is used to route requests to the correct microservice. The proxy service is configured to route requests to the following microservices:

- Booking API
- Parking API
- Reviews API
- Pricing API

The reason for using a proxy service is to allow the microservices to be hosted on different ports but still be accessible through a single port. The proxy service is also used to route through the IdentityServer to authenticate requests. There is also another authentication check in the microservices themselves to ensure that the correct user is accessing the correct data.

#### IdentityServer

The IdentityServer is used to authenticate requests to the microservices. The IdentityServer is configured to use the following users:

| Username | Password | Role |
| --- | --- | --- |
| jakubgawel | pass123$ | User |
| bob | pass123$ | Admin |

#### Service to Service
The service to service communication is cone via gRPC calls. API services that may require another service' resource may send a quick call to the gRPC service to check if the resource is available. If the resource is available, the API service will then proceed to make the request to the database. If the resource is not available, the API service will return a 404 error.

#### Database

The database is a PostgreSQL database that is hosted on a docker container. The database is used to store the data for the microservices. The database is also used to store the data for the IdentityServer.

| Database | Contents | Description |
| --- | --- | --- |
| parklinkdb | Booking, Parking, Reviews, Pricing | The data for the microservices |
| identity | IdentityServer | The data for the IdentityServer |

