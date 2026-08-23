FROM golang:1.27-alpine AS build
WORKDIR /app

COPY ["src/vulnerabilities/vulnintel-service/go.mod", "src/vulnerabilities/vulnintel-service/go.sum", "./"]
RUN go mod download

COPY ["src/vulnerabilities/vulnintel-service/", "./"]
RUN CGO_ENABLED=0 go build -o /out/vulnintel ./cmd/vulnintel

FROM alpine:3.20 AS final
RUN apk add --no-cache ca-certificates
WORKDIR /app
COPY --from=build /out/vulnintel /usr/local/bin/vulnintel

EXPOSE 8090
ENTRYPOINT ["vulnintel"]
