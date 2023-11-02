FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["BlazorApp-Adance-signalR/BlazorApp-Adance-signalR.csproj", "BlazorApp-Adance-signalR/"]
RUN dotnet restore "BlazorApp-Adance-signalR/BlazorApp-Adance-signalR.csproj"
COPY . .
WORKDIR "/src/BlazorApp-Adance-signalR"
RUN dotnet build "BlazorApp-Adance-signalR.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazorApp-Adance-signalR.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorApp-Adance-signalR.dll"]
