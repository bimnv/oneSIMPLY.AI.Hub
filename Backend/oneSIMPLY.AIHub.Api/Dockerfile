FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Backend/oneSIMPLY.AIHub.Api/oneSIMPLY.AIHub.Api.csproj Backend/oneSIMPLY.AIHub.Api/
RUN dotnet restore Backend/oneSIMPLY.AIHub.Api/oneSIMPLY.AIHub.Api.csproj
COPY Backend/oneSIMPLY.AIHub.Api/ Backend/oneSIMPLY.AIHub.Api/
RUN dotnet publish Backend/oneSIMPLY.AIHub.Api/oneSIMPLY.AIHub.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "oneSIMPLY.AIHub.Api.dll"]
