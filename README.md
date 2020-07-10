# akka.net-test

This repo provides some examples using Akka.net. [Akka.net](https://github.com/akkadotnet/akka.net) is a .Net based actor model.

## How to Run the Examples
Dotnet 3.1 is required to run the examples. Build the solution and .dll files will be fine in \bin\Debug\netcoreapp3.1 folder of each project. Direct to the `netcoreapp3.1` folder of each project and run the following command lines.
### Akka.Remote
For Akka.Remote, `HostLauncher` should be run first as `BrokerLauncher` will remote deploy `HostActors` on the `host` node.

**Run HostLauncher**
```csharp
dotnet ZKB.HostLauncher.dll
```

**Run BrokerLauncher**
```csharp
dotnet ZKB.BrokerLauncher.dll 1000 100 10 600
```
`1000` represents the total requests. `100` represents that the request length is 100b. `10` means Broker will deploy `10` HostActors on each node. `600` represents the request calculation time is `600ms`.

### Akka.Cluster
For Akka.Cluster, `SeedNodeLauncher` should be run first so that the cluster can be established. Then `BackendLauncher` should be run before `RouterLauncher` as `Router` will remote deploy `10` routees on each `backend` node. `FrontendLauncher` should be the last one to run as the `FrontendActor` will send request immediately when it is deployed. Build the solution and .dll files will be fine in \bin\Debug\netcoreapp3.1 folder of each project. Direct to the `netcoreapp3.1` folder of each project and run the following command lines.

**Run SeedNodeLauncher**
```csharp
dotnet Akka.Cluster.SeedNodeLauncher.dll
```

**Run BackendLauncher**
```csharp
dotnet Akka.Cluster.BackendLauncher.dll
```

**Run RouterLauncher**
```csharp
dotnet Akka.Cluster.RouterLauncher.dll
```

**Run FrontendLauncher**
```csharp
dotnet Akka.Cluster.FrontendLauncher.dll
```

### Akka.Persistence
The example uses `SQL Server` plugin. To run the sample, a database named "akka" should be build first and the connection string in the `App.config` file should be checked. Build the project and direct into `.\akka.net-test\Akka-test\Akka.Persistence\Akka.Persistence.Simple\bin\Debug\netcoreapp3.1`.

**Run Akka.Persistence**
```csharp
dotnet Akka.Persistence.Simple.dll
```
The first time to run the command, you should find the value **10** on the console. Terminate the console and run the command again, and you should find the value **20** on the console as the actor recover from the previous state.