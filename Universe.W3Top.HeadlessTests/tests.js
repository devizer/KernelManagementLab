#!/usr/bin/env node

const chromeLauncher = require('chrome-launcher');
const CDP = require('chrome-remote-interface');
const file = require('fs');

const delay = ms => new Promise(res => setTimeout(res, ms));

const w3topUrl = process.env.W3TOP_URL || "http://localhost:5050/mounts";

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

  const { DOM, Page, Emulation, Runtime} = protocol;
  
  const getExpression = async (expression) => {
      const innerText = await Runtime.evaluate({expression: expression});
      return innerText.result.value;
  };
  
  const getElementById = async (idName) => {
      return await getExpression(`document.getElementById('${idName}').innerText`);
  };

    const waitForTrigger = async (timeout, triggerKey, isSuccess) => {
        const start = new Date();
        while(true)
        {
            const value = await getExpression(`document.${triggerKey}`);
            // console.log(`Wait for ${triggerKey}: ${value} (${new Date() - start})`);
            if (value !== undefined && (isSuccess === undefined || isSuccess === null || isSuccess(value))) {
                console.debug(`Trigger [${triggerKey}] successfully confirmed in ${new Date() - start} milliseconds`);
                return true;
            }

            await delay(1);
            let elapsed = new Date() - start;
            if (elapsed > timeout) break;
        }

        console.warn(`Warning! trigger [${triggerKey}] was not raised in ${new Date() - start} milliseconds`);
        return false;
  };
  
  await Promise.all([Page.enable(), Runtime.enable(), DOM.enable()]);

  Page.navigate({ url: w3topUrl });

  Page.loadEventFired(async() => {
    console.log("PAGE LOADED");
    console.log(`TITLE: '${await getExpression("document.title")}'`);

/*
      console.log(`WHOLE DOCUMENT (incomplete): 
${await getExpression("document.body.innerText")}
`);
*/
      
    for(let sysInfoIndex=1; sysInfoIndex<=4; sysInfoIndex++)
    {
      let id=`FOOTER_INFO_HEADER_${sysInfoIndex}`;
      const headerValue = await getElementById(id);
      if (headerValue === undefined)
        console.error(`ERROR: Missed Footer Info Header ${id}`);
      else
        console.log(`Header [${id}]: '${headerValue ? headerValue : "MISSED"}'`);
    }

      let areMetricsArriving = await waitForTrigger(15000,"MetricsArriving", status => status === "true");
      if (!areMetricsArriving)
          console.error("ERROR: Web Socket broadcast for metrics is not obtained in 15 seconds");

      let areMetricsArrived = await waitForTrigger(5000,"MetricsArrived", status => status === "true");
      if (!areMetricsArrived)
          console.error("ERROR: Metrics are not bound in 5 seconds");

/*
      console.log(`WHOLE COMPLETED DOCUMENT: 
${await getExpression("document.body.innerText")}
`);
*/


      const ss = await Page.captureScreenshot({format: 'png', fromSurface: true});
      file.writeFile('bin/screenshot [home].png', ss.data, 'base64', function(err) {
          if (err) console.log(`Screenshot error: ${err}`);
      });

    protocol.close();
    chrome.kill();
    console.log("The End");
  });

})();

