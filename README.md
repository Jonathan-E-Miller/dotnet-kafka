# dotnet-kafka
A repository to demonstrate using kafka in dotnet.

## Prerequisites
* Docker desktop
* .NET 6.0

## Getting Started
To start all containers run the following commands

`docker compose build`

`docker compose up`

You can then access the producer API on https://localhost:8001/swagger and the consumer MVC web app on https://localhost:8002

You can view application logs at http://localhost:5342/#/events

## Useful commands

### Create a topic
`docker exec broker kafka-topics --bootstrap-server broker:9092 --create --topic <topic-name>`

### List all topics
`docker exec broker kafka-topics --bootstrap-server broker:9092 --list`

### Delete topic
`docker exec broker kafka-topics --delete --bootstrap-server broker:9092 --topic <topic-name>`

### Publish messages to the topic
`docker exec --interactive --tty broker kafka-console-producer --bootstrap-server broker:9092 --topic <topic-name>`

### Subscribe to messages from the topic run the following command
`docker exec --interactive --tty broker kafka-console-consumer --bootstrap-server broker:9092 --topic <topic-name> --from-beginning`

## Credit
https://medium.com/@marekzyla95/mongo-repository-pattern-700986454a0e
