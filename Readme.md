Sawmill - web server log file monitoring tool
===========================================

The application moniros an actively written web server log file. It displays statistics and raise alerts based on the collected data. It doesn't analyse the whole content of the file. It only monitors the the data that is appended to the file after the application is started.

## Input
* UTF-8 w3c-formatted HTTP access log file
* Each line must be terminated with `"\n"`, `"\r"` or `"\r\n"`
* Lines longer then `MaxLineLength` characters are discarded
* Only the data that is appended to the file, after starting the application, is collected

## Configuration
Configuration can be modified in two ways
1. With `appsettings.json` configuration file
2. With command line parameters, ex. `dotnet Sawmill.dll Input:Path="/data/logs/access.log" Input:MaxLineLength=1000`

### Configuration parameters
  `Input:Path` - path to the input file name  
  `Input:MaxLineLength` - max line length in charaters  
  `Alerts:MonitoredPeriodSeconds` - monitored period duration in seconds  
  `Alerts:DelaySeconds` - delay in seconds when the alert is raised or canceled  
  `Alerts:HitsPerSecondsThreshold` - alert traffic threashold in hits per seconds  
  `Statistics:MonitoredPeriodSeconds` - monitored period duration in seconds  
  `Statistics:ReportDelaySeconds` - reporting delay in seconds  
  `Statistics:MaxUrlSectionCount` - max number of url sections reported in statistics  

### Default configuration
```json
{
  "Input": {
    "Path": "/tmp/access.log",
    "MaxLineLength":  10000
  },
  "Alerts": {
    "MonitoredPeriodSeconds": 120,
    "DelaySeconds": 1,
    "HitsPerSecondsThreshold": 10
  },
  "Statistics": {
    "MonitoredPeriodSeconds": 10,
    "ReportDelaySeconds": 1,
    "MaxUrlSectionCount": 5
  }
}```

## Statistics
Two types of statisticss are collected - global statistics and periodic statistics.
Global statistics contains data that has been collected since the application startup. Periodic statistics contains data that from the last n seconds.

Both global and periodic statistics include:
- total hits
- number of requests with specific status codes (2xx, 3xx, 4xx, 5xx)
- total hits grupped by url secion, sorted by sections with the most hits

### Sample output
The first line presents the global statistics and the second line presents the periodic statistics from the last 10 seconds.
<pre>[13:25:30-        ] hits: 2971, 2xx: 751, 3xx: 748, 4xx: 759, 5xx: 713, sections: [/api: 1632, /endpoint1: 283, /: 281, /users: 281, /data: 247]
[13:26:10-13:26:20] hits: 436, 2xx: 127, 3xx: 108, 4xx: 110, 5xx: 91, sections: [/api: 242, /users: 49, /: 43, /endpoint1: 40, /endpoint2: 31]</pre>

Data is collected for the `Statistics:MonitoredPeriodSeconds` seconds and displayed `Statistics:ReportDelaySeconds` seconds later. After the statistics are displayed all subsequent requests with the timestamp older then the end of the last presented period will be rejected. Application informs about that fact by displaying a suitable message.

Global statistics always include all requests with timestamp equal or newer then the end of last displayed period. They don't take a delay into account. Therefore they may temporarely not be consistent with the periodic statistics.

## Alerts

Whenever the total number of requests for the past `Alerts:MonitoredPeriodSeconds` seconds exceeds `Alerts:HitsPerSecondsThreshold` on average an alert is raised with a suitable message. Whenever the total traffic drops again below that value on average for the past `Alerts:MonitoredPeriodSeconds` seconds the alert is canceled with another message.

Alerts are displayed with the `Alerts:MonitoredPeriodSeconds` seconds delay.

### Sample output
<pre>High traffic generated an alert - hits = 1250, triggered at 13:25:56
Recovered from the altert - hits = 1170, triggered at 13:28:02</pre>

