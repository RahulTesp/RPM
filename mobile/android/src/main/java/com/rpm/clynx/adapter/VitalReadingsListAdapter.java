package com.rpm.clynx.adapter;

import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.model.VitalReadingsModel;
import com.rpm.clynx.R;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link VitalReadingsModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class VitalReadingsListAdapter extends RecyclerView.Adapter<VitalReadingsListAdapter.ViewHolder> {

    private final List<VitalReadingsModel> mValues;
    public Context cxt;

    public VitalReadingsListAdapter(List<VitalReadingsModel> items) {
        mValues = items;
    }
    public VitalReadingsListAdapter(List<VitalReadingsModel> items,Context cxt)
    {
        mValues = items;
        this.cxt = cxt;
    }

    @Override
    public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.vital_readings_item_list, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(final ViewHolder holder, int position) {
        Log.d("VitalAdapter", "Vital Type: " + mValues.get(position).getVitalType());
        holder.mItem = mValues.get(position);
        RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(cxt, LinearLayoutManager.VERTICAL, false);
        holder.crvNotifications.setLayoutManager(layoutManager);
        VitalReadingsAdapter childRecyclerViewAdapter = new VitalReadingsAdapter(mValues.get(position).getVitalReadingsItemModels(),holder.crvNotifications.getContext());
        holder.crvNotifications.setAdapter(childRecyclerViewAdapter);
        // Find a reference to the itemvitalname TextView
        TextView itemVitalNameTextView = holder.itemView.findViewById(R.id.itemvitalname);
        // Check the condition and set the text accordingly
        if (mValues.get(position).getVitalType().equals("Blood Pressure")) {
            itemVitalNameTextView.setText("Blood Pressure");
        } else if (mValues.get(position).getVitalType().equals("Blood Glucose")) {
            itemVitalNameTextView.setText("Blood Glucose");
        } else if (mValues.get(position).getVitalType().equals("Weight")) {
            itemVitalNameTextView.setText("Weight");
        } else if (mValues.get(position).getVitalType().equals("Blood Oxygen")) {
            itemVitalNameTextView.setText("Blood Oxygen");
        }
    }

    @Override
    public int getItemCount() {
        return mValues.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {

        public final RecyclerView crvNotifications;
        public VitalReadingsModel mItem;

        public ViewHolder(View itemView) {
            super(itemView);
            crvNotifications = itemView.findViewById(R.id.listReadngVitals);
        }
    }
}