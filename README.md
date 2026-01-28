# PulseWord

PulseWord is a multiplayer Wordle game featuring daily challenges, real-time social features, and a competitive leaderboard.

## Tech Stack

- **Backend**: .NET 10 Web API
- **Database**: PostgreSQL
- **Real-time**: SignalR
- **Frontend**: Angular 21 with Angular Material
- **E2E Testing**: Playwright
- **CI/CD**: GitHub Actions
- **Containerization**: Docker & Docker Compose

## Project Structure

- `src/api`: .NET backend solution and projects.
  - `PulseWord.Api`: The Web API project.
  - `PulseWord.Core`: Core domain entities and interfaces.
  - `PulseWord.Infrastructure`: Data access and external services implementation.
  - `PulseWord.Api.IntegrationTests`: Integration tests for the API.
- `src/web/pulseword`: Angular frontend application.
- `scripts`: Utility scripts for development and testing.

## Local Development Setup

### Prerequisites

- .NET 10 SDK
- Node.js 22+
- Docker & Docker Compose

### Backend

1. Navigate to the API directory:
   ```bash
   cd src/api
   ```
2. Restore and build:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Run the application:
   ```bash
   dotnet run --project PulseWord.Api
   ```
   Note: Ensure a PostgreSQL instance is running or use Docker Compose.

### Frontend

1. Navigate to the web directory:
   ```bash
   cd src/web/pulseword
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   npm start
   ```
   The app will be available at `http://localhost:4200`.

### Using Docker Compose

To start the entire stack (API, Web, and DB):
```bash
docker-compose up --build
```

## How to Run Tests

### API Tests
```bash
dotnet test src/api/PulseWord.slnx
```

### Web Unit Tests
```bash
cd src/web/pulseword
npm test
```

### E2E Tests
```bash
cd src/web/pulseword
npx playwright test
```

## How to Generate API Client

The frontend uses an OpenAPI generated client to communicate with the backend.

1. Ensure the backend is running and Swagger is accessible at `http://localhost:5000/swagger/v1/swagger.json`.
2. Run the generation script:
   ```bash
   cd src/web/pulseword
   npm run generate-api:local
   ```

## Deployment

The project is configured for automated deployment via GitHub Actions.

1. **CI**: Every push to `main` or PR triggers a full build and test suite.
2. **CD**: Upon successful CI on the `main` branch, a Docker image is built and pushed to GitHub Container Registry (GHCR).
3. **Webhook**: Optionally, a deployment webhook (e.g., Dokploy) can be triggered after the image is pushed.

Ensure the following secrets are set in your GitHub repository:
- `DOKPLOY_WEBHOOK_URL` (Optional)
