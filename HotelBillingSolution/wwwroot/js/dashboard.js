document.addEventListener('DOMContentLoaded', function () {
    try {
        // revenue
        const revenueSeries = window.__hb_revenue_series || [];
        const revenueLabels = window.__hb_revenue_labels || [];

        if (typeof ApexCharts !== 'undefined' && document.getElementById('hbRevenueChart')) {
            var options = {
                chart: { type: 'area', height: 260, toolbar: { show: false } },
                series: [{ name: 'Revenue', data: revenueSeries }],
                xaxis: { categories: revenueLabels },
                colors: [getComputedStyle(document.documentElement).getPropertyValue('--primary-color') || '#4f46e5'],
                stroke: { curve: 'smooth' }
            };
            var chart = new ApexCharts(document.querySelector('#hbRevenueChart'), options);
            chart.render();
        }

        // room status donut
        const roomStatus = window.__hb_room_status || {};
        if (typeof ApexCharts !== 'undefined' && document.getElementById('hbRoomStatusChart')) {
            const keys = ['Occupied','Available','Reserved','Maintenance'];
            const data = keys.map(k => roomStatus[k] || 0);
            var opts = {
                chart: { type: 'donut', height: 220 },
                series: data,
                labels: keys,
                colors: [getComputedStyle(document.documentElement).getPropertyValue('--primary-color') || '#4f46e5', getComputedStyle(document.documentElement).getPropertyValue('--success-color') || '#10b981', getComputedStyle(document.documentElement).getPropertyValue('--warning-color') || '#f59e0b', getComputedStyle(document.documentElement).getPropertyValue('--danger-color') || '#ef4444']
            };
            var dchart = new ApexCharts(document.querySelector('#hbRoomStatusChart'), opts);
            dchart.render();
        }

        // occupancy radial
        const occ = window.__hb_occupancy || 0;
        if (typeof ApexCharts !== 'undefined' && document.getElementById('hbOccupancyChart')) {
            var ropts = {
                chart: { type: 'radialBar', height: 200 },
                series: [occ],
                plotOptions: { radialBar: { hollow: { size: '60%' }, dataLabels: { show: false } } },
                colors: [getComputedStyle(document.documentElement).getPropertyValue('--primary-color') || '#4f46e5']
            };
            var rchart = new ApexCharts(document.querySelector('#hbOccupancyChart'), ropts);
            rchart.render();
        }
    }
    catch (ex) {
        console.error('dashboard init error', ex);
    }
});
