#!/usr/bin/env bash
yarn build
cd build
node App.test.js

