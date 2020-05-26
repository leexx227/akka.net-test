FROM mcr.microsoft.com/dotnet/core/sdk:3.1
WORKDIR /app

ENV CLUSTER_SEEDS "[]"
ENV CLUSTER_IP ""
ENV CLUSTER_PORT "4053"

COPY ./bin/Debug/netcoreapp3.1/ /app

EXPOSE 4053 2551 2553 5054 9110

# Install Petabridge.Cmd client
RUN dotnet tool install --global pbm 

# Needed because https://stackoverflow.com/questions/51977474/install-dotnet-core-tool-dockerfile
ENV PATH="${PATH}:/root/.dotnet/tools"