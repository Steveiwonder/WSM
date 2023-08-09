# WSM (Windows Server Monitor) Overview
### What is WSM?

WSM is a service for monitoring different aspects of a Windows server and alerting when certain conditions are met.

### What can WSM monitor?
- Heartbeat, this is a general heartbeat that the client must send at a set interval. This could be used to determine if the server has gone down
- Disk Space, check for a minimum amount of required free space before alerting
- Docker Containers, checks for a "running" state of a docker container
- Running Processes, checks for at least one instance of the given process name
- TCP Port Connectivity, checks if a port is open using a TcpClient

### Why?
I had a server which ran lots of different services, Plex, Game services, VPN, DNS and a bunch of docker containers and something would periodically fail, I wouldn't usually find this out until someone using one of the versions let me know. I wanted a tool that was free, and super easy to set up but couldn't find one that did everything I wanted, also I like coding so figured it was a good candidate for a project, 3 days later WSM was born.

### Who is it for?
Anyone. I wouldn't suggest running this in a large enterprise environment with system-critical infrastructure even though you technically could. It's designed for use with home labs and _maybe_ small businesses.

### The Software
WSM is split into two main components, the server and the client(s).
#### Client (Windows Service)
The client is responsible for running all of the configured health checks and reporting their state to the server

#### Server (Docker Container)
The server is responsible for keeping track of all health checks and ensuring they report a good status at regular intervals. If the service detects any issues then alerts can be sent.
There are two different types of triggers for alerts
- Missed Check In
- Bad Status Reported

#### Missed Check In
This can occur when the configured health check doesn't report to the server within the defined interval

#### Bad Status Reported
By default, all health checks should report a state of "Available" to the server, any other state will be considered bad and will trigger alerts.

# Installation
WSM is designed to run on two separate servers. An example is having a very cheap cloud-based VM that you can run `wsm.server` on and the client on your home server.

## Server
There are two steps for installing the server
1. Create an appsettings.json file
2. Running the docker container

Create an appsettings.json file somewhere on your server _e.g._ /app/appsettings.json
Here is an example `appsettings.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ReportSlipDuration": "00:00:10",
  "AlertFrequency": "00:15:00",
  "BackgroundServiceDelay": "00:00:30",
  "Servers": [
    {
      "ApiKey": "YOUR_API_KEY",
      "Name": "Home Lab"
    }
  ],
  "NotificationType": "Email",
  "Email": {
    "Host": "",
    "Username": "",
    "Password": "",
    "Port": 587,
    "From": "",
    "To": ""

  },
  "Twilio": {
    "AccountId": "",
    "AuthToken": "",
    "From": "",
    "To": ""
  }
```

Let's break the configuration down

`ReportSlipDuration` allows a little bit of wiggle room should the server fail to check-in in time. If you have a health check configured to check-in every 15 seconds, 
this would essentially allow it to check in every 25 seconds (the above example has a 10s slip duration). Set this to "00:00:00" if you don't want any slip duration

`AlertFrequency` is the frequency that alerts are allowed to be sent out per health check. If a health check is reporting a bad status every 15 seconds, you 
don't want a notification every 15 seconds. In the example above, a notification will be sent every 15mins.

`BackgroundServiceDelay` is a small delay before the health check monitor actually starts up

`Servers` is the list of servers that you expect health check reports from. Each server consists of a `Name` and an `ApiKey`. You can have multiple servers.
```json
  "Servers": [
    {
      "ApiKey": "YOUR_API_KEY",
      "Name": "Home Lab1"
    },
    {
      "ApiKey": "YOUR_API_KEY",
      "Name": "Home Lab2"
    }
  ],
```
The `Name` of the server can be anything you one, it's included in alerts so make it something meaningful. The `ApiKey` can actually be anything you want too, it's just used for 
authentication between client and server, keep it secret, keep it safe 😜.
```json
  "Servers": [
    {
      "ApiKey": "ba88a0fe-73d2-4815-9265-6d8259541322",
      "Name": "Home Lab1"
    },
    {
      "ApiKey": "bf4d22e0-b1fd-48d4-87a2-28060f1b7b0b",
      "Name": "Home Lab2"
    }
  ],
```

`NotificationType` is the type of notification you want, the two supported values are `Email` and `WhatsApp`.  If using `Email` you must fill in the `Email` configuration. 
If you use `WhatsApp` then you must supply the `Twilio` configuration.

`Email` is only used when `NotificationType` is set to `Email`
- `Host` the host of the email server
- `Port` the port of the email server
- `Username` the username to authenticate with
- `Password` the password to authenticate with
- `From` the email address the email should come from
- `To` the email address the email should be sent to

`WhatsApp` is only used when `NotificationType` is set to `WhatsApp` - This could also be used to send SMS, but it's untested
- `AccountId` your Twilio Account Id - available within Twilio console https://console.twilio.com
- `AuthToken` your Twilio Auth Token - available within Twilio console https://console.twilio.com
- `From` your Twilio phone number that the message should be sent from. For WhatsApp it's in the format of `whatsapp:+1555555555555`
- `To` is the number the message should be sent to. For WhatsApp it's in the format of `whatsapp:+1555555555555`

Running the container

`docker run -d -p 80:80 -v /app/appsettings.json:/app/appsettings.json --name wsm.server --restart unless-stopped  steveiwonder/wsm.server`

## Client
There are three steps for installing the server
1. Extracting the release zip
2. Create an appsettings.json file
3. Install the Windows service

### Extracting the release
Grab the latest copy of the [wsm.client.zip](https://github.com/Steveiwonder/WSM/releases/latest) and extract it to a location you want to run the Windows Service from
`C:\wsm.server\` is where I have it.

### Creating appsettings.json
Add an `appsettings.json` just inside `C:\wsm.server\` here is an example of the `appsettings.json`
```json
{
  "ServiceConfig": {
    "ServiceName": "WSM",
    "DisplayName": "Windows Server Monitor",
    "Description": "Windows Server Monitor, for monitoring services!"
  },
  "App": {
    "Server": {
      "Url": "https://localhost:7180",
      "ApiKey": "YOUR_API_KEY"
    },
    "HealthChecks": [
      {
        "Name": "ServerHeartBeat",
        "Type": "Heartbeat",
        "Interval": "00:00:02"
      }      
    ]
  }
}
```
Here is where you'll define everything you want to monitor, let's go through what all the config means.
Everything within the `ServiceConfig` section is there for the Windows service, feel free to change this but the default is fine.
Everything within `App` section is the app's configuration, and you will need to change it.
`Server` - Here you need to configure both the `Url` of where the `wsm.server` docker image is hosted and the `ApiKey` that identifies this client
`HealthChecks` - This is everything you want to monitor
Every health check will have at least these values, each health check may have additional configuration.
- `Name` (Required) - The name of this health check, must be unique
- `Type` (Required) - The type of health check see [Health Check Types](#health-check-types) for more detail
- `Interval` (Optional) - The frequency at which this health check should run and is defined as `hh:mm:ss`. This is optional and defaults to `00:00:05` (5s)
- `MissedCheckInLimit` (Optional) - The number of allowed missed check-ins before alerts are triggered, the default is 2.
- `BadStatusLimit` (Optional) -The number of allowed bad status reports before alerts are triggered, the default is 2.

## Health Check Types

### Disk Space
Checks free space on a given disk
```json
{
  "Name": "C Disk Space",
  "Type": "DiskSpace",
  "Interval": "00:05:00"
  "DiskName": "C:\\",
  "MinimumFreeSpace": 16106127360,
}
```
- `DiskName` (Required) - The disk name you want to monitor
- `MinimumFreeSpace` (Required) - The minimum number of free bytes you want to maintain before alerts are sent.

### Docker Container
Checks a given docker container to ensure it has a "running" state
```json
{
  "Name": "Docker Cloudflare DNS",
  "Type": "DockerContainer",
  "Interval": "00:01:00"
  "ContainerName":"dns",
}
```
- `ContainerName` (Required) - The docker container name you want to monitor

### Heartbeat
A simple ping from client to server, to let the server know it's still online, only one of these can be configured
```json
{
  "Name": "ServerHeartBeat",
  "Type": "Heartbeat",
  "Interval": "00:00:05"
},
```

### Port (TCP Only)
Attempts to connect to a given TCP port to see if something is listening
```json
{
  "Name": "Web Server",
  "Type": "Port",
  "Interval": "00:01:00"
  "Port": 443,
  "Host": "google.com",
}
```
- `Port` (Required) - The port number to connect to
- `Host` (Optional) - The host you want to connect to, the default is `localhost`

### Process
Checks for the existence of the given process
```json
{
  "Name": "PostgreSQL",
  "Type": "Process",
  "Interval": "00:01:00",
  "ProcessName": "postgres",
  "MinCount": 1,
  "MaxCount": 1
}
```
- `ProcessName` (Required) - The name of the process to monitor without `.exe`
- `MinCount` (Optional) - The minimum number of instances, defaults to 1
- `MaxCount` (Optional) - The maximum number of instances. If not specified, there is no limit

### Installing the Windows service
Run `install-service.ps1` inside `C:\wsm.client\`, this will install and start the service

And that's it, you're done. If you have a notifier configured, you'll see notifications every time a new health check registers itself with the server.


### What's next for WSM?
1. Add HTTPS support
2. Add more health check types
3. ???

### What if a health check type isn't supported?
Ask, and I'll see if I can accommodate or create a PR. I will add a guide for writing your own health checks soon.
