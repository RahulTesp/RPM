export const CHART_OPTIONS = {
  responsive: true,
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
  maintainAspectRatio: false,
  line: {
    tension: 0.5,

  },
   scales: {
    x: {
      ticks: {
        autoSkip: false,
        maxRotation: 90,
        minRotation: 45
      }
    }
  },
  legend: {
    display: true,
    position: 'bottom',
    labels: {
      boxWidth: 10,
    },
  },
};

export const CHART_COLORS = [
  {
    fill: false,
    borderColor: 'lightgrey',
    pointBackgroundColor: 'green',
    pointBorderColor: 'green',
    pointHoverBackgroundColor: '#fff',
    pointHoverBorderColor: 'rgba(148,159,177,0.8)',
  },
  {
    fill: false,
    borderColor: 'orange',
    pointBackgroundColor: 'red',
    pointBorderColor: 'red',
    pointHoverBackgroundColor: '#fff',
    pointHoverBorderColor: 'rgba(148,159,177,0.8)',
  },
  {
    fill: false,
    borderColor: 'lightblue',
    pointBackgroundColor: 'blue',
    pointBorderColor: 'blue',
    pointHoverBackgroundColor: '#fff',
    pointHoverBorderColor: 'rgba(148,159,177,0.8)',
  },
];

export const CHART_LEGEND = true;
export const CHART_TYPE = 'line';
