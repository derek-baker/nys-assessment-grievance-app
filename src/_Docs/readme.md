## To set necessary env vars, run this from Powershell:
    `[System.Environment]::SetEnvironmentVariable('GOOGLE_APPLICATION_CREDENTIALS','C:\<Path>\<To>\<ServiceAccountKey>\<KEY_FILE>.json',[System.EnvironmentVariableTarget]::User)`
    Note that Visual Studio caches env vars for some reason...so restart after updating 
   

## To start the user-facing app
`cd {dir_containing_sln_file}`
`dotnet build .\App\App.csproj && dotnet run --project .\App\App.csproj`


## To build docker locally
`cd {PATH}:\{TO}\{DIR_WITH_DOCKERFILE}`
`docker build --tag asgr:1.0 .` or `docker build --tag asgrw:1.0 -f ./Dockerfile .`


## You can use this command if you need to run the container, but you usually shouldn't need to. (Run the app from VS or via CLI instead)
`docker run --publish 8000:8080 --interactive --name asgr asgr:1.0`

## To interactively and transiently start the container with a shell (optional: --e GOOGLE_APPLICATION_CREDENTIALS=/key.json)
`docker rm asgr; docker run -e 'GOOGLE_APPLICATION_CREDENTIALS=/key.json' -e 'ASPNETCORE_ENVIRONMENT=Development' -it --name asgr asgr:1.0 /bin/sh ` 

## To enter a running container
docker exec -it [container-id] /bin/sh

## To download entire GCP Cloud Storage Bucket:
`gsutil cp -r gs://bucket/folder .`


## HELPFUL DOCS:
    GCP Cloud Storage - https://googleapis.github.io/google-cloud-dotnet/docs/Google.Cloud.Storage.V1/index.html
    NET Core File Uploads - https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-3.1
    MongoDB - https://docs.mongodb.com/manual/reference/read-concern/
    MongoDB - https://docs.mongodb.com/manual/reference/connection-string/#connections-connection-options

    Sendgrid - https://github.com/sendgrid/sendgrid-csharp
    GCP Secret Manager - https://cloud.google.com/secret-manager/docs/reference/libraries
