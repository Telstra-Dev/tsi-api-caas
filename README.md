# Readme
Clone this repository https://dev.azure.com/BuiltEnvironmentEngineering/Telstra Smart Spaces/_git/Telstra.Core.Api?path=%2F&version=GBmaster

# Setting up Environment and variables
### appsettings.json 
This will have all default properties that's going to be there all the time 
with the project in all environments. \
\
For environment specific values, create appsettings.{ENV}.json at the
root of your project. ENV will be the value of "ASPNETCORE_ENVIRONMENT". check your launchsettings.json file

### OVERRIDING CONFIG VALUES WITHOUT ACCIDENTALLY COMMITTING IT.
values in appsettings.json can be safely overriden via 2 methods
usersecrets and environment variables. 

Right Click on API Project in Visual Studio -> Manage User Secrets
Here you can mention custom values of property you want to override

For VSCode, download this extension
Name: .NET Core User Secrets
Publisher: Adrian Wilczyński

Right Click on API Project file .csproj and select Manage User Secrets


appsettings.json file
```json
"connectionsString" : {
  "db": "{{CHECK IN USER SECRETD}}"
}
```
secrets.json file (Will override the connection string DB value from above)
[more about user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows)
```json
"connectionsString" : {
  "db": "host=localhost;port=5432 ...... "
}
```

Environment Override

```connectionString__db=host=localhost;port=5432 ...... ```


# Telstra.Core.Api->Startup.cs
Configures App
Comment the Authentication part if its not required initially.

# Telstra.Core.Data
All the db context are created.
All the Table Entities are created here

# Telstra.Core.Repo
All the Repository Classes are created here

# Telstra.Common
Helper functions and Extensions
<br/>
# Connecting to the Database
Define the connection strings here under "Storage" property. (in appsetting.json). <br/>
Define the database(in Appsetting.json). <br/>
Create db context, link all the tables(in Telstra.Core.Data\context). <br/>
Define your repository here (in Telstra.Core.Repo). <br/>
link the repository in constructor(in Telstra.Core.Api\Services\service(can be renamed)). <br/>
Create and Register your service in IoC.cs file
<br/>
# Adding new API
Telstra.Core.Api->Controllers It has a sample API which can be removed. <br/>
It has some references. This is the point where it connects all the projects. <br/>
Add Project API code here.
<br/>
# Adding test file
Telstra.Core.Api.Test<br/>
Add a new test file every time a new API is created