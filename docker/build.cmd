@echo off

rem Build new image
docker build -t sshnet -f DockerFile .

rem Remove danging images
docker image prune -f