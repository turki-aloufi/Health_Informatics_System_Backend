FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5098

ENV ASPNETCORE_URLS=http://+:5098


FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Health_Informatics_System_Backend/Health_Informatics_System_Backend/Health_Informatics_System_Backend.csproj", "Health_Informatics_System_Backend/Health_Informatics_System_Backend/"]
RUN dotnet restore "Health_Informatics_System_Backend/Health_Informatics_System_Backend/Health_Informatics_System_Backend.csproj"
COPY . .
WORKDIR "/src/Health_Informatics_System_Backend/Health_Informatics_System_Backend"
RUN dotnet build "Health_Informatics_System_Backend.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Health_Informatics_System_Backend.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Health_Informatics_System_Backend.dll"]
