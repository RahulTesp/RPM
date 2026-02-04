export const CHART_OPTIONS: any = {
  responsive: true,
  maintainAspectRatio: false,
  pan: {
    enabled: true,
    mode: 'xy',
    rangeMin: {
      x: null,
      y: null,
    },
    rangeMax: {
      x: null,
      y: null,
    },
    onPan: function (e: any) {},
  },
  zoom: {
    enabled: true,
    drag: false,
    mode: 'xy',
    rangeMin: {
      x: null,
      y: null,
    },
    rangeMax: {
      x: null,
      y: null,
    },
    speed: 0.1,
  },
  line: {
    tension: 0.5,
  },
  scales: {
    x: {
      ticks: {
        autoSkip: false,
        maxRotation: 45,      // ✅ Angled for better spacing and larger appearance
        minRotation: 45,
        color: '#666666',     // ✅ Kept the professional grey color
        font: {
          size: 20,           // ✅ Increased size to match your screenshot
          weight: 'normal',   // ✅ Kept standard weight (not bold)
          family: 'Rubik'
        }
      },
      grid: {
        display: true,
        drawOnChartArea: true,
        color: '#E0E0E0'      // Standard light grey grid
      }
    },
    y: {
      ticks: {
        color: '#666666',
        font: {
          size: 18,           // ✅ Match the x-axis size
          weight: 'normal'
        }
      },
      grid: {
        color: '#E0E0E0'
      }
    }
  },
  plugins: {
    legend: {
      display: true,
      position: 'bottom',
      labels: {
        boxWidth: 15,
        color: '#666666',     // ✅ Grey legend text
        font: {
          size: 16,
          weight: 'normal'
        }
      },
    }
  }
};

export const CHART_COLORS = [
  {
    fill: false,
    borderColor: 'lightgrey',
    pointBackgroundColor: 'green',
    pointBorderColor: 'green',
    pointHoverBackgroundColor: '#fff',
    pointHoverBorderColor: 'rgba(148,159,177,0.8)',
    borderWidth: 2
  },
  {
    fill: false,
    borderColor: 'orange',
    pointBackgroundColor: 'red',
    pointBorderColor: 'red',
    pointHoverBackgroundColor: '#fff',
    pointHoverBorderColor: 'rgba(148,159,177,0.8)',
    borderWidth: 2
  },
  {
    fill: false,
    borderColor: 'lightblue',
    pointBackgroundColor: 'blue',
    pointBorderColor: 'blue',
    pointHoverBackgroundColor: '#fff',
    pointHoverBorderColor: 'rgba(148,159,177,0.8)',
    borderWidth: 2
  },
];

export const CHART_LEGEND = true;
export const CHART_TYPE = 'line';
