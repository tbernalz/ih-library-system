FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["IH.LibrarySystem.Api/IH.LibrarySystem.Api.csproj", "IH.LibrarySystem.Api/"]
COPY ["IH.LibrarySystem.Application/IH.LibrarySystem.Application.csproj", "IH.LibrarySystem.Application/"]
COPY ["IH.LibrarySystem.Infrastructure/IH.LibrarySystem.Infrastructure.csproj", "IH.LibrarySystem.Infrastructure/"]
COPY ["IH.LibrarySystem.Domain/IH.LibrarySystem.Domain.csproj", "IH.LibrarySystem.Domain/"]

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "IH.LibrarySystem.Api/IH.LibrarySystem.Api.csproj"

COPY . .
WORKDIR "/src/IH.LibrarySystem.Api"
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "IH.LibrarySystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:PublishReadyToRun=true 

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "IH.LibrarySystem.Api.dll"]
