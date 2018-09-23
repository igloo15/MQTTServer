Igloo15.MQTTServer
===
A dotnet global tool of the [MQTTNet](https://github.com/chkr1011/MQTTnet) library

## Install

```
dotnet tools install -g Igloo15.MQTTServer.Tool
```

## Usage

```
dotnet mqttserver --help
mqttserver 0.10.0
Copyright (C) 2018 Igloo15

  -p, --port                      (Default: 5002) The port to run the mqtt server on

  -a, --address                   (Default: Any) The ip address of the server it defaults to Any

  --timeout                       (Default: 15) The amount of time a connection is maintained without communication
                                  before dropping

  --connection-backlog            (Default: 10) Amount of connections kept in backlog before dropping

  --message-backlog               (Default: 250) Amount of messages kept in backlog per client before dropping

  --persistent                    (Default: false) Persistent Sessions

  --encrypted                     (Default: false) Encrypted connections SSL/TLS requires certificate setting

  --cert-location                 The location of the certificate

  --cert-password                 The password of the certificate

  --message-retention-location    If this is defined the server will retian messages in json format at the defined
                                  location must be a file location

  --show-subscriptions            (Default: false) If defined it will show the subscriptions made by clients

  --show-messages                 (Default: false) If defined it will show a message is received, who it was from and
                                  on what topic

  --show-connections              (Default: false) If defined it will show when someone connects to server and
                                  disconnects

  -c, --config                    (Default: ./mqttserver.json) Defines the location of the config file. By default it
                                  will search for configs in the current directory with mqttserver.json file name.
                                  Config settings override command line settings

  --log-level                     (Default: Information) Log Levels are Trace, Debug, Information, Warning, Error,
                                  Critical

  -m, --make-config               (Default: false) When this options is defined the server will create a config in
                                  current working directory and immediately close

  --help                          Display this help screen.

  --version                       Display version information.
```

### Examples

```
dotnet mqttserver -a 127.0.0.1 -p 5000
```

This simple example will start a mqttserver on 127.0.0.1(Loopback address) with port 5000 good for local development

---
```
dotnet mqttserver -c .\configs\myconfig.json
```

This simple example will use the config specified in a specific place

---

```
dotnet mqttserver -m
```
This command will copy the default config to your working directory

