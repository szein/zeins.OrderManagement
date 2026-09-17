# Introduction 
Zeins Order managment Application

# Getting Started
1.	Installate dependencies
2.	Installate Software dependencies (docker, WSL for windwos ..etc)


# Build and Run Containers

Before you run docker commands:
**Make sure that you are in the root folder of the repository.**



# API

## API ENVIRONMENT Values:
Use the  **ASPNETCORE_ENVIRONMENT** variale to set one of the following 
- Development -> run it localy **without** database prsistance
- Staging -> run it localy **with** database prsistance
- Production or (none) -> for production scenario

## Build image and run localy

```
 docker build -f api/Dockerfile -t api . 

 docker run --rm -d -p 8088:8080 -e ASPNETCORE_ENVIRONMENT=Staging -v sqlite-data:/zeins.order-mng.stage/api/data --name api-dev -t api
```

## Test API

You can call the following endpoints to test api readyniss

- ``
<Api-Url>/api/health -> returns "I'm Alive" (Api runs)`
``

- ``
<Api-Url>/api/health/env -> returns running Environment  (Staging, Production ..etc)
``

- ``
<Api-Url>/api/health/db -> returns Sqlite db conntection string
``




# Web
## Build image and run localy
 Run the following command to start the web application

 ```
 docker build -f web/Dockerfile -t web .

 docker run --rm -d -p 8008:80 --name web-dev -t web
 ```




