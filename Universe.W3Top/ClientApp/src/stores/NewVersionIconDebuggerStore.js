let initDone = false;
let forced = false;

if (!initDone) {
    initDone = true;
    if (document && document.addEventListener) {
        document.addEventListener('keyup', function (event) {
            if (event.code === 'KeyN' && event.altKey && event.ctrlKey && event.shiftKey) {
                if (!forced) {
                    forced = true;
                    console.log("Force New Version Icon");
                }
            }
        });
        console.log('Press Ctrl-Alt-Shift-N to test new version layout');
    }
}

export const getForced = () => forced;