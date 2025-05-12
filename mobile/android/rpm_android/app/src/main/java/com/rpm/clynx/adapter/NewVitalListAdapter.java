package com.rpm.clynx.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.model.NewVitalsModel;
import com.rpm.clynx.R;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link NewVitalsModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class NewVitalListAdapter extends RecyclerView.Adapter<NewVitalListAdapter.ViewHolder> {

    private final List<NewVitalsModel> mValues;
    public Context cxt;

    public NewVitalListAdapter(List<NewVitalsModel> items) {
        mValues = items;
    }
    public NewVitalListAdapter(List<NewVitalsModel> items,Context cxt)
    {
        mValues = items;
        this.cxt = cxt;
    }

    @Override
    public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.new_vital_item_list, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(final ViewHolder holder, int position) {
        holder.mItem = mValues.get(position);
        holder.mNotificationDate.setText(mValues.get(position).getVitalName());
        RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(cxt, LinearLayoutManager.VERTICAL, false);
        holder.crvNotifications.setLayoutManager(layoutManager);
        NewVitalAdapter childRecyclerViewAdapter = new NewVitalAdapter(mValues.get(position).getNewVitalsItemModels(),holder.crvNotifications.getContext());
        holder.crvNotifications.setAdapter(childRecyclerViewAdapter);
    }

    @Override
    public int getItemCount() {
        return mValues.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {
        public final TextView mNotificationDate;
        public final RecyclerView crvNotifications;
        public NewVitalsModel mItem;

        public ViewHolder(View itemView) {
            super(itemView);
            mNotificationDate = itemView.findViewById(R.id.new_vitelname);
            crvNotifications = itemView.findViewById(R.id.newListVitals);
        }
    }
}