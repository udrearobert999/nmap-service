# Netowrk Mapper

## Introduction

This project is a distributed .NET application composed of two services: a REST API and a background worker.  
The API communicates asynchronously with the worker through Kafka, while both services share the same SQL database.  
The full development environment is containerized with Docker Compose for easy setup and local execution.  
This setup makes it simple to run, test, and extend the application in a consistent way.

## Approach

The application is designed around separation of responsibilities.  
The REST API handles client requests and publishes messages to Kafka, while the worker consumes those messages and performs background processing.  
Both services use the same SQL database to persist and access application data.  
Docker Compose is used to orchestrate all required services, including the API, worker, database, and Kafka broker.

## Installation

### Prerequisites

Before running the project, make sure you have the following installed:

- [Docker](https://www.docker.com/)
- Docker Compose

You can verify the installation with:

```bash
docker --version
docker compose version
```

## Functionalities
// TODO

## High Level Design

The solution is composed of two .NET services: a REST API and a background worker.  
The API receives scan requests and it both stores them in the DB and publishes them to Kafka, while the worker consumes those messages and performs the required background processing.  
Both services share the same SQL database, and Docker Compose is used to orchestrate the full environment locally.

### System Overview

<p align="center">
  <img src="img/NmapHLD.png" alt="High level design of the system" width="700"/>
</p>

### Clean Architecture

Both the API and the worker are implemented using Clean Architecture.  
This structure separates the core business logic from infrastructure and framework concerns, making the application easier to maintain, test, and extend.  
The `Domain` layer contains the business entities, the `Application` layer contains the use cases, the `Presentation` layer exposes the API endpoints or worker entry points, and the `Infrastructure` layer handles external dependencies such as the database and Kafka.

<p align="center">
  <img src="img/CleanArchitecture.png" alt="Clean Architecture used by API and Worker" width="600"/>
</p>

## Patterns Used
// TODO