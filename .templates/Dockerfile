FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ARG CONFIGURATION="Release"

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Application/Application.csproj", "Application/"]
COPY ["src/Domain/Domain.csproj", "Domain/"]
COPY ["src/WebApi/WebApi.csproj", "WebApi/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "WebApi/WebApi.csproj"
COPY . .
WORKDIR "/src/src/WebApi"
RUN dotnet build "WebApi.csproj" -c "${CONFIGURATION}" -o /app/build 

FROM build AS publish
RUN dotnet build "WebApi.csproj" -c "${CONFIGURATION}" -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT [ "dotnet", "WebApi.dll" ]