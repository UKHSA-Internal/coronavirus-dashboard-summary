// GoogleTag Initialise
"use strict";
// Strip PII from page-view submissions
var stripPII = function (str) {
    return str.replace(/(postcode=)([^&]+)/gi, '$1[redacted]')
}

function gtag() {
    window.dataLayer.push(arguments)
}

var setCookies = function () {
    window.dataLayer = window.dataLayer || [];

    gtag('js', new Date());
    gtag(
        'config',
        'UA-161400643-2',
        {
            'anonymize_ip': true,
            'allowAdFeatures': false
        }
    );
    window.ga('create', 'UA-145652997-1', 'auto', 'govuk_shared', { 'allowLinker': true });
    window.ga('govuk_shared.require', 'linker');
    window.ga('govuk_shared.set', 'anonymizeIp', true);
    window.ga('govuk_shared.set', 'allowAdFeatures', false);
    window.ga("govuk_shared.linker:autoLink", ["www.gov.uk", "coronavirus.data.gov.uk"]);
    window.ga('govuk_shared.set', 'location', window.location.href.split('?')[0] + stripPII(window.location.search));
    window.ga('send', 'pageview');
    window.ga('govuk_shared.send', 'pageview');
};
