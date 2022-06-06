# Build context for this Dockerfile MUST be the parent of chess-ui (../), so that
# "chess-ui/chess air_Server" and the vendored engine "chess-ui/chess-engine" are
# both visible. See docker-compose.yml at the repo root.

FROM mono:6.12 AS build

# Copy into paths with no spaces so msbuild's relative ProjectReference
# ("..\chess-engine\chess\chess.csproj") still resolves: from /src/chess-ui/chess-air-server,
# ".." is /src/chess-ui, which contains chess-engine/.
COPY ["chess-ui/chess air_Server", "/src/chess-ui/chess-air-server"]
COPY ["chess-ui/chess-engine", "/src/chess-ui/chess-engine"]

WORKDIR /src/chess-ui/chess-air-server
RUN msbuild "chess air_Server.csproj" /p:Configuration=Release /restore

FROM mono:6.12 AS runtime
WORKDIR /app

# ODBC driver manager + PostgreSQL driver (DBH.cs uses System.Data.Odbc)
RUN sed -i 's/deb.debian.org/archive.debian.org/g; s/security.debian.org/archive.debian.org/g; /buster-updates/d' /etc/apt/sources.list \
    && apt-get -o Acquire::Check-Valid-Until=false update && apt-get install -y --no-install-recommends unixodbc odbc-postgresql \
    && DRV=$(find /usr/lib -name 'psqlodbcw.so' | head -1) \
    && printf '[PostgreSQL Unicode]\nDriver = %s\nDriver64 = %s\n' "$DRV" "$DRV" > /etc/odbcinst.ini \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build ["/src/chess-ui/chess-air-server/bin/Release/", "./"]
COPY --from=build ["/src/chess-ui/chess-air-server/bin/Debug/captcha", "./captcha"]

EXPOSE 500

ENTRYPOINT ["mono", "chess air_Server.exe"]
