package com.rpm.clynx.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.model.ActivitySchedulesModel;
import com.rpm.clynx.R;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link ActivitySchedulesModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class ActivitySchedulesListAdapter extends RecyclerView.Adapter<ActivitySchedulesListAdapter.ViewHolder> {

    private final List<ActivitySchedulesModel> mValues;
    public Context cxt;
    public ActivitySchedulesListAdapter(List<ActivitySchedulesModel> items) {
        mValues = items;
    }
    public ActivitySchedulesListAdapter(List<ActivitySchedulesModel> items,Context cxt)
    {
        mValues = items;
        this.cxt = cxt;
    }

    @Override
    public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.activity_schedules_item_list, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(final ViewHolder holder, int position) {
        holder.mItem = mValues.get(position);
         holder.mScheduleDate.setText(mValues.get(position).getScheduleDate());

        RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(cxt, LinearLayoutManager.VERTICAL, false);
        holder.crvNotifications.setLayoutManager(layoutManager);

        ActivitySchedulesAdapter childRecyclerViewAdapter = new ActivitySchedulesAdapter(mValues.get(position).getActivitySchedulesItemModels(),holder.crvNotifications.getContext());
        holder.crvNotifications.setAdapter(childRecyclerViewAdapter);
    }

    @Override
    public int getItemCount() {
        return mValues.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {
        public final TextView mScheduleDate;
        public final RecyclerView crvNotifications;
        public ActivitySchedulesModel mItem;

        public ViewHolder(View itemView) {
            super(itemView);
            mScheduleDate = itemView.findViewById(R.id.sched_date);
            crvNotifications = itemView.findViewById(R.id.listActivitySchedules);
        }
    }
}