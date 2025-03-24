
# ASP.NET Core REST API

This project is an ASP.NET Core REST API with integrated Swagger documentation. It provides a robust foundation for building scalable web APIs using .NET 8.0.14.

## Overview

The API is designed following REST principles and comes with Swagger UI enabled for interactive API documentation and testing. It includes multiple launch profiles (HTTP, HTTPS, and IIS Express) to simplify development and testing.

## Prerequisites

- .NET SDK 8.0.14 or later installed on your machine.
- Install from https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- Run dotnet --list-sdks
- Make sure it list 8.0.407 [C:\Program Files\dotnet\sdk]


## Installation

1. Clone the repository:

   git clone https://github.com/Fong10921/TodoRESTApi.git

   cd TodoRESTAPi

2. Restore dependencies:

   dotnet restore

3. Run the project with:

   dotnet run

By default, the project is configured to launch on multiple URLs:
- HTTP: http://localhost:5229
- HTTPS: https://localhost:7286
- IIS: http://localhost:4242 Or https://localhost:44379

## Swagger Documentation

Swagger is integrated for auto-generated interactive API documentation. Once the application is running, open your browser and navigate to:

- HTTP: http://localhost:5229/swagger
- HTTPS: https://localhost:7286/swagger
- IIS: http://localhost:4242/swagger Or https://localhost:44379/swagger
