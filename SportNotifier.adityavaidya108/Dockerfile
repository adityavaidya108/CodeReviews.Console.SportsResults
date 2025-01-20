# Image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copying solution and project files
COPY SportNotifier.Asv.sln .
COPY SportsNotifierWorkerService/SportsNotifier.csproj SportsNotifierWorkerService/

# Restore dependencies
RUN dotnet restore "SportsNotifierWorkerService/SportsNotifier.csproj"

# Copy the remaining files
COPY . .

# Build the project
WORKDIR /app/SportsNotifierWorkerService
RUN dotnet build "SportsNotifier.csproj" -c Release -o /out

# Publish the application
RUN dotnet publish "SportsNotifier.csproj" -c Release -o /out

# Use the runtime image to run the app
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build-env /out .

# Entry point for the application
ENTRYPOINT ["dotnet", "SportsNotifier.dll"]
