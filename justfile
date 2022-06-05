set dotenv-load := true

# List available recipes
default:
    @just --list

# Create .env from .env.example if it doesn't exist yet
env:
    #!/usr/bin/env bash
    set -euo pipefail
    if [ ! -f .env ]; then
        cp .env.example .env
        echo "Created .env from .env.example - edit DB_SA_PASSWORD/SMTP_* before running 'just up'."
    else
        echo ".env already exists."
    fi

# Build both images (server + db)
build: env
    docker compose build

# Start the full stack in the background and wait for it to be ready
up: build
    docker compose up -d
    @just wait-healthy

# Block until the db container reports healthy
wait-healthy:
    #!/usr/bin/env bash
    set -euo pipefail
    echo "Waiting for db to become healthy..."
    until docker compose ps db 2>/dev/null | grep -q "healthy"; do sleep 2; done
    echo "db is healthy."

# Show container status
ps:
    docker compose ps

# Tail logs; pass a service name to filter, e.g. `just logs server`
logs service="":
    docker compose logs -f {{service}}

# Stop containers, keep the database volume
down:
    docker compose down

# Stop containers and wipe the database volume
clean:
    docker compose down -v

# Rebuild everything from scratch (no cache) and start fresh
rebuild: clean
    docker compose build --no-cache
    just up

# Restart the stack (keeps data)
restart: down up

# Open a shell in the running server container
shell-server:
    docker compose exec server /bin/bash

# Open a sqlcmd session against the running db container
sqlcmd:
    docker compose exec db /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$DB_SA_PASSWORD"

# Smoke test: confirm the server port is reachable and the seeded data is present
smoke:
    #!/usr/bin/env bash
    set -euo pipefail
    echo "Checking server TCP port..."
    nc -vz localhost 500
    echo "Checking database..."
    docker compose exec db /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$DB_SA_PASSWORD" \
        -Q "SELECT COUNT(*) AS user_count FROM [${DB_NAME:-ChessDB}].dbo.users;"
    echo "OK"

# Build the WinForms desktop client with the host's Mono toolchain (not containerized: no X11 in Linux containers)
client-build:
    cd "chess air_client" && xbuild chessair_client.csproj /p:Configuration=Release

# Start the backend (if not already up) and launch the desktop client against it
client: up client-build
    #!/usr/bin/env bash
    set -euo pipefail
    if [ -z "${DISPLAY:-}${WAYLAND_DISPLAY:-}" ]; then
        echo "No DISPLAY/WAYLAND_DISPLAY set - the client is a GUI app and needs a desktop session." >&2
        exit 1
    fi
    cd "chess air_client/bin/Release" && mono connect4_client.exe
