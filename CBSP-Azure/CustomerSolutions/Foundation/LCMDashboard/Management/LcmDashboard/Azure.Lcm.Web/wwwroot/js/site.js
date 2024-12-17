

const getLogLevel = (logLevel) => {
    switch (logLevel) {
        case 0:
            return { text: "Trace", trCls: '', badgeCls: 'text-bg-light' };
        case 1:
            return { text: "Debug", trCls: '', badgeCls: 'text-bg-light' };
        case 2:
            return { text: "Info", trCls: '', badgeCls: 'text-bg-info' };
        case 3:
            return { text: "Warning", trCls: 'table-warning', badgeCls: 'text-bg-warning' };
        case 4:
            return { text: "Error", trCls: 'table-danger', badgeCls: 'text-bg-danger' };
        case 5:
            return { text: "Critical", trCls: 'table-danger', badgeCls: 'text-bg-danger' };
        case 6:
            return { text: "None", trCls: 'table-dark', badgeCls: 'text-bg-secondary' };
        default:
            return { text: "Unknown", trCls: 'table-dark', badgeCls: 'text-bg-secondary' };
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
            const llSpec = getLogLevel(logEntry.logLevel);
            const rowHtml = [];
            var textKind = logEntry.logLevel > 2 ? "bi bi-exclamation-circle text-danger" : "bi bi-cpu-fill";
            rowHtml.push(`<tr class="${llSpec.trCls}">`);
            rowHtml.push(` <th scope="row" style="width: 100px; text-align: center;"><span class="${textKind}">${llSpec.text}</i></th>`);
            rowHtml.push(` <td style="width: 160px; text-align: center;" title="${dayjs(logEntry.timestamp)}">${dayjs(logEntry.timestamp).fromNow()}</td>`);                        
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
    dayjs.extend(window.dayjs_plugin_relativeTime);
    setInterval(pollStatus, 1000);
}


