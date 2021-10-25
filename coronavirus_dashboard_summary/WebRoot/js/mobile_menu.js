// GovUK JS tag
document.body.className = ((document.body.className) ? document.body.className + ' js-enabled' : 'js-enabled');

// Mobile menu
function initMobileButtons() {
    document.querySelector("#mobile-menu-btn").onclick = function() {
        var ms = document.getElementById("mobile-navigation").style;
        ms.display = ms.display === "none" ? "inline-block" : "none";
    }
}

document.readyState !== 'loading'
    ? initMobileButtons()
    : document.addEventListener('DOMContentLoaded', initMobileButtons);

function trimCode() {
    var elm = document.querySelector("form[name=postcode-search] input[name=postcode]");
    elm.value = elm.value.trim().toUpperCase()
}

function postcodeProcessorInit() {
    document.querySelector("form[name=postcode-search]").addEventListener("submit", trimCode)
}

document.readyState !== 'loading'
    ? postcodeProcessorInit()
    : document.addEventListener('DOMContentLoaded', postcodeProcessorInit);
