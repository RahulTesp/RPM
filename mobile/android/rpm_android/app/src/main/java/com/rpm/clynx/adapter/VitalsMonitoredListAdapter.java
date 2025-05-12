package com.rpm.clynx.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.model.VitalsMonitoredModel;
import com.rpm.clynx.R;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link VitalsMonitoredModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class VitalsMonitoredListAdapter extends RecyclerView.Adapter<VitalsMonitoredListAdapter.ViewHolder> {

    private final List<VitalsMonitoredModel> mValues;
    public Context cxt;

    public VitalsMonitoredListAdapter(List<VitalsMonitoredModel> items) {
        mValues = items;
    }
    public VitalsMonitoredListAdapter(List<VitalsMonitoredModel> items,Context cxt)
    {
        mValues = items;
        this.cxt = cxt;
    }

    @Override
    public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.vitals_monitored_item_list, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(final ViewHolder holder, int position) {
        holder.mItem = mValues.get(position);
        RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(cxt, LinearLayoutManager.VERTICAL, false);
        holder.crvNotifications.setLayoutManager(layoutManager);
        VitalsMonitoredAdapter childRecyclerViewAdapter = new VitalsMonitoredAdapter(mValues.get(position).getVitalsMonitoredItemModels(),holder.crvNotifications.getContext());
        holder.crvNotifications.setAdapter(childRecyclerViewAdapter);
    }

    @Override
    public int getItemCount() {
        return mValues.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {

        public final RecyclerView crvNotifications;
        public VitalsMonitoredModel mItem;

        public ViewHolder(View itemView) {
            super(itemView);
            crvNotifications = itemView.findViewById(R.id.listVitalsMonitored);
        }
    }
}