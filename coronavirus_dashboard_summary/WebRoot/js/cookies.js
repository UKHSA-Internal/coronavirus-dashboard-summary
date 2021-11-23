var determineCookieState = function () {
    var cookies = document.cookie.split(';');
    var cookiePreferences = cookies.find(c => c.trim().startsWith('cookies_preferences_set_21_3'));

    if ( !cookiePreferences || cookiePreferences.split('=')[1] !== 'true' ) {
        var cookieBanner = document.querySelector("#cookie-banner");
        cookieBanner.style.display = 'block';
        cookieBanner.style.visibility = 'visible';
    }
};

function showElement (elm) {
    elm.style.display = 'block';
    elm.style.visibility = 'visible';
}

function hideElement (elm) {
    elm.remove()
}

function runCookieJobs() {
    var cookieDecisionBanner = document.querySelector('#global-cookie-message');

    document.querySelector("#accept-cookies").onclick = function () {
        var today = new Date();
        var year = today.getFullYear();
        var month = today.getMonth();
        var day = today.getDate();
        var cookieExpiryDate = new Date(year, month + 1, day).toUTCString();

        document.cookie = "cookies_policy_21_3=" + encodeURIComponent('{"essential":true,"usage":true,"preferences":true}') + "; expires=" + cookieExpiryDate + ";";
        document.cookie = "cookies_preferences_set_21_3=true; expires=" + cookieExpiryDate + ";";
        setCookies();

        showElement(cookieDecisionBanner);

        var cookieBanner = document.querySelector("#cookie-banner");
        hideElement(cookieBanner);

        document.querySelector("#hide-cookie-decision").onclick = function () {
            hideElement(cookieDecisionBanner);
        };
    };

    document.querySelector("#reject-cookies").onclick = function () {
        var today = new Date();
        var year = today.getFullYear();
        var month = today.getMonth();
        var day = today.getDate();
        var cookieExpiryDate = new Date(year, month + 1, day).toUTCString();

        document.cookie = "cookies_policy_21_3=" + encodeURIComponent('{"essential":true,"usage":false,"preferences":false}') + "; expires=" + cookieExpiryDate + ";";
        document.cookie = "cookies_preferences_set_21_3=true; expires=" + cookieExpiryDate + ";";
        window['ga-disable-UA-161400643-2'] = false;
        window['ga-disable-UA-145652997-1'] = false;

        cookieDecisionBanner.innerHTML = cookieDecisionBanner.innerHTML.replace("accepted", "rejected");
        showElement(cookieDecisionBanner);

        var cookieBanner = document.querySelector("#cookie-banner");
        hideElement(cookieBanner);

        document.querySelector("#hide-cookie-decision").onclick = function () {
            hideElement(cookieDecisionBanner);
        };
    };

    determineCookieState();
}

document.readyState !== 'loading'
    ? runCookieJobs()
    : document.addEventListener('DOMContentLoaded', runCookieJobs);