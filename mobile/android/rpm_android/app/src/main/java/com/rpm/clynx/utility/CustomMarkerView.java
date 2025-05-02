package com.rpm.clynx.utility;

import android.content.Context;
import android.util.Log;
import android.widget.TextView;
import com.github.mikephil.charting.charts.LineChart;
import com.github.mikephil.charting.components.MarkerView;
import com.github.mikephil.charting.data.Entry;
import com.github.mikephil.charting.highlight.Highlight;
import com.github.mikephil.charting.utils.MPPointF;
import com.rpm.clynx.R;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

public class CustomMarkerView extends MarkerView {
    private final TextView tvContent;
    private final List<String> xAxisLabels;
    private final LineChart lineChart;
    private static final String TAG = "CustomMarkerView";

    public CustomMarkerView(Context context, int layoutResource, List<String> xAxisLabels, LineChart lineChart) {
        super(context, layoutResource);
        this.xAxisLabels = xAxisLabels;
        tvContent = findViewById(R.id.marker_text);
        this.lineChart = lineChart;
    }

    @Override
    public void refreshContent(Entry e, Highlight highlight) {
        if (lineChart == null || lineChart.getData() == null) {
            Log.e(TAG, "Error: Chart data is null");
            tvContent.setText("Error: Chart not available!");
            super.refreshContent(e, highlight);
            return;
        }

        int index = (int) e.getX();
        if (index < 0 || index >= xAxisLabels.size()) {
            Log.w(TAG, "Invalid index: " + index);
            tvContent.setText("Invalid Data");
            super.refreshContent(e, highlight);
            return;
        }

        String selectedDate = xAxisLabels.get(index);
        Log.d(TAG, "Selected X Index: " + index + ", Date: " + selectedDate);

        StringBuilder markerText = new StringBuilder("Date: ").append(selectedDate).append("\n");

        int datasetCount = lineChart.getData().getDataSetCount();
        Log.d(TAG, "Total Datasets: " + datasetCount);

        // Store all values at this X position
        List<Float> valuesAtX = new ArrayList<>();

        for (int i = 0; i < datasetCount; i++) {
            Entry entry = lineChart.getData().getDataSetByIndex(i).getEntryForXValue(e.getX(), Float.NaN);
            if (entry != null) {
                valuesAtX.add(entry.getY());
            }
        }

        // Check if all values are the same
        boolean allSameValue = valuesAtX.stream().distinct().count() == 1;

        // Display logic
        for (int i = 0; i < datasetCount; i++) {
            Entry entry = lineChart.getData().getDataSetByIndex(i).getEntryForXValue(e.getX(), Float.NaN);
            if (entry != null) {
                String label = lineChart.getData().getDataSetByIndex(i).getLabel();
                String value = String.format(Locale.getDefault(), "%.2f", entry.getY());

                if (allSameValue || highlight.getDataSetIndex() == i) {
                    Log.d(TAG, "Showing - Dataset: " + label + ", Value: " + value);
                    markerText.append(label).append(": ").append(value).append("\n");
                } else {
                    Log.d(TAG, "Skipping - Dataset: " + label + ", Value: " + value);
                }
            }
        }

        tvContent.setText(markerText.toString().trim());
        super.refreshContent(e, highlight);
    }

    @Override
    public MPPointF getOffset() {
        return new MPPointF(-getWidth() / 2f, -getHeight());
    }
}
