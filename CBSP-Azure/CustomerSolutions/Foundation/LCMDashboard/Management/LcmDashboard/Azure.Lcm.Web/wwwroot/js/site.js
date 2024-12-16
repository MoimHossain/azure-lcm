
const getLogLevel = (logLevel) => {
    switch (logLevel) {
        case 0:
            return "Trace";
        case 1:
            return "Debug";
        case 2:
            return "Information";
        case 3:
            return "Warning";
        case 4:
            return "Error";
        case 5:
            return "Critical";
        case 6:
            return "None";
        default:
            return "Unknown";
    }
}

const updateTableBody = (trArray) => {
    const tbody = document.querySelector("table tbody");    
    while (tbody.firstChild) {
        tbody.removeChild(tbody.firstChild);
    }

    trArray.forEach(trHTML => {
        tbody.insertAdjacentHTML('beforeend', trHTML);
    });
}

const readLogEntries = async () => {
    const response = await fetch('/api/traces');
    const logEntries = await response.json();
    return logEntries;
}


const pollStatus = async () => {
    const htmlLines = [];

    try
    {
        const logEntries = await readLogEntries();
        console.log("loaded logs", logEntries);
        logEntries.forEach(logEntry => {
            const rowHtml = [];
            var textKind = logEntry.logLevel > 2 ? "bi bi-exclamation-circle text-danger" : "bi bi-cpu-fill";
            rowHtml.push(`<tr>`);
            rowHtml.push(` <td>${logEntry.timestamp}</td>`);
            rowHtml.push(` <th scope="row"><i class="${textKind}">${getLogLevel(logEntry.logLevel)}</i></th>`);            
            rowHtml.push(` <td class="${textKind}">${logEntry.message}</td>`);
            rowHtml.push(`</tr>`);
            htmlLines.push(rowHtml.join(''));
        });
    }
    catch (error) {
        console.error(error);
    }

    updateTableBody(htmlLines.reverse());
}




const runTraceLoopAsync = async () => {    
    setInterval(pollStatus, 1000);
}


