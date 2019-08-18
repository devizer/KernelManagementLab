#!/usr/bin/env bash
mkdir -p bin
rm -rf bin/*
node tests.js
file bin/*
ls -la bin
