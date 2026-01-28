#!/bin/bash
set -e

echo "Starting test infrastructure..."
docker compose -f docker-compose.test.yml up -d postgres
docker compose -f docker-compose.test.yml up -d api
docker compose -f docker-compose.test.yml up -d web

echo "Waiting for services..."
sleep 15

echo "Running .NET integration tests..."
cd src/api && dotnet test PulseWord.Api.IntegrationTests --logger "trx;LogFileName=results.trx"
cd ../..

echo "Running Playwright E2E tests..."
docker compose -f docker-compose.test.yml run --rm playwright

echo "Collecting results..."
docker compose -f docker-compose.test.yml down

echo "Tests complete!"
