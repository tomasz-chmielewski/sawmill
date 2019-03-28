Sawmill - web server log file monitoring tool
===========================================

The application monitors an actively written web server log file. It displays statistics and raise alerts based on the collected data. It doesn't analyse the whole content of the file. It only monitors the data that is appended to the file after the application is started.

## Input
* UTF-8 w3c-formatted HTTP access log file
* Each line must be terminated with `"\n"`, `"\r"` or `"\r\n"`
* Lines longer then `MaxLineLength` characters are discarded
* Only the data that is appended to the file, after starting the application, is collected

## Configuration
Configuration can be modified in two ways
1. With `appsettings.json` configuration file
2. With command line parameters, ex. `dotnet Sawmill.dll Input:Path="/data/logs/access.log" Input:MaxLineLength=1000`

#### Configuration parameters
  `Input:Path` - path to the input file  
  `Input:MaxLineLength` - max line length in characters  
  `Alerts:MonitoredPeriodSeconds` - monitored period duration in seconds  
  `Alerts:DelaySeconds` - delay in seconds when the alert is raised or cancelled  
  `Alerts:HitsPerSecondsThreshold` - alert traffic threshold in hits per seconds  
  `Statistics:MonitoredPeriodSeconds` - monitored period duration in seconds  
  `Statistics:ReportDelaySeconds` - reporting delay in seconds  
  `Statistics:MaxUrlSectionCount` - max number of url sections reported in statistics  

#### Default configuration
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
}
```

## Statistics
Two types of statistics are collected - the global statistics and the periodic statistics.
The global statistics contains data that has been collected since the application startup. The periodic statistics contains data that from the last n seconds.

Both global and periodic statistics include:
- total hits
- number of requests with specific status codes (2xx, 3xx, 4xx, 5xx)
- total hits groupped by url section, sorted by sections with the most hits

#### Sample output
The first line presents the global statistics and the second line presents the periodic statistics from the last 10 seconds.
<pre>[13:25:30-        ] hits: 2971, 2xx: 751, 3xx: 748, 4xx: 759, 5xx: 713, sections: [/api: 1632, /endpoint1: 283, /: 281, /users: 281, /data: 247]
[13:26:10-13:26:20] hits: 436, 2xx: 127, 3xx: 108, 4xx: 110, 5xx: 91, sections: [/api: 242, /users: 49, /: 43, /endpoint1: 40, /endpoint2: 31]</pre>
* all the presented timestamps are expressed in local time zone

Data is collected for the `Statistics:MonitoredPeriodSeconds` seconds and displayed `Statistics:ReportDelaySeconds` seconds later. After the statistics are displayed all subsequent requests with the timestamp lesser then the end of the last presented period will be rejected. Application informs about that fact by displaying a suitable message.

Global statistics always include all requests with timestamp greater or equal then the end of last displayed period. They don't take a delay into account. Therefore they may temporarely not be consistent with the periodic statistics.

## Alerts

Whenever the total number of requests for the past `Alerts:MonitoredPeriodSeconds` seconds exceeds `Alerts:HitsPerSecondsThreshold` on average an alert is raised with a suitable message. Whenever the total traffic drops again below that value on average for the past `Alerts:MonitoredPeriodSeconds` seconds the alert is cancelled with another message.

Alert manager analyse requests which timestamp is in the "monitored period" of time and displays alert messages with `Alerts:DelaySeconds` seconds delay. Only the requests, which timestamp is greater or equal then the current time rounded down to the nearest second minus the delay, are accepted. Otherwise a displayed message could become invalid. Application informs about rejected requests by displaying a suitable message.

#### Example  
MonitoredPeriodSeconds is 10.  
Delay is 2 second.  
Current time is 10:20:31.200.  
Accepted requests timestamp >= 10:20:29.  
At 10:20:32 the alert for the [10:20:20, 10:20:30) period will be displayed (if the threshold is exceeded) and since now, all the requests with timestamp < 10:20:30 will no longer be accepted.

#### Sample output
<pre>High traffic generated an alert - hits = 1250, triggered at 13:25:56
Recovered from the alert - hits = 1170, triggered at 13:28:02</pre>
* all the presented timestamps are expressed in local time zone

## Links
[github](https://github.com/tomasz-chmielewski/sawmill)  
[docker hub](https://hub.docker.com/r/tomaszchmielewski/sawmill)