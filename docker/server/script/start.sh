#!/bin/ash
/usr/sbin/syslog-ng
# start PAM-enabled ssh daemon as we also want keyboard-interactive authentication to work
/usr/sbin/sshd.pam
tail -f < /var/log/auth.log