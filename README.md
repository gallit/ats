# ATS
Fan controller<br/>
This dotnet application is a linux service used to manage a fan controller.<br/>
The machine is an Armv8 RockPro64 development board.

## Requires
Linux system with 
* **pwm_fan** kernel module
* System D

### Install service
As root
```
cd /opt/ats
cp ats.service /etc/systemd/system/ats.service
systemctl daemon-reload
systemctl enable ats
```

#### Update binaries
As root
```
systemctl stop ats
rm -fr /opt/ats/*
cp -r /home/gallit/ats/net6.0/* /opt/ats/
systemctl start ats
systemctl status ats
tail -n50 /var/log/ats/ats_20230103.log
```

#### Testing
```
systemctl start ats
systemctl status ats
journalctl -u ats -n50

systemctl stop ats
systemctl status ats
journalctl -u ats -n50

tail -n50 /var/log/ats/ats_20230103.log
tail -f /var/log/ats/ats_20230103.log
stress --cpu 540 --io 8 --vm 4 --vm-bytes 128M --timeout 6000s
```
