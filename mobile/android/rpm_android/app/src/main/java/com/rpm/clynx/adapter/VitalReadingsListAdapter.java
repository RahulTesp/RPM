
package com.rpm.clynx.adapter;

import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.rpm.clynx.model.VitalReadingsModel;
import com.rpm.clynx.R;

import java.util.List;

public class VitalReadingsListAdapter extends RecyclerView.Adapter<VitalReadingsListAdapter.ViewHolder> {

    private final List<VitalReadingsModel> mValues;
    private final Context cxt;
    private final OnViewMoreClickListener viewMoreClickListener;
    private final boolean showViewMore;

    // Interface for callback
    public interface OnViewMoreClickListener {
        void onViewMoreClicked(VitalReadingsModel vital);
    }

    // Constructor
    public VitalReadingsListAdapter(List<VitalReadingsModel> items, Context cxt, OnViewMoreClickListener listener, boolean showViewMore) {
        mValues = items;
        this.cxt = cxt;
        this.viewMoreClickListener = listener;
        this.showViewMore = showViewMore;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.vital_readings_item_list, parent, false);
        return new ViewHolder(view);
    }


    @Override
    public void onBindViewHolder(@NonNull final ViewHolder holder, int position) {
        VitalReadingsModel vital = mValues.get(position);
        holder.mItem = vital;

        // Dynamically set width = 3/4 of screen width
        int screenWidth = holder.itemView.getContext().getResources().getDisplayMetrics().widthPixels;
        holder.itemView.getLayoutParams().width = (int) (screenWidth * 0.80f);
        holder.itemView.requestLayout();  // Important to apply the width change

        // Set vital name
        holder.itemVitalNameTextView.setText(vital.getVitalType());

        // Setup child RecyclerView (vertical inside card)
        RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(cxt, LinearLayoutManager.VERTICAL, false);
        holder.crvNotifications.setLayoutManager(layoutManager);
        VitalReadingsAdapter childAdapter = new VitalReadingsAdapter(vital.getVitalReadingsItemModels(), cxt);
        holder.crvNotifications.setAdapter(childAdapter);

        // Show or hide "View More"
        if (showViewMore) {
            holder.viewMoreTextView.setVisibility(View.VISIBLE);
            holder.viewMoreTextView.setOnClickListener(v -> {
                Log.d("VitalAdapter", "View More clicked for: " + vital.getVitalType());
                if (viewMoreClickListener != null) {
                    viewMoreClickListener.onViewMoreClicked(vital);
                }
            });
        } else {
            holder.viewMoreTextView.setVisibility(View.GONE);
        }
    }


    @Override
    public int getItemCount() {
        return mValues.size();
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        public final RecyclerView crvNotifications;
        public final TextView itemVitalNameTextView;
        public final TextView viewMoreTextView;
        public VitalReadingsModel mItem;

        public ViewHolder(View itemView) {
            super(itemView);
            crvNotifications = itemView.findViewById(R.id.listReadngVitals);
            itemVitalNameTextView = itemView.findViewById(R.id.itemvitalname);
            viewMoreTextView = itemView.findViewById(R.id.viewMore);
        }
    }
}