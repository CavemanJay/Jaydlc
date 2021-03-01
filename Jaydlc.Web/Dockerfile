#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Jaydlc.Web/Jaydlc.Web.csproj", "Jaydlc.Web/"]
RUN dotnet restore "Jaydlc.Web/Jaydlc.Web.csproj"
COPY . .
WORKDIR "/src/Jaydlc.Web"
RUN dotnet build "Jaydlc.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Jaydlc.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jaydlc.Web.dll"]