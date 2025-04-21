package com.rpm.clynx.utility;

import android.content.Context;
import com.github.mikephil.charting.components.MarkerView;
import com.github.mikephil.charting.utils.MPPointF;

public class MyMarkerView extends MarkerView {
    private boolean isFirstDataPoint;
    private boolean isLastDataPoint;
    private boolean enoughSpaceToRight;  // Add this field
    public MyMarkerView(Context context, int layoutResource) {
        super(context, layoutResource);
    }

    public void setFirstAndLastDataPoint(boolean isFirstDataPoint, boolean isLastDataPoint) {
        this.isFirstDataPoint = isFirstDataPoint;
        this.isLastDataPoint = isLastDataPoint;
    }

    // Add a setter method for enoughSpaceToRight
    public void setEnoughSpaceToRight(boolean enoughSpaceToRight) {
        this.enoughSpaceToRight = enoughSpaceToRight;
    }

    @Override
    public MPPointF getOffsetForDrawingAtPoint(float posX, float posY) {
        MPPointF offset = super.getOffsetForDrawingAtPoint(posX, posY);

        // Calculate the desired X offset based on the flags and enoughSpaceToRight
        float desiredXOffset = 0f;  // Default offset

        if (isFirstDataPoint) {
            desiredXOffset = 20f;  // Offset for the first data point
        } else if (isLastDataPoint) {
            desiredXOffset = -20f;  // Offset for the last data point
        }

        // If there's not enough space to the right, adjust the X offset further
        if (!enoughSpaceToRight) {
            desiredXOffset = -130f;  // Adjust as needed
        }

        // Apply the desired X offset
        offset.x = desiredXOffset;
        offset.y = 0f;  // No vertical offset

        return offset;
    }
}
