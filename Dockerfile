# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy the csproj file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining files and build the app
COPY . ./
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy the built app from the build stage
COPY --from=build /app/out ./

# Set the entry point for the app
ENTRYPOINT ["dotnet", "FirstMvcApp.dll"]

# Expose the port the app will listen on
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
