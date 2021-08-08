#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["JWTWebApp/JWTWebApp.csproj", "JWTWebApp/"]
RUN dotnet restore "JWTWebApp/JWTWebApp.csproj"
COPY . .
WORKDIR "/src/JWTWebApp"
RUN dotnet build "JWTWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JWTWebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JWTWebApp.dll"]