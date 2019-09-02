var colors = require('colors');

const asJSON = arg => {
    const s1 = JSON.stringify(arg,null, ' ');
    let s2 = s1.replace(/\n/g, " ").replace(/\r/g, " ");
    while(s2.indexOf("  ") >= 0) {s2 = s2.replace("  ", " "); }
    return s2;
};

const myFormatOptions = {useGrouping:true, minimumFractionDigits:1, maximumFractionDigits:1};
const myFormat = x => {
    return Number(x).toLocaleString(undefined, myFormatOptions);
};

const printProperties = arg => {
    for(let pro of Object.keys(arg))
        console.log(`  ${pro}: ` + arg[pro].green);
};

const trimHtml = (html) => {
    if (html === undefined || html === null) return html;
    const p = html.indexOf('>');
    return p < 0 ? html : html.substr(0,p) + " ...";
};

const isTravis = Boolean(process.env.TRAVIS !== undefined);

module.exports = {asJSON, myFormat, printProperties, trimHtml, isTravis};