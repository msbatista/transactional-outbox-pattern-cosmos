#! /bin/bash

mkdir src
mkdir test

touch CHANGELOG.md
touch .env-sample

cp ./.templates/README.md README.md
cp ./.templates/Makefile Makefile
cp ./.templates/.gitgnore.webapp .gitignore
cp ./.templates/Dockerfile Dockerfile
cp ./.templates/.dockerignore .dockerignore
cp ./.templates/docker-compose.yml docker-compose.yml
cp ./.templates/.env-sample .env-sample


dotnet new sln --name OutboxPattern

dotnet new webapi --name WebApi --output src/WebApi
dotnet new classlib --name Domain --output src/Domain
dotnet new classlib --name Infrastructure --output src/Infrastructure
dotnet new classlib --name Application --output src/Application
dotnet new xunit --name UnitTest --output test/UnitTest
dotnet new xunit --name ComponentTest --output test/ComponentTest
dotnet new xunit --name IntegrationTest --output test/IntegrationTest
dotnet new xunit --name EndToEnd --output test/EndToEnd

dotnet sln add ./src/WebApi/WebApi.csproj
dotnet sln add ./src/Domain/Domain.csproj
dotnet sln add ./src/Infrastructure/Infrastructure.csproj
dotnet sln add ./src/Application/Application.csproj
dotnet sln add ./test/UnitTest/UnitTest.csproj
dotnet sln add ./test/ComponentTest/ComponentTest.csproj
dotnet sln add ./test/IntegrationTest/IntegrationTest.csproj
dotnet sln add ./test/EndToEnd/EndToEnd.csproj

git init -b main

cp -r ./.hooks/prepare-commit-msg .git/hooks

chmod +x .git/hooks/prepare-commit-msg

git add .
git commit -m "Base project"

