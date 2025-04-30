package com.rpm.clynx.adapter;

import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.ViewGroup;
import android.widget.TextView;
import com.rpm.clynx.databinding.VitalSchedulesItemBinding;
import com.rpm.clynx.model.VitalSchedulesItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link VitalSchedulesItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class VitalSchedulesAdapter extends RecyclerView.Adapter<VitalSchedulesAdapter.VitalSchedulesViewHolder> {

    private final List<VitalSchedulesItemModel> gValues;
    public Context cxt;

    public VitalSchedulesAdapter(List<VitalSchedulesItemModel> items) {
        gValues = items;
    }

    public VitalSchedulesAdapter(List<VitalSchedulesItemModel> items,Context cxt) {
        gValues = items;
        this.cxt = cxt;
    }

    @Override
    public VitalSchedulesViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new VitalSchedulesViewHolder(VitalSchedulesItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final VitalSchedulesViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.mNormalMinimum.setText(gValues.get(position).getNormalMinimum());
        holder.mNormalMaximum.setText(gValues.get(position).getNormalMaximum());
        holder.mMeasureName.setText(gValues.get(position).getMeasureName());
        holder.mUnitName.setText(gValues.get(position).getUnitName());
    }

    @Override
    public int getItemCount() {
        return gValues.size();
    }

    public class VitalSchedulesViewHolder extends RecyclerView.ViewHolder {
        public final TextView mNormalMinimum;
        public final TextView mNormalMaximum;
        public final TextView mMeasureName;
        public final TextView mUnitName;
        public VitalSchedulesItemModel mItem;

        public VitalSchedulesViewHolder(VitalSchedulesItemBinding binding) {
            super(binding.getRoot());

            mNormalMinimum = binding.Range1;
            mNormalMaximum = binding.Range2;
            mMeasureName = binding.MeasureName;
            mUnitName= binding.UnitName;
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mNormalMinimum.getText() + "'" +  mNormalMaximum.getText() ;
        }
    }
}