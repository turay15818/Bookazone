# Bookazone Readme
## Mobile API v1 Documentation
## Migrations commands to execute

1. [x] dotnet tool update --global dotnet-ef
2. [x] dotnet ef migrations add InitialCreate0 --context BookazoneDbContext;
3. [x] dotnet ef database update --context ScslDbContext
4. [x] dotnet ef migrations add InitialCreate12 --context BookazoneDbContext; dotnet ef database update --context BookazoneDbContext
5. [x] Remember to change the token expiration time in Authenticate service