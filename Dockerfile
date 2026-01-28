# ============================================
# Stage 1: Build Angular Web App
# ============================================
FROM node:22-alpine AS angular-build

WORKDIR /app

# Copy package files first for better caching
COPY src/web/pulseword/package*.json ./

# Install dependencies
RUN npm ci

# Copy the rest of the Angular project
COPY src/web/pulseword/ .

# Build for production
RUN npm run build -- --configuration=production

# ============================================
# Stage 2: Build .NET API
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for restore (exclude test projects for production build)
COPY src/api/PulseWord.Core/PulseWord.Core.csproj PulseWord.Core/
COPY src/api/PulseWord.Infrastructure/PulseWord.Infrastructure.csproj PulseWord.Infrastructure/
COPY src/api/PulseWord.Api/PulseWord.Api.csproj PulseWord.Api/

# Restore dependencies for API project only
RUN dotnet restore PulseWord.Api/PulseWord.Api.csproj

# Copy the rest of the source code
COPY src/api/ .

# Build and publish
RUN dotnet publish "PulseWord.Api/PulseWord.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ============================================
# Stage 3: Final Runtime Image
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Copy .NET published output
COPY --from=dotnet-build /app/publish .

# Copy Angular build to wwwroot
COPY --from=angular-build /app/dist/pulseword/browser ./wwwroot

# Expose port (Dokploy typically uses 80 or 8080)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PulseWord.Api.dll"]
