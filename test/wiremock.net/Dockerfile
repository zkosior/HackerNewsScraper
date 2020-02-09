FROM sheyenrath/wiremock.net:latest
WORKDIR /app
COPY ./mappings ./__admin/mappings
EXPOSE 80
ENTRYPOINT ["./wiremock-net", "--Urls", "http://*:80", "--ReadStaticMappings", "true"]
