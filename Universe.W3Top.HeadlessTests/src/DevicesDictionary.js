// https://mediag.com/blog/popular-screen-resolutions-designing-for-all/
// http://javascriptkit.com/dhtmltutors/cssmediaqueries2.shtml
// http://screensiz.es/iphone-5-5c-5s
var colors = require('colors');

const devicesDictionary = [
    {name: 'iPhone 4', width: 320, height: 480, scale: 2},
    {name: 'iPhone 5', width: 320, height: 568, scale: 2},
    {name: 'iPhone 6', width: 375, height: 667, scale: 2},
    {name: 'iPhone 6 Plus', width: 414, height: 736, scale: 3},
    {name: 'iPad 1st and 2nd', width: 768, height: 1024, scale: 1},
    {name: 'iPad 3', width: 768, height: 1024, scale: 2},
    {name: 'Samsung Galaxy S I,II', width: 320, height: 533, scale: 1.5},
    {name: 'Samsung Galaxy S III', width: 360, height: 640, scale: 2},
    {name: 'HTX Evo 3D', width: 360, height: 640, scale: 1.5},

    {
        name: "iPhone 6(s) Plus",
        width: "414",
        height: "736",
        scale: "2.608"
    },
    {
        name: "iPhone 6(s)",
        width: "375",
        height: "667",
        scale: "2"
    },
    {
        name: "iPhone 5",
        width: "320",
        height: "568",
        scale: "2"
    },
    {
        name: "iPhone XR",
        width: 414,
        height: 896,
        scale: 2
    },
    {
        name: "iPhone XS",
        width: 375,
        height: 812,
        scale: 3
    },
    {
        name: "iPhone XS Max",
        width: 414,
        height: 896,
        scale: 3
    },
    {
        name: "iPhone X",
        width: 375,
        height: 812,
        scale: 3
    },
    {
        name: "iPhone 8 Plus",
        width: "414",
        height: "736",
        scale: "2.608"
    },
    {
        name: "iPhone 8",
        width: "375",
        height: "667",
        scale: "2"
    },
    {
        name: "iPhone 7 Plus",
        width: "414",
        height: "736",
        scale: "2.608"
    },
    {
        name: "iPhone 7",
        width: "375",
        height: "667",
        scale: "2"
    },

    {
        name: "iPod Touch",
        width: "320",
        height: "568",
        scale: "2"
    },
    {
        name: "iPad Pro",
        width: "1024",
        height: "1366",
        scale: "2"
    },
    {
        name: "iPad 3rd & 4th Gen",
        width: "768",
        height: "1024",
        scale: "2"
    },
    {
        name: "iPad Air 1 & 2",
        width: "768",
        height: "1024",
        scale: "2"
    },
    {
        name: "iPad Mini 2 & 3",
        width: "768",
        height: "1024",
        scale: "2"
    },
    {
        name: "iPad Mini",
        width: "768",
        height: "1024",
        scale: "1"
    },
    {
        name: "Nexus 6P",
        width: "412",
        height: "732",
        scale: "3.495"
    },
    {
        name: "Nexus 5X",
        width: "412",
        height: "732",
        scale: "2.621"
    },
    {
        name: "Google Pixel 3 XL",
        width: "412",
        height: "847",
        scale: "3.495"
    },
    {
        name: "Google Pixel 3",
        width: "412",
        height: "824",
        scale: "2.621"
    },
    {
        name: "Google Pixel 2 XL",
        width: "412",
        height: "732",
        scale: "3.495"
    },
    {
        name: "Google Pixel XL",
        width: "412",
        height: "732",
        scale: "3.495"
    },
    {
        name: "Google Pixel",
        width: "412",
        height: "732",
        scale: "2.621"
    },
    {
        name: "Samsung Galaxy Note 9",
        width: "360",
        height: "740",
        scale: "4"
    },
    {
        name: "Samsung Galaxy Note 5",
        width: "480",
        height: "853",
        scale: "3"
    },
    {
        name: "LG G5",
        width: "480",
        height: "853",
        scale: "3"
    },
    {
        name: "One Plus 3",
        width: "480",
        height: "853",
        scale: "2.25"
    },
    {
        name: "Samsung Galaxy S8(+) & S9(+)",
        width: "360",
        height: "740",
        scale: "4"
    },
    {
        name: "Samsung Galaxy S7 and Edge",
        width: "360",
        height: "640",
        scale: "4"
    },
    {
        name: "Nexus 9",
        width: "768",
        height: "1024",
        scale: "2"
    },
    {
        name: "Nexus 7 (2013)",
        width: "600",
        height: "960",
        scale: "2"
    },
    {
        name: "Samsung Galaxy Tab 10",
        width: "1280",
        height: "800",
        scale: "1"
    },
    {
        name: "Chromebook Pixel",
        width: "1280",
        height: "850",
        scale: "2"
    }
];

devicesDictionary.forEach(device => {
    device.width = Number(device.width);
    device.height = Number(device.height);
    device.scale = Number(device.scale);
});

let maxLength = 0;
devicesDictionary.forEach(d => {
    maxLength = Math.max(maxLength, d.name.length);
});

devicesDictionary.asString = (colorful) => {
    const nop = x => x;
    let t1 = colorful ? colors.yellow : nop,
        t2 = colorful ? colors.green : nop,
        t3 = colorful ? colors.magenta : nop;

    const ret = [];
    for(const device of devicesDictionary) {

        let vals = [
            t1(device.name.padEnd(maxLength)),
            t2(`${device.width}*${device.height}`.padEnd(12)),
            t3(`${device.scale}`)
        ];

        ret.push(vals.join(' '));
    }

    return ret.join('\n');
};

module.exports = devicesDictionary;

