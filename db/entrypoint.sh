#!/bin/bash
set -e

DATA_DIR=/var/opt/mssql/data
SEED_DIR=/opt/seed
DB_NAME="${DB_NAME:-ChessDB}"

# Named volumes can come up owned by root; sqlservr runs as the unprivileged
# mssql user and needs write access to its data/log/secrets directories.
chown -R mssql:mssql /var/opt/mssql

if [ ! -f "$DATA_DIR/DB.mdf" ]; then
  echo "Seeding database files into $DATA_DIR..."
  cp "$SEED_DIR/DB.mdf" "$SEED_DIR/DB_log.ldf" "$DATA_DIR/"
  chown mssql:mssql "$DATA_DIR/DB.mdf" "$DATA_DIR/DB_log.ldf"
fi

su mssql -s /bin/bash -c "/opt/mssql/bin/sqlservr" &
SQLPID=$!

echo "Waiting for SQL Server to start..."
until /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" >/dev/null 2>&1; do
  sleep 2
done
echo "SQL Server is up."

ALREADY_ATTACHED=$(/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -h -1 -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = '$DB_NAME'" | tr -d '[:space:]')

if [ "$ALREADY_ATTACHED" != "1" ]; then
  echo "Attaching database $DB_NAME..."
  /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "
    CREATE DATABASE [$DB_NAME] ON
      (FILENAME = N'$DATA_DIR/DB.mdf'),
      (FILENAME = N'$DATA_DIR/DB_log.ldf')
    FOR ATTACH;"
else
  echo "Database $DB_NAME already attached, skipping."
fi

wait $SQLPID
