#! /bin/bash
./wait-for-it.sh mypostgres:5432 -t 15
dotnet NPSQLConnector.Sample.dll