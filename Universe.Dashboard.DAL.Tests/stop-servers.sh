#!/usr/bin/env bash
sudo docker ps | grep -E "postgres-|mysql-" | awk '{print $1}' | sudo xargs docker stop
