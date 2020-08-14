# NPSQLConnector

## Motivation

This is a dummy postgresql connector library that I've been building up. Socket programming is not my strong suite. This is why I've came up with this project to work on this issue. In the meantime, I've learned a lot about Postgres Message Flow, Auth Flow and Simple Query Protocol. 

### Run

> rm -rf postgres-data
> docker-compose up --build

To check if the table is create or not

> docker exec -it netpsqlconnector_psql_1 psql -U postgres

To show relations in psql:

> \dt

## TODOS

* Dockerizing with Class Libraries 
(Copying the dependencies is plausible)
https://imranarshad.com/dockerize-net-core-web-app-with-dependent-class-library/
Another option would be creating a jenkins automation to build this outside of docker
* Integrate Microsoft.Extension.Configuration for easy configuration.
It is more reasonable to do this at application layer. No need to introduce a dependency
* Clean up WriteBuffer
* Implement a ReadBuffer for Backend Messages
* Implement DTO classes: RowDescriptor, DataRow etc
* Async ops
* Extended Query Protocol
* Create a generic Auth Flow
* Seperate Auth Flow from Connector 
* Unit tests

## Resources

https://www.pgcon.org/2014/schedule/attachments/330_postgres-for-the-wire.pdf
https://github.com/npgsql/npgsql/blob/ab01b4386fbfbaee988e569249fdad7e3ab2dc6f/src/Npgsql/NpgsqlConnector.Auth.cs
https://www.postgresql.org/docs/10/protocol-message-formats.html - CommandComplete 
