FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
EXPOSE 8080

COPY . .
RUN dotnet restore "IH.LibrarySystem.Api/IH.LibrarySystem.Api.csproj"

RUN dotnet publish "IH.LibrarySystem.Api/IH.LibrarySystem.Api.csproj" -c Release -o /app/out

WORKDIR /app/out
ENTRYPOINT ["dotnet", "IH.LibrarySystem.Api.dll"]
