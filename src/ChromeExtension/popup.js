window.addEventListener('load', function(evt) {
    init();
});

var isPrivatEnabled = true;
var isMonoEnabled = true;
var isAlfaEnabled = true;
var idKurscomuaEnabled = true;

function doCORSRequest(options, callback) {
    var cors_api_url = 'https://cors-anywhere.herokuapp.com/';
    var x = new XMLHttpRequest();
    x.open(options.method, cors_api_url + options.url);
    x.onload = x.onerror = function() {
        callback(
            x.responseText
        );
    };
    if (/^POST/i.test(options.method)) {
        x.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    }
    x.send(options.data);
}

function init(message) {
    loadPrivat();
    loadMono();
    loadAlfa();
    loadKurscomua();
}

function loadPrivat() {
    if (!isPrivatEnabled) {
        return;
    }
    fetch('https://api.privatbank.ua/p24api/pubinfo?json&exchange&coursid=11')
        .then(response => response.json())
        .then(data => {
            var usd = "not found";
            for (var i = data.length - 1; i >= 0; i--) {
                if (data[i].ccy === 'USD') {
                    usd = data[i].sale;
                }
            }
            if (usd === "not found") {
                setElementText('privatbank', usd);
            } else {
                drawChanges("privatbankchange", usd);
                setElementText('privatbank', (+usd).toFixed(3));
            }
        });
}

function loadMono() {
    if (!isMonoEnabled) {
        return;
    }
    fetch('https://api.monobank.ua/bank/currency')
        .then(response => response.json())
        .then(data => {
            var monousd = "not found";
            for (var i = data.length - 1; i >= 0; i--) {
                if (data[i].currencyCodeB === 980) {
                    monousd = data[i].rateSell;
                }
            }
            if (monousd === "not found") {
                setElementText('monobank', monousd);
            } else {
                drawChanges("monobankchange", monousd);
                setElementText('monobank', (+monousd).toFixed(3))
            }
        });
}

function loadAlfa() {
    if (!isAlfaEnabled) {
        return;
    }
    doCORSRequest({
        method: 'GET',
        url: 'https://alfabank.ua/',
        data: ''
    }, data => {
		data = data.substring(data.indexOf('<div class="currency-tab-block" data-tab="0">'));
        var startIndex = data.indexOf("Продаж") + 97;
		var usd = data.substring(startIndex, startIndex + 5);

        drawChanges("alfabankchange", usd);
        setElementText('alfabank', (+usd).toFixed(3));
    });
}

function loadKurscomua() {
    if (!idKurscomuaEnabled) {
        return;
    }
    doCORSRequest({
        method: 'GET',
        url: 'https://kurs.com.ua/ajax/getChart?size=big&type=interbank&currencies_from=usd&currencies_to=&organizations=&limit=&optimal=',
        data: ''
    }, data => {
        data = JSON.parse(data)
        if (data && data.view) {
            var chart = JSON.parse(data.view);
            var todayData = chart.series[0].data;
            var yesterdayData = chart.series[2].data;
            var kursRate = getLatestRate(todayData);
            if (kursRate === null) {
                kursRate = getLatestRate(yesterdayData);
                var el1 = document.getElementById('kurscomua');
                var el2 = document.getElementById('kurscomuachange');
                if (el1 && el2) {
                    el1.style.color = "grey";
                    el2.style.color = "grey";
                }
            }
            if (kursRate === null) {
                return;
            }
            drawChanges("kurscomuachange", kursRate);
            setElementText("kurscomua", (+kursRate).toFixed(3));
        }
    });
    return;
}

function setElementText(elementId, text) {
    var element = document.getElementById(elementId);
    if (!element) return;
    element.innerHTML = text;
}

function drawChanges(elementId, newRate) {
    var storage = window.localStorage;
    var element = document.getElementById(elementId);
    var currentKey = elementId + "Current";
    var previousKey = elementId + "Previous";
    var currentRate = storage.getItem(currentKey);
    var previousRate = storage.getItem(previousKey);
    if (currentRate === null || previousRate === null) {
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
    if (!element) return;
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

function getLatestRate(data) {
    if (data.length > 0) {
        var i = data.length - 1;
        do {
            if (data[i][1] !== null) {
                return data[i][1];
            }
        } while (--i >= 0)
    }
    return null;
}

if (chrome && chrome.runtime && chrome.runtime.onMessage) {
    // Listen to messages from the payload.js script and write to popout.html
    chrome.runtime.onMessage.addListener(init);
}