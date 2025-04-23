package com.rpm.clynx.adapter;

import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;
import com.rpm.clynx.databinding.NotificationItemBinding;
import com.rpm.clynx.model.NotificationItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link NotificationItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class NotificationAdapter extends RecyclerView.Adapter<NotificationAdapter.ViewHolder> {

    private final List<NotificationItemModel> mValues;
    public Context cxt;

    // Define the interface
    public interface OnDeleteClickListener {
        void onDeleteClick(String notiId); // Use String for the notification ID

    }

    private OnDeleteClickListener onDeleteClickListener;

    public NotificationAdapter(List<NotificationItemModel> items) {
        mValues = items;
    }

    public NotificationAdapter(List<NotificationItemModel> items,Context cxt) {
        mValues = items;
        this.cxt = cxt;
    }

    @Override
    public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new ViewHolder(NotificationItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final ViewHolder holder, int position) {
        holder.mItem = mValues.get(position);
        holder.mNotificationTime.setText(mValues.get(position).getCreatedTime());
        holder.mNotificationText.setText(mValues.get(position).getDescription());
    }

    @Override
    public int getItemCount() {
        return mValues.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {
        public final TextView mNotificationTime;
        public final TextView mNotificationText;

        public final ImageView mNotiDelete;
        public NotificationItemModel mItem;

        public ViewHolder(NotificationItemBinding binding) {
            super(binding.getRoot());
            mNotificationTime = binding.itemNotificationTime;
            mNotificationText = binding.itemNotificationText;
            mNotiDelete = binding.del;
            // Set mItem using the mValues list at the current position
            int position = getBindingAdapterPosition();
            if (position != RecyclerView.NO_POSITION) {
                mItem = mValues.get(position);
                mItem.getNotiId();
            }

            mNotiDelete.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {
                    Log.d("mNotiDelete", mItem.getNotiId());
                    int position = getAdapterPosition();

                   // onDeleteClickListener.onDeleteClick(mItem.getNotiId());
                    if (position != RecyclerView.NO_POSITION && onDeleteClickListener != null) {
                        Log.d("setOnClickListener", mItem.getNotiId());
                    }
                }
            });
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mNotificationText.getText() + "'";
        }
    }
}