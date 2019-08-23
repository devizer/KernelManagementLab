#!/usr/bin/env node
const TestContext = require("./TestContext");
const delay = ms => new Promise(res => setTimeout(res, ms));

const chromeLauncher = require('chrome-launcher');
const CDP = require('chrome-remote-interface');

const Utils = require("./Utils");

const {performance, PerformanceObserver } = require('perf_hooks');

let testIndex = 0;
// return array of errors
async function runTest (testCase, pageSpec, url) {

    testIndex++;
    const errors = [];

    const showResult = () => {
        if(errors.length === 0
        )
            console.log(`[${url}]`.yellow.bold + ` tests ` + `completed successfully`.green.bold + `. No errors or fails found.`);
        else
        {
            console.error(`[${url}] tests failed with ${errors.length} errors`.red);
            for (const i of errors) console.error(`${i}`.red);
        }
    };



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
        let ver = protocol.protocol.version;
        console.log(`CDP Protocol version: ` + Utils.asJSON(ver).green);
        const {DOM, Page, Emulation, Runtime, Browser, Network} = protocol;
        // console.log(protocol);

        console.log(`BROWSER VER:`); Utils.printProperties(await Browser.getVersion()); 


        const startAtByRequestId = {};
        let firstAt = undefined;
        Network.requestWillBeSent((params) => {
            const now = performance.now();
            startAtByRequestId[params.requestId] = now;
            if (firstAt === undefined) firstAt = now; 
            // console.log(`○► #${params.requestId} ${params.request.method} ${params.request.url}`);
        });

        let maxLength = 5, maxHeaderLength = 1;
        const devToolsResponses = new Map();
        const responseReceivedInfo = {};
        Network.responseReceived( params => {
            const startAt = startAtByRequestId[params.requestId];
            const now = performance.now();
            const duration = startAt !== undefined ? Math.round((now - startAt)*10)/10 : "";
            const sinceStart = firstAt !== undefined ? Math.round((now - firstAt)*10)/10 : "";
            let timings = "";
            const timing = params.response.timing;
            if (firstAt !== undefined && startAt !== undefined)
            {
                const v1 = startAt - firstAt; // sent
                const v2 = now - startAt; // spent for entire request
                const v3 = now - firstAt; // finished since start
                const vServer = Number(timing ? timing.receiveHeadersEnd : 0);
                const v1s = Utils.myFormat(v1), v2s = Utils.myFormat(v2), v3s = Utils.myFormat(v3), fServer = Utils.myFormat(vServer);
                const len = Math.max(v1s.length, v2s.length, v2s.length, maxLength);
                maxHeaderLength = Math.max(fServer.length, maxHeaderLength);
                timings = `${v1s.padStart(len)} + ${v2s.padStart(len)} = ${v3s.padStart(len)} (${fServer.padStart(maxHeaderLength)})`;
                maxLength = Math.max(maxLength, len);
            }
            const requestId = params.requestId;
            const deprecated = `${timing ? timing.receiveHeadersEnd : ""} ${duration} ${sinceStart}`;
            responseReceivedInfo[params.requestId] = `◄ ${timings} ${params.response.status} ${params.response.url}`;
            devToolsResponses.set(params.requestId, params.response);

            // console.log(params);
        });

        Network.loadingFinished( params => {
            const response = devToolsResponses.get(params.requestId);
            const responseInfo = responseReceivedInfo[params.requestId];
            if (response && responseInfo) {
                const encodedBodyLength = params.encodedDataLength - /*response.headersText.length*/ 0;
                console.log(`${responseInfo} (${encodedBodyLength} bytes)`);
            }
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
        showResult();
        resolveCopy(errors);
        return ret;
    }
    
    context = new TestContext(protocol, pageSpec, errors);

    protocol.Page.loadEventFired(async() => {
        
        let isError = false;
        try {
            const href = String(await context.getExpression("location.href"));
            console.log(`PAGE #${testIndex} `+`${url}`.yellow.bold + ` LOADED to ${href}`);
            if (href.startsWith("chrome-error://"))
            {
                isError = true;
                throw new Error(`Unable to load ${url}. It was redirected to chrome-error://`);
            }
            console.log(`• TITLE: '${await context.getExpression("document.title")}'`);
            console.log(`• Visibility State: '${await context.getExpression("document.visibilityState")}'`);
            console.log(`• User Agent: '${await context.getExpression("navigator.userAgent")}'`);
            console.log(`• LoadingStartedAt: '${Math.round(await context.getExpression("window.LoadingStartedAt"))} milliseconds'`);
        } catch (e) {
            errors.push(e);
            // return;
        }
        
        if (isError) {
            protocol.close();
            chrome.kill();
            showResult();
            resolveCopy(errors);
            return;
        }

        try {
            await testCase(context);
        } catch (e) {
            // console.error(`Fail tests for ${url}\n${e}`);
            errors.push(e);
        }

        
        showResult();
        protocol.close();
        chrome.kill();

        resolveCopy(errors);
    });
    
    new Promise(async () => {
        await delay(1);
        if (false && !resolved)
        {
            errors.push("Timeout expired. Page cannot be not loaded");
            resolveCopy(errors);
        }
    });
    
    return ret;
}

module.exports = runTest;