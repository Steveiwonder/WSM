# WSM (Windows Server Monitor) Overview
### What is WSM?

WSM is a service for monitoring different aspects of a Windows server and alerting when certain conditions are met.

### What can WSM monitor?
- Heartbeat, this is a general heartbeat that the client must send at a set interval. This could be used to determine if the server has gone down
- Disk Space, check for a minimum amount of required free space before alerting
- Docker Containers, checks for a "running" state of a docker container
- Running Processes, checks for at least one instance of the given process name
- TCP Port Connectivity, checks if a port is open using a TcpClient

### How does it work?
WSM is split into two components the server and the client.

#### Client (Windows Service)
The client is responsible for running all of the configured health checks and reporting their state to the server

#### Server (Docker Container)
The server is responsible for keeping track of all health checks and ensuring they report a good status and at regular intervals. If the service detects any issues then alerts can be sent.
There are two different types of triggers for alerts
- Missed Check In
- Bad Status Reported

#### Missed Check In
This can occur when the configured health check doesn't report to the server within the defined amount of time

#### Bad Status Reported
By default, all health checks should report a state of "Available" to the server, any other state will trigger the alert to be sent with an invalid state

# Installation
WSM is designed to run on two separate servers, a good example is having a very cheap cloud-based VM that you can run the server on, the run the client on your home lab/server.

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




