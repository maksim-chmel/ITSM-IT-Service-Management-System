#!/bin/sh
set -e

cd /src/ITSM

if [ "${AUTO_MIGRATE:-true}" = "true" ]; then
  if dotnet ef migrations has-pending-model-changes --project "ITSM.csproj" --startup-project "ITSM.csproj"; then
    echo "No pending model changes."
  else
    migration_name="AutoMigration_$(date +%Y%m%d%H%M%S)"
    echo "Pending model changes detected. Creating migration: ${migration_name}"
    dotnet ef migrations add "${migration_name}" --project "ITSM.csproj" --startup-project "ITSM.csproj"
  fi

  echo "Applying migrations..."
  dotnet ef database update --project "ITSM.csproj" --startup-project "ITSM.csproj"
fi

echo "Starting application..."
exec dotnet run --project "ITSM.csproj" --no-launch-profile --urls "http://0.0.0.0:8080"
