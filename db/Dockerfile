FROM mcr.microsoft.com/mssql/server:2022-latest

USER root
RUN mkdir -p /opt/seed
COPY --chown=mssql:mssql ["chess air_Server/DB.mdf", "chess air_Server/DB_log.ldf", "/opt/seed/"]
COPY --chown=mssql:mssql db/entrypoint.sh /opt/entrypoint.sh
RUN chmod +x /opt/entrypoint.sh

ENTRYPOINT ["/opt/entrypoint.sh"]
