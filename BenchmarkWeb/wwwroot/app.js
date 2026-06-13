const btnStart = document.getElementById('btnStart');
const btnStop = document.getElementById('btnStop');
const statusText = document.getElementById('statusText');
let eventSource = null;

// Initialize Chart
const ctx = document.getElementById('benchmarkChart').getContext('2d');
Chart.defaults.color = '#94a3b8';
Chart.defaults.font.family = "'Inter', sans-serif";

const colors = [
    '#3b82f6', '#ef4444', '#10b981', '#f59e0b', '#8b5cf6', '#ec4899', '#06b6d4'
];

let chartInstance = new Chart(ctx, {
    type: 'line',
    data: {
        labels: [],
        datasets: []
    },
    options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: 'top',
                labels: { color: '#f8fafc' }
            },
            tooltip: {
                mode: 'index',
                intersect: false,
            }
        },
        scales: {
            x: {
                title: { display: true, text: 'Array Size', color: '#94a3b8' },
                grid: { color: '#334155' }
            },
            y: {
                title: { display: true, text: 'Time (ms)', color: '#94a3b8' },
                grid: { color: '#334155' },
                beginAtZero: true
            }
        }
    }
});

let datasetsMap = new Map(); // key -> dataset index
let sizesSet = new Set();
let allResults = [];

function getSelected(name) {
    return Array.from(document.querySelectorAll(`input[name="${name}"]:checked`)).map(cb => cb.value);
}

function getSelectedInt(name) {
    return Array.from(document.querySelectorAll(`input[name="${name}"]:checked`)).map(cb => parseInt(cb.value));
}

btnStart.addEventListener('click', async () => {
    const config = {
        languages: getSelected('lang'),
        algorithms: getSelected('algo'),
        states: getSelected('state'),
        sizes: getSelectedInt('size').sort((a, b) => a - b),
        runs: 5
    };

    if (config.languages.length === 0 || config.algorithms.length === 0 || config.sizes.length === 0) {
        alert("Please select at least one language, one algorithm, and one size.");
        return;
    }

    try {
        const res = await fetch('/api/benchmark/start', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(config)
        });

        if (!res.ok) {
            alert(await res.text());
            return;
        }

        // Setup UI
        btnStart.disabled = true;
        btnStop.disabled = false;
        
        // Merge new sizes with existing ones
        const oldSizes = chartInstance.data.labels || [];
        const combinedSizes = Array.from(new Set([...oldSizes, ...config.sizes])).sort((a, b) => a - b);
        
        if (combinedSizes.join(',') !== oldSizes.join(',')) {
            chartInstance.data.labels = combinedSizes;
            chartInstance.data.datasets.forEach(ds => {
                const newData = new Array(combinedSizes.length).fill(null);
                oldSizes.forEach((oldSize, i) => {
                    const newIndex = combinedSizes.indexOf(oldSize);
                    newData[newIndex] = ds.data[i];
                });
                ds.data = newData;
            });
            chartInstance.update();
        }

        document.getElementById('summaryContainer').style.display = 'none';

        startStream();

    } catch (e) {
        alert("Failed to start: " + e.message);
    }
});

btnStop.addEventListener('click', async () => {
    await fetch('/api/benchmark/stop', { method: 'POST' });
    btnStop.disabled = true;
    statusText.innerText = 'Stopping...';
});

function startStream() {
    if (eventSource) {
        eventSource.close();
    }

    eventSource = new EventSource('/api/benchmark/stream');
    
    eventSource.onmessage = (event) => {
        const data = JSON.parse(event.data);
        
        if (data.type === 'status') {
            statusText.innerText = data.message;
            if (data.message === 'Completed' || data.message === 'Stopped') {
                btnStart.disabled = false;
                btnStop.disabled = true;
                eventSource.close();
                
                if (data.message === 'Completed') {
                    renderSummary(allResults);
                }
            }
        } 
        else if (data.type === 'result') {
            const res = data.data;
            const existingIndex = allResults.findIndex(r => 
                r.Language === res.Language && 
                r.Algorithm === res.Algorithm && 
                r.State === res.State && 
                r.Size === res.Size
            );
            
            if (existingIndex !== -1) {
                allResults[existingIndex] = res;
            } else {
                allResults.push(res);
            }
            
            if (!res.Error) {
                const key = `${res.Language} - ${res.Algorithm} (${res.State})`;
                
                if (!datasetsMap.has(key)) {
                    const color = colors[datasetsMap.size % colors.length];
                    const newDataset = {
                        label: key,
                        data: new Array(chartInstance.data.labels.length).fill(null),
                        borderColor: color,
                        backgroundColor: color,
                        tension: 0.2,
                        pointRadius: 4,
                        borderWidth: 2
                    };
                    chartInstance.data.datasets.push(newDataset);
                    datasetsMap.set(key, chartInstance.data.datasets.length - 1);
                }

                const dsIndex = datasetsMap.get(key);
                const sizeIndex = chartInstance.data.labels.indexOf(res.Size);
                
                if (sizeIndex !== -1) {
                    chartInstance.data.datasets[dsIndex].data[sizeIndex] = res.TimeMs;
                    chartInstance.update();
                }
            }
        }
        else if (data.type === 'error') {
            console.error("Benchmark Error:", data.message);
            statusText.innerText = "Error: " + data.message;
            statusText.style.color = 'var(--danger)';
        }
    };

    eventSource.onerror = () => {
        eventSource.close();
        btnStart.disabled = false;
        btnStop.disabled = true;
        statusText.innerText = "Connection lost";
    };
}

function renderSummary(results) {
    const container = document.getElementById('summaryContainer');
    const content = document.getElementById('summaryContent');
    content.innerHTML = '';
    
    const groups = {};
    results.forEach(r => {
        const key = `${r.Algorithm}|${r.State}|${r.Size}`;
        if (!groups[key]) groups[key] = {
            algorithm: r.Algorithm, state: r.State, size: r.Size, items: []
        };
        groups[key].items.push(r);
    });

    for (const key in groups) {
        const group = groups[key];
        const block = document.createElement('div');
        block.className = 'summary-block';

        const header = document.createElement('div');
        header.className = 'summary-header';
        header.innerText = `[ ${group.algorithm} | ${group.state} | ${group.size} елементів ]`;
        block.appendChild(header);

        let csharpRes = group.items.find(r => r.Language === 'C#');
        let cRes = group.items.find(r => r.Language === 'C');
        let cppRes = group.items.find(r => r.Language === 'C++');

        group.items.forEach(r => {
            const row = document.createElement('div');
            row.className = 'summary-row';

            const lang = document.createElement('div');
            lang.className = 'summary-lang';
            lang.innerText = r.Language + ':';

            if (r.Error) {
                const skip = document.createElement('div');
                skip.className = 'summary-skip';
                skip.innerText = `ПРОПУЩЕНО (Причина: запобіжник спрацював для великого об'єму даних)`;
                row.appendChild(lang);
                row.appendChild(skip);
            } else {
                const time = document.createElement('div');
                time.className = 'summary-time';
                time.innerText = r.TimeMs.toFixed(2) + ' ms';

                const barWrap = document.createElement('div');
                barWrap.className = 'summary-bar-wrapper';
                const bar = document.createElement('div');
                bar.className = 'summary-bar';
                let width = Math.min((r.TimeMs / 5.0) * 10, 400); 
                if (width < 2 && r.TimeMs >= 0) width = 2;
                bar.style.width = width + 'px';
                
                if (r.Language === 'C') bar.style.backgroundColor = '#10b981';
                else if (r.Language === 'C++') bar.style.backgroundColor = '#f59e0b';
                else bar.style.backgroundColor = '#3b82f6';

                barWrap.appendChild(bar);

                row.appendChild(lang);
                row.appendChild(time);
                row.appendChild(barWrap);
            }
            block.appendChild(row);
        });

        function compare(r1, r2, lang1, lang2) {
            if (r1 && !r1.Error && r2 && !r2.Error && r1.TimeMs > 0 && r2.TimeMs > 0) {
                let speedup = Math.max(r1.TimeMs, r2.TimeMs) / Math.min(r1.TimeMs, r2.TimeMs);
                if (speedup > 1.05) {
                    const faster = r1.TimeMs > r2.TimeMs ? lang2 : lang1;
                    const slower = r1.TimeMs > r2.TimeMs ? lang1 : lang2;
                    const diff = ((speedup - 1) * 100).toFixed(1);
                    
                    const comp = document.createElement('div');
                    comp.className = 'summary-compare';
                    comp.innerText = `-> ${faster} швидший за ${slower} на ${diff}% (${speedup.toFixed(2)}x)`;
                    block.appendChild(comp);
                } else {
                    const comp = document.createElement('div');
                    comp.className = 'summary-compare';
                    comp.innerText = `-> ${lang1} та ${lang2} мають приблизно однакову швидкість`;
                    block.appendChild(comp);
                }
            }
        }

        compare(csharpRes, cRes, "C#", "C");
        compare(csharpRes, cppRes, "C#", "C++");

        content.appendChild(block);
    }
    
    container.style.display = 'block';
}
