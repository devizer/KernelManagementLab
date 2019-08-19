#!/usr/bin/env node
const TestContext = require("./TestContext");

const chromeLauncher = require('chrome-launcher');
const CDP = require('chrome-remote-interface');

// return array of errors
async function runTest (testCase, pageSpec, url) {

    const errors = [];
    let resolveCopy = null, rejectCopy = null;
    const ret = new Promise( (resolve, reject) => {
        resolveCopy = resolve; rejectCopy = reject;
    });

    async function launchChrome() {
        return await chromeLauncher.launch({
            // chromePath: 'google-chrome',
            startingUrl: 'about:blank',
            chromeFlags: ['--headless', '--disable-gpu', "--no-sandbox", "--enable-logging"],
        });
    }

    let chrome = null, protocol = null;
    try {
        chrome = await launchChrome();
        console.log(`Chrome port: ${chrome.port}`);

        protocol = await CDP({port: chrome.port});
        let ver = protocol.Version;
        console.log(`VER: ${ver}`);

        const {DOM, Page, Emulation, Runtime, Browser} = protocol;
        // console.log(protocol);

        console.log(`BROWSER VER: 
%O`, await Browser.getVersion());

        await Promise.all([Page.enable(), Runtime.enable(), DOM.enable()]);

        Page.navigate({url: url});
    }
    catch(e)
    {
        errors.push(`Unable to create chrome with development API for [${url}]\n${e}`);
        rejectCopy(errors);
        if (protocol !== null) protocol.close();
        chrome.kill();
    }
    
    const context = new TestContext(protocol, pageSpec, errors);

    protocol.Page.loadEventFired(async() => {
        try {
            console.log(`PAGE ${url} LOADED`);
            console.log(` - TITLE: '${await context.getExpression("document.title")}'`);
            console.log(` - Visibility State: '${await context.getExpression("document.visibilityState")}'`);
            console.log(` - User Agent: '${await context.getExpression("navigator.userAgent")}'`);
            console.log(` - LoadingStartedAt: '${await context.getExpression("window.LoadingStartedAt")}'`);
        } catch (e) {
            errors.push(e);
        }

        try {
            await testCase(context);
        } catch (e) {
            console.error(`Fail tests for ${url}\n${e}`);
            errors.push(e);
        }
        
        if (errors.length === 0)
            console.log(`[${url}] tests completed successfully`);
        else {
            console.error(`[${url}] tests failed with ${errors.length} errors`);
            for(const i in errors) console.error(` error: ${i}`);
        }
        
        protocol.close();
        chrome.kill();

        resolveCopy(errors);
    });
    
    return ret;
}

module.exports = runTest;