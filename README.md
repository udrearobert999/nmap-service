# Project Name

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