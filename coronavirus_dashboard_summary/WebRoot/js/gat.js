// GoogleTag Initialise
"use strict";

var tagIds = ['UA-161400643-2', 'UA-145652997-1'];

var stripPII = function (str) {
    return str.replace(/(.*)([A-Z]{1,2}\d{1,2}[A-Z]?\s?\d{1,2}[A-Z]{1,2})(.*)/gi, '$1[REDACTED]$3')
}

var stripPIIUri = function (str) {
    return str.replace(/(.*)(postcode=[^&]+)(.*)/gi, '$1[REDACTED]$3')
}


function gtag() {
    window.dataLayer.push(arguments)
}

var setCookies = function () {
    tagIds.map(function (tag) {
        window['ga-disable-' + tag] = true;
        (function (w, d, s, l, i) {
            w[l] = w[l] || [];
            w[l].push({
                'gtm.start':
                    new Date().getTime(), event: 'gtm.js'
            });
            var f = d.getElementsByTagName(s)[0],
                j = d.createElement(s), dl = l !== 'dataLayer' ? '&l=' + l : '';
            j.async = true;
            j.src =
                'https://www.googletagmanager.com/gtag/js?id=' + i + dl;
            f.parentNode.insertBefore(j, f);
        })(window, document, 'script', 'dataLayer', tag);
    });
    window.dataLayer = window.dataLayer || [];

    gtag('js', new Date());
    
    tagIds.map(function (tag) {
        gtag(
            'config',
            tag,
            {
                'anonymize_ip': true,
                'allowAdFeatures': false,
                'allow_google_signals': false,
                'allow_ad_personalization_signals': false,
                'send_page_view': false,
            }
        );
        gtag('event', 'page_view', {
            page_title: stripPII(document.title),
            page_location: stripPIIUri(window.location.href),
            page_path: window.location.pathname + (window.location.search !== "" ? "?" + stripPII(window.location.search) : ""),
            send_to: tag
        });
    });
    
    console.log("gat initialised.")
};
