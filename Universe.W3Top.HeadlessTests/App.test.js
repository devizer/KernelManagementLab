import TestContext from "./TestContext";

const w3topUrl = process.env.W3TOP_APP_URL || "http://localhost:5050";

const pages = {
    home:          {url:`/`,       width: 1024, height: 600, fileName:"[home]", tests: [commonTest] },
    mounts:        {url:`/mounts`, width: 1024, height: 600, fileName:"mounts" },
    diskBenchmark: {url:'/disk-benchmark', width: 1180, height: 600, fileName:"disk-benchmark" },
    netLiveChart:  {url:'/net-v2', width: 1024, height: 1024, fileName:"net-live-chart" },
    diskLiveChart: {url:'/disks', width: 1024, height: 1024, fileName:"disk-live-chart" },
    page404:       {url:'/not-found-404', width: 1024, height: 400, fileName:"[404]" },
};



// const homePageTests = async (context)
// const context = new TestContext(null, pages.home);



const commonTest = async (context) => {

    let isBriefInfoArrived = await context.waitForTrigger(8000,"BriefInfoArrived");
    if (isBriefInfoArrived === false)
        console.error("ERROR: BriefInfo was not bound in 8 seconds");
    
    // next loop will fail if BriefInfoArrived is lost 
    for(let footerHeaderIndex=1; footerHeaderIndex<=4; footerHeaderIndex++)
    {
        let id=`COMMON_INFO_HEADER_${footerHeaderIndex}`;
        const headerValue = await context.getElementById(id);
        if (headerValue === undefined)
            console.error(`ERROR: Missed Footer Info Header ${id}`);
        else
            console.log(`Header [${id}]: '${headerValue ? headerValue : "MISSED"}'`);
    }

    let areMetricsArriving = await context.waitForTrigger(15000,"MetricsArriving");
    if (areMetricsArriving === false)
        console.error("ERROR: Web Socket broadcast for metrics was not obtained in 15 seconds");

    let areMetricsArrived = await context.waitForTrigger(5000,"MetricsArrived");
    if (areMetricsArrived === false)
        console.error("ERROR: Metrics were not bound in 5 seconds");

    context.saveScreenshot(`bin/${context.PageSpec.fileName}.png`);
};
