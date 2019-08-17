#!/usr/bin/env node

const chromeLauncher = require('chrome-launcher');
const CDP = require('chrome-remote-interface');
const file = require('fs');

const delay = ms => new Promise(res => setTimeout(res, ms));

const w3topUrl = process.env.W3TOP_URL || "http://localhost:5050/";

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
  await Promise.all([Page.enable(), Runtime.enable(), DOM.enable()]);

  Page.navigate({ url: w3topUrl });

  Page.loadEventFired(async() => {
    console.log("PAGE LOADED");
    
    const script_Title = "document.title";
    const result_Title = await Runtime.evaluate({expression: script_Title});
    const title = result_Title.result.value;
    console.log(`TITLE: '${title}'`);

    const script_Whole = "document.body.innerText";
    const result_Whole = await Runtime.evaluate({expression: script_Whole});
    const wholeText = result_Whole.result.value;
    console.log(`wholeText: '${wholeText}'`);

    for(let sysInfoIndex=1; sysInfoIndex<=4; sysInfoIndex++)
    {
      let id=`SYS_INFO_HEADER_${sysInfoIndex}`;
      const result_Header = await Runtime.evaluate({expression: `document.getElementById('${id}').innerText`});
      const headerValue = result_Header.result.value;
      if (headerValue === undefined)
        console.log(`Missed Sys Info Header ${id}`);
      else
        console.error(`ERROR: header [${id}]: '${headerValue ? headerValue : "MISSED"}'`);
    }

    // wait for websocket lazy message
    await delay(5000);
    console.log("Waited 5s");

    const ss = await Page.captureScreenshot({format: 'png', fromSurface: true});
    file.writeFile('bin/screenshot.png', ss.data, 'base64', function(err) {
      if (err) console.log(`Screenshot error: ${err}`);
    });

    protocol.close();
    chrome.kill();
  });

})();




