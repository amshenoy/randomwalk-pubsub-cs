
# Random Walk Stock Repository

### Dotnet VSCode

```sh
# Create new solution
dotnet new sln

# List boilerplate projects
dotnet new list

# Create new project in subdirectory
cd StockGrpc
dotnet new grpc
cd ../

# Add the csproj to sln
dotnet sln add "./StockGrpc/StockGrpc.csproj"

# Run the project
dotnet run --project StockGrpc

```

