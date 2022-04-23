#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CFG=Release
WORKDIR /src
COPY ["Web/Jossellware.Noodle.Web.Api/Jossellware.Noodle.Web.Api.csproj", "Web/Jossellware.Noodle.Web.Api/"]
RUN dotnet restore "Web/Jossellware.Noodle.Web.Api/Jossellware.Noodle.Web.Api.csproj"
COPY . .
WORKDIR "/src/Web/Jossellware.Noodle.Web.Api"
RUN dotnet build "Jossellware.Noodle.Web.Api.csproj" -c ${BUILD_CFG} -o /app/build

FROM build AS publish
ARG BUILD_CFG=Release
RUN dotnet publish "Jossellware.Noodle.Web.Api.csproj" -c ${BUILD_CFG} -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jossellware.Noodle.Web.Api.dll"]