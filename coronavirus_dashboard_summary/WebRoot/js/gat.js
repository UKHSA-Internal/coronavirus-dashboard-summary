// GoogleTag Initialise
"use strict";

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
    window['ga-disable-UA-161400643-2'] = true;
    window['ga-disable-UA-145652997-1'] = true;
    window.dataLayer = window.dataLayer || [];

    gtag('js', new Date());
    gtag(
        'config',
        'UA-161400643-2',
        {
            'anonymize_ip': true,
            'allowAdFeatures': false,
            'allow_google_signals': false,
            'allowLinker': true,
            'allow_ad_personalization_signals': false
        }
    );
    gtag(
        'config',
        'UA-145652997-1',
        {
            'anonymize_ip': true,
            'allowAdFeatures': false,
            'allowLinker': true,
            'allow_google_signals': false,
            'allow_ad_personalization_signals': false,
            'send_page_view': false,
        }
    );
    gtag('event', 'page_view', {
        page_title: stripPII(document.title),
        page_location: stripPIIUri(window.location.href),
        page_path: window.location.pathname + (window.location.search !== "" ? "?" + stripPII(window.location.search) : ""),
        send_to: 'UA-161400643-2'
    });
    gtag('event', 'page_view', {
        page_title: stripPII(document.title),
        page_location: stripPIIUri(window.location.href),
        page_path: window.location.pathname + (window.location.search !== "" ? "?" + stripPII(window.location.search) : ""),
        send_to: 'UA-145652997-1'
    });
    gtag('linker:autoLink', ["www.gov.uk", "coronavirus.data.gov.uk", "data.gov.uk", "gov.uk"]);
};
