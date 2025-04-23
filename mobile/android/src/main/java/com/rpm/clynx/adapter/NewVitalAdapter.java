package com.rpm.clynx.adapter;

import androidx.recyclerview.widget.RecyclerView;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.ViewGroup;
import android.widget.TextView;
import com.rpm.clynx.databinding.NewVitalsItemBinding;
import com.rpm.clynx.model.NewVitalsItemModel;
import java.util.List;

/**
 * {@link RecyclerView.Adapter} that can display a {@link NewVitalsItemModel}.
 * TODO: Replace the implementation with code for your data type.
 */
public class NewVitalAdapter extends RecyclerView.Adapter<NewVitalAdapter.ViewHolder> {

    private final List<NewVitalsItemModel> mValues;
    public Context cxt;

    public NewVitalAdapter(List<NewVitalsItemModel> items) {
        mValues = items;
    }

    public NewVitalAdapter(List<NewVitalsItemModel> items,Context cxt) {
        mValues = items;
        this.cxt = cxt;
    }

    @Override
    public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        return new ViewHolder(NewVitalsItemBinding.inflate(LayoutInflater.from(parent.getContext()), parent, false));
    }

    @Override
    public void onBindViewHolder(final ViewHolder holder, int position) {
        holder.mItem = mValues.get(position);
        holder.mNotificationTime.setText(mValues.get(position).getValue());
        holder.mNotificationText.setText(mValues.get(position).getUnit());
        holder.mSchedule.setText(mValues.get(position).getSchedule());
        holder.mNotificationTm.setText(mValues.get(position).getTime());
    }

    @Override
    public int getItemCount() {
        return mValues.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder {
        public final TextView mNotificationTime;
        public final TextView mNotificationText;
        public final TextView mSchedule;
        public final TextView mNotificationTm;
        public NewVitalsItemModel mItem;

        public ViewHolder(NewVitalsItemBinding binding) {
            super(binding.getRoot());
            mNotificationTime = binding.itemNewVitalValue;
            mNotificationText = binding.itemNewVitalUnit;
            mSchedule = binding.itemNewVitalsched;
            mNotificationTm = binding.itemNewVitaltm;
        }

        @Override
        public String toString() {
            return super.toString() + " '" + mNotificationText.getText() + "'";
        }
    }
}