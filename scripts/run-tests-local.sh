#!/bin/bash
set -e

echo "Running .NET Integration Tests (using Testcontainers for DB)..."
cd src/api
dotnet test PulseWord.Api.IntegrationTests
cd ../..

echo "Starting API and Web for Playwright tests..."
# This part assumes you have the API and Web running locally or we start them here.
# Since this is a 'local' script, we'll try to run them in the background.

export ASPNETCORE_ENVIRONMENT=Testing
export ConnectionStrings__PulseWord="Host=localhost;Database=pulseword_test;Username=pulseword_test;Password=testpassword"

# Note: This script assumes a local Postgres is NOT needed if Testcontainers is used in tests,
# but for the API to run for Playwright, it might need a real DB or be configured to use Testcontainers (which is unusual for the app itself).
# Alternatively, we just run the tests and assume the dev has the infrastructure ready.

echo "Running Playwright tests locally..."
cd src/web/pulseword
# Ensure dependencies are installed
npm install
# Install Playwright browsers if needed
npx playwright install --with-deps

# Run tests
npx playwright test
cd ../../..

echo "Local tests complete!"
