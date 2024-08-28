
# Random Walk Stock Repository

### Dotnet VSCode

```sh
# Create new solution
dotnet new sln

# List boilerplate projects
dotnet new list

# Create new project in subdirectory
cd StockStream
dotnet new grpc
cd ../

# Add the csproj to sln
dotnet sln add "./StockStream/StockStream.csproj"

# Run the project
dotnet run --project StockStream

# Test the grpc server using a client
node ./stock-stream-js/client.js

```

