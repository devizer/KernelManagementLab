let benchmark = (title,func) => {
  const durationLimit = 2000;
  func();
  let startAt = (window.performance) ? window.performance.now() : +new Date();
  let duration = 0, count = 0;
  while(duration < durationLimit) {
    const batch = 1234;
    for(let i=0; i<batch; i++) { func(); }
    count += batch;
    const stopwatch = (window.performance) ? window.performance.now() : +new Date();
    duration = stopwatch - startAt;
  }
  let format = num => (Math.round(num * 100.0) / 100.0).toLocaleString(undefined, {useGrouping: true});
  console.log(`%c%s PERFORMANCE: %s op/s`, 'color: #146D2B', title, format(count * 1000.0 / duration));
}
benchmark("[Error.stack] ", () => { let stack = new Error().stack });
benchmark("[No Operation]", () => { let ok = "sure" });
