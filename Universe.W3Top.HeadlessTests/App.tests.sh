#!/usr/bin/env bash
yarn build
cd lib
node App.test.js

