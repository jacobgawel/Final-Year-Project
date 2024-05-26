# E-Parking Solution

## Information about this repository

![Architecture]([IMAGES]\architecture_diagram.png)

#### Project Structure

The project arterfacts that are utilised to build the project are found in 2 main folders:
1. parklink-microservices - This folder contains the microservices that are used to build the backend services using docker orchestration.
2. parklink-frontend - This folder contains the frontend of the project that is ran using node

#### System Requirements

1. Docker - https://www.docker.com/products/docker-desktop/
2. Node - https://nodejs.org/en/download
3. NPM - https://docs.npmjs.com/downloading-and-installing-node-js-and-npm

### How to check if you have the required software installed
1. Open your terminal
2. Run the command `docker -v` to check if you have docker installed
3. Run the command `node -v` to check if you have node installed

#### How to run the project

1. Clone the repository
2. Navigate to the parklink-microservices folder
3. Run the command `docker-compose up --build -d`
4. Navigate to the parklink-frontend folder
5. Run the command `npm install`
6. Run the command `npm run dev`
7. Open your browser and navigate to `http://localhost:3000`

The docker container will build all the essential services required to run the project. The frontend will be running on port 3000.
All the license keys etc that are required to run some of the plugins that require PRO versions are included and populated automatically within the docker container e.g. Hangfire Pro or LocalStack PRO.


#### Docker Configurations
Some of the services such as Parklink API, and Booking API 
have support to switch between the development and production environment.
The development environment is used to run emulate AWS services using localstack locally on the host machine.
The production environment is used to send content to the official AWS services.


#### Localhost build (without Docker)
To build the project locally you will need to have the following installed:
1. .NET Core SDK - https://dotnet.microsoft.com/en-us/download (the most recent version is 8 but the SDK should work with 7 just fine)
2. Rider - https://www.jetbrains.com/rider/download/ (recommended)


#### This is a public release so a lot of the required access tokens and keys have been removed
1. Mapbox API
2. SendGrid
3. LocalStack (required for local development instances e.g. "Development" variable)
4. Hangfire Pro
5. AWS S3 (required for production environments)

