#!/usr/bin/env node
const TestContext = require("./TestContext");

const chromeLauncher = require('chrome-launcher');
const CDP = require('chrome-remote-interface');

let testIndex = 0;
// return array of errors
async function runTest (testCase, pageSpec, url) {

    testIndex++;
    const errors = [];

    let resolveCopy = null, rejectCopy = null;
    const ret = new Promise( (resolve, reject) => {
        resolveCopy = resolve; rejectCopy = reject;
    });

    async function launchChrome() {
        let chromeFlags = ['--disable-gpu', "--no-sandbox", "--enable-logging"];
        if (process.env.TRAVIS !== undefined || true) chromeFlags.push('--headless');
        return await chromeLauncher.launch({
            // chromePath: 'google-chrome',
            startingUrl: 'about:blank',
            chromeFlags: chromeFlags,
        });
    }

    let chrome = null, protocol = null, context = null;
    try {
        chrome = await launchChrome();
        console.log(`Chrome port: ${chrome.port}`);
        protocol = await CDP({port: chrome.port});
        let ver = protocol.get;
        console.log(`CDP Protocol version: ${ver}`);
        const {DOM, Page, Emulation, Runtime, Browser} = protocol;
        // console.log(protocol);

        console.log(`BROWSER VER: 
%O`, await Browser.getVersion());

        await Promise.all([Page.enable(), Runtime.enable(), DOM.enable()]);
        context = new TestContext(protocol, pageSpec, errors);
        if (pageSpec.width && pageSpec.height)
            await context.setWindowSize(pageSpec.width, pageSpec.height);
        
        Page.navigate({url: url});
    }
    catch(e)
    {
        errors.push(`Unable to create chrome with development API for [${url}]\n${e}`);
        if (protocol !== null) protocol.close();
        chrome.kill();
        // rejectCopy(e);
        resolveCopy(errors);
        return ret;
    }
    
    context = new TestContext(protocol, pageSpec, errors);

    protocol.Page.loadEventFired(async() => {
        
        try {
            console.log(`PAGE #${testIndex} ${url} LOADED`);
            console.log(` - TITLE: '${await context.getExpression("document.title")}'`);
            console.log(` - Visibility State: '${await context.getExpression("document.visibilityState")}'`);
            console.log(` - User Agent: '${await context.getExpression("navigator.userAgent")}'`);
            console.log(` - LoadingStartedAt: '${await context.getExpression("window.LoadingStartedAt")}'`);
        } catch (e) {
            errors.push(e);
            // return;
        }

        try {
            await testCase(context);
        } catch (e) {
            // console.error(`Fail tests for ${url}\n${e}`);
            errors.push(e);
        }

        if (errors.length === 0)
            console.log(`[${url}] tests completed successfully`);
        else {
            console.error(`[${url}] tests failed with ${errors.length} errors`);
            for(const i of errors) console.error(` error: ${i}`);
        }
        
        protocol.close();
        chrome.kill();

        resolveCopy(errors);
    });
    
    return ret;
}

module.exports = runTest;