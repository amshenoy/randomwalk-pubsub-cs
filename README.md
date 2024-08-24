
# Grpc Template

### Dotnet VSCode

```sh
# Create new solution
dotnet new sln

# List boilerplate projects
dotnet new list

# Create new project in subdirectory
cd MyProject
dotnet new grpc
cd ../

# Add the csproj to sln
dotnet sln add "./MyProject/MyProject.csproj"

# Run the project
dotnet run --project MyProject

```

