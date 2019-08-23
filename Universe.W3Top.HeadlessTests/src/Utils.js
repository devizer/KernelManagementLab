var colors = require('colors');

const asJSON = arg => {
    const s1 = JSON.stringify(arg,null, ' ');
    let s2 = s1.replace(/\n/g, " ");
    // .replace(/  /g, " ")
    while(s2.indexOf("  ") >= 0) {s2 = s2.replace("  ", " "); }
    return s2;
};

const myFormat = x => {
    return Number(x).toLocaleString(undefined, {useGrouping:true, minimumFractionDigits:1, maximumFractionDigits:1});
};

const printProperties = arg => {
    for(let pro of Object.keys(arg))
    {
        console.log(`  ${pro}: ` + arg[pro].green);
    }
}


module.exports = {asJSON, myFormat, printProperties};