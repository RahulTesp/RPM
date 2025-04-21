package com.rpm.clynx.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.model.NotificationsModel;
import com.rpm.clynx.R;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link NotificationsModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class NotificationListAdapter extends RecyclerView.Adapter<NotificationListAdapter.ViewHolder> {

    private final List<NotificationsModel> mValues;
    public Context cxt;
    public NotificationListAdapter(List<NotificationsModel> items) {
        mValues = items;
    }
    public NotificationListAdapter(List<NotificationsModel> items,Context cxt)
    {
        mValues = items;
        this.cxt = cxt;
    }

    @Override
    public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.notification_item_list, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(final ViewHolder holder, int position) {
        holder.mItem = mValues.get(position);
        holder.mNotificationDate.setText(mValues.get(position).getNotificationDate());
        RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(cxt, LinearLayoutManager.VERTICAL, false);
        holder.crvNotifications.setLayoutManager(layoutManager);
        NotificationAdapter childRecyclerViewAdapter = new NotificationAdapter(mValues.get(position).getNotificationsList(),holder.crvNotifications.getContext());
        holder.crvNotifications.setAdapter(childRecyclerViewAdapter);
    }

    @Override
    public int getItemCount() {
        return mValues.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {
        public final TextView mNotificationDate;
        public final RecyclerView crvNotifications;
        public NotificationsModel mItem;

        public ViewHolder(View itemView) {
            super(itemView);
            mNotificationDate = itemView.findViewById(R.id.notification_date);
            crvNotifications = itemView.findViewById(R.id.listNotifications);
        }
    }
}