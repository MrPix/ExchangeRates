// Inject the payload.js script into the current tab after the popout has loaded
window.addEventListener('load', function (evt) {
    chrome.extension.getBackgroundPage().chrome.tabs.executeScript(null, {
        file: 'payload.js'
    }, () => chrome.runtime.lastError);;
    
});

// Listen to messages from the payload.js script and write to popout.html
chrome.runtime.onMessage.addListener(function (message) {
    function drawChanges(elementId, newRate) {
        var storage = window.localStorage;
        var element = document.getElementById(elementId);
        var currentKey = elementId + "Current";
        var previousKey = elementId + "Previous";
        var currentRate = storage.getItem(currentKey);
        var previousRate = storage.getItem(previousKey);
        if (currentRate === null || previousRate === null){
            currentRate = previousRate = newRate;
            storage.setItem(currentKey, newRate);
            storage.setItem(previousKey, newRate);
        }
        var diff = (newRate - currentRate);
        if (diff > 0.0001 || diff < -0.0001) {
            previousRate = currentRate;
            currentRate = newRate;
            storage.setItem(currentKey, currentRate);
            storage.setItem(previousKey, previousRate);
        }
        var text = (newRate - previousRate).toFixed(3);
        element.innerHTML = text;
        if (currentRate < previousRate) {
            element.style.backgroundImage = "url('down.png')";
        } else if (currentRate > previousRate) {
            element.style.backgroundImage = "url('up.png')";
            element.innerHTML = "+" + element.innerHTML;
        } else {
            element.style.background = "none"
            element.innerHTML = "0.000";
        }
        element.style.backgroundRepeat = "no-repeat";
        element.style.backgroundPositionX = "6px";
        element.style.backgroundPositionY = "4px";
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
