package com.rpm.clynx.adapter;

import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.ViewGroup;
import android.widget.TextView;
import com.rpm.clynx.databinding.ActivitySchedulesItemBinding;
import com.rpm.clynx.model.ActivitySchedulesItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link ActivitySchedulesItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class ActivitySchedulesAdapter extends RecyclerView.Adapter<ActivitySchedulesAdapter.ActivitySchedulesViewHolder> {
    private final List<ActivitySchedulesItemModel> gValues;
    public Context cxt;
    public ActivitySchedulesAdapter(List<ActivitySchedulesItemModel> items) {
        gValues = items;
    }
    public ActivitySchedulesAdapter(List<ActivitySchedulesItemModel> items,Context cxt) {
        gValues = items;
        this.cxt = cxt;
    }

    @Override
    public ActivitySchedulesViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new ActivitySchedulesViewHolder(ActivitySchedulesItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final ActivitySchedulesViewHolder holder, int position) {
        holder.mItem = gValues.get(position);
        holder.mScheduleType.setText(gValues.get(position).getScheduleType());
        holder.mScheduleTime.setText(gValues.get(position).getScheduleTime());
        holder.mAssignedByName.setText(gValues.get(position).getAssignedByName());
    }

    @Override
    public int getItemCount() {
        return gValues.size();
    }

    public class ActivitySchedulesViewHolder extends RecyclerView.ViewHolder {
        public final TextView mScheduleType;
        public final TextView mScheduleTime;
        public final TextView mAssignedByName;

        public ActivitySchedulesItemModel mItem;

        public ActivitySchedulesViewHolder(ActivitySchedulesItemBinding binding) {
            super(binding.getRoot());
            mScheduleType = binding.itemSchedType;
            mScheduleTime = binding.itemSchedTime;
            mAssignedByName = binding.itemAssignedBy;
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mScheduleType.getText() + "'";
        }
    }
}