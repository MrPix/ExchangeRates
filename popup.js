// Inject the payload.js script into the current tab after the popout has loaded
window.addEventListener('load', function (evt) {
    chrome.extension.getBackgroundPage().chrome.tabs.executeScript(null, {
        file: 'payload.js'
    }, () => chrome.runtime.lastError);;
    
});

// Listen to messages from the payload.js script and write to popout.html
chrome.runtime.onMessage.addListener(function (message) {
    function drawChanges(elementId, currentRate) {
        var currentLocalStorage = window.localStorage.getItem(elementId + "Current");
        var previousLocalStorage = window.localStorage.getItem(elementId + "Previous");
        if (currentLocalStorage === null && previousLocalStorage === null){
            currentLocalStorage = previousLocalStorage = currentRate;
            window.localStorage.setItem(elementId + "Current", currentRate);
            window.localStorage.setItem(elementId + "Previous", currentRate);
        }
        var diff = (currentRate - currentLocalStorage);
        if (diff > 0.0001 || diff < -0.0001) {
            previousLocalStorage = currentLocalStorage;
            currentLocalStorage = currentRate;
            window.localStorage.setItem(elementId + "Current", currentLocalStorage);
            window.localStorage.setItem(elementId + "Previous", previousLocalStorage);
        }

        var text = (currentRate - previousLocalStorage).toFixed(3);
        document.getElementById(elementId).innerHTML = text;
        if (currentLocalStorage < previousLocalStorage) {
            document.getElementById(elementId).style.backgroundImage = "url('down.png')";
        } else if (currentLocalStorage > previousLocalStorage) {
            document.getElementById(elementId).style.backgroundImage = "url('up.png')";
            document.getElementById(elementId).innerHTML = "+" + document.getElementById(elementId).innerHTML;
        } else {
            document.getElementById(elementId).style.background = "none"
            document.getElementById(elementId).innerHTML = "0.000";
        }
        document.getElementById(elementId).style.backgroundRepeat = "no-repeat";
        document.getElementById(elementId).style.backgroundPositionX = "2px";
        document.getElementById(elementId).style.backgroundPositionY = "4px";
    }

    fetch('https://api.privatbank.ua/p24api/pubinfo?json&exchange&coursid=11')
    .then(response => response.json())
    .then(data => {
        var usd = "not found";
        for (var i = data.length - 1; i >= 0; i--) {
            if (data[i].ccy === 'USD'){
                usd = data[i].sale;
            }
        }
        if (usd === "not found"){
            document.getElementById('privatbank').innerHTML = usd;
        } else {
            drawChanges("privatbankchange", usd);
            document.getElementById('privatbank').innerHTML = (+usd).toFixed(3);
        }
    });

    fetch('https://api.monobank.ua/bank/currency')
    .then(response => response.json())
    .then(data => {
        var monousd = "not found";
        for (var i = data.length - 1; i >= 0; i--) {
            if (data[i].currencyCodeB === 980){
                monousd = data[i].rateSell;
            }
        }
        if (monousd === "not found") {
            document.getElementById('monobank').innerHTML = monousd;
        } else {
            drawChanges("monobankchange", monousd);
            document.getElementById('monobank').innerHTML = (+monousd).toFixed(3);
        }
    });
});
