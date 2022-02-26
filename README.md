
# One-time setup

## Admin account

Create an account that will be used for admin tasks (eg. stopping sshd).
This account should be member of the following groups:
* adm
* sudo

This account be allowed to use to sudo to run any command without password prompt.
For this the */etc/sudoers* file should contain the following:

<account> ALL=(ALL) NOPASSWD: ALL

## Regular account

TODO: Automate creation and configuration of this account?