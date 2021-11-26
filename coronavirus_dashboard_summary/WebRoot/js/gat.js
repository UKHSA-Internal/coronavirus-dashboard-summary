// GoogleTag Initialise
"use strict";

var tagIds = [
    ['dash', 'UA-161400643-2'],
    ['govuk', 'UA-145652997-1']
];

var stripPII = function (str) {
    return decodeURIComponent(str).replace(/(.*=?)([A-Z]{1,2}\d{1,2}[A-Z]?[\s+]?\d{1,2}[A-Z]{1,2})(.*)/gi, '$1[REDACTED]$3')
}

var stripPIIUri = function (str) {
    return decodeURIComponent(str).replace(/(.*)(postcode=[^&]+)(.*)/gi, '$1[REDACTED]$3')
}


function gtag() {
    window.dataLayer.push(arguments)
}

var setCookies = function () {
    window.dataLayer = window.dataLayer || [];
    
    tagIds.map(function (tag) {
        window['ga-disable-' + tag] = false;

        window.ga('create', {
            trackingId: tag[1],
            cookieDomain: 'auto',
            name: tag[0],
            allowLinker: true
        });
        window.ga(tag[0] + '.require', 'linker');
        window.ga(tag[0] + '.set', 'anonymizeIp', true);
        window.ga(tag[0] + '.set', 'allowAdFeatures', false);
        window.ga(tag[0] + '.linker:autoLink', ["www.gov.uk", "coronavirus.data.gov.uk", "data.gov.uk", "gov.uk"]);
        window.ga(tag[0] + '.send', {
            hitType: 'pageview',
            page: window.location.pathname + (window.location.search !== "" ? "?" + stripPII(window.location.search) : "").replace(/[?]+/, "?"),
            location: stripPIIUri(window.location.href).replace(/[?]+/, "?"),
            title: stripPII(document.title),
        });
        
    });
    
    console.log("gat initialised.")
};
