const _uniqueDomIdCounter = {};

export default function nextUniqueId(prefix)
{
    prefix = (prefix ? prefix : "dom");
    let prev = _uniqueDomIdCounter[prefix];
    prev = prev ? (prev+1) : 1;
    _uniqueDomIdCounter[prefix] = prev;
    return `${prefix}_${prev}`;
};
