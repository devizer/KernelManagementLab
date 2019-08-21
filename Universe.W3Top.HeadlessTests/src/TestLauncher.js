#!/usr/bin/env node
const TestContext = require("./TestContext");

const chromeLauncher = require('chrome-launcher');
const CDP = require('chrome-remote-interface');

const {
    performance,
    PerformanceObserver
} = require('perf_hooks');

const myFormat = x => {
    return Number(x).toLocaleString(undefined, {useGrouping:true, minimumFractionDigits:1, maximumFractionDigits:1});
}


let testIndex = 0;
// return array of errors
async function runTest (testCase, pageSpec, url) {

    testIndex++;
    const errors = [];

    let resolveCopy = null, rejectCopy = null, resolved = false;
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
        const {DOM, Page, Emulation, Runtime, Browser, Network} = protocol;
        // console.log(protocol);

        console.log(`BROWSER VER: 
%O`, await Browser.getVersion());

        const startAtByRequestId = {};
        let firstAt = undefined;
        Network.requestWillBeSent((params) => {
            const now = performance.now();
            startAtByRequestId[params.requestId] = now;
            if (firstAt === undefined) firstAt = now; 
            // console.log(`○► #${params.requestId} ${params.request.method} ${params.request.url}`);
        });

        let maxLength = 1, maxHeaderLength = 1;
        Network.responseReceived( params => {
            const startAt = startAtByRequestId[params.requestId];
            const now = performance.now();
            const duration = startAt !== undefined ? Math.round((now - startAt)*10)/10 : "";
            const sinceStart = firstAt !== undefined ? Math.round((now - firstAt)*10)/10 : "";
            let timings = "";
            if (firstAt !== undefined && startAt !== undefined)
            {
                const v1 = startAt - firstAt; // sent
                const v2 = now - startAt; // spent for entire request
                const v3 = now - firstAt; // finished since start
                const vServer = Number(params.response.timing.receiveHeadersEnd);
                const v1s = myFormat(v1), v2s = myFormat(v2), v3s = myFormat(v3), fServer = myFormat(vServer);
                const len = Math.max(v1s.length, v2s.length, v2s.length, maxLength);
                maxHeaderLength = Math.max(fServer.length, maxHeaderLength);
                timings = `${v1s.padStart(len)} + ${v2s.padStart(len)} = ${v3s.padStart(len)} (${fServer.padStart(maxHeaderLength)})`;
                maxLength = Math.max(maxLength, len);
            }
            const requestId = params.requestId;
            const deprecated = `${params.response.timing.receiveHeadersEnd} ${duration} ${sinceStart}`;
            console.log(`◄ ${timings} ${params.response.status} ${params.response.url}`);
        });


        await Promise.all([Page.enable(), Runtime.enable(), DOM.enable(), Network.enable()]);
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
            console.log(`• TITLE: '${await context.getExpression("document.title")}'`);
            console.log(`• Visibility State: '${await context.getExpression("document.visibilityState")}'`);
            console.log(`• User Agent: '${await context.getExpression("navigator.userAgent")}'`);
            console.log(`• LoadingStartedAt: '${await context.getExpression("window.LoadingStartedAt")}'`);
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
    
    new Promise(async () => {
        await delay(1000);
        if (!resolved)
        {
            errors.push("Timeout expired. Page cannot be not loaded");
            resolveCopy(errors);
        }
    });
    
    return ret;
}

module.exports = runTest;