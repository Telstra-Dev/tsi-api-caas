# Readme
Clone this repository https://dev.azure.com/BuiltEnvironmentEngineering/Telstra Smart Spaces/_git/Telstra.Core.Api?path=%2F&version=GBmaster

# Setting up Environment and variables
## Telstra.Core.Api-> appsettings.json
The appsettings stores the configuration values in name-value pairs using the JSON format. ​

Update the port,userAd,storage setting based on the project requirement.<br/>
example: the port no. specified needs to be changed depending on the available port.
```
​"ports": {
   ​"http": 1234,
   ​"https": 5678
 ​},
```
​ This can be done by modifying the env variables. <br/>
eg. MYAPP_ports_http = [yyyy] <br/>
​the prefix "MYAPP" will depend on the values specified in program.cs file 

# Telstra.Core.Api->Program.cs
Update the environment variable prefix. <br/>
eg. if project name is "MYWORKPLACE". <br/>
change the prefix to "MYWORKPLACE"

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