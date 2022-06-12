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

# supported Debian release: the mono:6.12 image is Debian 10, whose packages are no longer maintained
FROM debian:bookworm-slim AS runtime
WORKDIR /app

# Mono runtime with the assemblies the server references; ODBC driver manager + PostgreSQL driver (DBH.cs uses System.Data.Odbc)
RUN apt-get update && apt-get install -y --no-install-recommends \
        mono-runtime ca-certificates-mono \
        libmono-corlib4.5-cil libmono-system4.0-cil libmono-system-core4.0-cil \
        libmono-system-data4.0-cil libmono-system-data-datasetextensions4.0-cil \
        libmono-system-net-http4.0-cil libmono-system-xml4.0-cil libmono-system-xml-linq4.0-cil \
        libmono-microsoft-csharp4.0-cil \
        unixodbc odbc-postgresql \
    && DRV=$(find /usr/lib -name 'psqlodbcw.so' | head -1) \
    && printf '[PostgreSQL Unicode]\nDriver = %s\nDriver64 = %s\n' "$DRV" "$DRV" > /etc/odbcinst.ini \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build ["/src/chess-ui/chess-air-server/bin/Release/", "./"]
COPY --from=build ["/src/chess-ui/chess-air-server/bin/Debug/captcha", "./captcha"]

# The server writes log.txt into /app at runtime; allow the non-root user used by the Kubernetes deployment.
RUN chown -R 10001:10001 /app

EXPOSE 500

ENTRYPOINT ["mono", "chess air_Server.exe"]
