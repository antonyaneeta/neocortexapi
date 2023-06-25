#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["MyCloudProjectSample/MyCloudProject/MyCloudProject.csproj", "MyCloudProjectSample/MyCloudProject/"]
COPY ["MyCloudProjectSample/MyCloudProject.Common/MyCloudProject.Common.csproj", "MyCloudProjectSample/MyCloudProject.Common/"]
COPY ["MyCloudProjectSample/MyExperiment/MyExperiment.csproj", "MyCloudProjectSample/MyExperiment/"]
COPY ["NeoCortexApi/NeoCortexApi.csproj", "NeoCortexApi/"]
COPY ["NeoCortexArrayLib/NeoCortexArrayLib.csproj", "NeoCortexArrayLib/"]
COPY ["NeoCortexEntities/NeoCortexEntities.csproj", "NeoCortexEntities/"]
COPY ["UnitTestsProject/UnitTestsProject.csproj", "UnitTestsProject/"]
COPY ["NeoCortexApi.Parallel/NeoCortexApi.Parallel.csproj", "NeoCortexApi.Parallel/"]
COPY ["NeoCortexUtils/NeoCortexUtils.csproj", "NeoCortexUtils/"]
COPY ["WebSocketNeuroVisualizer/WebSocketNeuroVisualizer.csproj", "WebSocketNeuroVisualizer/"]
RUN dotnet restore "MyCloudProjectSample/MyCloudProject/MyCloudProject.csproj"
COPY . .
WORKDIR "/src/MyCloudProjectSample/MyCloudProject"
RUN dotnet build "MyCloudProject.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyCloudProject.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyCloudProject.dll"]