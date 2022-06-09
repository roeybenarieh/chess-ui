FROM python:3.13.18-alpine

RUN pip install --no-cache-dir websockify==0.13.0 \
    && adduser -D -u 10001 bridge

USER 10001
EXPOSE 6080

ENTRYPOINT ["websockify"]
CMD ["6080", "chess-server:500"]
