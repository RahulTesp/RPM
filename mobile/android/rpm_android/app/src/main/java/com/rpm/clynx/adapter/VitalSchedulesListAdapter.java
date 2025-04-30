package com.rpm.clynx.adapter;

import android.content.Context;
import android.graphics.Color;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.model.VitalSchedulesModel;
import com.rpm.clynx.R;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link VitalSchedulesModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class VitalSchedulesListAdapter extends RecyclerView.Adapter<VitalSchedulesListAdapter.ViewHolder> {

    private final List<VitalSchedulesModel> mValues;
    public Context cxt;

    public VitalSchedulesListAdapter(List<VitalSchedulesModel> items) {
        mValues = items;
    }
    public VitalSchedulesListAdapter(List<VitalSchedulesModel> items,Context cxt)
    {
        mValues = items;
        this.cxt = cxt;
    }

    @Override
    public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.vital_schedules_item_list, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(final ViewHolder holder, int position) {
        holder.mItem = mValues.get(position);
        holder.mVitalName.setText(mValues.get(position).getVitalName());
        holder.mScheduleName.setText(mValues.get(position).getScheduleName());
        holder.mVitalScheduleName.setText(mValues.get(position).getVitalScheduleName());

        if(mValues.get(position).getMorning() == "true")
        {
            holder.mMorning.setTextColor(Color.BLACK);
        }
        else
        {
            holder.mMorning.setTextColor(Color.GRAY);
        }

        if(mValues.get(position).getAfternoon() == "true")
        {
            holder.mAfternoon.setTextColor(Color.BLACK);
        }
        else
        {
            holder.mAfternoon.setTextColor(Color.GRAY);
        }

        if(mValues.get(position).getEvening() == "true")
        {
            holder.mEvening.setTextColor(Color.BLACK);
        }
        else
        {
            holder.mEvening.setTextColor(Color.GRAY);
        }

        if(mValues.get(position).getNight() == "true")
        {
            holder.mNight.setTextColor(Color.BLACK);
        }
        else
        {
            holder.mNight.setTextColor(Color.GRAY);
        }
        RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(cxt, LinearLayoutManager.VERTICAL, false);
        holder.crvNotifications.setLayoutManager(layoutManager);
        VitalSchedulesAdapter childRecyclerViewAdapter = new VitalSchedulesAdapter(mValues.get(position).getVitalSchedulesItemModel(),holder.crvNotifications.getContext());
        holder.crvNotifications.setAdapter(childRecyclerViewAdapter);
    }

    @Override
    public int getItemCount() {
        return mValues.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {
        public final TextView mVitalName;
        public final TextView mScheduleName;
        public final TextView mVitalScheduleName;
        public final TextView mMorning;
        public final TextView mAfternoon;
        public final TextView mEvening;
        public final TextView mNight;
        public final RecyclerView crvNotifications;
        public VitalSchedulesModel mItem;

        public ViewHolder(View itemView) {
            super(itemView);
            mVitalName =  itemView.findViewById(R.id.itemVitalMonitoring);
            mScheduleName =  itemView.findViewById(R.id.itemSchedule1);
            mVitalScheduleName = itemView.findViewById(R.id.itemSchedule2);
            mMorning = itemView.findViewById(R.id.itemMorning);
            mAfternoon = itemView.findViewById(R.id.itemAfternoon);
            mEvening = itemView.findViewById(R.id.itemEvening);
            mNight = itemView.findViewById(R.id.itemNight);

            Log.d("nim mMorning", mMorning.toString());
            Log.d("nim mAfternoon", mAfternoon.toString());
            Log.d("nim mEvening", mEvening.toString());
            Log.d("nim mEvening", mNight.toString());

            crvNotifications = itemView.findViewById(R.id.listVitalSchedules);
        }
    }
}