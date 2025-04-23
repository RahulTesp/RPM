package com.rpm.clynx.adapter;

import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.ViewGroup;
import android.widget.TextView;
import com.rpm.clynx.databinding.VitalsMonitoredItemBinding;
import com.rpm.clynx.model.VitalsMonitoredItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link VitalsMonitoredItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class VitalsMonitoredAdapter extends RecyclerView.Adapter<VitalsMonitoredAdapter.VitalsMonitoredViewHolder> {

    private final List<VitalsMonitoredItemModel> gValues;
    public Context cxt;

    public VitalsMonitoredAdapter(List<VitalsMonitoredItemModel> items) {
        gValues = items;
    }

    public VitalsMonitoredAdapter(List<VitalsMonitoredItemModel> items,Context cxt) {
        gValues = items;
        this.cxt = cxt;
    }

    @Override
    public VitalsMonitoredViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new VitalsMonitoredViewHolder(VitalsMonitoredItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final VitalsMonitoredViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.mVitalMonitoredName.setText(gValues.get(position).getVitalsMonitoredName());
    }

    @Override
    public int getItemCount() {
        return gValues.size();
    }

    public class VitalsMonitoredViewHolder extends RecyclerView.ViewHolder {
        public final TextView mVitalMonitoredName;
        public VitalsMonitoredItemModel mItem;
        public VitalsMonitoredViewHolder(VitalsMonitoredItemBinding binding) {
            super(binding.getRoot());
            mVitalMonitoredName = binding.itemvitalmonitoredname;
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mVitalMonitoredName.getText() + "'";
        }
    }
}