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

COPY --from=build ["/src/chess-ui/chess-air-server/bin/Release/", "./"]
COPY --from=build ["/src/chess-ui/chess-air-server/bin/Debug/captcha", "./captcha"]

EXPOSE 500

ENTRYPOINT ["mono", "chess air_Server.exe"]
