#!/usr/bin/env node 
const runTest = require("./TestLauncher");

const w3topUrl = process.env.W3TOP_APP_URL || "http://localhost:5050";

let pages = [
    {url:'/not-found-404', width: 560, height: 440, fileName:"[404]" },
    {url:`/mounts`, width: 1024, height: 600, fileName:"mounts" },
    {url:'/disk-benchmark',         width: 1180, height: 620, fileName:"disk-benchmark-start" },
    {url:'/disk-benchmark?history', width: 1180, height: 620, fileName:"disk-benchmark-history" },
    {url:'/net-v2', width: 570, height: 800, fileName:"net-live-chart" },
    {url:'/disks', width: 570, height: 800, fileName:"disk-live-chart" },
    {url:`/`,       width: 570, height: 800, fileName:"[home]", tests: [commonTest] },
];

// pages = [pages[0]]; 

const commonTest = async (context) => {

    let isBriefInfoArrived = await context.waitForTrigger(8000,"BriefInfoArrived");
    if (isBriefInfoArrived === false)
        console.error("Warning! BriefInfo was not bound in 8 seconds");
    
    // next loop will fail if BriefInfoArrived is lost 
    for(let footerHeaderIndex=1; footerHeaderIndex<=4; footerHeaderIndex++)
    {
        let id=`COMMON_INFO_HEADER_${footerHeaderIndex}`;
        const headerValue = await context.getElementById(id);
        if (headerValue === undefined)
            context.addError(`Missed Footer Info Header ${id}`);
        else
            console.log(`Header [${id}]: '${headerValue ? headerValue : "MISSED"}'`);
    }

    let areMetricsArriving = await context.waitForTrigger(15000,"MetricsArriving");
    if (areMetricsArriving === false)
        context.addError("Web Socket broadcast for metrics was not obtained in 15 seconds");

    let areMetricsArrived = await context.waitForTrigger(5000,"MetricsArrived");
    if (areMetricsArrived === false)
        context.addError("Metrics were not bound in 5 seconds");

    await context.delay(1500);
    const scrollHeight = await context.getExpression('document.documentElement.scrollHeight');
    const boundHeight = await context.getExpression('document.documentElement.getBoundingClientRect().height');
    console.log(`document.scrollElement.clientHeight: ${scrollHeight}, boundRect: ${boundHeight}`);
    await context.setWindowSize(undefined, boundHeight + 8);
    await context.delay(444);
    await context.saveScreenshot(`bin/${context.PageSpec.fileName}.png`);
};


const totalErrors = [];
(async function() {
    for(let page of pages)
    {
        let errors = await runTest(commonTest, page, `${w3topUrl}${page.url}`);
        totalErrors.push(...errors);
        console.log('');
    }
})().then( ok => {
    console.log(`The End. Total fails: ${totalErrors.length}`);
    if (totalErrors.length > 0) 
        throw new Error(`Total errors: ${totalErrors.length}\n${totalErrors.join('\n')}`);
});

