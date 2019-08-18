#!/usr/bin/env node

const chromeLauncher = require('chrome-launcher');
const CDP = require('chrome-remote-interface');
const file = require('fs');

const delay = ms => new Promise(res => setTimeout(res, ms));

const w3topUrl = process.env.W3TOP_APP_URL || "http://localhost:5050";

const pages = {
    home: {url:`/`, width: 1024, height: 600 },
    mounts: {url:`/mounts`, width: 1024, height: 600 },
    diskBenchmark: {url:'/disk-benchmark', width: 1180, height: 600 },
};

const pageSpec = pages.home;
const pageUrl = `${w3topUrl}${pageSpec.url}`;

(async function() {

    async function launchChrome() {
        return await chromeLauncher.launch({
            // chromePath: 'google-chrome',
            startingUrl: 'about:blank',
            chromeFlags: ['--headless', '--disable-gpu', "--no-sandbox", "--enable-logging"],
        });
    }

    const chrome = await launchChrome();
    console.log(`Chrome port: ${chrome.port}`);

    const protocol = await CDP({port: chrome.port});
    let ver = protocol.Version;
    console.log(`VER: ${ver}`);

    const { DOM, Page, Emulation, Runtime, Browser} = protocol;
    // console.log(protocol);
    
    console.log(`BROWSER VER: 
%O`, await Browser.getVersion());

    const setWindowSize = async (width, height) => {
        var window = await Browser.getWindowForTarget();
        console.log("New Size: [%d * %d], Prev Window: %o", width, height, window);
        window.bounds.width=width;
        window.bounds.height=height;
        await Browser.setWindowBounds(window);
    };
    
    await setWindowSize(pageSpec.width,pageSpec.height);

    const getExpression = async (expression) => {
        const expressionValue = await Runtime.evaluate({expression: expression});
        // console.log(`Expression [${expression}] value is [%o]`, expressionValue);
        
        if (expressionValue.subtype === "error")
            return undefined;
        
        if (expressionValue.result.className === "Date")
            return Date.parse(expressionValue.result.description);

        if (expressionValue.result.type === "number")
            return expressionValue.result.value;
        
        return expressionValue.result.value;
    };

    const getElementById = async (idName) => {
        return await getExpression(`document.getElementById('${idName}').innerText`);
    };
    
    const getVersion = async () => {
        const userAgent = await getExpression("navigator.userAgent");
        const raw = userAgent.match(/Chrom(e|ium)\/([0-9]+)\./);
        return raw ? parseInt(raw[2], 10) : false;
    };

    const waitForTrigger = async (timeout, triggerKey) => {
        const start = new Date();
        let ms = 1;
        while(true)
        {
            let value = await getExpression(`window.ApplicationLevelTriggers.${triggerKey}.first`);
            // console.log(`Wait for ${triggerKey}: ${value} (${new Date() - start})`);
            if (value > 0) {
                value = Math.round(value*10)/10;
                if (value < 0.00999) value = 0.01;
                console.debug(`Trigger [${triggerKey}] successfully confirmed in ${new Date() - start} milliseconds. Raised at ${value.toFixed(1)} milliseconds`);
                return ;
            }

            await delay(ms);
            ms = ms >= 128 ? ms : ms * 2;
            let elapsed = new Date() - start;
            if (elapsed > timeout) break;
        }

        console.warn(`Warning! trigger [${triggerKey}] was not raised in ${new Date() - start} milliseconds`);
        return false;
    };

    await Promise.all([Page.enable(), Runtime.enable(), DOM.enable()]);

    Page.navigate({ url: pageUrl });

    Page.loadEventFired(async() => {
        console.log("PAGE LOADED");
        console.log(`TITLE: '${await getExpression("document.title")}'`);
        console.log(`Visibility State: '${await getExpression("document.visibilityState")}'`);
        console.log(`User Agent: '${await getExpression("navigator.userAgent")}'`);
        console.log(`Version: '${await getVersion()}'`);
        console.log(`LoadingStartedAt: '${await getExpression("window.LoadingStartedAt")}'`);
        console.log(`LoadingStartedAt: '${await getExpression("document.LoadingStartedAt")}'`);
        


        /*
              console.log(`WHOLE DOCUMENT (incomplete): 
        ${await getExpression("document.body.innerText")}
        `);
        */

        let isBriefInfoArrived = await waitForTrigger(5000,"BriefInfoArrived", status => true);
        // next loop will fail if BriefInfoArrived is lost 
        for(let footerHeaderIndex=1; footerHeaderIndex<=4; footerHeaderIndex++)
        {
            let id=`FOOTER_INFO_HEADER_${footerHeaderIndex}`;
            const headerValue = await getElementById(id);
            if (headerValue === undefined)
                console.error(`ERROR: Missed Footer Info Header ${id}`);
            else
                console.log(`Header [${id}]: '${headerValue ? headerValue : "MISSED"}'`);
        }

        let areMetricsArriving = await waitForTrigger(15000,"MetricsArriving");
        if (!areMetricsArriving)
            console.error("ERROR: Web Socket broadcast for metrics is not obtained in 15 seconds");

        let areMetricsArrived = await waitForTrigger(5000,"MetricsArrived");
        if (!areMetricsArrived)
            console.error("ERROR: Metrics are not bound in 5 seconds");

        /*
              console.log(`WHOLE COMPLETED DOCUMENT: 
        ${await getExpression("document.body.innerText")}
        `);
        */

        console.log(`LoadingStartedAt: '${await getExpression("window.LoadingStartedAt")}'`);


        const ss = await Page.captureScreenshot({format: 'png', fromSurface: true});
        file.writeFile('bin/screenshot [home].png', ss.data, 'base64', function(err) {
            if (err) console.log(`Screenshot error: ${err}`);
        });

        protocol.close();
        chrome.kill();
        console.log("The End");
    });

})();

