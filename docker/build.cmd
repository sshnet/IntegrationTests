@echo off

rem Build new image
docker build -t sshnet -f DockerFile .
