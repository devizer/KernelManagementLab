#!/usr/bin/env node 
const runTest = require("./TestLauncher");
var colors = require('colors');
var devicesDictionary = require('./DevicesDictionary');

console.log(devicesDictionary.asString(true));

let w3topUrl = process.env.W3TOP_APP_URL || "http://localhost:5010/";
w3topUrl = w3topUrl.replace(new RegExp("[/]+$"), "");

const Utils = require("./Utils");

const diskBenchmarkFullTest = async (context) => {

    const click = async (selector) => await context.getExpression(`document.getElementById('${selector}').click()`);
    
    const setValue = async (elementId, value) => {
        await context.getExpression(`document.getElementById('${elementId}').focus()`);
        await context.getExpression(`document.getElementById('${elementId}').select()`);
        // https://github.com/GoogleChrome/puppeteer/blob/master/lib/Input.js
        // https://javascript.info/keyboard-events
        const valueAsString = String(value);
        if (value === null || value === undefined || valueAsString.length === 0)
            // 8: Backspace, 46: Delete
            await context.Protocol.Input.dispatchKeyEvent({type:"rawKeyDown", windowsVirtualKeyCode:8});
        else
            await context.Protocol.Input.dispatchKeyEvent({type:"keyDown", text:valueAsString});
    };

    const steps = [
        async () => { 
            await testSharedHeader(context); 
            await waitForMetrics(context);
        },
        // If any element on speps below are missed then the last step with the BTN_DISK_BENCHMARK_ANOTHER button is never reached
        async () => await click('DISK_1'),
        async () => await click('BTN_DISK_BENCHMARK_NEXT'),
        async () => {
            await setValue('benchmark-options-working-set', Utils.isTravis ? 256 : 1);
            await setValue('benchmark-options-duration', Utils.isTravis ? 4 : 1);
        },
        async () => {
            await click('BTN_DISK_BENCHMARK_NEXT');
            await context.delay(888);
        },
        async () => {
            const isFound = await context.waitForElement(50000, 'BTN_DISK_BENCHMARK_ANOTHER');
            if (!isFound) context.addError(`BTN_DISK_BENCHMARK_ANOTHER button expected at the finish of benchmark`);
            // NOWAY: await click('BTN_DISK_BENCHMARK_ANOTHER'); 
        },
    ];
    
    let stepIndex = 0;
    for(let step of steps) {
        await step();

        stepIndex++;
        await context.delay(77);
        await context.saveScreenshot(`bin/${context.PageSpec.fileName}-${stepIndex}.png`);
    }
    
}

const testSharedHeader = async (context) => {
    let isBriefInfoArrived = await context.waitForTrigger(8000,"BriefInfoArrived");
    if (isBriefInfoArrived === false)
        console.error("Warning! BriefInfo was not bound in 8 seconds");
    
    await context.delay(333);

    // next loop will fail if BriefInfoArrived is lost 
    for(let footerHeaderIndex=1; footerHeaderIndex<=4; footerHeaderIndex++)
    {
        let id=`COMMON_INFO_HEADER_${footerHeaderIndex}`;
        const headerValue = await context.getElementById(id);
        if (headerValue === undefined)
            context.addError(`Missed Footer Info Header ${id}`);
        else
            console.log(`Header [${id}]: ` + `'${headerValue ? headerValue : "MISSED"}'`.yellow.bold);
    }
};

const waitForMetrics = async (context) => {

    let areMetricsArriving = await context.waitForTrigger(15000,"MetricsArriving");
    if (areMetricsArriving === false)
        context.addError("Web Socket broadcast for metrics was not obtained in 15 seconds");

    let areMetricsArrived = await context.waitForTrigger(5000,"MetricsArrived");
    if (areMetricsArrived === false)
        context.addError("Metrics were not bound in 5 seconds");

};

const showDrawerTest = async(context) => {
    await testSharedHeader(context);
    await waitForMetrics(context);
    const idSystemIcon = "APP_SYSTEM_ICON";
    const buttonHtml = await context.getExpression(`document.getElementById('${idSystemIcon}').outerHTML`);
    if (!buttonHtml)
        context.addError(`Unable to find ${idSystemIcon} button`);
    
    console.log(`SYSTEM BUTTON: ${Utils.trimHtml(buttonHtml)}`);
    await context.getExpression(`document.getElementById('${idSystemIcon}').click()`);
    await context.delay(444);
    await context.saveScreenshot(`bin/${context.PageSpec.fileName}.png`);
};

showDrawerTest.description = "Drawer screenshot";



const commonTest = async (context) => {


    await testSharedHeader(context);
    await waitForMetrics(context);
    
    await context.delay(333);
    const scrollHeight = await context.getExpression('document.documentElement.scrollHeight');
    const boundHeight = await context.getExpression('document.documentElement.getBoundingClientRect().height');
    console.log(`document.scrollElement.clientHeight: ${scrollHeight}, boundRect: ${boundHeight}`);
    await context.setWindowSize(undefined, boundHeight + 8);
    await context.delay(444);
    await context.saveScreenshot(`bin/${context.PageSpec.fileName}.png`);
};

commonTest.description = "Common test for header and metrics binding";

const netStatTest = async(context) => {

    await testSharedHeader(context);
    await waitForMetrics(context);
    await context.delay(666);

    for(let netStatIndex=1; netStatIndex<=6; netStatIndex++)
    {
        const isOptional = Boolean(netStatIndex > 2); 
        let idName=`NET_NAME_${netStatIndex}`;
        const nameValue = await context.getElementById(idName);
        console.log(`${isOptional ? "Optional" : "MANDATORY"} Net Name [${idName}]: ` + `'${nameValue ? nameValue : ">MISSED<"}'`.yellow.bold);
        if (netStatIndex <= 2 && nameValue === undefined)
            context.addError(`Missed Net Live Chart ${netStatIndex}`);
    }

    await context.delay(1000);
    const boundHeight = await context.getExpression('document.documentElement.getBoundingClientRect().height');
    await context.setWindowSize(undefined, boundHeight + 8);
    await context.delay(444);
    await context.saveScreenshot(`bin/${context.PageSpec.fileName}.png`);
};


let pages = [
    { url:'/net-v2', width: 570, height: 800, fileName:"net-live-chart", tests: [commonTest, netStatTest] },
    { url:`/`,      width: 570, height: 800, fileName:"[home]", tests: [commonTest] },
    { url:`/`,      width: 700, height: 730, fileName:"Menu [home]", tests: [commonTest, showDrawerTest] },
    { url:'/disk-benchmark', width: 1180, height: 768, fileName:"disk-benchmark-progress", tests: [diskBenchmarkFullTest] },
    { url:`/mounts`, width: 1024, height: 600, fileName:"mounts", tests: [commonTest] },
    { url:'/disk-benchmark',         width: 1180, height: 620, fileName:"disk-benchmark-start", tests: [commonTest] },
    { url:'/disk-benchmark?history', width: 1180, height: 620, fileName:"disk-benchmark-history", tests: [commonTest] },
    { url:'/disks', width: 570, height: 800, fileName:"disk-live-chart", tests: [commonTest] },
    { url:'/not-found-404', width: 560, height: 440, fileName:"[404]", tests: [commonTest] },
];

// pages = [pages[0]]; 

const totalErrors = [];
(async function() {
    for(let page of pages)
    for(let testCase of page.tests)
    {
        // TODO:
        // for desktop we need 100%,125%, 150%, 200%, 300% pixel scale ratio
        // for mobile we should change browser size 
        let errors = await runTest(testCase, page, `${w3topUrl}${page.url}`);
        totalErrors.push(...errors);
        console.log('');
    }
})().then( ok => {
    console.log(`The End. Total fails: ${totalErrors.length}`);
    if (totalErrors.length > 0) {
        const errorMessage = `Total errors: ${totalErrors.length}\n${totalErrors.join('\n')}`;
        console.log(errorMessage.red.bold);
        process.exit(1);
    }
}).catch(e => {
    // already handled
    process.exit(1);
});

